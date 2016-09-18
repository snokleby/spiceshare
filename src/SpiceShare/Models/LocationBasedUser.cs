using SpiceShare.DataAccess;

namespace SpiceShare.Models
{
    public class LocationBasedUser
    {
        public double Distance { get; set; }
        public User User { get; set; }
        public bool ZipCodeBased { get;  set; }
    }
}