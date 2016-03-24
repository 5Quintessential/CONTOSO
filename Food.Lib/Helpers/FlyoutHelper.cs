using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Helpers
{
    public class FlyoutHelper
    {
        protected Popup m_Popup = new Popup();
        public Popup Show(Popup popup, FrameworkElement button, double verticalOffset = 50d, double horizontalOffset = 35d)
        {
            if (popup == null)
                throw new Exception("Popup is not defined");
            m_Popup = popup;
            if (button == null)
                throw new Exception("Button is not defined");
            if (double.IsNaN(verticalOffset))
                throw new Exception("Vertical Offset is not defined");
            if (double.IsNaN(horizontalOffset))
                throw new Exception("Horizontal Offset is not defined");
            var _Child = popup.Child as FrameworkElement;
            if (_Child == null)
                throw new Exception("Popup.Child is not defined");
            if (double.IsNaN(_Child.Height))
                throw new Exception("Popup.Child.Height is not defined");
            if (double.IsNaN(_Child.Width))
                throw new Exception("Popup.Child.Width is not defined");

            // get position of the button
            var _Page = Window.Current.Content as Page;
            var _Visual = button.TransformToVisual(_Page);
            var _Point = _Visual.TransformPoint(new Point(0, 0));
            var _Button = new
            {
                Top = _Point.Y,
                Left = _Point.X,
                Width = button.ActualWidth,
                Height = button.ActualHeight,
            };

            // determine location
            var _TargetTop = (_Button.Top + (_Button.Height / 2)) - _Child.Height - verticalOffset;
            if (_TargetTop < 0)
                _TargetTop = verticalOffset;

            var _TargetLeft = (_Button.Left + (_Button.Width / 2)) - (_Child.Width / 2);

            if ((_TargetLeft + _Child.Width) > Window.Current.Bounds.Width)
                _TargetLeft = Window.Current.Bounds.Width - _Child.Width - horizontalOffset;
            if (_TargetLeft < 0)
                _TargetLeft = horizontalOffset;

            // setup popup
            popup.VerticalOffset = _TargetTop;
            popup.HorizontalOffset = _TargetLeft;

            // add pretty animation(s)
            popup.ChildTransitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection 
            { 
                new Windows.UI.Xaml.Media.Animation.EntranceThemeTransition 
                { 
                    FromHorizontalOffset = 0, 
                    FromVerticalOffset = 20 
                }
            };

            // setup
            m_Popup.IsLightDismissEnabled = true;
            m_Popup.IsOpen = true;

            // handle when it closes
            m_Popup.Closed -= popup_Closed;
            m_Popup.Closed += popup_Closed;

            // handle making it close
            Window.Current.Activated -= Current_Activated;
            Window.Current.Activated += Current_Activated;

            // return
            return m_Popup;
        }

        protected void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (m_Popup == null)
                return;
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
                m_Popup.IsOpen = false;
        }

        protected void popup_Closed(object sender, object e)
        {
            Window.Current.Activated -= Current_Activated;
            if (m_Popup == null)
                return;
            m_Popup.IsOpen = false;
        }
    }
}
