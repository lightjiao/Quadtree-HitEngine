using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace HitEngine.Entities
{
    [UpdateAfter(typeof(QuadtreeCheckHitSystem))]
    public class RenderInHitSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Entities.ForEach uses ISharedComponentType RenderMesh. This is only supported when using .WithoutBurst() and  .Run()
            // error DC0020: ISharedComponentType RenderMesh can not be received by ref. Use by value or in.
            Entities.ForEach((in RenderMesh renderMesh, in IsInHitComponent isInHit) =>
                {
                    var color = isInHit.value ? Color.red : Color.white;

                    renderMesh.material.SetColor("_Color", color);
                }
            ).WithoutBurst().Run();
        }
    }
}