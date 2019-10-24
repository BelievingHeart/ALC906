using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HKVisions.Enums;
using MvCamCtrl.NET;

namespace HKVisions
{
    /// <summary>
    /// Manages HK cameras attached
    /// </summary>
    public static class HKCameraManager
    {
        
        
        /// <summary>
        /// Gets all the attached cameras on this machine
        /// </summary>
        public static List<HKCamera> AttachedCameras
        {
            get
            {

                var outputs = new List<HKCamera>();
                if (NumCameras == 0) return outputs;
                
                // If there is any camera attached ...
                for (int i = 0; i < NumCameras; i++)
                {
                    var cameraInfo = CameraInfos[i];
                    // Determine camera type and camera name as well as ip address
                    CameraType cameraType;
                    string ip = string.Empty, cameraName = string.Empty;
                    // If it is a gige camera...
                    if (cameraInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        cameraType = CameraType.Gige;
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(cameraInfo.SpecialInfo.stGigEInfo, 0);
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo =
                            (MyCamera.MV_GIGE_DEVICE_INFO) Marshal.PtrToStructure(buffer,
                                typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        // ch:显示IP | en:Display IP
                        UInt32 nIp1 = (gigeInfo.nCurrentIp & 0xFF000000) >> 24;
                        UInt32 nIp2 = (gigeInfo.nCurrentIp & 0x00FF0000) >> 16;
                        UInt32 nIp3 = (gigeInfo.nCurrentIp & 0x0000FF00) >> 8;
                        UInt32 nIp4 = (gigeInfo.nCurrentIp & 0x000000FF);
                        ip = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;

                        if (gigeInfo.chUserDefinedName != "")
                        {
                            cameraName = gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")";
                        }
                        else
                        {
                            cameraName = gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" +
                                         gigeInfo.chSerialNumber + ")";
                        }
                    }
                    // If it is a use camera ...
                    else if (cameraInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        cameraType = CameraType.USB;
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(cameraInfo.SpecialInfo.stUsb3VInfo, 0);
                        MyCamera.MV_USB3_DEVICE_INFO usbInfo =
                            (MyCamera.MV_USB3_DEVICE_INFO) Marshal.PtrToStructure(buffer,
                                typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        if (usbInfo.chUserDefinedName != "")
                        {
                            cameraName = usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")";
                        }
                        else
                        {
                            cameraName = usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" +
                                         usbInfo.chSerialNumber;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Can not support this type of camera currently");
                    }
                    
                    outputs.Add(new HKCamera()
                    {
                        IpAddress = ip,
                        Name = cameraName,
                        CameraType = cameraType,
                        CameraInfo = CameraInfos[i]
                    });
                }

                return outputs;
            }
        }


        /// <summary>
        /// Try to get information of all the attached cameras
        /// and store it as internal data
        /// This should be called when a camera is attached to or removed from a machine
        /// </summary>
        /// <returns>Indicates if init successes</returns>
        public static bool Init()
        {
            // Try to get all the information of the attached cameras
            MyCamera.MV_CC_DEVICE_INFO_LIST cameraList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            var nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE,
                ref cameraList);
            var success = nRet == 0;
            if (!success) return false;
            
            // Store camera information as internal data
            foreach (var infoPtr in cameraList.pDeviceInfo)
            {
                MyCamera.MV_CC_DEVICE_INFO info =
                    (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(infoPtr,
                        typeof(MyCamera.MV_CC_DEVICE_INFO));
                CameraInfos.Add(info);
            }

            return true;
        }


        public static int NumCameras
        {
            get { return CameraInfos.Count; }
        }

        internal static List<MyCamera.MV_CC_DEVICE_INFO> CameraInfos;
    }
}




 