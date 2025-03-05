namespace HexaEngine.Scenes.Managers
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;

    public interface ISelectableHitTest : IComponent
    {
        public bool SelectRayTest(Ray ray, ref float depth);
    }

    public class ObjectPickerManager : ISceneSystem
    {
        private readonly ComponentTypeQuery<ISelectableHitTest> pickables = new();

        public string Name { get; } = "Object Picker";

        public SystemFlags Flags { get; } = SystemFlags.Awake;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(pickables);
        }

        public unsafe GameObject? SelectObject(Ray ray)
        {
            float min = float.MaxValue;
            GameObject? selectedObject = null;
            for (int i = 0; i < pickables.Count; i++)
            {
                var pickable = pickables[i];
                float depth = 0;
                if (pickable.SelectRayTest(ray, ref depth))
                {
                    if (min > depth)
                    {
                        min = depth;
                        selectedObject = pickable.GameObject;
                    }
                }
            }

            return selectedObject;
        }
    }
}