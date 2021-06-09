using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace HitEngine.Entitas
{
    public class RenderHitableSystem : ReactiveSystem<GameEntity>
    {
        public RenderHitableSystem(Contexts contexts) : base(contexts.game)
        {
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var e in entities)
            {
                var renderer = e.view.go.TryAddComponent<MeshRenderer>();
                renderer.materials = new Material[] { new Material(Shader.Find("Standard")) };

                var meshfilter = e.view.go.TryAddComponent<MeshFilter>();
                if (e.hasCircleHitable)
                {
                    meshfilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                    e.view.go.transform.localScale = new Vector3(e.circleHitable.radius, e.circleHitable.radius, 0.01f);
                }
                if (e.hasCapuleHitable)
                {
                    meshfilter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
                    e.view.go.transform.localScale = new Vector3(e.capuleHitable.radius, e.capuleHitable.radius, 0.01f);
                }
                if (e.hasRectHitable)
                {
                    meshfilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                    e.view.go.transform.localScale = new Vector3(e.rectHitable.length, e.rectHitable.height, 0.01f);
                }
            }
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasView;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(
                GameMatcher.AnyOf(
                    GameMatcher.CircleHitable,
                    GameMatcher.RectHitable,
                    GameMatcher.CapuleHitable
                )
            );
        }
    }
}