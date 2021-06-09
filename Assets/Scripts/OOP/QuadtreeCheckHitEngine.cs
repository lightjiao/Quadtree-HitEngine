using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace HitEngine.OOP
{

    public struct AABB
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public float MiddleLength => (Left + Right) / 2;
        public float MiddleHeight => (Top + Bottom) / 2;

        public bool IsIn(AABB b)
        {
            if (Left >= b.Left && Right <= b.Right && Top <= b.Top && Bottom >= b.Bottom)
            {
                return true;
            }

            return false;
        }
    }

    public struct QuadtreeNode
    {
        public int Index;
        public AABB Box;
        public List<MyCircleCollider> Colliders;
    }

    public class QuadtreeCheckHitEngine : MonoBehaviour
    {
        public float BackgroundLength = 100f;
        [SerializeField] private int depth = 3;
        [SerializeField] private float m_LooseSpacing = 5f;

        [SerializeField]
        private List<MyCircleCollider> m_AllColliders = new List<MyCircleCollider>();
        private QuadtreeNode[] m_Quadtree;

        // 碰撞检测的Job
        private struct CheckHitJob : IJob
        {
            public void Execute()
            {
                throw new System.NotImplementedException();
            }
        }

        private void Awake()
        {
            InitQuadtree();
        }

        private void FixedUpdate()
        {
            UpdateQuadtree();
            CheckHit();
        }

        public void RegisterOne(MyCircleCollider myCollider)
        {
            myCollider.inQuadTreeIndex = -1;
            m_AllColliders.Add(myCollider);
        }

        private void InitQuadtree()
        {
            var nodeSum = 0;
            for (var i = 0; i < depth; i++)
            {
                nodeSum += (int)Mathf.Pow(4, i);
            }

            // 用数组表示四叉树
            m_Quadtree = new QuadtreeNode[nodeSum];
            var rootBox = new AABB();
            for (var i = 0; i < nodeSum; i++)
            {
                var aabb = new AABB();

                if (i == 0)
                {
                    aabb.Left = -BackgroundLength / 2 - m_LooseSpacing;
                    aabb.Right = BackgroundLength / 2 + m_LooseSpacing;
                    aabb.Top = BackgroundLength / 2 + m_LooseSpacing;
                    aabb.Bottom = -BackgroundLength / 2 - m_LooseSpacing;
                    rootBox = aabb;
                }
                else
                {
                    var parentAABB = m_Quadtree[(i - 1) / 4].Box;
                    var curNodeIdx = i % 4;
                    // 1、2、3、0 分别代表 左上、右上、左下、右下
                    switch (curNodeIdx)
                    {
                        case 1:
                            aabb.Left = Mathf.Max(parentAABB.Left - m_LooseSpacing, rootBox.Left);
                            aabb.Right = parentAABB.MiddleLength + m_LooseSpacing;
                            aabb.Top = Mathf.Min(parentAABB.Top + m_LooseSpacing, rootBox.Top);
                            aabb.Bottom = parentAABB.MiddleHeight - m_LooseSpacing;
                            break;
                        case 2:
                            aabb.Left = parentAABB.MiddleLength - m_LooseSpacing;
                            aabb.Right = Mathf.Min(parentAABB.Right + m_LooseSpacing, rootBox.Right);
                            aabb.Top = Mathf.Min(parentAABB.Top + m_LooseSpacing, rootBox.Top);
                            aabb.Bottom = parentAABB.MiddleHeight - m_LooseSpacing;
                            break;
                        case 3:
                            aabb.Left = Mathf.Max(parentAABB.Left - m_LooseSpacing, rootBox.Left);
                            ;
                            aabb.Right = parentAABB.MiddleLength + m_LooseSpacing;
                            aabb.Top = parentAABB.MiddleHeight + m_LooseSpacing;
                            aabb.Bottom = Mathf.Max(parentAABB.Bottom - m_LooseSpacing, rootBox.Bottom);
                            break;
                        case 0:
                            aabb.Left = parentAABB.MiddleLength - m_LooseSpacing;
                            aabb.Right = Mathf.Min(parentAABB.Right + m_LooseSpacing, rootBox.Right);
                            aabb.Top = parentAABB.MiddleHeight + m_LooseSpacing;
                            aabb.Bottom = Mathf.Max(parentAABB.Bottom - m_LooseSpacing, rootBox.Bottom);
                            break;
                    }
                }

                var quadtreeNode = new QuadtreeNode
                {
                    Index = i,
                    Box = aabb,
                    Colliders = new List<MyCircleCollider>()
                };
                m_Quadtree[i] = quadtreeNode;
            }
        }

        private void UpdateQuadtree()
        {
            foreach (var myCollider in m_AllColliders)
            {
                if (false == UpdateQuadtree(myCollider.inQuadTreeIndex, myCollider))
                {
                    UpdateQuadtree(0, myCollider);    
                }
            }
        }

        private bool UpdateQuadtree(int index, MyCircleCollider myCollider)
        {
            if (index < 0 || index >= m_Quadtree.Length) return false;

            var node = m_Quadtree[index];
            if (index != 0 && !myCollider.box.IsIn(node.Box))
            {
                return false;
            }

            // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
            for (var i = 0; i < 4; i++)
            {
                if (UpdateQuadtree(4 * (node.Index + 1) - i, myCollider))
                {
                    return true;
                }
            }

            // 位置没变
            var oldIndex = myCollider.inQuadTreeIndex;
            if (oldIndex == index) return true;

            // 在旧的节点移除，添加到新的节点
            if (oldIndex > 0 && oldIndex < m_Quadtree.Length)
            {
                m_Quadtree[oldIndex].Colliders.Remove(myCollider);
            }

            m_Quadtree[index].Colliders.Add(myCollider);
            myCollider.inQuadTreeIndex = index;
            return true;
        }

        private void CheckHit()
        {
            foreach (var myCollider in m_AllColliders)
            {
                myCollider.SetInHitStatus(false);

                var stack = new Stack<QuadtreeNode>();
                stack.Push(m_Quadtree[0]);
                while (stack.Count > 0)
                {
                    var quadTreeNode = stack.Pop();
                    var aabb = quadTreeNode.Box;
                    var index = quadTreeNode.Index;
                    if (false == myCollider.box.IsIn(aabb)) continue;

                    // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                    if (4 * (index + 1) < m_Quadtree.Length)
                    {
                        stack.Push(m_Quadtree[4 * (index + 1) - 3]);
                        stack.Push(m_Quadtree[4 * (index + 1) - 2]);
                        stack.Push(m_Quadtree[4 * (index + 1) - 1]);
                        stack.Push(m_Quadtree[4 * (index + 1) - 0]);
                    }

                    foreach (var otherCollider in quadTreeNode.Colliders)
                    {
                        if (myCollider == otherCollider) continue;
                        if (myCollider.CheckHit(otherCollider))
                        {
                            myCollider.SetInHitStatus(true);
                            otherCollider.SetInHitStatus(true);
                        }
                    }
                }

                myCollider.FlushHitStatus();
            }
        }
    }
}