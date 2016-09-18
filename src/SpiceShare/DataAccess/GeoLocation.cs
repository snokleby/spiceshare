using System.ComponentModel.DataAnnotations;

namespace SpiceShare.DataAccess
{
    public class GeoLocation
    {
        public int GeoLocationId { get; set; }
        public double? Lat { get; set; }

        public double? Lon { get; set; }

        [MaxLength(200)]
        public string Country { get; set; }

        [MaxLength(50)]

        public string ZipCode { get; set; }

        [MaxLength(200)]
        public string TownOrArea { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}