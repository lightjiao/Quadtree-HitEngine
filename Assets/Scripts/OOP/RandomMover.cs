using UnityEngine;

namespace HitEngine.OOP
{
    public class RandomMover : MonoBehaviour
    {
        public Vector2 Speed;
        private float backgroundLength;

        public void InitRandSpeed()
        {
            Speed = new Vector2();
            Speed.x = Random.Range(-10f, 10f);
            Speed.y = Random.Range(-10f, 10f);
        }

        private void Start()
        {
            backgroundLength = FindObjectOfType<QuadtreeCheckHitEngine>().BackgroundLength;
        }

        private void Update()
        {
            Vector2 newPos = transform.position;
            newPos += Speed * Time.deltaTime;

            if (newPos.x < -backgroundLength / 2 || newPos.x > backgroundLength / 2)
            {
                Speed.x = -Speed.x;
                newPos.x = Mathf.Clamp(newPos.x, -backgroundLength / 2, backgroundLength / 2);
            }

            if (newPos.y > backgroundLength / 2 || newPos.y < -backgroundLength / 2)
            {
                Speed.y = -Speed.y;
                newPos.y = Mathf.Clamp(newPos.y, -backgroundLength / 2, backgroundLength / 2);
            }

            transform.position = newPos;
        }
    }
}