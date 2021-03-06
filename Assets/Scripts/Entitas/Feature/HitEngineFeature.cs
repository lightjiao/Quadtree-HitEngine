namespace HitEngine.Entitas
{
    public class HitEngineFeature : Feature
    {
        public HitEngineFeature(Contexts contexts) : base("Hit Engine")
        {
            //Add(new DefaultCheckHitSystem(contexts));
            Add(new QuadtreeCheckHitEngine(contexts));
        }
    }
}