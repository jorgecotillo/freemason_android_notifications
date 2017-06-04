
using Android.App;
using Android.Content;
using Android.OS;

namespace com.freemason.mobile.provisioning
{
    [Activity(Label = "CallbackInterceptorActivity")]
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND")]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "freemason",
        DataHost = "callback")]
    public class CallbackInterceptorActivity :  Activity
    {       
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Finish();

            // get URI, send with mediator
            AndroidClientChromeCustomTabsApplication.Mediator.Send(Intent.DataString);

            StartActivity(typeof(MainActivity));
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}