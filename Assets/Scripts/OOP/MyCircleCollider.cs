using UnityEngine;

namespace HitEngine.OOP
{
    public class MyCircleCollider : MonoBehaviour
    {
        public Vector2 point;
        public float radius;
        public AABB box;
        public int inQuadTreeIndex = -1;

        public int Uid;

        private Renderer _renderer;
        private bool m_IsInHit = false;

        public void InitRandCircle()
        {
            radius = Random.Range(1f, 3f);
            transform.localScale = new Vector3(radius * 2, radius * 2, 0.1f);
            box = new AABB();

            _renderer = GetComponent<Renderer>();
        }

        public bool CheckHit(MyCircleCollider other)
        {
            var disSqr = (point - other.point).sqrMagnitude;
            var radiusSumSqr = (radius + other.radius) * (radius + other.radius);

            return disSqr <= radiusSumSqr;
        }

        public void SetInHitStatus(bool isInHit)
        {
            m_IsInHit = isInHit;
        }

        public void FlushHitStatus()
        {
            var color = m_IsInHit ? Color.red : Color.white;
            _renderer.material.SetColor("_Color", color);
        }

        private void FixedUpdate()
        {
            var pos = transform.position;
            point.x = pos.x;
            point.y = pos.y;

            box.Left = point.x - radius;
            box.Right = point.x + radius;
            box.Top = point.y + radius;
            box.Bottom = point.y - radius;
        }
    }
}

