using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TDKiosk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IndicetableButton : ContentView
    {
        public IndicetableButton()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public static readonly BindableProperty TextProperty = 
            BindableProperty.Create("Text", typeof(string), typeof(IndicetableButton), default(string));    
        
        public static readonly BindableProperty ProgressProperty = 
            BindableProperty.Create("Progress", typeof(float), typeof(IndicetableButton), default(float));

        public static readonly BindableProperty MainColorProperty = 
            BindableProperty.Create("MainColor", typeof(Color), typeof(IndicetableButton), default(Color));

        public static readonly BindableProperty SubColorProperty = 
            BindableProperty.Create("SubColor", typeof(Color), typeof(IndicetableButton), default(Color));

        public static readonly BindableProperty DeactiveColorProperty = 
            BindableProperty.Create("DeactiveColor", typeof(Color), typeof(IndicetableButton), default(Color));

        public static readonly BindableProperty IsReverseProperty = 
            BindableProperty.Create("IsReverse", typeof(bool), typeof(IndicetableButton), default(bool));

        public static readonly BindableProperty IsLoaderActiveProperty = 
            BindableProperty.Create("IsLoaderActive", typeof(bool), typeof(IndicetableButton), default(bool));

        public bool IsLoaderActive
        {
            get { return (bool)GetValue(IsLoaderActiveProperty); }
            set { SetValue(IsLoaderActiveProperty, value); }
        }
        public bool IsReverse
        {
            get { return (bool)GetValue(IsReverseProperty); }
            set 
            {
                SetDesignOrientation(value);
                SetValue(IsReverseProperty, value);
            }
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public Color DeactiveColor
        {
            get { return (Color)GetValue(DeactiveColorProperty); }
            set { SetValue(DeactiveColorProperty, value); }
        }
        public Color SubColor
        {
            get { return (Color)GetValue(SubColorProperty); }
            set { SetValue(SubColorProperty, value); }
        }
        public Color MainColor
        {
            get { return (Color)GetValue(MainColorProperty); }
            set { SetValue(MainColorProperty, value); }
        }
        public float Progress
        {
            get { return (float)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public void SetDesignOrientation(bool isReverse)
        {
            MyArc.SweepAngle = 90;

            if (!isReverse)
            {
                MyArc.StartAngle = 180;
            }
            else
            {
                MyArc.StartAngle = 90;
            }
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                // Use default vibration length
                Vibration.Vibrate();
            }
            catch (FeatureNotSupportedException ex)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }

        }
    }
}