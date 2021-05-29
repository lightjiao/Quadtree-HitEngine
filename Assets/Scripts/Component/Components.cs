﻿using Entitas;
using UnityEngine;

[Game]
public class NeedViewComponent : IComponent
{
}

[Game]
public class ViewComponent : IComponent
{
    public GameObject go;
}

[Game]
public class PositionComponent : IComponent
{
    public Vector2 value;
}

/**
 * 位置
 * 碰撞对象：圆、胶囊体、矩形
 * 一个碰撞对象由几个Component组成？
 *  Position
 *      --> RenderPosition，将Position同步到GO上
 *  View
 *      --> AddViewSystem 专门用来创建GameObject
 *
 *  HitableComponent(Circle Rect Capsule.etc)
 *      --> RenderHitableSystem : 创建一个Mesh到View上 (HitableViewComponent : 保存Mesh, 用于实现InHit)
 *      --> UpdateHitableSystem : 将Position坐标同步到Hitable上
 *
 *  InHitComponent
 *      --> RenderHitableInHitSystem : 有view 有 hitableComponent
 *
 *  ViewFeature
 *      AddViewSystem
 *      RenderPosition
 *      RenderHitableSystem
 *      RenderHitableInHitSystem
 */