using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace HitEngine.Entities
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(QuadtreeCheckHitSystem))]
    internal class MoveInBackgroundSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var backgroundEntity = GetEntityQuery(ComponentType.ReadOnly<BackgroundComponent>()).GetSingletonEntity();
            var backgroundData = GetComponent<BackgroundComponent>(backgroundEntity);

            var deltaTime = Time.DeltaTime;
            Entities.ForEach((ref Translation pos, ref MoverBySpeedComponent mover) =>
            {
                var newPos = pos.Value;
                newPos.x += mover.speed.x * deltaTime;
                newPos.y += mover.speed.y * deltaTime;

                if (newPos.x < backgroundData.left || newPos.x > backgroundData.right)
                {
                    mover.speed.x = -mover.speed.x;
                    newPos.x = math.clamp(newPos.x, backgroundData.left, backgroundData.right);
                }

                if (newPos.y > backgroundData.top || newPos.y < backgroundData.bottom)
                {
                    mover.speed.y = -mover.speed.y;
                    newPos.y = math.clamp(newPos.y, backgroundData.bottom, backgroundData.top);
                }

                pos.Value = newPos;
            }).Schedule();
        }
    }
}