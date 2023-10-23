namespace HexaEngine.Editor
{
    using HexaEngine.Scenes.Serialization;
    using System.Text;

    public abstract class FileTemplate
    {
        public abstract string Name { get; }

        public abstract string ExtensionTarget { get; }

        public abstract byte[] GetContent();
    }

    public class BinaryFileTemplate : FileTemplate
    {
        private byte[] content;

        public BinaryFileTemplate(string name, string extensionTarget, byte[] content)
        {
            Name = name;
            ExtensionTarget = extensionTarget;
            this.content = content;
        }

        public override string Name { get; }

        public override string ExtensionTarget { get; }

        public byte[] Content
        {
            get => content;
            set => content = value;
        }

        public override byte[] GetContent() => content;
    }

    public class TextFileTemplate : FileTemplate
    {
        private string text;
        private byte[] content;
        private Encoding encoding;

        public TextFileTemplate(string name, string extensionTarget, string text, Encoding encoding)
        {
            Name = name;
            ExtensionTarget = extensionTarget;
            this.text = text;
            this.encoding = encoding;
            content = encoding.GetBytes(text);
        }

        public TextFileTemplate(string name, string extensionTarget, string text)
        {
            Name = name;
            ExtensionTarget = extensionTarget;
            this.text = text;
            encoding = Encoding.UTF8;
            content = encoding.GetBytes(text);
        }

        public override string Name { get; }

        public override string ExtensionTarget { get; }

        public Encoding Encoding
        {
            get => encoding;
            set
            {
                encoding = value;
                content = encoding.GetBytes(text);
            }
        }

        public string Text
        {
            get => text;
            set
            {
                text = value;
                content = encoding.GetBytes(text);
            }
        }

        public override byte[] GetContent() => content;
    }

    public static class FileTemplateManager
    {
        private static readonly List<FileTemplate> templates = new();
        private static readonly FileTemplate EmptyTemplate = new BinaryFileTemplate("Empty", "*", []);

        private static readonly object _lock = new();

        static FileTemplateManager()
        {
            templates.Add(EmptyTemplate);
            templates.Add(new BinaryFileTemplate("Scene", ".hexlvl", SceneSerializer.Serialize(new())));
        }

        public static IReadOnlyList<FileTemplate> Templates => templates;

        public static object SyncObject => _lock;

        public static void AcquireLock()
        {
            Monitor.Enter(_lock);
        }

        public static void ReleaseLock()
        {
            Monitor.Exit(_lock);
        }

        public static FileTemplate FindTemplate(string extension)
        {
            lock (_lock)
            {
                for (int i = 0; i < templates.Count; i++)
                {
                    var template = templates[i];
                    if (template.ExtensionTarget == extension)
                    {
                        return template;
                    }
                }
            }

            return EmptyTemplate;
        }

        public static void WriteFileTemplate(string path)
        {
            var extension = Path.GetExtension(path);
            var template = FindTemplate(extension);
            File.WriteAllBytes(path, template.GetContent());
        }

        public static void Add(FileTemplate template)
        {
            lock (_lock)
            {
                templates.Add(template);
            }
        }

        public static void Remove(FileTemplate template)
        {
            lock (_lock)
            {
                templates.Remove(template);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                templates.Clear();
            }
        }
    }
}