using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.DPE.ReferenceApps.Food.Client.Controls
{
    public sealed partial class RatingsControl : UserControl
    {
        #region Properties

        public static readonly DependencyProperty StarColorProperty =
            DependencyProperty.Register("StarColor", 
            typeof(Color), 
            typeof(RatingsControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty StarBrushProperty =
            DependencyProperty.Register("StarBrush",
            typeof(SolidColorBrush),
            typeof(RatingsControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty RatingValueProperty =
            DependencyProperty.Register("RatingValue", 
            typeof(double),
            typeof(RatingsControl),
            new PropertyMetadata(null));

        public Color StarColor
        {
            get { return (Color)GetValue(StarColorProperty); }
            set { SetValue(StarColorProperty, (Color)value); }
        }

        public SolidColorBrush StarBrush
        {
            get { return (SolidColorBrush)GetValue(StarBrushProperty); }
            set {  SetValue(StarBrushProperty, (SolidColorBrush)value); }
        }

        public double RatingValue
        {
            get { return (double)GetValue(RatingValueProperty); }
            set { SetValue(RatingValueProperty, (double)value); }
        }

        #endregion Properties

        public RatingsControl()
        {
            this.InitializeComponent();
        }
    }
}
