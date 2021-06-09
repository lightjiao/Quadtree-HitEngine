using Entitas;
using Entitas.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace HitEngine.Entitas
{
    public class AddViewSystem : ReactiveSystem<GameEntity>
    {
        private readonly Transform _viewContainer = new GameObject("View Container").transform;

        public AddViewSystem(Contexts contexts) : base(contexts.game)
        {
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var e in entities)
            {
                var go = new GameObject("Game View");
                go.transform.SetParent(_viewContainer, false);
                e.AddView(go);
                go.Link(e);
            }
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasView == false;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            // 有View的都要加View
            return context.CreateCollector(GameMatcher.NeedView);
        }
    }
}