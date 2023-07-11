namespace HexaEngine.Rendering.Graph
{
    public class Ref<T>
    {
        private T? value;

        public T? Value
        {
            get => value;
            set
            {
                this.value = value;
                Changed?.Invoke(value);
            }
        }

        public event Action<T?>? Changed;
    }
}