using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiceShare.Models
{
    public class Paths
    {
        public static string GetPrivatePathForIdentity(IdentityType idType, Guid identity)
        {
            string path;
            if (idType == IdentityType.Spice)
            {
                path = "/Spice/PrivateSpicePage";
            }
            else
            {
                path = "/User/PrivateUserPage";
            }
            return path + "?identity=" + identity.ToString();
        }
    }
}
