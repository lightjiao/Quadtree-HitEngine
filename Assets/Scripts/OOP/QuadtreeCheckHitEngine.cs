using System.Collections.Generic;
using Unity.Burst;
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

        public bool IsHit(AABB other)
        {
            if (Right >= other.Left && Left <= other.Right)
            {
                if (Bottom <= other.Top && Top >= other.Bottom)
                {
                    return true;
                }
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
        [SerializeField] private int ColliderNumPerJob = 100;

        [SerializeField] private List<MyCircleCollider> m_AllColliders = new List<MyCircleCollider>();
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
                nodeSum += (int) Mathf.Pow(4, i);
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

        public struct QuadtreeNodeJobData
        {
            public int Index;
            public AABB Box;
            public int ColliderStartIdx; // 包含的碰撞体开始下标(include)
            public int ColliderCount; // 包含的碰撞体结束下标(exclude)
        }

        /// <remarks>
        /// Burst中不能创建 managed type，可以理解为被托管的类型，会有GC的类型
        /// 所以将之前的stack的实现换成了递归调用函数
        /// </remarks>
        [BurstCompile]
        private struct CheckHitJob : IJobParallelFor
        {
            public NativeArray<MyCircleColliderData> WriteColliders;
            [ReadOnly] public NativeArray<QuadtreeNodeJobData> Quadtree;
            [ReadOnly] public NativeArray<MyCircleColliderData> ReadCollidersInTree;

            public void Execute(int i)
            {
                var dataCopy = WriteColliders[i];
                dataCopy.IsInHit = false;
                CheckHit(ref dataCopy, 0);
                WriteColliders[i] = dataCopy;
            }

            private void CheckHit(ref MyCircleColliderData dataCopy, int treeIdx)
            {
                if (treeIdx < 0 || treeIdx >= Quadtree.Length) return;
                var treeNode = Quadtree[treeIdx];

                if (false == dataCopy.box.IsHit(treeNode.Box)) return;

                for (var j = 0; j < treeNode.ColliderCount; j++)
                {
                    var otherColliderData = ReadCollidersInTree[treeNode.ColliderStartIdx + j];
                    if (dataCopy.Uid == otherColliderData.Uid) continue;
                    if (dataCopy.CheckHit(otherColliderData))
                    {
                        dataCopy.IsInHit = true;
                    }
                }

                // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                CheckHit(ref dataCopy, 4 * (treeIdx + 1) - 3);
                CheckHit(ref dataCopy, 4 * (treeIdx + 1) - 2);
                CheckHit(ref dataCopy, 4 * (treeIdx + 1) - 1);
                CheckHit(ref dataCopy, 4 * (treeIdx + 1) - 0);
            }
        }

        private void CheckHit()
        {
            var jobQuadtree = new NativeArray<QuadtreeNodeJobData>(m_Quadtree.Length, Allocator.TempJob);
            var jobReadAllColliders = new NativeArray<MyCircleColliderData>(m_AllColliders.Count, Allocator.TempJob);
            var collidersPreSum = 0;
            for (var i = 0; i < m_Quadtree.Length; i++)
            {
                for (var j = 0; j < m_Quadtree[i].Colliders.Count; j++)
                {
                    jobReadAllColliders[collidersPreSum + j] = m_Quadtree[i].Colliders[j].Data;
                }

                jobQuadtree[i] = new QuadtreeNodeJobData
                {
                    Index = m_Quadtree[i].Index,
                    Box = m_Quadtree[i].Box,
                    ColliderStartIdx = collidersPreSum,
                    ColliderCount = m_Quadtree[i].Colliders.Count,
                };
                collidersPreSum += m_Quadtree[i].Colliders.Count;
            }

            var jobWriteAllColliders = new NativeArray<MyCircleColliderData>(m_AllColliders.Count, Allocator.TempJob);
            for (var i = 0; i < m_AllColliders.Count; i++)
            {
                jobWriteAllColliders[i] = m_AllColliders[i].Data;
            }

            var job = new CheckHitJob
            {
                WriteColliders = jobWriteAllColliders,
                Quadtree = jobQuadtree,
                ReadCollidersInTree = jobReadAllColliders
            };

            // 第一个参数为需要指定split参数的总长度，也往往意味着JobParallel中的Execute()会执行多少次，入参即为 0~总长度
            // 第二个参数为指定一个bath执行多少个Execute，一个bath可以认为是一个Job
            job.Schedule(m_AllColliders.Count, ColliderNumPerJob).Complete();

            for (var i = 0; i < jobWriteAllColliders.Length; i++)
            {
                m_AllColliders[i].Data = jobWriteAllColliders[i];
                m_AllColliders[i].FlushHitStatus();
            }

            jobQuadtree.Dispose();
            jobReadAllColliders.Dispose();
            jobWriteAllColliders.Dispose();
        }
    }
}