using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Uri = Android.Net.Uri;

namespace GoogleVisionBarCodeScanner
{
    public class BarcodeSettings
    {
        public BarcodeSettings()
        {
        }

        public bool VibrationOnDetected { get; set; }
        public bool DefaultTorchOn { get; set; }
        public bool AutoStartScanning { get; set; }
        public float? RequestedFPS { get; set; }
        public int ScanInterval { get; set; }
        public bool TorchOn { get; set; }

        public class Builder
        {
            private readonly BarcodeSettings _barcodeSettings = new BarcodeSettings();

            public Builder SetVibrationOnDetected(bool value)
            {
                _barcodeSettings.VibrationOnDetected = value;
                return this;
            }

            public Builder SetDefaultTorchOn(bool value)
            {
                _barcodeSettings.DefaultTorchOn = value;
                return this;
            }

            public Builder SetAutoStartScanning(bool value)
            {
                _barcodeSettings.AutoStartScanning = value;
                return this;
            }

            public Builder SetRequestedFPS(float? value)
            {
                _barcodeSettings.RequestedFPS = value;
                return this;
            }

            public Builder SetScanInterval(int value)
            {
                _barcodeSettings.ScanInterval = value;
                return this;
            }

            public Builder SetTorchOn(bool value)
            {
                _barcodeSettings.TorchOn = value;
                return this;
            }

            public BarcodeSettings Build() => _barcodeSettings;
        }
    }
}