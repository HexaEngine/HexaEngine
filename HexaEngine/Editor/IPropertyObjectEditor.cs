namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IPropertyObjectEditor : IDisposable
    {
        public Type Type { get; }

        bool CanEditMultiple { get; }

        public bool CanEdit(object obj);

        public void Edit(IGraphicsContext context, object obj);

        public void EditMultiple(IGraphicsContext context, ICollection<object> objects);
    }

    public interface IPropertyObjectEditor<T> : IPropertyObjectEditor
    {
        Type IPropertyObjectEditor.Type => typeof(T);

        bool IPropertyObjectEditor.CanEdit(object obj)
        {
            if (obj is T t)
            {
                return CanEdit(t);
            }
            return false;
        }

        void IPropertyObjectEditor.Edit(IGraphicsContext context, object obj)
        {
            if (obj is T t)
            {
                Edit(context, t);
            }
        }

        public bool CanEdit(T obj)
        {
            return true;
        }

        public void Edit(IGraphicsContext context, T obj);
    }
}