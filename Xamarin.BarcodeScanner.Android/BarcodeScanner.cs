using System;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.BarcodeScanner.Android.Models;
using Xamarin.BarcodeScanner.Android.Settings;

namespace Xamarin.BarcodeScanner.Android
{
    public class BarcodeScanner
    {
        private readonly BarcodeSettings _barcodeSettings;
        private readonly Context _context;

        public BarcodeScanner(Context context, BarcodeSettings barcodeSettings)
        {
            _context = context;
            _barcodeSettings = barcodeSettings;
        }

        public void Scan(Action<ScanResult> onResult)
        {
            ScannerActivity.StartActivity(_context, new BarcodeActivityTransferModel
            {
                BarcodeSettings = _barcodeSettings,
                OnResult = onResult
            });
        }

        public async Task<ScanResult> ScanAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<ScanResult>();
            Scan(scanResult => taskCompletionSource.SetResult(scanResult));
            return await taskCompletionSource.Task;
        }
    }
}