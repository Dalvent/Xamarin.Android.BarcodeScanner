using System;
using Xamarin.BarcodeScanner.Android.Settings;

namespace Xamarin.BarcodeScanner.Android.Models
{
    internal class BarcodeActivityTransferModel
    {
        public BarcodeSettings BarcodeSettings { get; set; }
        public Action<ScanResult> OnResult { get; set; }
    }
}