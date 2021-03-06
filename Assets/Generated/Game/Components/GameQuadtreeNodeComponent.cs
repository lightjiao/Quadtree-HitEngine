//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class GameEntity {

    public HitEngine.Entitas.QuadtreeNodeComponent quadtreeNode { get { return (HitEngine.Entitas.QuadtreeNodeComponent)GetComponent(GameComponentsLookup.QuadtreeNode); } }
    public bool hasQuadtreeNode { get { return HasComponent(GameComponentsLookup.QuadtreeNode); } }

    public void AddQuadtreeNode(int newIndex, HitEngine.Entitas.AsixAligendBoundingBox newBox) {
        var index = GameComponentsLookup.QuadtreeNode;
        var component = (HitEngine.Entitas.QuadtreeNodeComponent)CreateComponent(index, typeof(HitEngine.Entitas.QuadtreeNodeComponent));
        component.index = newIndex;
        component.box = newBox;
        AddComponent(index, component);
    }

    public void ReplaceQuadtreeNode(int newIndex, HitEngine.Entitas.AsixAligendBoundingBox newBox) {
        var index = GameComponentsLookup.QuadtreeNode;
        var component = (HitEngine.Entitas.QuadtreeNodeComponent)CreateComponent(index, typeof(HitEngine.Entitas.QuadtreeNodeComponent));
        component.index = newIndex;
        component.box = newBox;
        ReplaceComponent(index, component);
    }

    public void RemoveQuadtreeNode() {
        RemoveComponent(GameComponentsLookup.QuadtreeNode);
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
public sealed partial class GameMatcher {

    static Entitas.IMatcher<GameEntity> _matcherQuadtreeNode;

    public static Entitas.IMatcher<GameEntity> QuadtreeNode {
        get {
            if (_matcherQuadtreeNode == null) {
                var matcher = (Entitas.Matcher<GameEntity>)Entitas.Matcher<GameEntity>.AllOf(GameComponentsLookup.QuadtreeNode);
                matcher.componentNames = GameComponentsLookup.componentNames;
                _matcherQuadtreeNode = matcher;
            }

            return _matcherQuadtreeNode;
        }
    }
}
