using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

/// <summary>
/// Axis-aligned Bouding Box(轴对齐矩形框)
/// 用于空间划分，或者包裹一个碰撞体
/// </summary>
[Game]
public class AxisAlignedBoundingBoxComponent : IComponent
{
    public float Left;
    public float Right;
    public float Top;
    public float Bottom;

    public float MiddleHeight => (Top + Bottom) / 2;
    public float MiddleLength => (Left + Right) / 2;
}

/// <summary>
/// 保存碰撞对象的四叉树
/// </summary>
[Game, Unique]
public class QuadtreeComponent : IComponent
{

}