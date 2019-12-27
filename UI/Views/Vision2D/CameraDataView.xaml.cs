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
            var header = ApplicationViewModel.Instance.I40Check.AlgHeader;
            var dict = ApplicationViewModel.Instance.I40Check.AlgDictionary;
            
            PART_MatrixView.ClearGrid();
            PART_MatrixView.Header = header;
            PART_MatrixView.StringMatrix = dict.ToStringMatrix();
        }

        private void SaveMiscData(object sender, RoutedEventArgs e)
        {
            var stringMatrix = PART_MatrixView.ParseContent();

            ApplicationViewModel.Instance.I40Check.AlgDictionary = stringMatrix.ToDict();
            ApplicationViewModel.Instance.I40Check.SaveAlgParam();
            Logger.LogStateChanged("保存其他数据成功");
        }

        private void LoadFindLineData(object sender, RoutedEventArgs e)
        {
            var header = ApplicationViewModel.Instance.I40Check.SearchLineHeader;
            var dict = ApplicationViewModel.Instance.I40Check.SearchLineDictionary;
            
            PART_MatrixView.ClearGrid();
            PART_MatrixView.Header = header;
            PART_MatrixView.StringMatrix = dict.ToStringMatrix();
        }

        private void SaveFindLineData(object sender, RoutedEventArgs e)
        {
            var stringMatrix = PART_MatrixView.ParseContent();

            ApplicationViewModel.Instance.I40Check.SearchLineDictionary = stringMatrix.ToDict();
            ApplicationViewModel.Instance.I40Check.SaveSearchLineParam();
            Logger.LogStateChanged("保存找边数据成功");
        }

        private void LoadResultData(object sender, RoutedEventArgs e)
        {
            var header = ApplicationViewModel.Instance.I40Check.ResultHeader;
            var dict = ApplicationViewModel.Instance.I40Check.ResultDictionary1;
            
            PART_MatrixView.ClearGrid();
            PART_MatrixView.Header = header;
            PART_MatrixView.StringMatrix = dict.ToStringMatrix();        }

        private void SaveResultData(object sender, RoutedEventArgs e)
        {
            var stringMatrix = PART_MatrixView.ParseContent();

            ApplicationViewModel.Instance.I40Check.ResultDictionary1 = stringMatrix.ToDict();
            ApplicationViewModel.Instance.I40Check.SaveResultLimitParam();
            ApplicationViewModel.Instance.Update2DMinMax();
            Logger.LogStateChanged("保存结果数据成功");
        }

        private void OnCameraDataViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstLoaded)
            {
                MatrixType = StringMatrixType.Results;
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