using Entitas;
using UnityEngine;

[Game]
public class InHitComponent : IComponent
{
}

/// <summary>
/// 圆形碰撞框
/// </summary>
[Game]
public class CircleHitableComponent : IComponent
{
    // 圆心
    //public Vector2 point;

    // 半径
    public float radius;
}

/// <summary>
/// 矩形碰撞框
/// </summary>
[Game]
public class RectHitableComponent : IComponent
{
    //public Vector2 position;
    public float height;

    public float length;

    //public float left => position.x - length / 2;
    //public float right => position.x + length / 2;
    //public float top => position.y + height / 2;
    //public float bottom => position.y - height / 2;
}

/// <summary>
/// 胶囊体碰撞框
/// </summary>
[Game]
public class CapuleHitableComponent : IComponent
{
    // 胶囊体圆心
    //public Vector2 point;

    // 胶囊体半径
    public float radius;

    // 胶囊体向量
    // 胶囊体的半径和向量长度最好满足 1 : 2，不然画出来的mesh不好看
    public Vector2 vec;
}