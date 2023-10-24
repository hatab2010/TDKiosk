using TDKiosk.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocalFiles))]
namespace TDKiosk.Droid
{
    public class LocalFiles : IBaseUrl
    {
        public string GetUrl()
        {
            return "file:///android_asset/";
        }
    }
}