using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Enums;
using Core.ImageProcessing;
using Core.ViewModels.Results;
using HalconDotNet;
using UI.Views.Vision2D;
using WPFCommon.Commands;

namespace UI.Views.LineScan
{
    public partial class LineScanView : UserControl
    {
        public LineScanView()
        {
            InitializeComponent();
        }
        
    }
}