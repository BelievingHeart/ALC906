using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views.Home
{
    public partial class BasicInfoView : UserControl
    {
        public BasicInfoView()
        {
            InitializeComponent();
        }

        #region Yield

        public static readonly DependencyProperty YieldProperty = DependencyProperty.Register(
            "Yield", typeof(double), typeof(BasicInfoView), new PropertyMetadata(default(double), OnYieldChanged));

        private static void OnYieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (BasicInfoView) d;
            var yield = (double) e.NewValue;
            var percentText = $"{100 * yield:N1}%";
            sender.PART_YieldTextBlock.Text = percentText;
        }

        public double Yield
        {
            get { return (double) GetValue(YieldProperty); }
            set { SetValue(YieldProperty, value); }
        }

        public static readonly DependencyProperty UphProperty = DependencyProperty.Register(
            "Uph", typeof(int), typeof(BasicInfoView), new PropertyMetadata(default(int), OnUphChanged));

        private static void OnUphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (BasicInfoView) d;
            var uph = (int) e.NewValue;
            sender.PART_UphTextBlock.Text = uph.ToString(CultureInfo.InvariantCulture);
        }

        public int Uph
        {
            get { return (int) GetValue(UphProperty); }
            set { SetValue(UphProperty, value); }
        }

        #endregion
        
        
    }
}