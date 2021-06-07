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
    public int index;
}

// 还是需要一个树来保存四叉树每个节点的大小范围
// --> 碰撞裁剪
// --> 
public class QuadtreeNode : IComponent
{
    public int index; // --> 根据index计算parent与child
    public AsixAligendBoundingBox boundBox; // --> 空间
}

/// <summary>
/// 保存碰撞对象的四叉树
/// </summary>
[Game, Unique]
public class QuadtreeComponent : IComponent
{
    public Quadtree root;
}

public class Quadtree
{
    public static Dictionary<GameEntity, Quadtree> NodeLookup = new Dictionary<GameEntity, Quadtree>();

    /// 空间划分时的稀疏范围
    public const float m_LooseSpacing = 10f;

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