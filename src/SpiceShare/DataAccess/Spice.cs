using System;
using System.ComponentModel.DataAnnotations;

namespace SpiceShare.DataAccess
{
    public class Spice
    {
        public int SpiceId { get; set; }

        public Guid SpiceIdentity { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public string ImageFileName { get; set; }
        public DateTime ImgUploaded { get; set; }
        public bool IsStock { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
        public string ImageFileExtension { get; set; }

        [MaxLength(200)]
        public string AltText { get; set; }

        [MaxLength(200)]    
        public string Info { get;  set; }
    }
}