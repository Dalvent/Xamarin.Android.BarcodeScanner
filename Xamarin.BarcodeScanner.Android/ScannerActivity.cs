using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Camera2.InterOp;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using AndroidX.Lifecycle;
using Google.Common.Util.Concurrent;
using Java.Lang;
using Java.Util.Concurrent;
using Xamarin.BarcodeScanner.Android.Models;
using Xamarin.BarcodeScanner.Android.Settings;
using AspectRatio = Xamarin.BarcodeScanner.Android.Enum.AspectRatio;
using Exception = System.Exception;
using Range = Android.Util.Range;

namespace Xamarin.BarcodeScanner.Android
{
    [Activity]
    internal class ScannerActivity : AppCompatActivity
    {
        private const int CameraRequestCode = 100;
        private const string SCANNER_INTENT_KEY = "SCANNER_INTENT_KEY";

        private static readonly Dictionary<int, BarcodeActivityTransferModel> _barcodeActivityTransferModels = new();
        private static int _scannersCount;
        private BarcodeSettings _barcodeSettings;
        private ICamera _camera;

        private IExecutorService _cameraExecutor;
        private IListenableFuture _cameraFuture;

        private ScanResult _lastScanResult;

        private Action<ScanResult> _onResultAction;

        private PreviewView _previewView;
        private int _scannerActivityIndex;

        internal static void StartActivity(Context context, BarcodeActivityTransferModel model)
        {
            var intent = new Intent(context, typeof(ScannerActivity));
            int scannerIndex = _scannersCount++;
            _barcodeActivityTransferModels.Add(scannerIndex, model);
            intent.PutExtra(SCANNER_INTENT_KEY, scannerIndex);
            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }

        private static BarcodeActivityTransferModel GetArgsModel(int scannerActivityIndex)
        {
            BarcodeActivityTransferModel model = _barcodeActivityTransferModels[scannerActivityIndex];
            return model;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SupportActionBar?.Hide();

            _scannerActivityIndex = Intent.GetIntExtra(SCANNER_INTENT_KEY, -1);
            if (_scannerActivityIndex == -1)
                throw new ArgumentException();

            InitWithModel(GetArgsModel(_scannerActivityIndex));

            SetContentView(CreateScannerLayout());

            RequestPermissions();

            _cameraExecutor = Executors.NewSingleThreadExecutor();
            _cameraFuture = ProcessCameraProvider.GetInstance(this);
            _cameraFuture.AddListener(new Runnable(CameraCallback), ContextCompat.GetMainExecutor(this));
        }

        private FrameLayout CreateScannerLayout()
        {
            var frameLayout = new FrameLayout(this);
            frameLayout.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            frameLayout.SetBackgroundColor(Color.Black);

            _previewView = new PreviewView(this);
            _previewView.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, GravityFlags.Center);
            _previewView.SetBackgroundColor(Color.Black);
            frameLayout.AddView(_previewView);

            ILayoutApplier layoutApplier = _barcodeSettings.LayoutApplier ?? DefaultLayoutApplier();
            layoutApplier.ApplyLayout(this, frameLayout);

            return frameLayout;
        }

        private static HeaderFooterTextLayoutApplier DefaultLayoutApplier()
        {
            return new("", "");
        }

        private void InitWithModel(BarcodeActivityTransferModel model)
        {
            _barcodeSettings = model.BarcodeSettings;
            _onResultAction = model.OnResult;
        }

