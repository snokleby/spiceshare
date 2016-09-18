
namespace SpiceShare.ImageProcessing
{
    public class MultiSizeImage
    {    
        public MultiSizeImage()
        {

        }
        public MultiSizeImage(string fileName, string fileExtension)
        {
            BaseFileName = fileName;
            FileExtension = fileExtension;
        }

        public string BaseFileName { get; set; }
        public string FileExtension { get; set; }

        public string DirectoryName { get; set; }

        public string AlternativeDescription { get; set; }
        public string OriginalFileName => $"{BaseFileName}_original.{FileExtension}";
        public long CacheBuster { get; set; }

        public string FileNameForSize(ImageSize size)
        {
            return $"{BaseFileName}_{size}.jpg";
        }

        public string WebPathForSize(ImageSize size) => DirectoryName + "/" + FileNameForSize(size) + "?v=" + CacheBuster;
    

        public int WidthForSize(ImageSize size)
        {
            switch (size)
            {
                 case ImageSize.Tiny:
                    return 200;
                    case ImageSize.Small:
                    return 320;
                    case ImageSize.Medium:
                    return 640;
                    case ImageSize.Large:
                    return 1024;
                    case ImageSize.Huge:
                    return 1600;
            }
            return 200;
        }
    }

    public enum ImageSize
    {
        Tiny, Small, Medium, Large, Huge
    }
}
