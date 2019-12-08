using System;
using System.Collections.Generic;
using System.IO;
using HalconDotNet;

namespace Core.Helpers
{
    public static class SerializationHelper
    {
        public static string SerializeImagesWith2D3DMatched(List<HImage> images2d, List<HImage> images3d, bool shouldSerialize2d, bool shouldSerialize3d, string serializationDir)
        {
            var currentTime = DateTime.Now.ToString("hh-mm-ss-ffff");
            if (shouldSerialize2d)
            {
                var imageDir2d = Path.Combine(serializationDir, "2D");
                SerializeImages(images2d, imageDir2d, currentTime);
            }

            if (shouldSerialize3d)
            {
                var imageDir3d = Path.Combine(serializationDir, "3D");
                SerializeImages(images3d, imageDir3d, currentTime, "tiff");
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