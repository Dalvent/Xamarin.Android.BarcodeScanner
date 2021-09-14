using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Xamarin.BarcodeScanner.Android;
using Xamarin.BarcodeScanner.Android.Settings;

namespace SampleApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var barcodeScanner = new BarcodeScanner(this, new BarcodeSettings
            {
                VibrationOnDetected = true
            });
            var result = await barcodeScanner.ScanAsync();
            int x = 2;
        }
    }
}