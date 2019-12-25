using System;
using System.Collections.Generic;
using System.IO;
using Core.Constants;
using Core.Enums;
using Core.IoC.Loggers;
using HalconDotNet;

namespace Core.Helpers
{
    public static class SerializationHelper
    {
        public static DateTime SerializeImagesWith2D3DMatched(List<HImage> images2d, List<HImage> images3d, bool shouldSerialize2d, bool shouldSerialize3d, CavityType cavityType, bool saveNgImagesOnly, ProductLevel productLevel)
        {
            var currentTime = DateTime.Now;
            var currentTimeText = currentTime.ToString(NameConstants.DateTimeFormat);
            var dirName = cavityType.ToString();
            try
            {
                if ((saveNgImagesOnly && productLevel != ProductLevel.OK) || !saveNgImagesOnly)
                {
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
                }
            }
            catch (HOperatorException e)
            {
                if (e.Message.Contains("#5566")) Logger.LogHighLevelWarningNormal("磁盘已满, 请立即清理磁盘");
                else throw;
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

        /// <summary>
        /// Remove outdated files recursively if outdated
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="expireDays"></param>
        public static void RemoveOutdatedFiles(string dir, int expireDays)
        {
            Directory.CreateDirectory(dir);
            foreach (string path in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
            {
                var timeSpan = (DateTime.Now - new FileInfo(path).LastWriteTime).Days;
                if(timeSpan > expireDays) File.Delete(path);
            }
        }
        
    }
}