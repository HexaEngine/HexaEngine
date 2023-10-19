namespace HexaEngine.UI
{
    using System.Numerics;

    public interface IUINavigationElement
    {
        public bool TryNavigateTo(IUINavigationElement element, Vector2 direction)
        {
            if (CanNavigate(element, direction))
            {
                NavigateTo(element);
                return true;
            }
            return false;
        }

        public bool CanNavigate(IUINavigationElement element, Vector2 direction);

        public void NavigateTo(IUINavigationElement element);
    }

    public class UINavigationElement : IUINavigationElement
    {
        private readonly List<UINavigationPair> pairs;

        public UINavigationElement()
        {
            pairs = new();
        }

        [JsonConstructor]
        public UINavigationElement(IEnumerable<UINavigationPair> pairs)
        {
            this.pairs = new(pairs);
        }

        public IReadOnlyList<UINavigationPair> Pairs => pairs;

        public void AddPair(IUIElement from, IUIElement to)
        {
            UINavigationPair pair = new(from, to, from.ComputeDirection(to, out float weight), weight);
            if (!pairs.Contains(pair))
            {
                pairs.Add(pair);
            }
        }

        public void RemovePair(IUINavigationElement from, IUINavigationElement to)
        {
            UINavigationPair pair = new(from, to, Vector2.Zero, 0);
            pairs.Remove(pair);
        }

        public void Clear()
        {
            pairs.Clear();
        }

        public IEnumerable<UINavigationPair> GetNavOptions(IUINavigationElement component)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                if (pair.HasComponent(component))
                {
                    yield return pair;
                }
            }
        }

        public bool CanNavigate(IUINavigationElement element, Vector2 direction)
        {
            foreach (var option in GetNavOptions(element))
            {
                if (option.Direction.Y >= 0 && direction.Y >= 0 && option.Direction.X >= 0 && direction.X >= 0)
                {
                    return true;
                }
                if (option.Direction.Y <= 0 && direction.Y <= 0 && option.Direction.X >= 0 && direction.X >= 0)
                {
                    return true;
                }
                if (option.Direction.Y >= 0 && direction.Y >= 0 && option.Direction.X <= 0 && direction.X <= 0)
                {
                    return true;
                }
                if (option.Direction.Y <= 0 && direction.Y <= 0 && option.Direction.X <= 0 && direction.X <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void NavigateTo(IUINavigationElement element)
        {
        }
    }
}