namespace HexaEngine.Core.Scenes
{
    using System.Runtime.CompilerServices;

    public struct SceneUpdateCallbacks : ISceneUpdateCallbacks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FixedUpdate(GameObject root)
        {
            for (int i = 0; i < root.Children.Count; i++)
            {
                root.Children[i].FixedUpdate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(GameObject root)
        {
            for (int i = 0; i < root.Children.Count; i++)
            {
                root.Children[i].Update();
            }
        }
    }
}