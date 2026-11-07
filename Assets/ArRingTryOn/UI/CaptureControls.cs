using System;
using ArRingTryOn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArRingTryOn.UI
{
    /// <summary>The shutter button and the short message that follows a photo.</summary>
    public sealed class CaptureControls : MonoBehaviour
    {
        [SerializeField] private Button _shutter;
        [SerializeField] private float _shutterBottomMargin = 230f;
        [SerializeField] private GameObject _toast;
        [SerializeField] private Text _toastText;
        [SerializeField] private Button _toastAction;
        [SerializeField] private Text _toastActionLabel;
        [SerializeField] private float _toastSeconds = 5f;

        private Action _onShare;
        private Action _onOpenSettings;
        private PhotoCaptureFlow.FollowUp _offer;
        private float _hideAt;

        /// <summary>Raised when the shutter is tapped.</summary>
        public event Action ShutterPressed;

        private void Awake()
        {
            _toast.SetActive(false);
            _shutter.onClick.AddListener(() => ShutterPressed?.Invoke());
            _toastAction.onClick.AddListener(OnToastAction);
        }

        private void Start()
        {
            SafeArea.LiftAboveSystemBar((RectTransform)_shutter.transform, _shutterBottomMargin);
        }

        public void Bind(Action onShare, Action onOpenSettings)
        {
            _onShare = onShare;
            _onOpenSettings = onOpenSettings;
        }

        public void SetBusy(bool busy)
        {
            _shutter.interactable = !busy;
        }

        public void ShowResult(string message, PhotoCaptureFlow.FollowUp offer)
        {
            _offer = offer;
            _toastText.text = message;
            _toastAction.gameObject.SetActive(offer != PhotoCaptureFlow.FollowUp.None);
            _toastActionLabel.text = offer == PhotoCaptureFlow.FollowUp.OpenSettings ? "Settings" : "Share";
            _toast.SetActive(true);
            _hideAt = Time.unscaledTime + _toastSeconds;
        }

        private void Update()
        {
            if (_toast.activeSelf && Time.unscaledTime >= _hideAt)
            {
                _toast.SetActive(false);
            }
        }

        private void OnToastAction()
        {
            _toast.SetActive(false);
            if (_offer == PhotoCaptureFlow.FollowUp.Share)
            {
                _onShare?.Invoke();
            }
            else if (_offer == PhotoCaptureFlow.FollowUp.OpenSettings)
            {
                _onOpenSettings?.Invoke();
            }
        }
    }
}
