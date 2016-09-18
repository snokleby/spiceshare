using System;

namespace SpiceShare.Location
{
    public class LocationHelper
    {
        public static double DistanceBetweenTwoPointsInMeters(double lat1, double lon1, double lat2, double lon2)
        {

            var R = 6371;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            lat1 = ToRad(lat1);
            lat2 = ToRad(lat2);
            var a = Math.Pow(Math.Sin(dLat / 2), 2) + (Math.Pow(Math.Sin(dLon / 2), 2) * Math.Cos(lat1) * Math.Cos(lat2));
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;
            return distance*1000;
        }

        public static double ToRad(double degs)
        {
            return degs * (Math.PI / 180.0);
        }
    }
}

