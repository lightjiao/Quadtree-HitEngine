using Entitas;
using Entitas.CodeGeneration.Attributes;
using System.Collections.Generic;

[Game]
public class AABB : IComponent
{
    public AsixAligendBoundingBox box;
}

/// <summary>
/// 保存碰撞对象的四叉树
/// </summary>
[Game, Unique]
public class QuadtreeComponent : IComponent
{
    public Quadtree root;
}

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

public class Quadtree
{
    public static Dictionary<GameEntity, Quadtree> NodeLookup = new Dictionary<GameEntity, Quadtree>();

    /// 空间划分时的稀疏范围
    private const float m_LooseSpacing = 10f;

    private const int MaxDepth = 3;

    // 树节点的空间
    public AsixAligendBoundingBox BoundBox { get; private set; }

    // 这个节点包含的可碰撞entity
    public List<GameEntity> hitableEntities;

    public Quadtree Parent;
    public Quadtree LeftTop;
    public Quadtree RightTop;
    public Quadtree LeftBottom;
    public Quadtree RightBottom;

    public Quadtree(AsixAligendBoundingBox boundBox, Quadtree parent = null)
    {
        BoundBox = boundBox;
        Parent = parent;
        hitableEntities = new List<GameEntity>();

        if (parent == null)
        {
            CreateChildRecursive(MaxDepth);
        }
    }

    private void CreateChildRecursive(int maxDepth)
    {
        if (maxDepth == 0) return;

        var leftTopBox = new AsixAligendBoundingBox
        {
            Left = BoundBox.Left,
            Right = BoundBox.MiddleLength + m_LooseSpacing,
            Top = BoundBox.Top,
            Bottom = BoundBox.MiddleHeight - m_LooseSpacing
        };
        LeftTop = new Quadtree(leftTopBox, this);
        LeftTop.CreateChildRecursive(maxDepth - 1);

        var rightTopBox = new AsixAligendBoundingBox
        {
            Left = BoundBox.MiddleLength - m_LooseSpacing,
            Right = BoundBox.Right,
            Top = BoundBox.Top,
            Bottom = BoundBox.MiddleHeight - m_LooseSpacing
        };
        RightTop = new Quadtree(rightTopBox, this);
        RightTop.CreateChildRecursive(maxDepth - 1);

        var leftBottomBox = new AsixAligendBoundingBox
        {
            Left = BoundBox.Left,
            Right = BoundBox.MiddleLength + m_LooseSpacing,
            Top = BoundBox.MiddleHeight + m_LooseSpacing,
            Bottom = BoundBox.Bottom
        };
        LeftBottom = new Quadtree(leftBottomBox, this);
        LeftBottom.CreateChildRecursive(maxDepth - 1);

        var rightBottomBox = new AsixAligendBoundingBox
        {
            Left = BoundBox.MiddleLength - m_LooseSpacing,
            Right = BoundBox.Right,
            Top = BoundBox.MiddleHeight + m_LooseSpacing,
            Bottom = BoundBox.Bottom
        };
        RightBottom = new Quadtree(rightBottomBox, this);
        RightBottom.CreateChildRecursive(maxDepth - 1);
    }
}