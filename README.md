# Xamarin.Android.BarcodeScanner
Barcode library dependence on Google MLKit API.

Works on Android (MonoAndroid10.0 and MonoAndroid11.0)
# Setup
manifest:
```XML
  <uses-permission android:name="android.permission.CAMERA" />
  <uses-feature android:name="android.hardware.camera" />
 ```
 # Scanning
 ```C#
  var barcodeScanner = new BarcodeScanner(this, new BarcodeSettings
  {
      VibrationOnDetected = true
  });
 ```
 
