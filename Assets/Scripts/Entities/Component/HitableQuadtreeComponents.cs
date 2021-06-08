using Unity.Entities;

namespace HitEngine.Entities
{
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

    public struct AABB : IComponentData
    {
        public AsixAligendBoundingBox value;
    }

    /// <summary>
    /// 四叉树可以用一维数组表示，Entity包含这个Component后
    /// 表示Entity在四叉树的哪一个节点中
    /// </summary>
    public struct InQuadtreeIdxComponent : IComponentData
    {
        public int value;
    }

    public struct QuadtreeRootTag : IComponentData
    {
    }

    public struct QuadtreeNodeComponent : IComponentData
    {
        public int index;
        public AsixAligendBoundingBox box;
    }

    public struct QuadtreeChildContainerComponent : IComponentData
    {
        public Entity leftTop;
        public Entity rightTop;
        public Entity leftBottom;
        public Entity rightBottom;
    }
}