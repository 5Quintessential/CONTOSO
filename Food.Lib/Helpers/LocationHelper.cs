using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Helpers
{
    public static class LocationHelper
    {
        public static async Task<Windows.Devices.Geolocation.Geoposition> GetMyLocationAsync()
        {
            Windows.Devices.Geolocation.Geoposition _Postion = null;
            try
            {
                var _Locator = new Windows.Devices.Geolocation.Geolocator();
                var _Token = new System.Threading.CancellationTokenSource().Token;
                _Postion = await _Locator.GetGeopositionAsync().AsTask(_Token);
                
            }
            catch { /* continue */ }
            return _Postion;
        }
    }
}
