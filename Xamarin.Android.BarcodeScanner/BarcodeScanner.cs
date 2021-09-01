using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using GoogleVisionBarCodeScanner;
using Xamarin.Google.MLKit.Vision.BarCode;
using Task = Android.Gms.Tasks.Task;

namespace Xamarin.Android.BarcodeScanner
{
    public class BarcodeScanner
    {
        private readonly Context _context;
        private readonly BarcodeSettings _barcodeSettings;

        public BarcodeScanner(Context context, BarcodeSettings barcodeSettings)
        {
            _context = context;
            _barcodeSettings = barcodeSettings;
        }

        public async void Scan(Action<List<BarcodeResult>> onDetected)
        {
            ScannerActivity.StartActivity(_context, new BarcodeActivityTransferModel()
            {
                BarcodeSettings = _barcodeSettings,
                OnDetected = onDetected
            });
        }
    }
}