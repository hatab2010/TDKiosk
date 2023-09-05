using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.App.Admin;
using Android.Content;
using Android.Widget;
using Android.Views;

namespace TDKiosk.Droid
{
    [Activity(
        Label = "TDKiosk", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize,
        ScreenOrientation = ScreenOrientation.Landscape
        )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ////TODO - временно отключил, дебагер не корректно работает
            //// Полноэкранный режим
            //this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            //this.Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);

            //// Запрет на выход из полноэкранного режима
            //this.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.ImmersiveSticky | SystemUiFlags.Fullscreen);

            ComponentName deviceAdmin = new ComponentName(ApplicationContext, Java.Lang.Class.FromType(typeof(DeviceAdmin)).Name);
            DevicePolicyManager dpm = (DevicePolicyManager)GetSystemService(Context.DevicePolicyService);

            if (dpm.IsDeviceOwnerApp(PackageName))
            {
                IntentFilter intentFilter = new IntentFilter(Intent.ActionMain);
                intentFilter.AddCategory(Intent.CategoryLauncher);
                string[] lockTaskPackages;
                lockTaskPackages = new string[] { PackageName };
                dpm.SetLockTaskPackages(deviceAdmin, lockTaskPackages);
                //StartLockTask(); //TODO - временно отключил, дебагер не корректно работает
            }

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }        
    }

    [BroadcastReceiver(
            Name = "com.companyname.tdkiosk.DeviceAdmin", 
            Exported = false, 
            Permission = "android.permission.BIND_DEVICE_ADMIN"
        )]
    [IntentFilter(new string[] { "android.app.action.DEVICE_ADMIN_ENABLED", "android.app.action.DEVICE_OWNER_CHANGED" })]
    [MetaData("android.app.device_admin", Resource = "@xml/device_admin")]
    public class DeviceAdmin : DeviceAdminReceiver
    {
        public override void OnEnabled(Context context, Intent intent)
        {
            base.OnEnabled(context, intent);
            Toast.MakeText(context, "Device Admin Enabled", ToastLength.Short).Show();
        }

        public override void OnDisabled(Context context, Intent intent)
        {
            base.OnDisabled(context, intent);
            Toast.MakeText(context, "Device Admin Disabled", ToastLength.Short).Show();
        }

        //public override string OnDisableRequestedFormatted([GeneratedEnum] Context context, Intent intent)
        //{
        //    return "User wants to disable your application as device admin";
        //}

        public override void OnLockTaskModeExiting(Context context, Intent intent)
        {
            base.OnLockTaskModeExiting(context, intent);            
        }

        public override void OnLockTaskModeEntering(Context context, Intent intent, string pkg)
        {
            //
            base.OnLockTaskModeEntering(context, intent, pkg);
        }
    }
}