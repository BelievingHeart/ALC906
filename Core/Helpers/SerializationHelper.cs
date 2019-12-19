using System;
using System.Collections.Generic;
using System.IO;
using Core.Constants;
using Core.Enums;
using HalconDotNet;

namespace Core.Helpers
{
    public static class SerializationHelper
    {
        public static DateTime SerializeImagesWith2D3DMatched(List<HImage> images2d, List<HImage> images3d, bool shouldSerialize2d, bool shouldSerialize3d, CavityType cavityType)
        {
            var currentTime = DateTime.Now;
            var currentTimeText = currentTime.ToString("HH-mm-ss-ffff");
            var dirName = cavityType.ToString();
            if (shouldSerialize2d)
            {
                var imageDir2d = Path.Combine(DirectoryConstants.ImageDir2D, dirName);
                SerializeImages(images2d, imageDir2d, currentTimeText);
            }

            if (shouldSerialize3d)
            {
                var imageDir3d = Path.Combine(DirectoryConstants.ImageDir3D, dirName);
                SerializeImages(images3d, imageDir3d, currentTimeText, "tiff");
            }

            return currentTime;
        }

        private static void SerializeImages(IReadOnlyList<HImage> images, string serializationDir, string commonName, string extension = "bmp")
        {
         
            Directory.CreateDirectory(serializationDir);
            for (var i = 0; i < images.Count; i++)
            {
                var imageName = $"{commonName}-{i:D2}.{extension}";
                images[i].WriteImage(extension, 0, Path.Combine(serializationDir, imageName));
            }
        }
        
    }
}