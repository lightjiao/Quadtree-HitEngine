using UnityEngine;

namespace HitEngine.OOP
{
    public struct MyCircleColliderData
    {
        public int Uid;
        public Vector2 point;
        public float radius;
        public int inQuadTreeIndex;
        public bool IsInHit;
        public AABB box;
        
        public bool CheckHit(MyCircleColliderData other)
        {
            var disSqr = (point - other.point).sqrMagnitude;
            var radiusSumSqr = (radius + other.radius) * (radius + other.radius);

            return disSqr <= radiusSumSqr;
        }
    }

    public class MyCircleCollider : MonoBehaviour
    {
        public MyCircleColliderData Data;

        private Renderer _renderer;

        public void InitRandCircle()
        {
            Data = new MyCircleColliderData
            {
                Uid = -1,
                point = new Vector2(),
                radius = Random.Range(1f, 3f),
                inQuadTreeIndex = -1,
                IsInHit = false,
                box = new AABB(),
            };

            transform.localScale = new Vector3(Data.radius * 2, Data.radius * 2, 0.1f);

            _renderer = GetComponent<Renderer>();
        }

        public void FlushHitStatus()
        {
            var color = Data.IsInHit ? Color.red : Color.green;
            _renderer.material.SetColor("_Color", color);
        }

        private void Update()
        {
            var pos = transform.position;
            Data.point.x = pos.x;
            Data.point.y = pos.y;

            Data.box.Left = Data.point.x - Data.radius;
            Data.box.Right = Data.point.x + Data.radius;
            Data.box.Top = Data.point.y + Data.radius;
            Data.box.Bottom = Data.point.y - Data.radius;
        }
    }
}