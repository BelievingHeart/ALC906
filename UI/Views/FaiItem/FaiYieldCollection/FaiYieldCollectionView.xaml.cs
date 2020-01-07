using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Fai.FaiYieldCollection;

namespace UI.Views.FaiItem.FaiYieldCollection
{
    public partial class FaiYieldCollectionView : UserControl
    {
        public FaiYieldCollectionView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModelNew = e.NewValue as FaiYieldCollectionViewModel;
            var viewModelOld =  e.OldValue as FaiYieldCollectionViewModel;

            if (viewModelNew == null) return;
            
            // Unhook events
            if(viewModelOld!=null)
            {
                viewModelOld.YieldsUpdated -= OnYieldsUpdated;
                ClearItemsView();
            }
            
            // Hook events
             viewModelNew.YieldsUpdated += OnYieldsUpdated;
             
             // Init elements
             OnYieldsUpdated();
        }

        private void ClearItemsView()
        {
            PART_ListBox.ItemsSource = null;
        }

        private void OnYieldsUpdated()
        {
            Dispatcher?.InvokeAsync(() =>
            {
                // Create items' view models
                var viewModel = (FaiYieldCollectionViewModel) DataContext;
                var yieldItemViewModels =
                    viewModel.PercentDict.Select(ele => new FaiYieldItemViewModel() {Name = ele.Key, Percent = ele.Value, NgCount = viewModel.NgCountDict[ele.Key]});

                // Set ItemSource
                PART_ListBox.ItemsSource = yieldItemViewModels;
            });
        }


        public static readonly DependencyProperty WarningValueProperty = DependencyProperty.Register(
            "WarningValue", typeof(int), typeof(FaiYieldCollectionView), new PropertyMetadata(default(int)));

        public int WarningValue
        {
            get { return (int) GetValue(WarningValueProperty); }
            set { SetValue(WarningValueProperty, value); }
        }

        public static readonly DependencyProperty LethalValueProperty = DependencyProperty.Register(
            "LethalValue", typeof(int), typeof(FaiYieldCollectionView), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ProgressBarHeightProperty = DependencyProperty.Register("ProgressBarHeight", typeof(double), typeof(FaiYieldCollectionView), new PropertyMetadata(default(double)));

        public int LethalValue
        {
            get { return (int) GetValue(LethalValueProperty); }
            set { SetValue(LethalValueProperty, value); }
        }

        public double ProgressBarHeight
        {
            get { return (double) GetValue(ProgressBarHeightProperty); }
            set { SetValue(ProgressBarHeightProperty, value); }
        }
    }
}