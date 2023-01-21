namespace HexaEngine.Core.Scenes
{
    using System.Collections;
    using System.Collections.Generic;

    public partial class GameObject
    {
        public class GameObjectSelection : IEnumerable
        {
            private readonly List<GameObject> _objects = new();

            public GameObject this[int index] { get => ((IList<GameObject>)_objects)[index]; set => ((IList<GameObject>)_objects)[index] = value; }

            public int Count => ((ICollection<GameObject>)_objects).Count;

            public bool IsReadOnly => ((ICollection<GameObject>)_objects).IsReadOnly;

            public void PurgeSelection()
            {
                foreach (GameObject gameObject in _objects)
                {
                    gameObject.Parent?.RemoveChild(gameObject);
                }
                ClearSelection();
            }

            public void MoveSelection(GameObject parent)
            {
                foreach (GameObject gameObject in _objects)
                {
                    gameObject.Uninitialize();
                }
                foreach (GameObject gameObject in _objects)
                {
                    parent.AddChild(gameObject);
                }
            }

            public void AddSelection(GameObject gameObject)
            {
                gameObject.isEditorSelected = true;
                _objects.Add(gameObject);
            }

            public void AddOverwriteSelection(GameObject gameObject)
            {
                ClearSelection();
                gameObject.isEditorSelected = true;
                _objects.Add(gameObject);
            }

            public void AddMultipleSelection(IEnumerable<GameObject> gameObjects)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    AddSelection(gameObject);
                }
            }

            public void RemoveSelection(GameObject item)
            {
                item.isEditorSelected = false;
                _objects.Remove(item);
            }

            public void RemoveMultipleSelection(IEnumerable<GameObject> gameObjects)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    RemoveSelection(gameObject);
                }
            }

            public void ClearSelection()
            {
                foreach (GameObject gameObject in _objects)
                {
                    gameObject.isEditorSelected = false;
                }
                ((ICollection<GameObject>)_objects).Clear();
            }

            public GameObject? First() => _objects.Count == 0 ? null : _objects[0];

            public GameObject Last() => _objects[^1];

            public bool Contains(GameObject item)
            {
                return ((ICollection<GameObject>)_objects).Contains(item);
            }

            public void CopyTo(GameObject[] array, int arrayIndex)
            {
                ((ICollection<GameObject>)_objects).CopyTo(array, arrayIndex);
            }

            public IEnumerator<GameObject> GetEnumerator()
            {
                return ((IEnumerable<GameObject>)_objects).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_objects).GetEnumerator();
            }
        }
    }
}