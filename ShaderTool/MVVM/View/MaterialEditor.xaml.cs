namespace ShaderTool.MVVM.View
{
    using HexaEngine.Rendering;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MaterialEditor.xaml
    /// </summary>
    public partial class MaterialEditor : Window
    {
        public MaterialEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty MaterialProperty =
            DependencyProperty.Register(
                "Material",
                typeof(Material),
                typeof(MaterialEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnMaterialChanged)));

        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }

        private static void OnMaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}