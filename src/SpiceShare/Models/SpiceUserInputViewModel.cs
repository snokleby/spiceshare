using System;
using System.ComponentModel.DataAnnotations;

namespace SpiceShare.Models
{
    public class SpiceUserInputViewModel
    {
        public Guid SpiceIdentity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(300)]
        public string Info { get; set; }

        [MaxLength(300)]
        public string AltText { get; set; }


    }
}