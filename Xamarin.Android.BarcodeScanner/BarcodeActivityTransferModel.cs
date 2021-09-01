using System;
using System.Collections.Generic;
using GoogleVisionBarCodeScanner;

namespace Xamarin.Android.BarcodeScanner
{
    internal class BarcodeActivityTransferModel
    {
        public BarcodeSettings BarcodeSettings { get; set; }
        public Action<List<BarcodeResult>> OnDetected { get; set; }
    }
}