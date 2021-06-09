using UnityEngine;

namespace HitEngine.OOP
{
    public struct MyCircleColliderData
    {
        public Vector2 point;
        public float radius;
        public int inQuadTreeIndex;
        public bool IsInHit;
    }

    public class MyCircleCollider : MonoBehaviour
    {
        public MyCircleColliderData Data;
        public AABB box;

        public int Uid;

        private Renderer _renderer;

        public void InitRandCircle()
        {
            Data = new MyCircleColliderData
            {
                radius = Random.Range(1f, 3f),
                inQuadTreeIndex = -1,
                IsInHit = false,
            };

            transform.localScale = new Vector3(Data.radius * 2, Data.radius * 2, 0.1f);
            box = new AABB();

            _renderer = GetComponent<Renderer>();
        }

        public bool CheckHit(MyCircleCollider other)
        {
            var disSqr = (Data.point - other.Data.point).sqrMagnitude;
            var radiusSumSqr = (Data.radius + other.Data.radius) * (Data.radius + other.Data.radius);

            return disSqr <= radiusSumSqr;
        }

        public void SetInHitStatus(bool isInHit)
        {
            Data.IsInHit = isInHit;
        }

        public void FlushHitStatus()
        {
            var color = Data.IsInHit ? Color.red : Color.green;
            _renderer.material.SetColor("_Color", color);
        }

        private void FixedUpdate()
        {
            var pos = transform.position;
            Data.point.x = pos.x;
            Data.point.y = pos.y;

            box.Left = Data.point.x - Data.radius;
            box.Right = Data.point.x + Data.radius;
            box.Top = Data.point.y + Data.radius;
            box.Bottom = Data.point.y - Data.radius;
        }
    }
}