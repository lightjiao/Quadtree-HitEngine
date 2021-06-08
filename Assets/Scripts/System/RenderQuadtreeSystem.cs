using System;
using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class RenderQuadtreeSystem : ReactiveSystem<GameEntity>
{
    public RenderQuadtreeSystem(Contexts contexts) : base(contexts.game)
    {
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.QuadtreeNode);
    }

    protected override bool Filter(GameEntity entity)
    {
        return true;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (var entity in entities)
        {
            var box = entity.quadtreeNode.box;

            // Debug.DrawLine(box.Left, );
        }
    }
}