namespace ShaderTool.MVVM.View
{
    using System.Numerics;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for Vector3Control.xaml
    /// </summary>
    public partial class Vector3Control : UserControl
    {
        public Vector3Control()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty VectorProperty =
    DependencyProperty.Register(
        nameof(Vector),
        typeof(Vector3),
        typeof(Vector3Control),
        new PropertyMetadata(null));

        public Vector3 Vector
        {
            get { return (Vector3)GetValue(VectorProperty); }
            set { SetValue(VectorProperty, value); }
        }
    }
}