using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpiceShare.DataAccess;
using SpiceShare.Models;

namespace SpiceShare.Helpers
{
    public class SearchHelper
    {

        public static List<LocationBasedUser> FindUsersByCountryAndZip(string country, string zipcode)
        {
            country = Normalize(country);
            zipcode = Normalize(zipcode);
            using (var db = new SpiceContext())
            {
                var users = db.Users.Include(user => user.Spices)
                    .Include(user => user.Location)
                    .Where(m => m.Location != null && (!string.IsNullOrEmpty(m.Location.Country) && Normalize(m.Location.Country).Contains(country)) && (!string.IsNullOrEmpty(m.Location.ZipCode) && Normalize(m.Location.ZipCode) == zipcode));
                    if (!users.Any())
                {
                    //find closest zipcode by number.
                    var allUsers =
                        db.Users.Include(user => user.Location)
                            .Include(user => user.Spices)
                            .Where(m => m.Location != null && !string.IsNullOrEmpty(m.Location.Country) && Normalize(m.Location.Country).Contains(country));
                    var locationList = new List<LocationBasedUser>();
                    if (allUsers == null)
                    {
                        return locationList;
                    }
                    foreach (var user in allUsers)
                    {
                        if (user.Location == null || string.IsNullOrEmpty(user.Location.Country) ||
                            string.IsNullOrEmpty(user.Location.ZipCode))
                        {
                            continue;
                        }
                        var numericZipCode = GetNumericZipCode(user.Location.ZipCode);
                        locationList.Add(new LocationBasedUser()
                        {
                            User = user,
                            Distance = Math.Abs(numericZipCode - GetNumericZipCode(zipcode)),
                            ZipCodeBased = true
                        });
                    }
                   return locationList;
                }
                return users.Select(m => new LocationBasedUser() {User = m, ZipCodeBased = true, Distance = -1}).ToList();
            }
        }



        private  static int GetNumericZipCode(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return 0;
            }
            var numStr = new string(val.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(numStr) ?
            0 : int.Parse(numStr);

        }

        private static string Normalize(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return "";
            }
            return val.ToLower().Trim().Replace(" ", "");
        }


        public static List<LocationBasedUser> FindUsersByLatLon(double lat, double lon, SpiceContext db)
        {
            var allUsers = db.Users.Include(users => users.Location).Include(users => users.Spices);
            var locationList = new List<LocationBasedUser>();
            foreach (var user in allUsers)
            {
                if (user.Location == null || user.Location.Lat == null || user.Location.Lon == null)
                {
                    continue;
                }
                var distanceAway = GetAproxDistance(lat, lon, user);
                locationList.Add(new LocationBasedUser() { Distance = distanceAway, User = user });
            }
            return locationList;
        }

        public static List<LocationBasedUser> GetUsersWithSpiceMatch(IEnumerable<LocationBasedUser> users, string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return new List<LocationBasedUser>();
            }
            search = search.ToLower();
            var res = users.Where(m => m.User.Spices.Any(s => (s.Name != null && s.Name.ToLower().Contains(search)) || (s.Info != null && s.Info.ToLower().Contains(search))));
            foreach (var user in res)
            {
                user.User.Spices = user.User.Spices.Where(s => (s.Name != null && s.Name.ToLower().Contains(search)) || (s.Info != null && s.Info.ToLower().Contains(search))).ToList();
            }
            return res.Take(10).ToList();

        }


        public static double GetAproxDistance(double lat, double lon, User theUser)
        {
            return Math.Max(200,
                Math.Round(
                    Location.LocationHelper.DistanceBetweenTwoPointsInMeters(lat, lon, theUser.Location.Lat.Value,
                        theUser.Location.Lon.Value) / 100) * 100);
        }
    }
}
