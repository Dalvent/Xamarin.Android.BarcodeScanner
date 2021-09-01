using System;
using System.Collections.Generic;
using System.Drawing;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
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
using GoogleVisionBarCodeScanner;
using Java.Lang;
using Java.Util.Concurrent;
using Xamarin.Essentials;
using Color = Android.Graphics.Color;
using Exception = Java.Lang.Exception;
using Range = Android.Util.Range;


[assembly: Application(HardwareAccelerated = true)]

namespace Xamarin.Android.BarcodeScanner
{
    [Activity]
    public class ScannerActivity : AppCompatActivity
    {
        private const int CameraRequestCode = 100;
        private static Queue<BarcodeActivityTransferModel> _barcodeActivityTransferModelQueue = new Queue<BarcodeActivityTransferModel>();
        
        internal static void StartActivity(Context context, BarcodeActivityTransferModel model)
        {
            var intent = new Intent(context, typeof(ScannerActivity));
            _barcodeActivityTransferModelQueue.Enqueue(model);
            context.StartActivity(intent);
        }

        private PreviewView _previewView;
        private BarcodeSettings _barcodeSettings;

        private Action<List<BarcodeResult>> _onDetected;
        
        private IExecutorService _cameraExecutor;
        private IListenableFuture _cameraFuture;

        private ICamera _camera;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _previewView = new PreviewView(this);
            _previewView.LayoutParameters = new PreviewView.LayoutParams(PreviewView.LayoutParams.MatchParent, PreviewView.LayoutParams.MatchParent);
            _previewView.SetBackgroundColor(Color.Black);
            _previewView.SetForegroundGravity(GravityFlags.Center);
            SetContentView(_previewView);

            if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted) {
                RequestPermissions(new [] { Manifest.Permission.Camera }, CameraRequestCode);
            }
            
            SupportActionBar?.Hide();

            var transferModel = _barcodeActivityTransferModelQueue.Peek();
            _barcodeSettings = transferModel.BarcodeSettings;
            _onDetected = transferModel.OnDetected;

            _cameraExecutor = Executors.NewSingleThreadExecutor();
            _cameraFuture = ProcessCameraProvider.GetInstance(this);
            _cameraFuture.AddListener(new Runnable(CameraCallback), ContextCompat.GetMainExecutor(this));
        }

        private void OnDetected(List<BarcodeResult> obj)
        {
            FinishActivity(0);
            _onDetected.Invoke(obj);
        }


        private void CameraCallback()
        {
            var cameraProvider = (ProcessCameraProvider) _cameraFuture.Get();
            // Used to bind the lifecycle of cameras to the lifecycle owner

            if (cameraProvider == null)
                return;

            // Preview
            _previewView.SetImplementationMode(PreviewView.ImplementationMode.Compatible);

            var preview = new Preview.Builder().Build();
            preview.SetSurfaceProvider(_previewView.SurfaceProvider);

            // Frame by frame analyze
            var imageAnalyzerBuilder = new ImageAnalysis.Builder();
            if (_barcodeSettings.RequestedFPS.HasValue)
            {
                Camera2Interop.Extender ext = new Camera2Interop.Extender(imageAnalyzerBuilder);
                ext.SetCaptureRequestOption(CaptureRequest.ControlAeMode, 0);
                ext.SetCaptureRequestOption(CaptureRequest.ControlAeTargetFpsRange, new Range((int) _barcodeSettings.RequestedFPS.Value, (int) _barcodeSettings.RequestedFPS.Value));
            }

            var imageAnalyzer = imageAnalyzerBuilder.Build();
            imageAnalyzer.SetAnalyzer(_cameraExecutor, new BarcodeAnalyzer(_barcodeSettings, OnDetected));

            // Select back camera as a default, or front camera otherwise
            
            CameraSelector cameraSelector = null;
            if (cameraProvider.HasCamera(CameraSelector.DefaultBackCamera))
                cameraSelector = CameraSelector.DefaultBackCamera;
            else if (cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera))
                cameraSelector = CameraSelector.DefaultFrontCamera;
            else
                throw new System.Exception("Camera not found");

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
            catch (Exception exc)
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
            DisableTorchIfNeeded();

            _cameraExecutor?.Shutdown();
            _cameraExecutor?.Dispose();
            _cameraExecutor = null;

            _cameraFuture?.Cancel(true);
            _cameraFuture?.Dispose();
            _cameraFuture = null;

            base.OnDestroy();
        }
    }
}