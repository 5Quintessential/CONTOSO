using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.DPE.ReferenceApps.Food.Lib.Common;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Controls
{
  public sealed class RatingsControl : ItemsControl
  {



    public double Value
    {
      get { return (double)GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(RatingsControl), new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));

    public static void OnValueChanged(DependencyObject src, DependencyPropertyChangedEventArgs e)
    {
      if (src != null)
        (src as RatingsControl).OnValueChangedImpl((double)e.NewValue); 
    }

    private void OnValueChangedImpl(double Value)
    {
      if (Value == null) return;
      if (_itemsSourceData == null) return;
      //is value exactly divisible by 0.5 ?
      if (Math.IEEERemainder(Value, 0.5) != 0.0)
        Value = Math.Round(Value);

      for (int i = 0; i < Value * 2; i++) 
      {
        if (i % 2 == 0) //even
          _itemsSourceData[(int)(i / 2)].LeftFill = true;
        else
          _itemsSourceData[(int)Math.Floor((double)i / 2)].RightFill = true;
      }

    }
    public int Maximum
    {
      get { return (int)GetValue(MaximumProperty); }
      set { SetValue(MaximumProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(int), typeof(RatingsControl), new PropertyMetadata(5, new PropertyChangedCallback(OnMaximumChanged)));

    public static void OnMaximumChanged(DependencyObject src, DependencyPropertyChangedEventArgs e)
    {
      if (src != null) 
        (src as RatingsControl).OnMaximumChangedImpl((int)e.NewValue); 
    }

    private void OnMaximumChangedImpl(int MaxValue)
    {
      _itemsSourceData = new ObservableCollection<FillState>();


      for (int i = 0; i < MaxValue ; i++) 
          _itemsSourceData.Add(new FillState() { LeftFill = false, RightFill = false });  

      if (Value != 0.0)
        OnValueChangedImpl(Value);
    }


    public RatingsControl()
    {
      this.DefaultStyleKey = typeof(RatingsControl);
    }

    protected override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      if(_itemsSourceData == null)
        OnMaximumChangedImpl(Maximum);

      this.ItemsSource = ItemsSourceData;

    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
      return false;
    }


    protected override DependencyObject GetContainerForItemOverride()
    {
      var ret = new RatingItemContainer();

      if (this.ItemContainerStyle != null)
        ret.Style = this.ItemContainerStyle;

      return ret;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      base.PrepareContainerForItemOverride(element, item);
    }
 

    private ObservableCollection<FillState> _itemsSourceData;
    public ObservableCollection<FillState> ItemsSourceData
    {
      get
      {
        return _itemsSourceData;
      }
    }


  }

  public sealed class RatingItemContainer : Control, INotifyPropertyChanged
  {
    public Brush Stroke
    {
      get { return (Brush)GetValue(StrokeProperty); }
      set { SetValue(StrokeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register("Stroke", typeof(Brush), typeof(RatingItemContainer), new PropertyMetadata(new SolidColorBrush(Colors.LimeGreen), new PropertyChangedCallback(OnStrokeChanged)));

    public static void OnStrokeChanged(DependencyObject src, DependencyPropertyChangedEventArgs e)
    {
      if (src != null)
      {
        (src as RatingItemContainer).RaisePropertyChanged("LeftStroke");
        (src as RatingItemContainer).RaisePropertyChanged("RightStroke");
      }
    }
    public Brush Fill
    {
      get { return (Brush)GetValue(FillProperty); }
      set { SetValue(FillProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register("Fill", typeof(Brush), typeof(RatingItemContainer), new PropertyMetadata(new SolidColorBrush(Colors.LimeGreen),new PropertyChangedCallback(OnFillChanged)));

    public static void OnFillChanged(DependencyObject src, DependencyPropertyChangedEventArgs e)
    {
      if (src != null)
      {
        (src as RatingItemContainer).RaisePropertyChanged("LeftFill");
        (src as RatingItemContainer).RaisePropertyChanged("RightFill");
      }
    }

    //public double StrokeThickness
    //{
    //  get { return (double)GetValue(StrokeThicknessProperty); }
    //  set { SetValue(StrokeThicknessProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty StrokeThicknessProperty =
    //    DependencyProperty.Register("StrokeThickness", typeof(double), typeof(RatingItemContainer), new PropertyMetadata(2.0, OnStrokeThicknessChanged));

    //public static void OnStrokeThicknessChanged(DependencyObject src, DependencyPropertyChangedEventArgs e)
    //{
    //  if (src != null)
    //  {
    //    (src as RatingItemContainer).RaisePropertyChanged("LeftStrokeThickness");
    //    (src as RatingItemContainer).RaisePropertyChanged("RightStrokeThickness");
    //  }
    //}

    //public Geometry LeftHalfGeometry
    //{
    //  get { return (Geometry)GetValue(LeftHalfGeometryProperty); }
    //  set { SetValue(LeftHalfGeometryProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for LeftHalfGeometry.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty LeftHalfGeometryProperty =
    //    DependencyProperty.Register("LeftHalfGeometry", typeof(Geometry), typeof(RatingItemContainer), new PropertyMetadata(null));



    //public Geometry RightHalfGeometry
    //{
    //  get { return (Geometry)GetValue(RightHalfGeometryProperty); }
    //  set { SetValue(RightHalfGeometryProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for RightHalfGeometry.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty RightHalfGeometryProperty =
    //    DependencyProperty.Register("RightHalfGeometry", typeof(Geometry), typeof(RatingItemContainer), new PropertyMetadata(null));


    internal int LeftIndex { get; set; }
    internal int RightIndex { get; set; }

    public RatingItemContainer()
    {
      base.DefaultStyleKey = typeof(RatingItemContainer);
    }

    protected override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      Path left = GetTemplateChild("Path_LeftHalf") as Path;
      Path right = GetTemplateChild("Path_RightHalf") as Path; 

      if (left != null)
        left.Width = left.Height / 2;
      if (right != null)
        right.Width = right.Height / 2;

     RaisePropertyChanged("LeftStroke");
     RaisePropertyChanged("RightStroke");
     //RaisePropertyChanged("LeftStrokeThickness");
     //RaisePropertyChanged("RightStrokeThickness");
     RaisePropertyChanged("LeftFill");
     RaisePropertyChanged("RightFill");

      return;
    }
     
    public Brush LeftStroke
    {
      get { 
        return (this.DataContext != null && this.DataContext is FillState) ? 
          ((this.DataContext as FillState).LeftFill ? null : Stroke) :
          null; } 
    }
     
    public Brush RightStroke
    {
      get { return (this.DataContext != null && this.DataContext is FillState) ? ((this.DataContext as FillState).RightFill ? null : Stroke) : null; } 
    } 

    public Brush LeftFill
    {
      get { return (this.DataContext != null && this.DataContext is FillState) ? ((this.DataContext as FillState).LeftFill ? Fill : null) : null; } 
    } 
    public Brush RightFill
    {
      get { return (this.DataContext != null && this.DataContext is FillState) ? ((this.DataContext as FillState).RightFill ? Fill : null) : null; } 
    } 
    //public double LeftStrokeThickness
    //{
    //  get { return (this.DataContext != null && this.DataContext is FillState) ? ((this.DataContext as FillState).LeftFill ? 0.0 : StrokeThickness) : 0.0; }       
    //}

    //public double RightStrokeThickness
    //{
    //  get { return (this.DataContext != null && this.DataContext is FillState) ? ((this.DataContext as FillState).RightFill ? 0.0 : StrokeThickness) : 0.0; } 
    
    //}
    
    internal void RaisePropertyChanged(string PropName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(PropName));
    }
    public event PropertyChangedEventHandler PropertyChanged;
  }

  public class FillState : BindableBase
  {
    private bool _leftFill;

    public bool LeftFill
    {
      get { return _leftFill; }
      set { SetProperty(ref _leftFill, value); }
    }

    private bool _rightFill;

    public bool RightFill
    {
      get { return _rightFill; }
      set { SetProperty(ref _rightFill, value); }
    }
 
  }

  
}
