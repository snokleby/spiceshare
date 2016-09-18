using System;
using SpiceShare.DataAccess;
using SpiceShare.ImageProcessing;

namespace SpiceShare.Models
{
    public class SpiceViewModel: SpiceUserInputViewModel
    {
        public int SpiceId { get; set; }

        public int UserId { get; set; }

        public Guid UserIdentity { get; set; }

   
        public string UserName { get; set; }

        public MultiSizeImage Image { get; set; }
        public bool IsComplete => !string.IsNullOrEmpty(Info) && IsValid;
        public bool IsValid => !string.IsNullOrEmpty(Name) && AltTextOk;


        public bool AltTextOk => (string.IsNullOrEmpty(Image.BaseFileName) ||
                                   !string.IsNullOrEmpty(Image.BaseFileName) && !string.IsNullOrEmpty(AltText));
    }
}