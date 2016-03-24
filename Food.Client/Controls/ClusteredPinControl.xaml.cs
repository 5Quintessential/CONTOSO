using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bing.Maps;
using Microsoft.DPE.ReferenceApps.Food.Lib.Controls;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.DPE.ReferenceApps.Food.Client.Controls
{
    public sealed partial class ClusteredPinControl : BindableUserControl
    {
        #region Bindable Properties

        private List<Restaurant> _restaurants;
        public List<Restaurant> Restaurants
        {
            get { return this._restaurants; }
            set { this.SetProperty<List<Restaurant>>(ref this._restaurants, value); }
        }

        private Location _location;
        public Location Location
        {
            get { return this._location; }
            private set { this.SetProperty<Location>(ref this._location, value); }
        }

        #endregion Bindable Properties

        #region Constructors

        public ClusteredPinControl() : base()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Public Methods

        public async void ZoomToLocationAsync(Map mapControl, TimeSpan animationDuration, bool zoom = false)
        {
            Location nw, se, loc;

            if (1 < this._restaurants.Count)
            {
                nw = new Location(
                    this._restaurants.Max((r) => r.YelpRecord.latitude),
                    this._restaurants.Min((r) => r.YelpRecord.longitude));

                se = new Location(
                    this._restaurants.Min((r) => r.YelpRecord.latitude),
                    this._restaurants.Max((r) => r.YelpRecord.longitude));

                LocationRect bounds = new LocationRect(nw, se);

                loc = bounds.Center;

                MapLayer.SetPosition(this, loc);
                this.Location = loc;

                if (zoom)
                {
                    bounds = new LocationRect(nw, se);
                    mapControl.SetView(bounds, animationDuration);
                }
            }
            else
            {
                loc = new Location(
                    this._restaurants.Min((r) => r.YelpRecord.latitude),
                    this._restaurants.Min((r) => r.YelpRecord.longitude));

                MapLayer.SetPosition(this, loc);
                this.Location = loc; 

                if (zoom)
                    mapControl.SetView(loc, animationDuration);
            }

            if (zoom)
            {
                if (TimeSpan.MinValue != animationDuration)
                    await Task.Delay(animationDuration.Add(TimeSpan.FromMilliseconds(100d)));

                if (1 < this._restaurants.Count)
                    mapControl.SetZoomLevel(mapControl.ZoomLevel - 0.8);
                else
                    mapControl.SetZoomLevel(18);
            }
        }

        #endregion Public Methods
    }
}
