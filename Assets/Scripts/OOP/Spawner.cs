using System.Collections;
using UnityEngine;

namespace HitEngine.OOP
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private int number = 100;

        private QuadtreeCheckHitEngine _quadtreeCheckHitEngine;

        private void Start()
        {
            _quadtreeCheckHitEngine = FindObjectOfType<QuadtreeCheckHitEngine>();
            StartCoroutine(SpawnOne());
        }

        private IEnumerator SpawnOne()
        {
            var i = 0;
            while (i < number)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = i.ToString();
                go.transform.parent = transform;
                Destroy(go.GetComponent<Collider>());

                var randomMover = go.AddComponent<RandomMover>();
                randomMover.InitRandSpeed();

                var myCircleCollider = go.AddComponent<MyCircleCollider>();
                myCircleCollider.InitRandCircle();
                myCircleCollider.Data.Uid = i;
                _quadtreeCheckHitEngine.RegisterOne(myCircleCollider);

                i++;
                yield return null;
            }

            yield return null;
        }
    }
}