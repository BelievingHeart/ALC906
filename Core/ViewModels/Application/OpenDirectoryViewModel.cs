using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Core.Constants;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Application
{
    public class OpenDirectoryViewModel : ViewModelBase
    {
        #region props

        public ICommand OpenConfigDir2DCommand { get; set; }
        public ICommand OpenLogDirCommand { get; set; }
        public ICommand OpenImageDirCommand { get; set; }

        #endregion

        #region ctor

        public OpenDirectoryViewModel()
        {
            OpenImageDirCommand = new RelayCommand(()=>OpenDir(DirectoryConstants.ImageBaseDir));
            OpenConfigDir2DCommand = new RelayCommand(()=>OpenDir(DirectoryConstants.ConfigDir2D));
            OpenLogDirCommand = new RelayCommand(()=>OpenDir(DirectoryConstants.ErrorLogDir));
        }

        #endregion

        #region impl

        private void OpenDir(string dir)
        {
            Directory.CreateDirectory(dir);
            Process.Start(dir);
        }

        #endregion
    }
}