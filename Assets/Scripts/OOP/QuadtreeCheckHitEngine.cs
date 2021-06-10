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

        // 碰撞检测的Job
        private struct CheckHitJob : IJob
        {
            public NativeArray<MyCircleColliderData> CheckedColliders;
            [ReadOnly] public NativeArray<QuadtreeNodeJobData> Quadtree;
            [ReadOnly] public NativeArray<MyCircleColliderData> AllColliders;

            public void Execute()
            {
                for (var i = 0; i < CheckedColliders.Length; i++)
                {
                    var dataCopy = CheckedColliders[i];
                    dataCopy.IsInHit = true;
                    CheckedColliders[i] = dataCopy;
                }

                for (var i = 0; i < CheckedColliders.Length; i++)
                {
                    var dataCopy = CheckedColliders[i];
                    dataCopy.IsInHit = false;

                    var stack = new Stack<int>();
                    stack.Push(0);
                    while (stack.Count > 0)
                    {
                        var index = stack.Pop();
                        if (index < 0 || index >= Quadtree.Length) continue;

                        var treeNode = Quadtree[index];
                        var aabb = treeNode.Box;
                        if (false == dataCopy.box.IsIn(aabb)) continue;

                        // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                        stack.Push(4 * (index + 1) - 3);
                        stack.Push(4 * (index + 1) - 2);
                        stack.Push(4 * (index + 1) - 1);
                        stack.Push(4 * (index + 1) - 0);

                        for (var otherColliderIdx = treeNode.ColliderStartIdx;
                            otherColliderIdx < treeNode.ColliderCount;
                            otherColliderIdx++)
                        {
                            var otherColliderData = AllColliders[otherColliderIdx];
                            if (dataCopy.Uid == otherColliderData.Uid) continue;
                            if (dataCopy.CheckHit(otherColliderData))
                            {
                                dataCopy.IsInHit = true;
                            }
                        }
                    }

                    CheckedColliders[i] = dataCopy;
                }
            }
        }

        private void CheckHit()
        {
            var jobQuadtree = new NativeArray<QuadtreeNodeJobData>(m_Quadtree.Length, Allocator.TempJob);
            var jobAllColliders = new NativeArray<MyCircleColliderData>(m_AllColliders.Count, Allocator.TempJob);

            var collidersPreSum = 0;
            for (var i = 0; i < m_Quadtree.Length; i++)
            {
                for (var j = 0; j < m_Quadtree[i].Colliders.Count; j++)
                {
                    jobAllColliders[collidersPreSum + j] = m_Quadtree[i].Colliders[j].Data;
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

            var jobs = new CheckHitJob[CalJobCount()];
            var jobHandles = new JobHandle[CalJobCount()];
            for (var i = 0; i < jobHandles.Length; i++)
            {
                jobs[i] = CreateCheckHitJob(jobQuadtree, jobAllColliders, i);
                jobHandles[i] = jobs[i].Schedule();
            }

            foreach (var jobHandle in jobHandles)
            {
                jobHandle.Complete();
            }

            for (var i = 0; i < jobs.Length; i++)
            {
                for (var j = 0; j < ColliderNumPerJob; j++)
                {
                    var realIdx = i * ColliderNumPerJob + j;
                    if (realIdx >= m_AllColliders.Count) continue;
                    m_AllColliders[realIdx].Data = jobs[i].CheckedColliders[j];
                    m_AllColliders[realIdx].FlushHitStatus();
                }
            }

            jobQuadtree.Dispose();
            jobAllColliders.Dispose();
            foreach (var job in jobs)
            {
                job.CheckedColliders.Dispose();
            }
        }

        private int CalJobCount()
        {
            if (m_AllColliders.Count == 0) return 1;

            if (m_AllColliders.Count % ColliderNumPerJob == 0)
            {
                return m_AllColliders.Count / ColliderNumPerJob;
            }

            return (m_AllColliders.Count / ColliderNumPerJob) + 1;
        }

        private CheckHitJob CreateCheckHitJob(NativeArray<QuadtreeNodeJobData> jobQuadtree, NativeArray<MyCircleColliderData> jobAllColliders, int jobIndex)
        {
            var leftCollidersNum = m_AllColliders.Count - jobIndex * ColliderNumPerJob;
            var checkedColliderNum = Mathf.Min(leftCollidersNum, ColliderNumPerJob);
            
            var jobCheckedColliders = new NativeArray<MyCircleColliderData>(checkedColliderNum, Allocator.TempJob);
            for (var i = 0; i < checkedColliderNum; i++)
            {
                jobCheckedColliders[i] = m_AllColliders[ColliderNumPerJob * jobIndex + i].Data;
            }

            return new CheckHitJob
            {
                CheckedColliders = jobCheckedColliders,
                Quadtree = jobQuadtree,
                AllColliders = jobAllColliders
            };
        }
    }
}