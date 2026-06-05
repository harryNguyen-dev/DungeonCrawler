using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if FUSION_NETWORK
using Fusion;
#endif

namespace Core
{
    /// <summary>
    /// Global pool for SFX one-shot. 
    /// </summary>
    public sealed class SoundEffectsPool : MonoBehaviour
    {
        /* ───── singleton ───── */
        public static SoundEffectsPool Instance
        {
            get
            {
                if (_instance) return _instance;

                var go = new GameObject(nameof(SoundEffectsPool));
                _instance = go.AddComponent<SoundEffectsPool>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }
        private static SoundEffectsPool _instance;

        /* ───── config ───── */
        [SerializeField] private int maxConcurrent = 32;
        [SerializeField] private int initialPoolSize = 8;

#if FUSION_NETWORK
        private NetworkObjectPool _netPool;     // opcional
#else
        private readonly Queue<AudioSource> _pool = new();
        private readonly HashSet<AudioSource> _inUse = new();
#endif

        /* ───── lifecycle ───── */
        private void Awake()
        {
            if (_instance && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

#if FUSION_NETWORK
            _netPool = GetComponent<NetworkObjectPool>();
            if (!_netPool) Debug.LogWarning(
                $"[{nameof(SoundEffectsPool)}] NetworkObjectPool not found");
#endif
            PreWarm(initialPoolSize);
        }

        /* ───── PUBLIC API ───── */

        public static void PlayClip(AudioClip clip,
                                    Vector3 pos,
                                    float volume = 1f,
                                    float pitch = 1f)
        {
            if (!clip) return;

            // limite global
            if (Instance.maxConcurrent > 0 &&
#if FUSION_NETWORK
                Instance._playingCount >= Instance.maxConcurrent)
#else
                Instance._inUse.Count >= Instance.maxConcurrent)
#endif
                return;

            var src = Instance.GetSource();
            if (!src) return;

            src.transform.position = pos;
            src.volume = volume;
            src.pitch = pitch;
            src.clip = clip;
            src.Play();

            Instance.ReleaseAfter(src, clip.length).Forget();
        }

        /* ───── helpers (local + network) ───── */

        private AudioSource GetSource()
        {
#if FUSION_NETWORK
            if (!_localPool.TryDequeue(out var src) || !src)
                src = gameObject.AddComponent<AudioSource>();

            _playingCount++;
            return Prepare(src);
#else
            AudioSource src = null;
            while (_pool.Count > 0 && !src)
            {
                var cand = _pool.Dequeue();
                if (cand) src = cand;        
            }
            if (!src) src = gameObject.AddComponent<AudioSource>();
            _inUse.Add(src);
            return Prepare(src);
#endif
        }

        private static AudioSource Prepare(AudioSource src)
        {
            src.playOnAwake = false;
            src.loop = false;
            return src;
        }

        private async UniTaskVoid ReleaseAfter(AudioSource src, float seconds)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds));
            ReturnSource(src);
        }

        private void ReturnSource(AudioSource src)
        {
            if (!src) return;
            src.Stop();
            src.clip = null;

#if FUSION_NETWORK
            _playingCount = Mathf.Max(0, _playingCount - 1);
            _localPool.Enqueue(src);
#else
            _inUse.Remove(src);
            _pool.Enqueue(src);
#endif
        }

        private void PreWarm(int count)
        {
#if FUSION_NETWORK
            _localPool = new Queue<AudioSource>(count);
            for (int i = 0; i < count; ++i)
                _localPool.Enqueue(gameObject.AddComponent<AudioSource>());
#else
            for (int i = 0; i < count; ++i)
                _pool.Enqueue(gameObject.AddComponent<AudioSource>());
#endif
        }

#if FUSION_NETWORK
        private Queue<AudioSource> _localPool;
        private int _playingCount;
#endif
    }
}
