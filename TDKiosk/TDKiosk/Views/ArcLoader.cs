using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace TDKiosk
{
    public class ArcLoader : SKCanvasView
    {
        public static readonly BindableProperty StartAngleProperty = 
            BindableProperty.Create(nameof(StartAngle), typeof(float), typeof(ArcLoader), default(float), propertyChanged: RedrawCanvas);

        public static readonly BindableProperty SweepAngleProperty = 
            BindableProperty.Create(nameof(SweepAngle), typeof(float), typeof(ArcLoader), default(float), propertyChanged: RedrawCanvas);

        public static readonly BindableProperty ArcColorProperty = 
            BindableProperty.Create(nameof(ArcColor), typeof(Color), typeof(ArcLoader), Color.Black, propertyChanged: RedrawCanvas);

        public static readonly BindableProperty LoaderColorProperty =
            BindableProperty.Create(nameof(LoaderColor), typeof(Color), typeof(ArcLoader), Color.White, propertyChanged: RedrawCanvas);

        public static readonly BindableProperty StrokeWidthProperty = 
            BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(ArcLoader), 1f, propertyChanged: RedrawCanvas);

        public static readonly BindableProperty IsLoaderActiveProperty = 
            BindableProperty.Create("IsLoaderActive", typeof(bool), typeof(ArcLoader), default(bool), propertyChanged: RedrawCanvas);
        
        public static readonly BindableProperty ProgressProperty = 
            BindableProperty.Create("Progress", typeof(float), typeof(ArcLoader), 0f, propertyChanged: RedrawCanvas);

        public static readonly BindableProperty DeactiveColorProperty = 
            BindableProperty.Create("DeactiveColor", typeof(Color), typeof(ArcLoader), default(Color), propertyChanged: RedrawCanvas);

        public static readonly BindableProperty IsReverseProperty = 
            BindableProperty.Create("IsReverse", typeof(bool), typeof(ArcLoader), default(bool), propertyChanged: RedrawCanvas);

        public bool IsReverse
        {
            get { return (bool)GetValue(IsReverseProperty); }
            set { SetValue(IsReverseProperty, value); }
        }

        public bool IsLoaderActive
        {
            get { return (bool)GetValue(IsLoaderActiveProperty); }
            set { SetValue(IsLoaderActiveProperty, value); }
        }

        public Color DeactiveColor
        {
            get { return (Color)GetValue(DeactiveColorProperty); }
            set { SetValue(DeactiveColorProperty, value); }
        }

        public float Progress
        {
            get { return (float)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public float StrokeWidth
        {
            get { return (float)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        public float StartAngle
        {
            get { return (float)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public float SweepAngle
        {
            get { return (float)GetValue(SweepAngleProperty); }
            set { SetValue(SweepAngleProperty, value); }
        }

        public Color ArcColor
        {
            get { return (Color)GetValue(ArcColorProperty); }
            set { SetValue(ArcColorProperty, value); }
        }

        public Color LoaderColor
        {
            get { return (Color)GetValue(LoaderColorProperty); }
            set { SetValue(LoaderColorProperty, value); }
        }

        public ArcLoader()
        {
            PaintSurface += OnPaintSurface;
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear();

            using (var backgroundPath = new SKPath())
            using (var foregraoundPath= new SKPath())
            using (var arkPaint = new SKPaint())
            using (var lodarePaint = new SKPaint())
            {
                float x = info.Width / 2f;
                float y = info.Height / 2f;

                float size = (info.Width < info.Height ? info.Width : info.Height);
                float padding = StrokeWidth / 2;
                float startAngle = IsReverse ? StartAngle + 180 : StartAngle;
                float sweepAngle = IsReverse ? -SweepAngle : SweepAngle;

                SKRect rect = new SKRect(padding, padding, size - padding, size - padding);


                arkPaint.Style = SKPaintStyle.Stroke;
                arkPaint.StrokeWidth = StrokeWidth;
                arkPaint.Color = IsLoaderActive ? ArcColor.ToSKColor() : DeactiveColor.ToSKColor();

                backgroundPath.AddArc(rect, startAngle, sweepAngle);
                canvas.DrawPath(backgroundPath, arkPaint);

                if (IsLoaderActive)
                {
                    float arkPadding = IsReverse ? -4 : 4;
                    float loaderStartAngle = startAngle + arkPadding;
                    float loaderSweepAngle = sweepAngle - arkPadding*2;

                    loaderSweepAngle *= Progress;

                    lodarePaint.Style = SKPaintStyle.Stroke;
                    lodarePaint.Color = LoaderColor.ToSKColor();
                    lodarePaint.StrokeWidth = StrokeWidth * 0.4f;

                    foregraoundPath.AddArc(rect, loaderStartAngle, loaderSweepAngle);

                    canvas.DrawPath(foregraoundPath, lodarePaint);
                }               
            }
        }

        private static void RedrawCanvas(BindableObject bindable, object oldValue, object newValue)
        {
            var arc = bindable as ArcLoader;
            arc?.InvalidateSurface();
        }
    }
}
