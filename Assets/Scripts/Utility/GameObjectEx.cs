using UnityEngine;

internal static class GameObjectEx
{
    public static T TryAddComponent<T>(this GameObject go) where T : Component
    {
        var com = go.GetComponent<T>();
        if (com != null) return com;

        return go.AddComponent<T>();
    }
}