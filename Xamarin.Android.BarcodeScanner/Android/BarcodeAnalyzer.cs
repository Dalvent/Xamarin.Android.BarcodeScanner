using System;
using System.Collections.Generic;
using Android.Content;
using Android.Gms.Extensions;
using AndroidX.Camera.Core;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;

namespace GoogleVisionBarCodeScanner
{
    internal class BarcodeAnalyzer : Java.Lang.Object, ImageAnalysis.IAnalyzer
    {
        private readonly IBarcodeScanner _barcodeScanner;
        private readonly BarcodeSettings _settings;
        private readonly Action<List<BarcodeResult>> _onDetected;

        public BarcodeAnalyzer(BarcodeSettings settings, Action<List<BarcodeResult>> onDetected)
        {
            _settings = settings;
            _onDetected = onDetected;
            _barcodeScanner = BarcodeScanning.GetClient(new BarcodeScannerOptions.Builder().SetBarcodeFormats(
                    Barcode.FormatQrCode)
                .Build());
        }

        public async void Analyze(IImageProxy proxy)
        {

            var mediaImage = proxy.Image;
            if (mediaImage == null) return;

            try
            {
                var image = InputImage.FromMediaImage(mediaImage, proxy.ImageInfo.RotationDegrees);
                // Pass image to the scanner and have it do its thing
                var result = await _barcodeScanner.Process(image);
                var final = Methods.Process(result);
                if (final != null)
                {
                    _onDetected.Invoke(final);
                    if (_settings.VibrationOnDetected)
                        Xamarin.Essentials.Vibration.Vibrate(200);
                }
            }
            catch (Exception ex)
            {
                //Log somewhere
            }
            finally
            {
                proxy.Close();
            }
        }
    }
}