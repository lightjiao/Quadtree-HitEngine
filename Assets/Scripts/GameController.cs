using Entitas;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Systems _systems;

    // Start is called before the first frame update
    private void Start()
    {
        var contexts = Contexts.sharedInstance;

        var initFeature = new Feature("Init");
        initFeature.Add(new RenderBackground(contexts));
        initFeature.Add(new InitSystem(contexts));

        _systems = new Feature("System")
            .Add(initFeature)
            .Add(new ViewFeature(contexts))
            .Add(new HitEngineFeature(contexts));

        _systems.Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        _systems.Execute();
        _systems.Cleanup();
    }
}