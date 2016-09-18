using SpiceShare.DataAccess;

namespace SpiceShare.Model
{
    public class UserPageViewModel: UserPageUserInputViewModel
    {
        public string Distance { get; set; }

        public User User { get; set; }
    }

    public class UserPageUserInputViewModel
    {
        public double Lat { get; set; }
        public double Lon { get; set; }

        public double Id { get; set; }
    }
}