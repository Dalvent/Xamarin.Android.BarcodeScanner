using Xamarin.BarcodeScanner.Android.Enum;

namespace Xamarin.BarcodeScanner.Android.Settings
{
    public class BarcodeSettings
    {
        public bool VibrationOnDetected { get; set; }
        public float? RequestedFPS { get; set; }
        public bool TorchOn { get; set; }
        public ILayoutApplier LayoutApplier { get; set; }
        public AspectRatio AspectRatio { get; set; }
    }
}