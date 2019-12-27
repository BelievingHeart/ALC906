using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.Views.Summary
{
    public partial class BinItemView : UserControl
    {
        public BinItemView()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(BinItemView), new PropertyMetadata(default(string)));

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
            "HeaderBackground", typeof(Brush), typeof(BinItemView), new PropertyMetadata(default(Brush)));

        public Brush HeaderBackground
        {
            get { return (Brush) GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register(
            "ContentText", typeof(string), typeof(BinItemView), new PropertyMetadata(default(string)));

        public string ContentText
        {
            get { return (string) GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }
    }
}