using System;
using System.Collections;
using System.Diagnostics;
using ArRingTryOn.Core;
using Mediapipe;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using UnityEngine;
using UnityEngine.Rendering;
using BaseOptions = Mediapipe.Tasks.Core.BaseOptions;
using Debug = UnityEngine.Debug;
using ImageProcessingOptions = Mediapipe.Tasks.Vision.Core.ImageProcessingOptions;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;

namespace ArRingTryOn.HandTracking
{
    /// <summary>
    /// Runs MediaPipe's hand landmarker on the camera feed and publishes each result as a plain
    /// <see cref="HandFrame"/>. This class and its helpers are the only MediaPipe-aware code.
    /// </summary>
    public sealed class MediaPipeHandTracker : MonoBehaviour
    {
        private const string ModelFileName = "hand_landmarker.bytes";

        // A CPU Image wraps the frame's pixels without copying them, and a released frame may
        // still be in use by MediaPipe, so the pool must be deep enough that reuse comes late.
        private const int FramePoolSize = 10;

        [Header("Camera")]
        [SerializeField] private int _requestedWidth = 1280;
        [SerializeField] private int _requestedHeight = 720;
        [SerializeField] private int _requestedFrameRate = 30;
        [SerializeField] private bool _preferFrontCamera;

        [Header("Detection")]
        [SerializeField, Range(1, 2)] private int _maxHands = 1;
        [SerializeField, Range(0f, 1f)] private float _minDetectionConfidence = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _minPresenceConfidence = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _minTrackingConfidence = 0.5f;

        private readonly WebCamFeed _camera = new WebCamFeed();
        private readonly Stopwatch _clock = new Stopwatch();

        private Coroutine _routine;
        private HandLandmarker _landmarker;
        private TextureFramePool _framePool;
        private CameraView _view;
        private bool _glogInitialized;
        private long _lastTimestampMs = -1;

        // Written from MediaPipe's callback thread, read from the main thread.
        private volatile HandFrame _latestFrame;

        public HandTrackerState State { get; private set; } = HandTrackerState.Idle;

        public string FailureReason { get; private set; }

        public Texture CameraTexture => _camera.Texture;

        public CameraView View => _view;

        /// <summary>The most recent result, or null until the first one arrives.</summary>
        public HandFrame LatestFrame => _latestFrame;

        public void StartTracking()
        {
            if (State != HandTrackerState.Idle)
            {
                return;
            }

            State = HandTrackerState.Starting;
            _routine = StartCoroutine(Guard(Run()));
        }

        private IEnumerator Run()
        {
            InitializeNativeLogging();

            IResourceManager resources = new StreamingAssetsResourceManager();
            yield return resources.PrepareAssetAsync(ModelFileName, ModelFileName, false);

            yield return _camera.Open(_requestedWidth, _requestedHeight, _requestedFrameRate, _preferFrontCamera);
            if (_camera.PermissionDenied)
            {
                State = HandTrackerState.PermissionDenied;
                yield break;
            }

            _view = _camera.View;

            var inferenceDelegate = BaseOptions.Delegate.CPU;
#if UNITY_ANDROID && !UNITY_EDITOR
            yield return GpuManager.Initialize();
            if (GpuManager.IsInitialized)
            {
                inferenceDelegate = BaseOptions.Delegate.GPU;
            }
            else
            {
                Debug.LogWarning("GPU unavailable, running hand tracking on the CPU.");
            }
#endif

            _landmarker = HandLandmarker.CreateFromOptions(BuildOptions(inferenceDelegate), GpuManager.GpuResources);
            _framePool = new TextureFramePool(_view.ImageWidth, _view.ImageHeight, TextureFormat.RGBA32, FramePoolSize);
            _clock.Restart();
            State = HandTrackerState.Running;

            yield return ProcessFrames();
        }

        private IEnumerator ProcessFrames()
        {
            var source = _camera.Texture;
            var imageTransform = ImageTransformationOptions.Build(
                _view.IsFrontFacing,
                _view.IsVerticallyMirrored,
                (RotationAngle)_view.RotationDegrees);
            var processing = new ImageProcessingOptions(rotationDegrees: _view.RotationDegrees);

            AsyncGPUReadbackRequest readback = default;
            var waitForReadback = new WaitUntil(() => readback.done);
            var waitForEndOfFrame = new WaitForEndOfFrame();
            var useAsyncReadback = SystemInfo.supportsAsyncGPUReadback;

            while (true)
            {
                if (!source.didUpdateThisFrame || !_framePool.TryGetTextureFrame(out var frame))
                {
                    yield return null;
                    continue;
                }

                if (useAsyncReadback)
                {
                    readback = frame.ReadTextureAsync(source, imageTransform.flipHorizontally, imageTransform.flipVertically);
                    yield return waitForReadback;

                    if (readback.hasError)
                    {
                        frame.Release();
                        Debug.LogWarning("Failed to read back a camera frame.");
                        continue;
                    }
                }
                else
                {
                    yield return waitForEndOfFrame;
                    frame.ReadTextureOnCPU(source, imageTransform.flipHorizontally, imageTransform.flipVertically);
                }

                var image = frame.BuildCPUImage();
                frame.Release();
                _landmarker.DetectAsync(image, NextTimestampMs(), processing);
            }
        }

        private HandLandmarkerOptions BuildOptions(BaseOptions.Delegate inferenceDelegate)
        {
            return new HandLandmarkerOptions(
                new BaseOptions(inferenceDelegate, modelAssetPath: ModelFileName),
                runningMode: RunningMode.LIVE_STREAM,
                numHands: _maxHands,
                minHandDetectionConfidence: _minDetectionConfidence,
                minHandPresenceConfidence: _minPresenceConfidence,
                minTrackingConfidence: _minTrackingConfidence,
                resultCallback: OnResult);
        }

        // Called on a MediaPipe thread. The result's buffers are reused, so it is copied here.
        private void OnResult(HandLandmarkerResult result, Mediapipe.Image image, long timestampMs)
        {
            _latestFrame = LandmarkConverter.ToHandFrame(result, _view, timestampMs);
        }

        // MediaPipe rejects timestamps that do not strictly increase.
        private long NextTimestampMs()
        {
            _lastTimestampMs = Math.Max(_clock.ElapsedMilliseconds, _lastTimestampMs + 1);
            return _lastTimestampMs;
        }

        private void InitializeNativeLogging()
        {
            Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);
            Glog.Logtostderr = true;
            Glog.Minloglevel = 1;
            Glog.Initialize("ArRingTryOn");
            _glogInitialized = true;
        }

        // Unity logs an exception thrown inside a coroutine and silently stops it. Stepping the
        // routine here, including nested ones, lets a failure reach the State the UI displays.
        private IEnumerator Guard(IEnumerator routine)
        {
            while (State != HandTrackerState.Failed)
            {
                object current;
                try
                {
                    if (!routine.MoveNext())
                    {
                        yield break;
                    }

                    current = routine.Current;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    FailureReason = exception.Message;
                    State = HandTrackerState.Failed;
                    yield break;
                }

                yield return current is IEnumerator nested ? Guard(nested) : current;
            }
        }

        private void OnDestroy()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _landmarker?.Close();
            _landmarker = null;
            _framePool?.Dispose();
            _framePool = null;
            _camera.Close();

            GpuManager.Shutdown();
            if (_glogInitialized)
            {
                Glog.Shutdown();
                Protobuf.ResetLogHandler();
            }
        }
    }
}
