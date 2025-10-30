namespace HexaEngine.UI
{
    using HexaEngine.UI.Markup;
    using System;

    [ContentProperty("Setters")]
    [DictionaryKeyProperty("TargetType")]
    public class Style : IAddChild, INameScope, IQueryAmbient
    {
        public Style()
        {
        }

        public Style(Type targetType)
        {
            TargetType = targetType;
        }

        public Style(Type targetType, Style basedOn)
        {
            TargetType = targetType;
            BasedOn = basedOn;
        }

        [Ambient]
        public Type TargetType { get; set; } = null!;

        [Ambient]
        public Style? BasedOn { get; set; }

        public void RegisterName(string name, object scopedElement)
        {
            throw new NotImplementedException();
        }

        public void UnregisterName(string name)
        {
            throw new NotImplementedException();
        }

        void IAddChild.AddChild(object value)
        {
            throw new NotImplementedException();
        }

        void IAddChild.AddText(string text)
        {
            throw new NotImplementedException();
        }

        object INameScope.FindName(string name)
        {
            throw new NotImplementedException();
        }

        bool IQueryAmbient.IsAmbientPropertyAvailable(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}