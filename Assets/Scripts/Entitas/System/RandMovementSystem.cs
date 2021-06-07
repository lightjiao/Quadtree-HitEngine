using Entitas;
using UnityEngine;

internal class RandMovementSystem : IExecuteSystem
{
    private readonly GameContext _context;

    public RandMovementSystem(Contexts contexts)
    {
        _context = contexts.game;
    }

    public void Execute()
    {
        var background = _context.background;
        var group = _context.GetGroup(GameMatcher.AllOf(GameMatcher.RandMover, GameMatcher.Position));

        foreach (var e in group.AsEnumerable())
        {
            var newPos = e.position.value + e.randMover.speed * Time.deltaTime;
            if (newPos.x < background.left || newPos.x > background.right)
            {
                e.randMover.speed.x = -e.randMover.speed.x;
                newPos.x = Mathf.Clamp(newPos.x, background.left, background.right);
            }

            if (newPos.y > background.top || newPos.y < background.bottom)
            {
                e.randMover.speed.y = -e.randMover.speed.y;
                newPos.y = Mathf.Clamp(newPos.y, background.bottom, background.top);
            }

            e.ReplacePosition(newPos);

            // TODO: 处理圆形或者什么其他的边界
        }
    }
}