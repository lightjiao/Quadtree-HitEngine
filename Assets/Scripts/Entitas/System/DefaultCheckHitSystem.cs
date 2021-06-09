using Entitas;
using System.Collections.Generic;

namespace HitEngine.Entitas
{
    internal class DefaultCheckHitSystem : ReactiveSystem<GameEntity>
    {
        private readonly GameContext _context;

        public DefaultCheckHitSystem(Contexts contexts) : base(contexts.game)
        {
            _context = contexts.game;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            var circles = _context.GetGroup(GameMatcher.CircleHitable);
            var rects = _context.GetGroup(GameMatcher.RectHitable);
            var capsules = _context.GetGroup(GameMatcher.CapuleHitable);

            foreach (var e in entities)
            {
                e.isInHit = false;

                if (e.hasCircleHitable)
                {
                    foreach (var c in circles)
                    {
                        if (e == c) continue;

                        if (UtilityCheckHit.CheckCirclesAndCircles(e, c))
                        {
                            e.isInHit = true;
                        }
                    }
                }
            }
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasPosition && (entity.hasCapuleHitable || entity.hasCircleHitable || entity.hasRectHitable);
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            // 只对Position有变化的物体做碰撞检测
            return context.CreateCollector(GameMatcher.Position);
        }
    }
}