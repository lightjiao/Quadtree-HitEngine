using Entitas;
using System.Collections.Generic;

internal class QuadtreeCheckHitEngint : ReactiveSystem<GameEntity>, IInitializeSystem
{
    private readonly GameContext _context;

    public QuadtreeCheckHitEngint(Contexts contexts) : base(contexts.game)
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
        var group = _context.GetGroup(GameMatcher.AnyOf(GameMatcher.CircleHitable, GameMatcher.RectHitable, GameMatcher.CapuleHitable));
        foreach (var e in group.AsEnumerable())
        {
            e.isInHit = false;
        }

        var quadtreeRoot = _context.quadtree.root;
        foreach (var e in entities)
        {
            RefreshAABB(e);
            UpdateEntityInTree(quadtreeRoot, e);

            // 遍历树，检查是否碰撞
            var node = Quadtree.NodeLookup[e];
            foreach (var e2 in node.hitableEntities)
            {
                if (e == e2) continue;
                if (CheckHit(e, e2))
                {
                    e.isInHit = true;
                    e2.isInHit = true;
                }
            }
        }
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasPosition && (entity.hasCapuleHitable || entity.hasCircleHitable || entity.hasRectHitable);
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Position);
    }

    /*
     * 其他函数
     */

    private void InitQuadtree()
    {
        var background = _context.background;

        var root = new Quadtree(new AsixAligendBoundingBox
        {
            Left = background.left,
            Right = background.right,
            Top = background.top,
            Bottom = background.bottom
        });

        _context.quadtree.root = root;
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

        /// Replaces an existing component at the specified index
        /// or adds it if it doesn't exist yet.
        entity.ReplaceAABB(aabb);
    }

    /// <summary>
    /// 只有位置有变更的entity才会执行到这里
    /// </summary>
    /// <param name="entity"></param>
    private bool UpdateEntityInTree(Quadtree node, GameEntity entity)
    {
        if (node == null) return false;

        if (node == _context.quadtree.root)
        {
            // 所有物体一定会在root中
        }
        else if (false == IsInAABB(entity.aABB.box, node.BoundBox))
        {
            return false;
        }

        // 检查并更新到子节点
        var isInChild = UpdateEntityInTree(node.LeftTop, entity) ||
                        UpdateEntityInTree(node.RightTop, entity) ||
                        UpdateEntityInTree(node.LeftBottom, entity) ||
                        UpdateEntityInTree(node.RightBottom, entity);
        if (isInChild)
        {
            return true;
        }

        if (Quadtree.NodeLookup.TryGetValue(entity, out var oldNode))
        {
            if (oldNode == node)
            {
                return true;
            }
        }

        oldNode?.hitableEntities.Remove(entity);
        node.hitableEntities.Add(entity);
        Quadtree.NodeLookup[entity] = node;

        return true;
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