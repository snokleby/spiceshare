using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using ImageProcessorCore;
using ImageProcessorCore.Formats;
using Microsoft.AspNetCore.Hosting;
using SpiceShare.Models;

namespace SpiceShare.ImageProcessing
{
    public class ImageFileAccess
    {
        public static string GetUploadsFolder(IdentityType t)
        {
            if (t == IdentityType.StockSpice)
                return "Stock";
            return "uploads";
        }
        private readonly IHostingEnvironment _hostingEvn;
   
        public ImageFileAccess(IHostingEnvironment hostingEnvironment)
        {
            _hostingEvn = hostingEnvironment;
        }
        public static string GetWebPath(string id, IdentityType typeOfImage)
        {
            return "/" + GetUploadsFolder(typeOfImage) + "/" + GetFolderForImageType(typeOfImage) + "/" + id;
        }
        public static string GetPath(string id, IdentityType typeOfImage)
        {            
            return Path.Combine(GetFolderForImageType(typeOfImage), id);
        }

        private static string GetFolderForImageType(IdentityType typeOfImage)
        {
            string path;
            if (typeOfImage == IdentityType.User )
            {
                path = "userImage";
            }
            else
            {
                path = "spiceImage";
            }
            return path;
        }

        public void GenerateSizesOnDisk(string subPath, MultiSizeImage image, IdentityType typeOfImage)
        {
            GenerateSizeOnDisk(image.WidthForSize(ImageSize.Huge), subPath, image.OriginalFileName,
                       image.FileNameForSize(ImageSize.Huge), typeOfImage);
            foreach (ImageSize size in Enum.GetValues(typeof(ImageSize)))
            {
                if (size == ImageSize.Huge)
                    continue;
                if (size == ImageSize.Tiny)
                   continue;
                
            GenerateSizeOnDisk(image.WidthForSize(size), subPath, image.FileNameForSize(ImageSize.Huge),
                        image.FileNameForSize(size), typeOfImage);
                
            }
            GenerateImageWithSize(image.WidthForSize(ImageSize.Tiny), 2400, subPath, image.FileNameForSize(ImageSize.Medium),
                    image.FileNameForSize(ImageSize.Tiny), 50, typeOfImage);
            
        }

        private void GenerateSizeOnDisk(int width, string subPath, string inFileName, string outFileName, IdentityType typeOfImage)
        {
            subPath = GetCompleteFileName(subPath, typeOfImage);
            var fullInPath = Path.Combine(subPath, inFileName);
            var fullOutPath = Path.Combine(subPath, outFileName);
            using (FileStream stream = System.IO.File.OpenRead(fullInPath))
            using (FileStream output = System.IO.File.OpenWrite(fullOutPath))
            {
                var image = new Image(stream);
                image.Resize(width, 0)
                    .Save(output, new JpegEncoder() { Quality = 75 });
            }
        }

        public void GenerateImageWithSize(int width, int fileSize, string subPath, string inFileName, string outFileName, int quality, IdentityType typeOfImage)
        {
            subPath = GetCompleteFileName(subPath, typeOfImage);
            var fullInPath = Path.Combine(subPath, inFileName);
            var fullOutPath = Path.Combine(subPath, outFileName);
            using (FileStream stream = System.IO.File.OpenRead(fullInPath))
            using (FileStream output = System.IO.File.OpenWrite(fullOutPath))
            {
                var image = new Image(stream);              
                image.Resize(width, 0).Save(output, new JpegEncoder() {Quality = quality});
            }
            var generatedFileSize = new System.IO.FileInfo(fullOutPath).Length;
            if (generatedFileSize > fileSize)
            {
                File.Delete(fullOutPath);
                if (quality > 15)
                {
                    GenerateImageWithSize(width, fileSize, subPath, inFileName, outFileName, (int)(quality - 10), typeOfImage);
                } else
                {
                    GenerateImageWithSize((int)Math.Round(width*0.75), fileSize, subPath, inFileName, outFileName, (int)(quality + 30), typeOfImage);
                }
               
            }
        }
        public double SaveImageFromBinaryStream(Stream dataStream, string subPath, MultiSizeImage imageDescriber, IdentityType typeOfImage)
        {
            var uploads = GetCompleteFileName(subPath, typeOfImage);
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            var fileName = Path.Combine(uploads, imageDescriber.OriginalFileName);
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                dataStream.CopyTo(fileStream);
            }
            var generatedFileSize = new System.IO.FileInfo(fileName).Length;
            return generatedFileSize;
        }

        public void SaveImageFromBase64Stream(string dataStream, string subPath, MultiSizeImage imageDescriber, IdentityType typeOfImage)
        {
            var base64Data = Regex.Match(dataStream, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            var uploads = GetCompleteFileName(subPath, typeOfImage);
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            using (var fileStream = new FileStream(Path.Combine(uploads, imageDescriber.OriginalFileName), FileMode.Create))
            {
                fileStream.Write(binData, 0, binData.Length);
            }
        }

        public void TryDeleteImageFolder( string subPath)
        {
            try
            {
                Directory.Delete(GetCompleteFileName(subPath, IdentityType.Spice), true);
            }
            catch (Exception)
            {
            }
        }

        public string GetCompleteFileName(string subPath, IdentityType typeOfImage)
        {
            var uploads = Path.Combine(_hostingEvn.WebRootPath, GetUploadsFolder(typeOfImage));
            uploads = Path.Combine(uploads, subPath);
            return uploads;
        }
    }
}
