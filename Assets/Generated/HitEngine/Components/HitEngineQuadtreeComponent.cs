//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class HitEngineContext {

    public HitEngineEntity quadtreeEntity { get { return GetGroup(HitEngineMatcher.Quadtree).GetSingleEntity(); } }

    public bool isQuadtree {
        get { return quadtreeEntity != null; }
        set {
            var entity = quadtreeEntity;
            if (value != (entity != null)) {
                if (value) {
                    CreateEntity().isQuadtree = true;
                } else {
                    entity.Destroy();
                }
            }
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class HitEngineEntity {

    static readonly QuadtreeComponent quadtreeComponent = new QuadtreeComponent();

    public bool isQuadtree {
        get { return HasComponent(HitEngineComponentsLookup.Quadtree); }
        set {
            if (value != isQuadtree) {
                var index = HitEngineComponentsLookup.Quadtree;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : quadtreeComponent;

                    AddComponent(index, component);
                } else {
                    RemoveComponent(index);
                }
            }
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class HitEngineMatcher {

    static Entitas.IMatcher<HitEngineEntity> _matcherQuadtree;

    public static Entitas.IMatcher<HitEngineEntity> Quadtree {
        get {
            if (_matcherQuadtree == null) {
                var matcher = (Entitas.Matcher<HitEngineEntity>)Entitas.Matcher<HitEngineEntity>.AllOf(HitEngineComponentsLookup.Quadtree);
                matcher.componentNames = HitEngineComponentsLookup.componentNames;
                _matcherQuadtree = matcher;
            }

            return _matcherQuadtree;
        }
    }
}
