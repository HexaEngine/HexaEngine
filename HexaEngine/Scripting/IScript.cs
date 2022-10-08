namespace HexaEngine.Scripting
{
    using HexaEngine.Objects;

    public interface IScript : IComponent
    {
        public void Awake()
        {
        }

        public void Destroy()
        {
        }

        public void Update()
        {
        }

        public void FixedUpdate()
        {
        }
    }
}