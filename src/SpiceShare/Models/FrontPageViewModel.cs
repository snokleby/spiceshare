using System.Linq;
using SpiceShare.DataAccess;
using System.Collections.Generic;

namespace SpiceShare.Models
{
    public class FrontPageViewModel
    {
        public FrontPageViewModel()
        {
        }

        public List<Spice> LatestSpices { get; set; }
    }
}