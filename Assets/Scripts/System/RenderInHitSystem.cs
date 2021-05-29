using Entitas;
using System.Collections.Generic;
using UnityEngine;

internal class RenderInHitSystem : ReactiveSystem<GameEntity>
{
    public RenderInHitSystem(Contexts contexts) : base(contexts.game)
    {
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (var e in entities)
        {
            var color = e.isInHit ? Color.red : Color.white;

            var renderer = e.view.go.GetComponent<Renderer>();
            renderer.material.SetColor("_Color", color);
        }
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasView;
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.InHit);
    }
}