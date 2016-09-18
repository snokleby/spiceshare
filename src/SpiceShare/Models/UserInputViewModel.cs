using System;
using System.ComponentModel.DataAnnotations;

namespace SpiceShare.Models
{
    public class UserInputViewModel
    {
        public Guid UserIdentity { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email-address.")]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string Country { get; set; }

        [Required]
        [MaxLength(50)]
        public string ZipCode { get; set; }

        public string Lat { get; set; }

        public string Lon { get; set; }
    }
}