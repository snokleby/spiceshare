using System;

namespace SpiceShare.Models
{
    public class ImageUploadViewModel
    {
        public Guid Identity { get; set; }

        public string UploadApiPath
        {
            get
            {
                string path;
                if (IdType == IdentityType.Spice)
                {
                    path = "/api/newSpiceImage";
                }
                else
                {
                    path = "/api/newUserImage";
                }
                return path + "?identity=" + Identity.ToString();
            }
        }

        public string PrivatePagePath
        {
            get
            {
                return Paths.GetPrivatePathForIdentity(IdType, Identity);
            }
        }

        public string FileTypeError { get; set; }
        public IdentityType IdType { get; set; }
    }
}