using Entitas;
using System.Collections.Generic;

namespace HitEngine.Entitas
{
    public class RenderPositionSystem : ReactiveSystem<GameEntity>
    {
        public RenderPositionSystem(Contexts contexts) : base(contexts.game)
        {
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var e in entities)
            {
                e.view.go.transform.position = e.position.value;
            }
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasPosition && entity.hasView;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.Position);
        }
    }
}