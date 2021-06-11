using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace HitEngine.OOP
{
    public class SimpleHitEngine : MonoBehaviour
    {
        [SerializeField] private int DataPerJob = 100;

        private List<MyCircleCollider> m_AllColliders;

        private void Awake()
        {
            m_AllColliders = new List<MyCircleCollider>();
        }

        private void Update()
        {
            CheckHit();
        }

        public void RegisterOne(MyCircleCollider myCollider)
        {
            m_AllColliders.Add(myCollider);
        }


        private void CheckHit()
        {
            JobSystemCheckHit();
        }

        private void SimpleCheckHit()
        {
            foreach (var t1 in m_AllColliders)
            {
                t1.Data.IsInHit = false;

                foreach (var t in m_AllColliders)
                {
                    if (t1.Data.CheckHit(t.Data))
                    {
                        t1.Data.IsInHit = true;
                    }
                }

                t1.FlushHitStatus();
            }
        }

        [BurstCompile]
        private struct CheckHitJob : IJobParallelFor
        {
            public NativeArray<MyCircleColliderData> CheckedColliderDatas;
            [ReadOnly] public NativeArray<MyCircleColliderData> AllColliderDatas;

            public void Execute(int index)
            {
                var dataCopy = CheckedColliderDatas[index];
                dataCopy.IsInHit = false;
                
                // 使用foreach的话会有Burst编译报错:
                // Burst error BC1005: The `try` construction is not supported
                for (var i = 0; i < AllColliderDatas.Length; i++)
                {
                    // var otherData = AllColliderDatas[i];
                    if (dataCopy.Uid == AllColliderDatas[i].Uid) continue;
                    if (dataCopy.CheckHit(AllColliderDatas[i]))
                    {
                        dataCopy.IsInHit = true;
                    }
                }

                CheckedColliderDatas[index] = dataCopy;
            }
        }

        private void JobSystemCheckHit()
        {
            var jobAllColliderData = new NativeArray<MyCircleColliderData>(m_AllColliders.Count, Allocator.TempJob);
            for (var i = 0; i < m_AllColliders.Count; i++)
            {
                jobAllColliderData[i] = m_AllColliders[i].Data;
            }

            var checkedColliderData = new NativeArray<MyCircleColliderData>(m_AllColliders.Count, Allocator.TempJob);
            for (var i = 0; i < m_AllColliders.Count; i++)
            {
                checkedColliderData[i] = m_AllColliders[i].Data;
            }

            var job = new CheckHitJob
            {
                CheckedColliderDatas = checkedColliderData,
                AllColliderDatas = jobAllColliderData,
            };

            // 第一个参数为需要指定split参数的总长度，也往往意味着JobParallel中的Execute()会执行多少次，入参即为 0~总长度
            // 第二个参数为指定一个bath执行多少个Execute，一个bath可以认为是一个Job
            job.Schedule(m_AllColliders.Count, DataPerJob).Complete();

            for (var i = 0; i < m_AllColliders.Count; i++)
            {
                m_AllColliders[i].Data = checkedColliderData[i];
                m_AllColliders[i].FlushHitStatus();
            }

            checkedColliderData.Dispose();
            jobAllColliderData.Dispose();
        }
    }
}