using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.DPE.ReferenceApps.Food.Client.Controls
{
    public sealed partial class RestaurantCustomBalloon : UserControl, INotifyPropertyChanged
    {
        private Visibility _balloonFlyoutVisibililty = Visibility.Visible;
        public Visibility BalloonFlyoutVisibililty
        {
            get { return _balloonFlyoutVisibililty; }
            set
            {
                this.balloonFlyout.Visibility = value;
                SetProperty<Visibility>(ref _balloonFlyoutVisibililty, value);
            }
        }

        private int _restaurantID;
        public int RestaurantID
        {
            get { return _restaurantID; }
            set
            {
                SetProperty<int>(ref _restaurantID, value);
            }
        }


        private Color _bgColor = Colors.Black;
        public Color BGColor
        {
            get { return _bgColor; }
            set
            {
                SetProperty<Color>(ref _bgColor, value);
                SetProperty<SolidColorBrush>(ref _bgFillBrush, new SolidColorBrush(value));
            }
        }

        private Restaurant _restaurant;
        public Restaurant Restaurant
        {
            get { return _restaurant; }
            set
            {
                SetProperty<Restaurant>(ref _restaurant, value);
            }
        }

        private double _averageRating = 0.0;
        public double AverageRating
        {
            get { return _averageRating; }
            set
            {
                SetProperty<double>(ref _averageRating, value);
            }
        }

        private SolidColorBrush _bgFillBrush = new SolidColorBrush(Colors.Black);
        public Brush BGFillBrush
        {
            get { return _bgFillBrush; }
        }

        // CONSTRUCTOR
        public RestaurantCustomBalloon(Color bgColor)
        {
            BGColor = bgColor;
            this.DataContext = this;
            this.InitializeComponent();

        }


        #region INotifyPropertyChanged Members

        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Members

        public void ToggleBalloonFlyoutVisibility()
        {
            if (BalloonFlyoutVisibililty == Windows.UI.Xaml.Visibility.Collapsed)
            {
                BalloonFlyoutVisibililty = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                BalloonFlyoutVisibililty = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}
