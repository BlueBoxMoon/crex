using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace Sample.Android
{
    [Activity( Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true )]
    [IntentFilter( new[] { Intent.ActionMain }, Categories = new[] { Intent.CategoryLeanbackLauncher } )]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            new Crex.Android.Application().Run( this );
        }
    }
}