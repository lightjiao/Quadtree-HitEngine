using Entitas;
using UnityEngine;

namespace HitEngine.Entitas
{
    internal class RenderInHitSystem : IExecuteSystem
    {
        private GameContext _context;

        public RenderInHitSystem(Contexts contexts)
        {
            _context = contexts.game;
        }

        public void Execute()
        {
            var group = _context.GetGroup(GameMatcher.AllOf(GameMatcher.View)
                .AnyOf(
                    GameMatcher.CircleHitable,
                    GameMatcher.RectHitable,
                    GameMatcher.CapuleHitable
                )
            );

            foreach (var e in group.AsEnumerable())
            {
                var color = e.isInHit ? Color.red : Color.white;

                var renderer = e.view.go.GetComponent<Renderer>();
                renderer.material.SetColor("_Color", color);
            }
        }
    }
}