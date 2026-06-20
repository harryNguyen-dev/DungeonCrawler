using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    /// <summary>
    /// Plays a UI click sound when the attached <see cref="Button"/> is pressed.
    /// Skips playback when the catalog clip is not assigned.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonSound : MonoBehaviour
    {
        public enum SoundKind
        {
            Confirm,
            Back,
            Tab,
            Error
        }

        [SerializeField] private SoundKind sound = SoundKind.Confirm;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Play);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Play);
        }

        private void Play()
        {
            switch (sound)
            {
                case SoundKind.Back:
                    GameAudio.PlayUiBack();
                    break;
                case SoundKind.Tab:
                    GameAudio.PlayUiTab();
                    break;
                case SoundKind.Error:
                    GameAudio.PlayUiError();
                    break;
                default:
                    GameAudio.PlayUiConfirm();
                    break;
            }
        }
    }
}
