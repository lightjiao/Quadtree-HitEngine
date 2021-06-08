using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace HitEngine.Entities
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }

    public struct MoverBySpeedComponent : IComponentData
    {
        public float2 speed;
    }

    public struct BackgroundComponent : IComponentData
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }
}