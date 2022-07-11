namespace FontEditor.IO
{
    public interface IConverter<Tin, Tout>
    {
        public Tout Convert(Tin t);
    }
}