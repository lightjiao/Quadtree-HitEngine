using Entitas;
using UnityEngine;

namespace HitEngine.Entitas
{
    /// <summary>
    /// 初始化时创建很多个可碰撞对象
    /// </summary>
    public class InitSystem : IInitializeSystem
    {
        private readonly GameContext _context;

        public InitSystem(Contexts contexts)
        {
            _context = contexts.game;
        }

        public void Initialize()
        {
            for (var i = 0; i < 500; i++)
            {
                var circleEntity = _context.CreateEntity();
                circleEntity.isNeedView = true;
                circleEntity.AddPosition(new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)));
                circleEntity.AddCircleHitable(Random.Range(1, 3));
                circleEntity.AddRandMover(new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
                circleEntity.AddInQuadtreeIdx(-1);

                //var rectEntity = _context.CreateEntity();
                //rectEntity.isNeedView = true;
                //rectEntity.AddPosition(new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)));
                //rectEntity.AddRectHitable(Random.Range(1, 3), Random.Range(1, 3));

                //var capsuleEntity = _context.CreateEntity();
                //capsuleEntity.isNeedView = true;
                //capsuleEntity.AddPosition(new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)));
                //var capsuleRadius = Random.Range(1, 3);
                //var capsuleVec = Vector3.up * capsuleRadius * 2;
                //capsuleEntity.AddCapuleHitable(capsuleRadius, capsuleVec);
            }
        }
    }
}