        private void RequestPermissions()
        {
            if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted) RequestPermissions(new[] {Manifest.Permission.Camera}, CameraRequestCode);
        }

        private void OnDetected(ScanResult scanResult)
        {
            _lastScanResult = scanResult;
            Finish();
        }

        private void CameraCallback()
        {
            var cameraProvider = (ProcessCameraProvider) _cameraFuture.Get();
            // Used to bind the lifecycle of cameras to the lifecycle owner

            if (cameraProvider == null)
                return;

            _previewView.SetImplementationMode(PreviewView.ImplementationMode.Compatible);

            var previewBuilder = new Preview.Builder();

            switch (_barcodeSettings.AspectRatio)
            {
                case AspectRatio.Auto:
                    break;
                case AspectRatio.Ratio16_9:
                    previewBuilder.SetTargetAspectRatio(AndroidX.Camera.Core.AspectRatio.Ratio169);
                    break;
                case AspectRatio.Ratio4_3:
                    previewBuilder.SetTargetAspectRatio(AndroidX.Camera.Core.AspectRatio.Ratio43);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Preview preview = previewBuilder.Build();
            preview.SetSurfaceProvider(_previewView.SurfaceProvider);

            // Frame by frame analyze
            var imageAnalyzerBuilder = new ImageAnalysis.Builder();
            if (_barcodeSettings.RequestedFPS.HasValue)
            {
                var ext = new Camera2Interop.Extender(imageAnalyzerBuilder);
                ext.SetCaptureRequestOption(CaptureRequest.ControlAeMode, 0);
                ext.SetCaptureRequestOption(CaptureRequest.ControlAeTargetFpsRange, new Range((int) _barcodeSettings.RequestedFPS.Value, (int) _barcodeSettings.RequestedFPS.Value));
            }

            ImageAnalysis imageAnalyzer = imageAnalyzerBuilder.Build();
            imageAnalyzer.SetAnalyzer(_cameraExecutor, new BarcodeAnalyzer(_barcodeSettings, OnDetected));

            // Select back camera as a default, or front camera otherwise

            CameraSelector cameraSelector = null;
            if (cameraProvider.HasCamera(CameraSelector.DefaultBackCamera))
                cameraSelector = CameraSelector.DefaultBackCamera;
            else if (cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera))
                cameraSelector = CameraSelector.DefaultFrontCamera;
            else
                throw new Exception("Camera not found");

            try
            {
                // Unbind use cases before rebinding
                cameraProvider.UnbindAll();
                // Bind use cases to camera
                var lifeCycleOwner = this as ILifecycleOwner;
                _camera = cameraProvider.BindToLifecycle(lifeCycleOwner, cameraSelector, preview, imageAnalyzer);
                //HandleTorch();
                HandleTorch();
            }
            catch (Java.Lang.Exception exc)
            {
                Log.Debug(nameof(CameraCallback), "Use case binding failed", exc);
            }
        }

        private void HandleTorch()
        {
            if (_camera == null || !_camera.CameraInfo.HasFlashUnit) return;
            if (_barcodeSettings.TorchOn && IsTorchOn() || !_barcodeSettings.TorchOn && !IsTorchOn())
                return;
            _camera.CameraControl.EnableTorch(_barcodeSettings.TorchOn);
        }

        private bool IsTorchOn()
        {
            if (_camera == null || !_camera.CameraInfo.HasFlashUnit)
                return false;
            return (int) _camera.CameraInfo.TorchState?.Value == TorchState.On;
        }

        private void DisableTorchIfNeeded()
        {
            if (_camera == null || !_camera.CameraInfo.HasFlashUnit || (int) _camera.CameraInfo.TorchState?.Value != TorchState.On)
                return;
            _camera.CameraControl.EnableTorch(false);
        }

        protected override void OnDestroy()
        {
            if (IsFinishing)
            {
                _onResultAction.Invoke(_lastScanResult ?? new ScanResult {IsSuccessful = false});
                RemoveCurrentArgsFromStatic();
            }

            DisableTorchIfNeeded();

            _cameraExecutor?.Shutdown();
            _cameraExecutor?.Dispose();
            _cameraExecutor = null;

            _cameraFuture?.Cancel(true);
            _cameraFuture?.Dispose();
            _cameraFuture = null;

            base.OnDestroy();
        }

        private void RemoveCurrentArgsFromStatic()
        {
            _barcodeActivityTransferModels.Remove(_scannerActivityIndex);
        }
    }
}