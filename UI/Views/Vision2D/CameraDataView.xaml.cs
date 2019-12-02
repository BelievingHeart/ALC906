using System.Windows;
using System.Windows.Controls;
using Core.Enums;
using Core.Helpers;
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
    }
}