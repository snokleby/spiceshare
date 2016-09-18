using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiceShare.Models
{
    public class NearMeViewModel: NearMeUserInputViewModel
    {
        public List<LocationBasedUser> Users { get; set; }
        public bool ZipCodeSearch { get; set; }

        public string GenError { get; set; }
    }
}
