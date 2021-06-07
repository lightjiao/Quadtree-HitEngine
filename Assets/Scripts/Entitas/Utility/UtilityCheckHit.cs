internal static class UtilityCheckHit
{
    public static bool CheckCirclesAndCircles(GameEntity a, GameEntity b)
    {
        var deltaX = a.position.value.x - b.position.value.x;
        var deltaY = a.position.value.y - b.position.value.y;
        var disSqrt = deltaX * deltaX + deltaY * deltaY;

        var radiusSum = a.circleHitable.radius + b.circleHitable.radius;
        return disSqrt < (radiusSum * radiusSum);
    }

    public static bool CheckCapsuleAndCapsule(GameEntity a, GameEntity b)
    {
        // TODO:
        return false;
    }
}