using System;
using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.Media;
using Android.OS;
using Android.Runtime;
using AndroidX.Camera.Core;
using Java.Util;
using Xamarin.BarcodeScanner.Android.Enum;
using Xamarin.BarcodeScanner.Android.Models;
using Xamarin.BarcodeScanner.Android.Settings;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;
using Object = Java.Lang.Object;

namespace Xamarin.BarcodeScanner.Android
{
    internal class BarcodeAnalyzer : Object, ImageAnalysis.IAnalyzer
    {
        private readonly IBarcodeScanner _barcodeScanner;
        private readonly Action<ScanResult> _onDetected;
        private readonly BarcodeSettings _settings;

        public BarcodeAnalyzer(BarcodeSettings settings, Action<ScanResult> onDetected)
        {
            _settings = settings;
            _onDetected = onDetected;
            _barcodeScanner = BarcodeScanning.GetClient(new BarcodeScannerOptions.Builder().SetBarcodeFormats(
                    Barcode.FormatQrCode)
                .Build());
        }

        public async void Analyze(IImageProxy proxy)
        {
            Image mediaImage = proxy.Image;
            if (mediaImage == null) return;

            try
            {
                InputImage image = InputImage.FromMediaImage(mediaImage, proxy.ImageInfo.RotationDegrees);
                // Pass image to the scanner and have it do its thing
                Object result = await _barcodeScanner.Process(image);
                ScanResult final = Process(result);
                if (final != null)
                {
                    _onDetected.Invoke(final);
                    if (_settings.VibrationOnDetected)
                        Vibrate(200);
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


        private static ScanResult Process(Object result)
        {
            if (result == null)
                return null;

            var scannedBarcodesJavaList = result.JavaCast<ArrayList>();
            if (scannedBarcodesJavaList.IsEmpty)
                return null;

            Object[] scannedBarcodes = scannedBarcodesJavaList.ToArray();
            var resultBarcodes = new BarcodeResult[scannedBarcodes.Length];
            for (var i = 0; i < scannedBarcodes.Length; i++)
            {
                var scannedBarcode = scannedBarcodes[i].JavaCast<Barcode>();

                resultBarcodes[i] = new BarcodeResult
                {
                    BarcodeType = ConvertBarcodeResultTypes(scannedBarcode.ValueType),
                    DisplayValue = scannedBarcode.DisplayValue,
                    RawValue = scannedBarcode.RawValue
                };
            }

            return new ScanResult
            {
                IsSuccessful = true,
                BarcodeResults = resultBarcodes
            };
        }

        private void Vibrate(int vibrationMilliseconds)
        {
            var vibrator = (Vibrator) Application.Context.GetSystemService(Context.VibratorService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                vibrator.Vibrate(VibrationEffect.CreateOneShot(vibrationMilliseconds, VibrationEffect.DefaultAmplitude));
            else //deprecated in API 26 
                vibrator.Vibrate(vibrationMilliseconds);
        }

        private static BarcodeTypes ConvertBarcodeResultTypes(int barcodeValueType)
        {
            switch (barcodeValueType)
            {
                case Barcode.TypeCalendarEvent:
                    return BarcodeTypes.CalendarEvent;
                case Barcode.TypeContactInfo:
                    return BarcodeTypes.ContactInfo;
                case Barcode.TypeDriverLicense:
                    return BarcodeTypes.DriversLicense;
                case Barcode.TypeEmail:
                    return BarcodeTypes.Email;
                case Barcode.TypeGeo:
                    return BarcodeTypes.GeographicCoordinates;
                case Barcode.TypeIsbn:
                    return BarcodeTypes.Isbn;
                case Barcode.TypePhone:
                    return BarcodeTypes.Phone;
                case Barcode.TypeProduct:
                    return BarcodeTypes.Product;
                case Barcode.TypeSms:
                    return BarcodeTypes.Sms;
                case Barcode.TypeText:
                    return BarcodeTypes.Text;
                case Barcode.TypeUrl:
                    return BarcodeTypes.Url;
                case Barcode.TypeWifi:
                    return BarcodeTypes.WiFi;
                default: return BarcodeTypes.Unknown;
            }
        }
    }
}