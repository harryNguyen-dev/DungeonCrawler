using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Điều khiển <see cref="CinemachineBasicMultiChannelPerlin"/> bằng cách nhấp <see cref="AmplitudeGain"/>
    /// (Perlin mặc định là rung liên tục — cần code để có cú "đập" khi hit).
    /// </summary>
    public class CameraShakeController : MonoBehaviour
    {
        [SerializeField] CinemachineBasicMultiChannelPerlin _perlin;
        [Tooltip("Amplitude khi không shake (0 = tắt noise lúc đứng yên).")]
        [SerializeField] float _baseAmplitude = 0f;
        [SerializeField] float _hitPeakAmplitude = 1.25f;
        [SerializeField] float _hitShakeDuration = 0.14f;

        int _shakeGeneration;

        void Awake()
        {
            if (_perlin == null)
                _perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (_perlin != null)
                _perlin.AmplitudeGain = _baseAmplitude;
        }

        public void PlayHitShake()
        {
            int id = ++_shakeGeneration;
            RunHitShake(id).Forget();
        }

        async UniTaskVoid RunHitShake(int id)
        {
            if (_perlin == null) return;

            _perlin.ReSeed();
            float elapsed = 0f;

            while (elapsed < _hitShakeDuration)
            {
                if (id != _shakeGeneration) return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _hitShakeDuration);
                float ease = 1f - t;
                ease *= ease;
                _perlin.AmplitudeGain = Mathf.Lerp(_baseAmplitude, _hitPeakAmplitude, ease);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (id == _shakeGeneration && _perlin != null)
                _perlin.AmplitudeGain = _baseAmplitude;
        }
    }
}
