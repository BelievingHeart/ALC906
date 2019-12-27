using System.Windows;
using System.Windows.Controls;
using Core.Enums;
using Core.Helpers;
using Core.IoC.Loggers;
using Core.ViewModels.Application;

namespace UI.Views.Vision2D
{
    public partial class CameraDataView : UserControl
    {
        public CameraDataView()
        {
            InitializeComponent();
        }

        private bool _isFirstLoaded = true;

        public static readonly DependencyProperty MatrixTypeProperty = DependencyProperty.Register(
            "MatrixType", typeof(StringMatrixType), typeof(CameraDataView), new PropertyMetadata(default(StringMatrixType)));

        public StringMatrixType MatrixType
        {
            get { return (StringMatrixType) GetValue(MatrixTypeProperty); }
            set { SetValue(MatrixTypeProperty, value); }
        }
        
        private void LoadMiscData(object sender, RoutedEventArgs e)
        {
            MatrixType = StringMatrixType.Misc;
            var data = ApplicationViewModel.Instance.LoadData2D(StringMatrixType.Misc);
            
            PART_MatrixView.ClearGrid();
            PART_MatrixView.Header = data.Header;
            PART_MatrixView.StringMatrix = data.Content;
        }

        private void SaveMiscData(object sender, RoutedEventArgs e)
        {
            if (MatrixType != StringMatrixType.Misc) return;
            var stringMatrix = PART_MatrixView.ParseContent();

            ApplicationViewModel.Instance.SaveData2D(stringMatrix, StringMatrixType.Misc);
        }

        private void LoadFindLineData(object sender, RoutedEventArgs e)
        {
            MatrixType = StringMatrixType.FindLine;
            var data = ApplicationViewModel.Instance.LoadData2D(StringMatrixType.FindLine);
            
            PART_MatrixView.ClearGrid();
            PART_MatrixView.Header = data.Header;
            PART_MatrixView.StringMatrix = data.Content;
        }

        private void SaveFindLineData(object sender, RoutedEventArgs e)
        {
            if (MatrixType != StringMatrixType.FindLine) return;
            var stringMatrix = PART_MatrixView.ParseContent();

            ApplicationViewModel.Instance.SaveData2D(stringMatrix, StringMatrixType.FindLine);
        }

        private void LoadResultData(object sender, RoutedEventArgs e)
        {
            MatrixType = StringMatrixType.Results;
            var data = ApplicationViewModel.Instance.LoadData2D(StringMatrixType.Results);
            
            PART_MatrixView.ClearGrid();
            PART_MatrixView.Header = data.Header;
            PART_MatrixView.StringMatrix = data.Content;
        }

        private void SaveResultData(object sender, RoutedEventArgs e)
        {
            if (MatrixType != StringMatrixType.Results) return;
            var stringMatrix = PART_MatrixView.ParseContent();

            ApplicationViewModel.Instance.SaveData2D(stringMatrix, StringMatrixType.Results);
        }

        private void OnCameraDataViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstLoaded)
            {
                LoadResultData(null, null);
            }

            _isFirstLoaded = false;
        }

        #region IsEditable

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
            "IsEditable", typeof(bool), typeof(CameraDataView), new PropertyMetadata(true, OnIsEditableChanged));

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CameraDataView) d;
            var isEnable = (bool) e.NewValue;
            sender.PART_saveButton.IsEnabled = isEnable;
            sender.PART_saveResultConfigButton.IsEnabled = isEnable;
            sender.PART_saveFindLineConfigButton.IsEnabled = isEnable;
        }

        public bool IsEditable
        {
            get { return (bool) GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        #endregion
    }
}