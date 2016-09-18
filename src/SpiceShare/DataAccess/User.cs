using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpiceShare.DataAccess
{
    public class User
    {
        public int UserId { get; set; }
        public Guid UserIdentity { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public GeoLocation Location { get; set; }

        public List<Spice> Spices { get; set; }
     
        [MaxLength(200)]
        public string Email { get; set; }
    }
}