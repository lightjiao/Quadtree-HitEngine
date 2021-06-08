using Entitas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class QuadtreeCheckHitEngine : ReactiveSystem<GameEntity>, IInitializeSystem
{
    private const float m_LooseSpacing = 2;

    private readonly GameContext _context;

    private IGroup<GameEntity> m_QuadtreeNodes;
    private GameEntity m_QuadtreeRoot;

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
        var hitAbleGroup = _context.GetGroup(
            GameMatcher.AnyOf(GameMatcher.CircleHitable, GameMatcher.RectHitable, GameMatcher.CapuleHitable)
        );
        foreach (var e in hitAbleGroup.AsEnumerable())
        {
            e.isInHit = false;
        }

        m_QuadtreeNodes = _context.GetGroup(GameMatcher.QuadtreeNode);
        m_QuadtreeRoot = _context.GetEntities(GameMatcher.QuadtreeRootTag)[0];

        foreach (var e in entities)
        {
            RefreshAABB(e);
            UpdateEntityInTree(e);
        }

        foreach (var e in entities)
        {
            // 遍历树，检查是否碰撞
            // 要从树的根部开始遍历，因为有一些比较大的对象跨越了多个区域的时候会挂在中间的某个树节点
            var stack = new Stack<GameEntity>();
            stack.Push(m_QuadtreeRoot);
            while (stack.Count > 0)
            {
                var quadTreeNode = stack.Pop();
                var aabb = quadTreeNode.quadtreeNode.box;
                var index = quadTreeNode.quadtreeNode.index;
                if (false == IsInAABB(e.aABB.box, aabb)) continue;
            
                // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                if (quadTreeNode.hasQuadtreeChildContainer)
                {
                    var child = quadTreeNode.quadtreeChildContainer;
                    stack.Push(child.leftTop);
                    stack.Push(child.rightTop);
                    stack.Push(child.leftBottom);
                    stack.Push(child.rightBottom);
                }
            
                foreach (var e2 in hitAbleGroup.AsEnumerable())
                {
                    if (e2.inQuadtreeIdx.value != index) continue;
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
        var entityArray = new GameEntity[nodeSum];
        var rootBox = new AsixAligendBoundingBox();
        for (var i = 0; i < nodeSum; i++)
        {
            var entity = _context.CreateEntity();
            var aabb = new AsixAligendBoundingBox();

            if (i == 0)
            {
                entity.isQuadtreeRootTag = true;

                var background = _context.background;
                aabb.Left = background.left - m_LooseSpacing;
                aabb.Right = background.right + m_LooseSpacing;
                aabb.Top = background.top + m_LooseSpacing;
                aabb.Bottom = background.bottom - m_LooseSpacing;
                rootBox = aabb;
            }
            else
            {
                var parentAABB = entityArray[(i - 1) / 4].quadtreeNode.box;
                var curNodeIdx = i % 4;
                // 1、2、3、0 分别代表 左上、右上、左下、右下
                switch (curNodeIdx)
                {
                    case 1:
                        aabb.Left = Mathf.Max(parentAABB.Left - m_LooseSpacing, rootBox.Left);
                        aabb.Right = parentAABB.MiddleLength + m_LooseSpacing;
                        aabb.Top = Mathf.Min(parentAABB.Top + m_LooseSpacing, rootBox.Top);
                        aabb.Bottom = parentAABB.MiddleHeight - m_LooseSpacing;
                        break;
                    case 2:
                        aabb.Left = parentAABB.MiddleLength - m_LooseSpacing;
                        aabb.Right = Mathf.Min(parentAABB.Right + m_LooseSpacing, rootBox.Right);
                        aabb.Top = Mathf.Min(parentAABB.Top + m_LooseSpacing, rootBox.Top);
                        aabb.Bottom = parentAABB.MiddleHeight - m_LooseSpacing;
                        break;
                    case 3:
                        aabb.Left = Mathf.Max(parentAABB.Left - m_LooseSpacing, rootBox.Left);;
                        aabb.Right = parentAABB.MiddleLength + m_LooseSpacing;
                        aabb.Top = parentAABB.MiddleHeight + m_LooseSpacing;
                        aabb.Bottom = Mathf.Max(parentAABB.Bottom - m_LooseSpacing, rootBox.Bottom);
                        break;
                    case 0:
                        aabb.Left = parentAABB.MiddleLength - m_LooseSpacing;
                        aabb.Right = Mathf.Min(parentAABB.Right + m_LooseSpacing, rootBox.Right);
                        aabb.Top = parentAABB.MiddleHeight + m_LooseSpacing;
                        aabb.Bottom = Mathf.Max(parentAABB.Bottom - m_LooseSpacing, rootBox.Bottom);
                        break;
                }
            }

            entity.AddQuadtreeNode(i, aabb);
            entityArray[i] = entity;
        }

        // 关联子节点信息
        for (var i = 0; i < nodeSum; i++)
        {
            var entity = entityArray[i];

            // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
            // 同时要注意边界
            if (4 * (i + 1) - 3 >= nodeSum) continue;
            
            entity.AddQuadtreeChildContainer(
                entityArray[4 * (i + 1) - 3],
                entityArray[4 * (i + 1) - 2],
                entityArray[4 * (i + 1) - 1],
                entityArray[4 * (i + 1) - 0]
            );
        }
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
        var inQuadTreeIdx = e.inQuadtreeIdx.value;
        if (inQuadTreeIdx >= 0)
        {
            var curNode = m_QuadtreeNodes.AsEnumerable().First(x => x.quadtreeNode.index == inQuadTreeIdx);
            if (UpdateEntityInTree(curNode, e))
            {
                return;
            }
        }

        UpdateEntityInTree(m_QuadtreeRoot, e);
    }

    private bool UpdateEntityInTree(GameEntity quadtreeNode, GameEntity entity)
    {
        if (quadtreeNode == null) return false;

        var nodeIdx = quadtreeNode.quadtreeNode.index;
        var nodeBox = quadtreeNode.quadtreeNode.box;
        if (nodeIdx != 0 && IsInAABB(entity.aABB.box, nodeBox))
        {
            return false;
        }

        // 检查并更新到子节点
        if (quadtreeNode.hasQuadtreeChildContainer)
        {
            var children = quadtreeNode.quadtreeChildContainer;
            var isInChild = UpdateEntityInTree(children.leftTop, entity)
                            || UpdateEntityInTree(children.rightTop, entity)
                            || UpdateEntityInTree(children.leftBottom, entity)
                            || UpdateEntityInTree(children.rightBottom, entity);
            if (isInChild) return true;    
        }

        entity.ReplaceInQuadtreeIdx(nodeIdx);

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