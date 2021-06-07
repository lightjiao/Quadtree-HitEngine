using Entitas;
using Entitas.CodeGeneration.Attributes;
using System.Collections.Generic;

/// <summary>
/// 轴对齐矩形框
/// 用于空间划分，或者包裹一个碰撞体
/// </summary>
public struct AsixAligendBoundingBox
{
    public float Left;
    public float Right;
    public float Top;
    public float Bottom;
    public float MiddleHeight => (Top + Bottom) / 2;
    public float MiddleLength => (Left + Right) / 2;
}

[Game]
public class AABB : IComponent
{
    public AsixAligendBoundingBox box;
}

/// <summary>
/// 四叉树可以用一维数组表示，Entity包含这个Component后
/// 表示Entity在四叉树的哪一个节点中
/// </summary>
[Game]
public class InQuadtreeIdxComponent : IComponent
{
    public int value;
}

/// <summary>
/// singleton，一维数组表示的四叉树的空间
/// </summary>
[Game, Unique]
public class QuadtreeArrayComponent : IComponent
{
    public AsixAligendBoundingBox[] array;
}
