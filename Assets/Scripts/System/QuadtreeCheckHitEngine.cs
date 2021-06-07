using Entitas;
using System.Collections.Generic;
using UnityEngine;

internal class QuadtreeCheckHitEngine : ReactiveSystem<GameEntity>, IInitializeSystem
{
    private const float m_LooseSpacing = 5;

    private readonly GameContext _context;

    public QuadtreeCheckHitEngine(Contexts contexts) : base(contexts.game)
    {
        _context = contexts.game;
    }

    void IInitializeSystem.Initialize()
    {
        InitQuadtree();
    }

    protected override void Execute(List<GameEntity> entities)
    {
        // 所有的对象isInHit 设置为false
        var group = _context.GetGroup(
            GameMatcher.AnyOf(GameMatcher.CircleHitable, GameMatcher.RectHitable, GameMatcher.CapuleHitable)
        );
        foreach (var e in group.AsEnumerable())
        {
            e.isInHit = false;
        }

        foreach (var e in entities)
        {
            RefreshAABB(e);
            UpdateEntityInTree(e);
        }

        foreach (var e in entities)
        {
            // 遍历树，检查是否碰撞
            // 要从树的根部开始遍历，因为有一些比较大的对象跨越了多个区域的时候会挂在中间的某个树节点
            var stack = new Stack<int>();
            stack.Push(0);
            while (stack.Count > 0)
            {
                var idx = stack.Pop();
                if (idx < 0 || idx >= _context.quadtreeArray.array.Length) continue;

                var aabb = _context.quadtreeArray.array[idx];
                if (false == IsInAABB(e.aABB.box, aabb)) continue;

                // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                stack.Push(4 * (idx + 1) - 3);
                stack.Push(4 * (idx + 1) - 2);
                stack.Push(4 * (idx + 1) - 1);
                stack.Push(4 * (idx + 1) - 0);

                foreach (var e2 in group.AsEnumerable())
                {
                    if (e2.inQuadtreeIdx.value != idx) continue;
                    if (e == e2) continue;
                    if (CheckHit(e, e2))
                    {
                        e.isInHit = true;
                        e2.isInHit = true;
                    }
                }
            }
        }
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasPosition &&
               entity.hasInQuadtreeIdx &&
               (entity.hasCapuleHitable || entity.hasCircleHitable || entity.hasRectHitable);
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Position);
    }

    /*
     * 其他函数
     */

    /// <summary>
    /// 四叉树可以用二维数组存储，所以使用entity来表示一个四叉树
    /// </summary>
    private void InitQuadtree()
    {
        // 根节点下标为0
        // 记每个节点的下表为 i
        //  - 它的子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
        //  - 它的父节点下标为 (i-1)/4 并且向下取整(0没有父节点)
        // 
        // 计算总的节点数与层数的关系
        // 1 --> 4^0
        // 2 --> 4^1
        // 3 --> 4^2
        // 遍历下标, 算出child，创建四个点，每个点按照parent划分四个区域

        // 根据层级计算出总的节点数量
        const int depth = 3;
        var nodeSum = 0;
        for (var i = 0; i <= depth; i++)
        {
            nodeSum += (int) Mathf.Pow(4, i);
        }

        // 用数组表示四叉树
        var quadtreeAABBArray = new AsixAligendBoundingBox[nodeSum];
        for (var i = 0; i < nodeSum; i++)
        {
            var aabb = new AsixAligendBoundingBox();

            if (i == 0)
            {
                var background = _context.background;
                aabb.Left = background.left - m_LooseSpacing;
                aabb.Right = background.right + m_LooseSpacing;
                aabb.Top = background.top + m_LooseSpacing;
                aabb.Bottom = background.bottom - m_LooseSpacing;
            }
            else
            {
                var parentAABB = quadtreeAABBArray[(i - 1) / 4];
                var curNodeIdx = i % 4;
                // 1、2、3、4 分别代表 左上、右上、左下、右下
                switch (curNodeIdx)
                {
                    case 1:
                        aabb.Left = parentAABB.Left;
                        aabb.Right = parentAABB.MiddleLength + m_LooseSpacing;
                        aabb.Top = parentAABB.Top + m_LooseSpacing;
                        aabb.Bottom = parentAABB.MiddleHeight - m_LooseSpacing;
                        break;
                    case 2:
                        aabb.Left = parentAABB.MiddleLength - m_LooseSpacing;
                        aabb.Right = parentAABB.Right;
                        aabb.Top = parentAABB.Top;
                        aabb.Bottom = parentAABB.MiddleHeight - m_LooseSpacing;
                        break;
                    case 3:
                        aabb.Left = parentAABB.Left;
                        aabb.Right = parentAABB.MiddleLength + m_LooseSpacing;
                        aabb.Top = parentAABB.MiddleHeight + m_LooseSpacing;
                        aabb.Bottom = parentAABB.Bottom;
                        break;
                    case 4:
                        aabb.Left = parentAABB.MiddleLength - m_LooseSpacing;
                        aabb.Right = parentAABB.Right;
                        aabb.Top = parentAABB.MiddleHeight + m_LooseSpacing;
                        aabb.Bottom = parentAABB.Bottom;
                        break;
                }
            }

            quadtreeAABBArray[i] = aabb;
        }

        _context.SetQuadtreeArray(quadtreeAABBArray);
    }

    private void RefreshAABB(GameEntity entity)
    {
        var pos = entity.position.value;

        var aabb = new AsixAligendBoundingBox();
        if (entity.hasCircleHitable)
        {
            var radius = entity.circleHitable.radius;
            aabb.Left = pos.x - radius;
            aabb.Right = pos.x + radius;
            aabb.Top = pos.y + radius;
            aabb.Bottom = pos.y - radius;
        }

        // Replaces an existing component at the specified index
        // or adds it if it doesn't exist yet.
        entity.ReplaceAABB(aabb);
    }

    private void UpdateEntityInTree(GameEntity e)
    {
        // 跨越了节点的情况是少数，所以先判断是否还在原来的节点空间中
        if (e.inQuadtreeIdx.value >= 0)
        {
            if (UpdateEntityInTree(e.inQuadtreeIdx.value, e))
            {
                return;
            }
        }

        UpdateEntityInTree(0, e);
    }

    private bool UpdateEntityInTree(int treeIdx, GameEntity entity)
    {
        var treeArray = _context.quadtreeArray.array;

        if (treeIdx < 0 || treeIdx >= treeArray.Length)
        {
            return false;
        }

        // 判断是否在当前节点（所有物体一定都在根节点）
        var aabb = treeArray[treeIdx];
        if (treeIdx != 0 && IsInAABB(entity.aABB.box, aabb))
        {
            return false;
        }

        // 检查并更新到子节点
        // 节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
        var isInChild = false;
        for (var i = 0; i < 4; i++)
        {
            isInChild = isInChild || UpdateEntityInTree(4 * (treeIdx + 1) - i, entity);
        }

        if (isInChild) return true;

        entity.inQuadtreeIdx.value = treeIdx;

        return false;
    }

    private bool CheckHit(GameEntity a, GameEntity b)
    {
        if (a.hasCircleHitable)
        {
            if (b.hasCircleHitable)
            {
                return UtilityCheckHit.CheckCirclesAndCircles(a, b);
            }

            // TODO: more
        }

        return false;
    }

    private bool IsInAABB(AsixAligendBoundingBox a, AsixAligendBoundingBox b)
    {
        if (a.Left >= b.Left && a.Right <= b.Right && a.Top <= b.Top && a.Bottom >= b.Bottom)
        {
            return true;
        }

        return false;
    }
}