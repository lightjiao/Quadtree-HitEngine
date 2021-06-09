using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

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

    public struct QuadtreeNodeData
    {
        public int Index;
        public AABB Box;
        public NativeArray<MyCircleColliderData> ColliderDatas;
    }

    public class QuadtreeCheckHitEngine : MonoBehaviour
    {
        public float BackgroundLength = 100f;
        [SerializeField] private int depth = 3;
        [SerializeField] private float m_LooseSpacing = 5f;

        [SerializeField]
        private List<MyCircleCollider> m_AllColliders = new List<MyCircleCollider>();
        private QuadtreeNode[] m_Quadtree;

        private void Awake()
        {
            InitQuadtree();
        }

        private void Update()
        {
            UpdateQuadtree();
            CheckHit();
        }

        public void RegisterOne(MyCircleCollider myCollider)
        {
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
                if (false == UpdateQuadtree(myCollider.Data.inQuadTreeIndex, myCollider))
                {
                    UpdateQuadtree(0, myCollider);
                }
            }
        }

        private bool UpdateQuadtree(int index, MyCircleCollider myCollider)
        {
            if (index < 0 || index >= m_Quadtree.Length) return false;

            var node = m_Quadtree[index];
            if (index != 0 && !myCollider.Data.box.IsIn(node.Box))
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
            var oldIndex = myCollider.Data.inQuadTreeIndex;
            if (oldIndex == index) return true;

            // 在旧的节点移除，添加到新的节点
            if (oldIndex > 0 && oldIndex < m_Quadtree.Length)
            {
                m_Quadtree[oldIndex].Colliders.Remove(myCollider);
            }

            m_Quadtree[index].Colliders.Add(myCollider);
            myCollider.Data.inQuadTreeIndex = index;
            return true;
        }


        // 碰撞检测的Job
        private struct CheckHitJob : IJob
        {
            // 100个碰撞体
            public NativeArray<MyCircleColliderData> HundreadColliders;
            //public NativeArray<QuadtreeNodeData> quadtree;

            public void Execute()
            {
                for (var i = 0; i < HundreadColliders.Length; i++)
                {
                    var dataCopy = HundreadColliders[i];
                    dataCopy.IsInHit = true;
                    HundreadColliders[i] = dataCopy;
                }

                //for (var i = 0; i < HundreadColliders.Length; i++)
                //{
                //    var dataCopy = HundreadColliders[i];
                //    dataCopy.IsInHit = false;

                //    var stack = new Stack<int>();
                //    stack.Push(0);
                //    while (stack.Count > 0)
                //    {
                //        var index = stack.Pop();
                //        if (index < 0 || index >= quadtree.Length) continue;

                //        var quadTreeNode = quadtree[index];
                //        var aabb = quadTreeNode.Box;
                //        if (false == dataCopy.box.IsIn(aabb)) continue;

                //        // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                //        stack.Push(4 * (index + 1) - 3);
                //        stack.Push(4 * (index + 1) - 2);
                //        stack.Push(4 * (index + 1) - 1);
                //        stack.Push(4 * (index + 1) - 0);

                //        foreach (var otherColliderData in quadTreeNode.ColliderDatas)
                //        {
                //            if (dataCopy.Uid == otherColliderData.Uid) continue;
                //            if (dataCopy.CheckHit(otherColliderData))
                //            {
                //                dataCopy.IsInHit = true;
                //            }
                //        }
                //    }

                //    HundreadColliders[i] = dataCopy;
                //}
            }
        }

        private void CheckHit()
        {
            //var quadtreeNativeArray = new NativeArray<QuadtreeNodeData>(m_Quadtree.Length, Allocator.TempJob);
            //for (var i = 0; i < quadtreeNativeArray.Length; i++)
            //{
            //    var collidersData = new NativeArray<MyCircleColliderData>(m_Quadtree[i].Colliders.Count, Allocator.TempJob);
            //    for (var j = 0; j < m_Quadtree[i].Colliders.Count; j++)
            //    {
            //        collidersData[i] = m_Quadtree[i].Colliders[i].Data;
            //    }

            //    quadtreeNativeArray[i] = new QuadtreeNodeData
            //    {
            //        Index = m_Quadtree[i].Index,
            //        Box = m_Quadtree[i].Box,
            //        ColliderDatas = collidersData
            //    };
            //}

            var job = new CheckHitJob
            {
                HundreadColliders = new NativeArray<MyCircleColliderData>(m_AllColliders.Count, Allocator.TempJob),
                //quadtree = quadtreeNativeArray,
            };

            job.Schedule().Complete();

            for (var i = 0; i < job.HundreadColliders.Length; i++)
            {
                m_AllColliders[i].Data = job.HundreadColliders[i];
                m_AllColliders[i].FlushHitStatus();
            }

            job.HundreadColliders.Dispose();
        }
    }
}