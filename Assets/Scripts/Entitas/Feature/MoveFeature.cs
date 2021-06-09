namespace HitEngine.Entitas
{
    internal class MoveFeature : Feature
    {
        public MoveFeature(Contexts contexts) : base("Move")
        {
            Add(new RandMovementSystem(contexts));
        }
    }
}