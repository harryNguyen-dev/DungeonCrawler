using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace Core
{
    /// <summary>
    /// Centralized loading screen controller for scene transitions and network scene loads.
    /// Shows progress, animated text and guarantees a hard timeout so the UI never gets stuck.
    /// </summary>
    public sealed class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Singleton;

        [Header("UI Elements")]
        [Tooltip("Text label that shows 'Loading...' with animated dots and percentage.")]
        [SerializeField] private TextMeshProUGUI loadingText;

        [Tooltip("Root GameObject of the loading screen UI.")]
        [SerializeField] private GameObject uiLoading;

        [Tooltip("Optional progress bar for visual feedback.")]
        [SerializeField] private Slider loadingProgressBar;

        [Header("Loading Settings")]
        [Tooltip("Minimum duration (seconds) the loading screen should stay visible.")]
        [SerializeField] private float minimumLoadingTime = 2f;

        [Tooltip("Absolute timeout (seconds). After this time the loading UI will be forcefully closed even if nobody called Close().")]
        [SerializeField] private float hardCloseTimeout = 4f;

        [Header("Audio")]
        [SerializeField] private AudioClip loadingAudioClip;
        [SerializeField] private string loadingTag = "master";

        /// <summary>True while the loading UI is visible.</summary>
        [HideInInspector] public bool isLoading;

        private enum LoadingMode
        {
            Timed,
            Manual
        }

        private LoadingMode _mode;
        private bool _running;
        private float _requestedMinTime;
        private float _manualProgress;
        private string _manualStatus = "Loading";
        private Coroutine _routine;

        private float _dotTimer;
        private int _dotCount;

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Shows the loading UI and starts the countdown. If it is already running, the call is ignored.
        /// </summary>
        /// <param name="minTime">Minimum time to keep the loading screen on. The hard timeout still applies.</param>
        public void Open(float minTime = 2f)
        {
            if (_running && _mode == LoadingMode.Timed)
                return;

            StopRoutine();
            _mode = LoadingMode.Timed;
            _running = true;
            isLoading = true;
            _requestedMinTime = Mathf.Max(0.1f, minTime);
            _routine = StartCoroutine(SimpleLoaderRoutine());
        }

        /// <summary>
        /// Shows loading UI until <see cref="Close"/> is called. Progress is driven externally.
        /// </summary>
        public void OpenManual(string statusText = "Loading")
        {
            StopRoutine();
            _mode = LoadingMode.Manual;
            _running = true;
            isLoading = true;
            _manualProgress = 0f;
            _manualStatus = string.IsNullOrWhiteSpace(statusText) ? "Loading" : statusText;

            if (uiLoading) uiLoading.SetActive(true);
            TryPlayAudio();
            ApplyManualUi();
            _routine = StartCoroutine(ManualDotRoutine());
        }

        public void SetProgress(float progress)
        {
            _manualProgress = Mathf.Clamp01(progress);
            if (_mode == LoadingMode.Manual && isLoading)
                ApplyManualUi();
        }

        public void SetStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return;

            _manualStatus = status;
            if (_mode == LoadingMode.Manual && isLoading)
                ApplyManualUi();
        }

        /// <summary>
        /// Requests to close the loading UI. Manual mode closes immediately; timed mode respects minimum duration.
        /// </summary>
        public void Close()
        {
            _running = false;

            if (_mode == LoadingMode.Manual)
            {
                StopRoutine();
                ForceHideUi();
                return;
            }

            if (!isLoading)
                ForceHideUi();
        }

        /// <summary>
        /// Scene-load version (kept just in case you still call this one from old code).
        /// </summary>
        public IEnumerator LoadSceneWithLoadingScreen(string sceneName)
        {
            Open(minimumLoadingTime);

            var asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                float pct = Mathf.Clamp01(asyncOp.progress / 0.9f);

                if (loadingProgressBar)
                    loadingProgressBar.value = pct;

                if (loadingText)
                    loadingText.text = $"Loading{GetAnimatedDots()}\n{Mathf.RoundToInt(pct * 100f)}%";

                // allow scene only after min time
                if (pct >= 1f)
                    asyncOp.allowSceneActivation = true;

                yield return null;
            }

            Close();
        }

        private IEnumerator SimpleLoaderRoutine()
        {
            // UI on
            if (uiLoading) uiLoading.SetActive(true);
            TryPlayAudio();

            float start = Time.unscaledTime;
            float minDeadline = start + _requestedMinTime;
            float hardDeadline = start + Mathf.Max(_requestedMinTime, hardCloseTimeout);

            while (true)
            {
                float now = Time.unscaledTime;
                float elapsed = now - start;

                // progress bar (based on min time)
                if (loadingProgressBar)
                {
                    float pct = Mathf.InverseLerp(0f, _requestedMinTime, elapsed);
                    loadingProgressBar.value = pct;
                }

                if (loadingText)
                {
                    float pct = Mathf.InverseLerp(0f, _requestedMinTime, elapsed);
                    loadingText.text = $"Loading{GetAnimatedDots()}\n{Mathf.RoundToInt(pct * 100f)}%";
                }

                // 1) hard timeout ⇒ always exit
                if (now >= hardDeadline)
                    break;

                // 2) someone called Close() AND min time already passed ⇒ exit
                if (!_running && now >= minDeadline)
                    break;

                yield return null;
            }

            ForceHideUi();
        }

        private IEnumerator ManualDotRoutine()
        {
            while (_running && _mode == LoadingMode.Manual)
            {
                ApplyManualUi();
                yield return null;
            }
        }

        private void ApplyManualUi()
        {
            if (loadingProgressBar)
                loadingProgressBar.value = _manualProgress;

            if (loadingText)
                loadingText.text = $"{_manualStatus}{GetAnimatedDots()}\n{Mathf.RoundToInt(_manualProgress * 100f)}%";
        }

        private void StopRoutine()
        {
            if (_routine == null)
                return;

            StopCoroutine(_routine);
            _routine = null;
        }

        /// <summary>Stops audio, hides UI and clears flags.</summary>
        private void ForceHideUi()
        {
            if (uiLoading && uiLoading.activeSelf)
                uiLoading.SetActive(false);

            TryStopAudio();

            isLoading = false;
        }

        private void TryPlayAudio()
        {
            if (loadingAudioClip == null) return;
            if (AudioManager.Singleton == null) return;
            AudioManager.Singleton.PlayLoadingMenu(loadingAudioClip, loadingTag);
        }

        private void TryStopAudio()
        {
            if (AudioManager.Singleton == null) return;
            AudioManager.Singleton.StopLoadingAudioPlay();
        }

        /// <summary>Returns ".", "..", "..." loop to animate the label.</summary>
        private string GetAnimatedDots()
        {
            _dotTimer += Time.unscaledDeltaTime;
            if (_dotTimer >= 0.5f)
            {
                _dotTimer = 0f;
                _dotCount = (_dotCount + 1) % 4;
            }
            return new string('.', _dotCount);
        }
    }
}
