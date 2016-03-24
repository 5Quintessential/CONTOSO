using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Helpers
{
    public static class MiscHelper
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static bool MergeObservables<T>(string key, ObservableCollection<T> mergeTo, IEnumerable<T> mergeFrom)
        {
            if (mergeFrom == null) // ignore nulls
                return false;

            // prepare queriable list
            var _Property = typeof(T).GetRuntimeProperty(key);
            var _MergeTo = mergeTo.Select(x => new { Key = _Property.GetValue(x).ToString(), Object = x }).ToArray();
            var _MergeFrom = mergeFrom.Select(x => new { Key = _Property.GetValue(x).ToString(), Object = x }).ToArray();

            // query
            var _AddThese = _MergeFrom.Where(x => !_MergeTo.Select(y => y.Key).Contains(x.Key)).ToArray();
            var _DelThese = _MergeTo.Where(x => !_MergeFrom.Select(y => y.Key).Contains(x.Key)).ToArray();

            // process
            foreach (var item in _AddThese.AsParallel())
                mergeTo.Add(item.Object);
            foreach (var item in _DelThese.AsParallel())
                mergeTo.Remove(item.Object);

            // EXTREME EDGE HACK
            if (!mergeTo.Any())
                mergeTo.Clear();

            // return if any changes?
            return (_AddThese.Any() || _DelThese.Any());
        }

        public static bool MergeLists<T>(string key, List<T> mergeTo, IEnumerable<T> mergeFrom)
        {
            if (mergeFrom == null) // ignore nulls
                return false;

            // prepare queriable list
            var _Property = typeof(T).GetRuntimeProperty(key);
            var _MergeTo = mergeTo.Select(x => new { Key = _Property.GetValue(x).ToString(), Object = x }).ToArray();
            var _MergeFrom = mergeFrom.Select(x => new { Key = _Property.GetValue(x).ToString(), Object = x }).ToArray();

            // query
            var _AddThese = _MergeFrom.Where(x => !_MergeTo.Select(y => y.Key).Contains(x.Key)).ToArray();
            var _DelThese = _MergeTo.Where(x => !_MergeFrom.Select(y => y.Key).Contains(x.Key)).ToArray();

            // process
            foreach (var item in _AddThese.AsParallel())
                mergeTo.Add(item.Object);
            foreach (var item in _DelThese.AsParallel())
                mergeTo.Remove(item.Object);

            // return if any changes?
            return (_AddThese.Any() || _DelThese.Any());
        }

        public static ObservableCollection<T> ToObservable<T>(IEnumerable<T> list)
        {
            var _List = new ObservableCollection<T>();
            foreach (var item in list)
                _List.Add(item);
            return _List;
        }

        // Gets the distance (in mi) between two coordinate (lat-long) points
        // Uses the Haversine formula (http://www.movable-type.co.uk/scripts/latlong.html)
        public static double GetDistance(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
        {
            int earthRadius = 6371;         // earth's radius (mean) in km
            double chgLat = DegreeToRadians(toLatitude - fromLatitude);
            double chgLong = DegreeToRadians(toLongitude - fromLongitude);
            double a = Math.Sin(chgLat / 2) * Math.Sin(chgLat / 2) + Math.Cos(DegreeToRadians(fromLatitude))
                * Math.Cos(DegreeToRadians(toLatitude)) * Math.Sin(chgLong / 2) * Math.Sin(chgLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = earthRadius * c;
            return ConvertKilometersToMiles(d);
        }

        private static double DegreeToRadians(double value)
        {
            return Math.PI * value / 180.0;
        }

        public static double ConvertMilesToKilometers(double miles)
        {
            return miles * 1.609344;
        }

        public static double ConvertKilometersToMiles(double kilometers)
        {
            return kilometers * 0.621371192;
        }

    }
}
