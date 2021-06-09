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
public struct AABB : IComponent
{
    public AsixAligendBoundingBox box;
}

/// <summary>
/// 四叉树可以用一维数组表示，Entity包含这个Component后
/// 表示Entity在四叉树的哪一个节点中
/// </summary>
[Game]
public struct InQuadtreeIdxComponent : IComponent
{
    public int value;
}

[Game]
public struct QuadtreeRootTag : IComponent
{
}

[Game]
public struct QuadtreeNodeComponent : IComponent
{
    public int index;
    public AsixAligendBoundingBox box;
}

[Game]
public struct QuadtreeChildContainerComponent : IComponent
{
    public GameEntity leftTop;
    public GameEntity rightTop;
    public GameEntity leftBottom;
    public GameEntity rightBottom;
}