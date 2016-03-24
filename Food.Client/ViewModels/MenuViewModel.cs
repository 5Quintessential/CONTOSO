using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Common;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    public class MenuViewModel : BindableBase
    {
        #region Bindable Properties

        private Restaurant _restaurant;
        public Restaurant Restaurant
        {
            get { return this._restaurant; }
            set {  this.SetProperty<Restaurant>(ref this._restaurant, value); }
        }

        public int MenuItemCount
        {
            get
            {
                int numItems = 0;

                if (null != this.Restaurant.Menus)
                {
                    foreach (var menu in this.Restaurant.Menus)
                        foreach (var group in menu.Groups)
                            numItems += group.Items.Count;
                }

                return numItems;
            }
        }

        private int _numZoomedInRows = 3;
        public int NumZoomedInRows
        {
            get { return this._numZoomedInRows; }
            set { this.SetProperty<int>(ref this._numZoomedInRows, value); }
        }

        private int _numZoomedOutRows = 5;
        public int NumZoomedOutRows
        {
            get { return this._numZoomedOutRows; }
            set { this.SetProperty<int>(ref this._numZoomedOutRows, value); }
        }

        private double _maxHeight = 820d;
        public double MaxHeight
        {
            get { return this._maxHeight; }
            set { this.SetProperty<double>(ref this._maxHeight, value); }
        }
            
        #endregion Bindable Properties

        #region Constructors

        public MenuViewModel()
        {
            this.CheckSize();
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        #endregion Constructors

        #region Private Methods

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.CheckSize();
        }

        private void CheckSize()
        {
            if (Window.Current.Bounds.Height >= 1080)
            {
                this.NumZoomedInRows = 3;
                this.NumZoomedOutRows = 5;
                this.MaxHeight = 820d;
            }
            else
            {
                this.NumZoomedInRows = 3;
                this.NumZoomedOutRows = 3;
                this.MaxHeight = 508d;
            }
                
        }

        #endregion Private Methods
    }
}
