namespace Xamarin.BarcodeScanner.Android.Models
{
    public class ScanResult
    {
        public bool IsSuccessful { get; set; }
        public BarcodeResult[] BarcodeResults { get; set; }
    }
}