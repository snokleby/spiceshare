using System.Collections.Generic;

namespace SpiceShare.Models
{
    public class UserViewModel: UserInputViewModel
    {
        public int Id { get; set; }

        public bool ProfileIsComplete => ProfileIsValid &&
                                         !string.IsNullOrEmpty(Lat) &&
                                         !string.IsNullOrEmpty(Lon);

        public bool ProfileIsValid => !string.IsNullOrEmpty(Name) &&
                                         !string.IsNullOrEmpty(Email) &&
                                         !string.IsNullOrEmpty(Country) &&
                                         !string.IsNullOrEmpty(ZipCode);
        public List<SpiceViewModel> Spices { get; set; }
    }
}