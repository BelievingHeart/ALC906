using Core.Enums;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.CameraHost
{
    public class CameraHostViewModel : ViewModelBase
    {
        public CameraPageType CurrentCameraPage { get; set; } = CameraPageType.TopCamera;
    }
}