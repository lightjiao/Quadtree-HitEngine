using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

public interface IHiableComponent : IComponent
{

}

/// <summary>
/// 矩形碰撞框
/// </summary>
[HitEngine]
public class RectHitableComponent : IHiableComponent
{
    public float Left;
    public float Right;
    public float Top;
    public float Bottom;
}

[HitEngine]
public class RectViewComponent : IHiableComponent
{
    public GameObject Rect;
}

/// <summary>
/// 圆形碰撞框
/// </summary>
[HitEngine]
public class CircleHitableComponent : IHiableComponent
{
    // 圆心
    public Vector2 Point;

    // 半径
    public float Radius;
}

[HitEngine]
public class CircleViewComponent : IHiableComponent
{
    public GameObject Circle;
}

/// <summary>
/// 胶囊体碰撞框
/// </summary>
[HitEngine]
public class CapuleHitableComponent : IHiableComponent
{
    // 胶囊体圆心
    public Vector2 Point;

    // 胶囊体半径
    public float Radius;

    // 胶囊体向量
    public Vector2 Vec;
}

[HitEngine]
public class CapuleViewComponent : IHiableComponent
{
    public GameObject Capsule;
}
