namespace HexaEngine.UI
{
    using System.Numerics;

    public static class VisualTreeHelper
    {
        public static T FindRoot<T>(DependencyObject element) where T : Visual
        {
            return element.ResolveObject<T>() ?? throw new Exception();
        }

        public static IEnumerable<T?> FindPath<T>(DependencyObject child, DependencyObject? parent) where T : Visual
        {
            DependencyObject? current = child.Parent;
            while (current != null)
            {
                if (current == parent)
                {
                    yield break;
                }

                yield return current as T;

                current = current.Parent;
            }
        }

        public static void FindPath<T>(IList<T?> path, DependencyObject child, DependencyObject? parent) where T : Visual
        {
            DependencyObject? current = child.Parent;
            while (current != null)
            {
                if (current == parent)
                {
                    return;
                }

                path.Add(current as T);

                current = current.Parent;
            }
        }

        public static DependencyObject? GetParent(DependencyObject element)
        {
            if (element is Visual visual)
            {
                return visual.VisualParent;
            }
            return null;
        }

        public static Vector2 GetOffset(Visual visual)
        {
            return visual.VisualOffset;
        }

        public static HitTestResult HitTest(Visual visual, Vector2 point)
        {
            HitTestResult result = default;

            // no possible hit in tree, early exit.
            if (!visual.BoundingBox.Contains(point))
            {
                return result;
            }

            Visual target = visual;
            while (true)
            {
                bool exit = true;
                for (int i = 0; i < target.VisualChildren.Count; i++)
                {
                    var child = target.VisualChildren[i];
                    if (child.BoundingBox.Contains(point))
                    {
                        target = child;
                        exit = false;
                        break;
                    }
                }

                // breaks when no hit was found.
                if (exit)
                {
                    break;
                }
            }

            result.VisualHit = target;
            return result;
        }

        public static void HitTest(Visual visual, HitTestFilter? filter, HitTestHitCallback callback, Vector2 point)
        {
            // no possible hit in tree, early exit.
            if (!visual.BoundingBox.Contains(point))
            {
                return;
            }

            HitTestFilterBehavior behavior = callback(new(visual));

            if (behavior != HitTestFilterBehavior.Continue)
            {
                return;
            }

            HitTestResult result = default;
            Visual target = visual;
            while (true)
            {
                bool exit = true;
                for (int i = 0; i < target.VisualChildren.Count; i++)
                {
                    var child = target.VisualChildren[i];

                    var filterBehavior = filter?.Invoke(child) ?? HitTestFilterBehavior.Continue;

                    if (filterBehavior == HitTestFilterBehavior.Skip)
                    {
                        continue;
                    }

                    if (filterBehavior == HitTestFilterBehavior.Break)
                    {
                        break;
                    }

                    if (filterBehavior == HitTestFilterBehavior.SkipSelf)
                    {
                        target = child;
                        exit = false;
                        break;
                    }

                    if (child.BoundingBox.Contains(point))
                    {
                        result.VisualHit = child;
                        behavior = callback(result);

                        if (filterBehavior == HitTestFilterBehavior.SkipChildren || behavior == HitTestFilterBehavior.SkipChildren)
                        {
                            exit = true;
                            break;
                        }

                        if (behavior == HitTestFilterBehavior.Skip)
                        {
                            continue;
                        }

                        if (behavior == HitTestFilterBehavior.Break)
                        {
                            exit = true;
                            break;
                        }

                        target = child;
                        exit = false;
                        break;
                    }
                }

                // breaks when no hit was found.
                if (exit)
                {
                    break;
                }
            }
        }
    }

    public delegate HitTestFilterBehavior HitTestFilter(DependencyObject dependencyElement);

    public delegate HitTestFilterBehavior HitTestHitCallback(HitTestResult result);

    public enum HitTestFilterBehavior
    {
        Continue,
        Skip,
        SkipChildren,
        SkipSelf,
        Break,
    }

    public struct HitTestResult
    {
        public Visual VisualHit;

        public HitTestResult(Visual visualHit)
        {
            VisualHit = visualHit;
        }
    }
}