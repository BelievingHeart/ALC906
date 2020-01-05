using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.ViewModels.Fai.FaiYieldCollection;

namespace UI.Views.FaiItem.FaiYieldCollection
{
    public partial class FaiYieldItemView : UserControl
    {
        public static readonly DependencyProperty ProgressBarHeightProperty =
            DependencyProperty.Register("ProgressBarHeight", typeof(double), typeof(FaiYieldItemView),
                new PropertyMetadata(default(double), OnProgressBarHeightChanged));

        private static void OnProgressBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (FaiYieldItemView) d;
            view.PART_ValueIndicator.Height = (double) e.NewValue;
        }

        public FaiYieldItemView()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (FaiYieldItemViewModel) DataContext;
            if (viewModel == null) return;

            var percent = viewModel.Percent;
            PART_ValueIndicator.Background = new SolidColorBrush(percent < LethalValue ? Colors.Red :
                percent < WarningValue ? Colors.Gold : Colors.LawnGreen);
            PART_ValueIndicator.Height = percent;
        }


        public double ProgressBarHeight
        {
            get { return (double) GetValue(ProgressBarHeightProperty); }
            set { SetValue(ProgressBarHeightProperty, value); }
        }


        public static readonly DependencyProperty WarningValueProperty = DependencyProperty.Register(
            "WarningValue", typeof(int), typeof(FaiYieldItemView), new PropertyMetadata(default(int)));

        public int WarningValue
        {
            get { return (int) GetValue(WarningValueProperty); }
            set { SetValue(WarningValueProperty, value); }
        }

        public static readonly DependencyProperty LethalValueProperty = DependencyProperty.Register(
            "LethalValue", typeof(int), typeof(FaiYieldItemView), new PropertyMetadata(default(int)));

        public int LethalValue
        {
            get { return (int) GetValue(LethalValueProperty); }
            set { SetValue(LethalValueProperty, value); }
        }
    }
}