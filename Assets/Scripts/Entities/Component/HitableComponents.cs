using Unity.Entities;

namespace HitEngine.Entities
{
    public struct IsInHitComponent:IComponentData
    {
        public bool value;
    }

    // 圆形碰撞器
    public struct CircleHitableComponent : IComponentData
    {
        // 半径
        public float radius;
    }
}