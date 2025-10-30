namespace HexaEngine.UI.Markup
{
    using System;

    public abstract class MarkupExtension
    {
        public abstract object ProvideValue(IServiceProvider serviceProvider);
    }
}