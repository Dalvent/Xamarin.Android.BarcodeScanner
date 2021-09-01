using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using GoogleVisionBarCodeScanner;
using Xamarin.Android.BarcodeScanner;

namespace SampleApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var barcodeScanner = new BarcodeScanner(this, new BarcodeSettings.Builder()
                .SetVibrationOnDetected(true)
                .Build());
            barcodeScanner.Scan(obj =>
            {
                var d = 2;
                var x = 1;
            });
        }
    }
}