using Entitas;
using Entitas.Unity;
using UnityEngine;

internal class RenderBackground : IInitializeSystem
{
    private const float backgroundLength = 20f;
    private const float planeLength = 10f;

    private GameContext _context;

    public RenderBackground(Contexts contexts)
    {
        _context = contexts.game;
    }

    public void Initialize()
    {
        var backgroundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        backgroundGo.name = "Background";
        backgroundGo.transform.position = new Vector3(0, 0, 0.1f);
        backgroundGo.transform.up = Vector3.back;
        backgroundGo.transform.localScale = new Vector3(backgroundLength / planeLength, 1, backgroundLength / planeLength);
        backgroundGo.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);

        _context.SetBackground(backgroundGo,
            -backgroundLength / 2,
            backgroundLength / 2,
             backgroundLength / 2,
            -backgroundLength / 2
        );
        backgroundGo.Link(_context.backgroundEntity);
    }
}