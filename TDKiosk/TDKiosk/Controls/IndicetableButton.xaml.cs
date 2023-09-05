using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace TDKiosk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IndicetableButton : ContentView
    {
        public event Action Pressed;
        public event Action Unpressed;
        public event Action<string> Relesed;

        private Loader _loader;

        public IndicetableButton()
        {

            InitializeComponent();
            BindingContext = this;
            _loader = new Loader(1000 * 10 * 1, 0.01f);

            _loader.Updated += OnUpdate;
            _loader.Relesed += OnRelesed;
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
                //SetDesignOrientation(value);
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
        private bool _isPress = false;
        private float _durationIncrase = 0.1f;
        

        private void Button_Released(object sender, EventArgs e)
        {           
            _isPress = false;
            _loader.StopIncrase();           

            ButtonElipse.Scale = 1;
        }

        public void Restart()
        {
            IsLoaderActive = true;
            _loader.Restart();
        }

        public void Stop()
        {
            IsLoaderActive = false;
            _loader.Stop();
        }

        private void Button_Pressed(object sender, EventArgs e)
        {           
            _isPress = true;
            ButtonElipse.Scale = 0.9;

            if (IsLoaderActive)
            {
                _loader.StartIncrase();
            }
            else
            {
                Relesed?.Invoke(StateName.Text);
            }
        }

        private void OnRelesed()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                if (IsLoaderActive == false)
                    return;

                IsLoaderActive = false;
                Relesed?.Invoke(StateName.Text);
            });
        }

        private void OnUpdate()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                Progress = _loader.Progress;
            });
        }
    }
}