# 纯 ECS 实现的四叉树结构的碰撞检测

## 四叉树 Entity 的 Component

四叉树可以用数组表示，所以用多个 Entity 分别表示树的节点，并且标识出在数组中的下标

```
Entity
{
    QuadtreeNodeComponent, // 这个四叉树节点的下标 和 AABB框
    QuadtreeChildContainer, // 存储这个树节点的子节点Entity的引用
}
```

## 碰撞体 Entity 的 Component 结构

```
Entity
{
    InQuadtreeIdx, // 表示这个Entity在四叉树中的哪个节点
    AABB,          // 这个碰撞体的AABB 框，便于更新在四叉树中的位置
}
```