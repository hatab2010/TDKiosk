using Android.Net.Wifi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TDKiosk
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Card : ContentView
	{
		public event Action<Card> OnPressed;
        private CancellationTokenSource animateTimerCancellationTokenSource;


        public Card ()
		{
			InitializeComponent();
            BindingContext = this;
            _ = RotateImageContinously(FLBlink);
            _ = RotateImageContinously(RLBlink, -1);
        }

		public static readonly BindableProperty FlashEnableProperty = BindableProperty.Create("FlashEnable", typeof(bool), typeof(bool), default(bool));

		public bool FlashEnable
		{
			get { return (bool)GetValue(FlashEnableProperty); }
			set { SetValue(FlashEnableProperty, value); }
		}
		public static readonly BindableProperty pathProperty = BindableProperty.Create("path", typeof(string), typeof(string), default(string));
		public static readonly BindableProperty isActiveProperty = BindableProperty.Create("isActive", typeof(bool), typeof(bool), default(bool));
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(string), default(string));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public bool isActive
		{
			get { return (bool)GetValue(isActiveProperty); }
			set { SetValue(isActiveProperty, value); }
		}

		public string path
		{
			get { return (string)GetValue(pathProperty); }
			set { SetValue(pathProperty, value); }
		}


        private void Button_Pressed(object sender, EventArgs e)
        {
			OnPressed?.Invoke(this);
        }

        private void Button_Released(object sender, EventArgs e)
        {

        }
        async Task RotateImageContinously(Image image, int delta = 1)
        {
            while (true) // a CancellationToken in real life ;-)
            {
                if (image.Rotation >= 360f) image.Rotation = 0;
                await image.RotateTo(360* delta, 50000, Easing.Linear);
            }
        }

        async void StartAnimationTimer(CancellationTokenSource tokenSource)
        {
            try
            {
                //maintain a reference to the token so we can cancel when needed
                animateTimerCancellationTokenSource = tokenSource;

                int idleTime = 1000; //ms

                await Task.Delay(TimeSpan.FromMilliseconds(idleTime), tokenSource.Token);

                //Do something here
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (FLBlink != null && FLBlink.IsVisible)
                    {
                        FLBlink.RelRotateTo(500, 1000, Easing.SinOut);
                        //Do this if you want to have it possibly happen again
                        StartAnimationTimer(new CancellationTokenSource());
                    }                       
                    
                });
            }
            catch (TaskCanceledException ex)
            {
                //if we cancel/reset, this catch block gets called
                Debug.WriteLine(ex);
            }
            // if we reach here, this timer has stopped
        }
    }
}