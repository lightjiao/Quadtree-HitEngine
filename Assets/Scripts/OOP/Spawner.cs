using System.Collections;
using UnityEngine;

namespace HitEngine.OOP
{
    public class Spawner : MonoBehaviour
    {
        private enum EngineType
        {
            SimpleEngine,
            QuadtreeEngine
        }

        [SerializeField] private EngineType m_EngineType;
        [SerializeField] private int m_TotalNumber = 100;
        [SerializeField] private int m_SpawnedNumber = 0;

        private QuadtreeCheckHitEngine _quadtreeCheckHitEngine;
        private SimpleHitEngine _simpleHitEngine;

        private void Start()
        {
            _quadtreeCheckHitEngine = FindObjectOfType<QuadtreeCheckHitEngine>();
            _simpleHitEngine = FindObjectOfType<SimpleHitEngine>();
            StartCoroutine(SpawnOne());
        }

        private IEnumerator SpawnOne()
        {
            m_SpawnedNumber = 0;
            while (m_SpawnedNumber < m_TotalNumber)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = m_SpawnedNumber.ToString();
                go.transform.parent = transform;
                Destroy(go.GetComponent<Collider>());

                var randomMover = go.AddComponent<RandomMover>();
                randomMover.InitRandSpeed();

                var myCircleCollider = go.AddComponent<MyCircleCollider>();
                myCircleCollider.InitRandCircle();
                myCircleCollider.Data.Uid = m_SpawnedNumber;

                if (m_EngineType == EngineType.QuadtreeEngine)
                {
                    _quadtreeCheckHitEngine.RegisterOne(myCircleCollider);
                }
                else if (m_EngineType == EngineType.SimpleEngine)
                {
                    _simpleHitEngine.RegisterOne(myCircleCollider);
                }

                m_SpawnedNumber++;
                yield return null;
            }

            yield return null;
        }
    }
}