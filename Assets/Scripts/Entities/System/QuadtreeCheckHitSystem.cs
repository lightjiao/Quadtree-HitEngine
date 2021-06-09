using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace HitEngine.Entities
{
    [DisableAutoCreation]
    public class QuadtreeCheckHitSystem : SystemBase
    {
        private const float m_LooseSpacing = 5;
        private BackgroundComponent m_backgroundData;
        private NativeArray<Entity> m_QuadtreeNodes;
        private Entity m_QuadtreeRoot;

        protected override void OnCreate()
        {
            base.OnCreate();

            var backgroundEntity = GetEntityQuery(ComponentType.ReadOnly<BackgroundComponent>()).GetSingletonEntity();
            m_backgroundData = GetComponent<BackgroundComponent>(backgroundEntity);

            InitQuadtree();
            m_QuadtreeRoot = GetEntityQuery(ComponentType.ReadOnly<QuadtreeRootTag>()).GetSingletonEntity();
            m_QuadtreeNodes = GetEntityQuery(ComponentType.ReadOnly<QuadtreeNodeComponent>()).ToEntityArray(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            // Refresh AABB
            Entities.ForEach((ref AABB aabb, in Translation pos, in CircleHitableComponent circle) =>
            {
                aabb.value.Left = pos.Value.x - circle.radius;
                aabb.value.Right = pos.Value.x + circle.radius;
                aabb.value.Top = pos.Value.y + circle.radius;
                aabb.value.Bottom = pos.Value.y - circle.radius;
            }).Schedule();

            // UpdateEntityInTree
            Entities.ForEach((Entity entity, ref InQuadtreeIdxComponent inQuadtreeIdx, in AABB box) =>
            {
                UpdateEntityInQuadtree(m_QuadtreeRoot, entity, ref inQuadtreeIdx, box);
            }).WithoutBurst().Run();

            // CheckHit
            Entities.ForEach((Entity entity, ref IsInHitComponent isinHit, in AABB aabb, in Translation pos, in CircleHitableComponent circle) =>
            {
                isinHit.value = false;

                var hitableEntities = GetEntityQuery(
                    ComponentType.ReadWrite<IsInHitComponent>(),
                    ComponentType.ReadOnly<InQuadtreeIdxComponent>(),
                    ComponentType.ReadOnly<AABB>(),
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<CircleHitableComponent>()
                );

                var stack = new Stack<Entity>();
                stack.Push(m_QuadtreeRoot);
                while (stack.Count > 0)
                {
                    var quadTreeNode = stack.Pop();

                    var index = EntityManager.GetComponentData<QuadtreeNodeComponent>(quadTreeNode).index;
                    var nodeAabb = EntityManager.GetComponentData<QuadtreeNodeComponent>(quadTreeNode).box;
                    if (false == IsInBox(aabb.value, nodeAabb)) continue;

                    if (EntityManager.HasComponent<QuadtreeChildContainerComponent>(quadTreeNode))
                    {
                        var child = EntityManager.GetComponentData<QuadtreeChildContainerComponent>(quadTreeNode);
                        stack.Push(child.leftTop);
                        stack.Push(child.rightTop);
                        stack.Push(child.leftBottom);
                        stack.Push(child.rightBottom);
                    }

                    
                    foreach (var e2 in hitableEntities.ToEntityArray(Allocator.Temp))
                    {
                        var e2QuadTreeIdx = EntityManager.GetComponentData<InQuadtreeIdxComponent>(e2).value;
                        if (e2QuadTreeIdx != index) continue;

                        if (entity == e2) continue;

                        var e2Pos = EntityManager.GetComponentData<Translation>(e2);
                        var e2Circe = EntityManager.GetComponentData<CircleHitableComponent>(e2);

                        var disSqr = math.distancesq(e2Pos.Value, pos.Value);
                        var radiusSumSqr = math.sqrt(circle.radius + e2Circe.radius);
                        if (disSqr < radiusSumSqr)
                        {
                            isinHit.value = true;
                        }
                    }
                }

            }).WithoutBurst().Run();
        }

        private bool UpdateEntityInQuadtree(Entity quadtreeNode, Entity entity, ref InQuadtreeIdxComponent inQuadtreeIdx, in AABB box)
        {
            var nodeIdx = EntityManager.GetComponentData<QuadtreeNodeComponent>(quadtreeNode).index;
            var nodeBox = EntityManager.GetComponentData<QuadtreeNodeComponent>(quadtreeNode).box;
            if (nodeIdx != 0 && !IsInBox(box.value, nodeBox))
            {
                return false;
            }

            if (EntityManager.HasComponent<QuadtreeChildContainerComponent>(quadtreeNode))
            {
                var child = EntityManager.GetComponentData<QuadtreeChildContainerComponent>(quadtreeNode);
                var inChild = UpdateEntityInQuadtree(child.leftTop, entity, ref inQuadtreeIdx, box)
                    || UpdateEntityInQuadtree(child.rightTop, entity, ref inQuadtreeIdx, box)
                    || UpdateEntityInQuadtree(child.leftBottom, entity, ref inQuadtreeIdx, box)
                    || UpdateEntityInQuadtree(child.rightBottom, entity, ref inQuadtreeIdx, box);
                if (inChild)
                {
                    return true;
                }
            }

            inQuadtreeIdx.value = nodeIdx;

            return true;
        }

        private static bool IsInBox(AsixAligendBoundingBox a, AsixAligendBoundingBox b)
        {
            if (a.Left >= b.Left && a.Right <= b.Right && a.Top <= b.Top && a.Bottom >= b.Bottom)
            {
                return true;
            }

            return false;
        }

        private void InitQuadtree()
        {
            // 根据层级计算出总的节点数量
            const int depth = 3;
            var nodeSum = 0;
            for (var i = 0; i <= depth; i++)
            {
                nodeSum += (int)Mathf.Pow(4, i);
            }

            // 用数组表示四叉树
            var entityArray = new Entity[nodeSum];
            var rootBox = new AsixAligendBoundingBox();
            for (var i = 0; i < nodeSum; i++)
            {
                var entity = EntityManager.CreateEntity();
                var aabb = new AsixAligendBoundingBox();

                if (i == 0)
                {
                    EntityManager.AddComponentData(entity, new QuadtreeRootTag());

                    aabb.Left = m_backgroundData.left - m_LooseSpacing;
                    aabb.Right = m_backgroundData.right + m_LooseSpacing;
                    aabb.Top = m_backgroundData.top + m_LooseSpacing;
                    aabb.Bottom = m_backgroundData.bottom - m_LooseSpacing;
                    rootBox = aabb;
                }
                else
                {
                    var parentEntity = entityArray[(i - 1) / 4];
                    var parentAABB = EntityManager.GetComponentData<QuadtreeNodeComponent>(parentEntity).box;
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
                            aabb.Left = Mathf.Max(parentAABB.Left - m_LooseSpacing, rootBox.Left); ;
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

                EntityManager.AddComponentData(entity, new QuadtreeNodeComponent
                {
                    index = i,
                    box = aabb,
                });
                entityArray[i] = entity;
            }

            // 关联子节点信息
            for (var i = 0; i < nodeSum; i++)
            {
                var entity = entityArray[i];

                // 子节点下标为 4(i+1)-3, 4(i+1)-2, 4(i+1)-1, 4(i+1)-0
                // 同时要注意边界
                if (4 * (i + 1) - 3 >= nodeSum) continue;

                EntityManager.AddComponentData(entity, new QuadtreeChildContainerComponent
                {
                    leftTop = entityArray[4 * (i + 1) - 3],
                    rightTop = entityArray[4 * (i + 1) - 2],
                    leftBottom = entityArray[4 * (i + 1) - 1],
                    rightBottom = entityArray[4 * (i + 1) - 0]
                });
            }
        }
    }
}