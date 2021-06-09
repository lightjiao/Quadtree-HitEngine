using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace HitEngine.Entitas
{
    [Game]
    public class NeedViewComponent : IComponent
    {
    }

    [Game]
    public class ViewComponent : IComponent
    {
        public GameObject go;
    }

    [Game]
    public class PositionComponent : IComponent
    {
        public Vector2 value;
    }

    [Game]
    public class RandMoverComponent : IComponent
    {
        public Vector2 speed;
    }

    [Game, Unique]
    public class BackgroundComponent : IComponent
    {
        public GameObject go;
        public float left;
        public float right;
        public float top;
        public float bottom;
    }
}