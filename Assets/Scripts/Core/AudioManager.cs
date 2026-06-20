using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core
{
   
    /// <summary>
    /// Manages all game audio, storing per-category volumes (Master, VFX, Ambience, Custom tags)
    /// and computing the effective output as <c>Master × Category</c>.
    /// Volumes are persisted with <see cref="SecurePrefs"/> in an encrypted JSON file so
    /// they survive the “clean login” flow.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        /*────────────────────────── Serialized Fields ──────────────────────────*/
        [Header("Audio Sources")]
        [Tooltip("Main AudioSource for sounds that follow the listener (Master output).")]
        public AudioSource masterAudioSource;
        [Tooltip("Dedicated looping AudioSource for ambient tracks.")]
        public AudioSource ambientAudioSource;
        [Tooltip("One-shot AudioSource for loading/menu clips.")]
        public AudioSource loadingAudioSource;

        [Header("Volume Settings (0–1)")]
        [Range(0, 1)] public float masterVolume = 1f;   // Multiplicative root
        [Range(0, 1)] public float vfxVolume = 1f;   // Relative to Master
        [Range(0, 1)] public float ambienceVolume = 1f;   // Relative to Master

        [Tooltip("Extra categories (e.g. “ui”, “others”)")]
        public List<CustomAudioTag> customTagVolumes = new();

        [Header("Audio Limitations")]
        [Tooltip("Global cap – simultaneous clips (0 = unlimited).")]
        public int maxConcurrentAudio = 20;

        /*────────────────────────── Runtime ───────────────────────────*/
        private int _currentAudioCount;
        public static AudioManager Singleton;

        /*────────────────────────── Persistence Keys ──────────────────────────*/
        private const string KEY_MASTER = "AUDIO_MASTER_VOLUME";
        private const string KEY_VFX = "AUDIO_VFX_VOLUME";
        private const string KEY_AMBIENCE = "AUDIO_AMBIENCE_VOLUME";

        /*────────────────────────── Unity - Lifecycle ─────────────────────────*/
        private void Awake()
        {
            if (Singleton != null) { Destroy(gameObject); return; }

            Singleton = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioSources();
            ambientAudioSource.loop = true;
            LoadVolumeSettings();
            ApplyVolumeSettings();
        }

        void EnsureAudioSources()
        {
            var sources = GetComponents<AudioSource>();

            if (masterAudioSource == null)
                masterAudioSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();

            if (ambientAudioSource == null)
                ambientAudioSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

            if (loadingAudioSource == null)
                loadingAudioSource = sources.Length > 2 ? sources[2] : gameObject.AddComponent<AudioSource>();
        }

        /*────────────────────────── Public API – Volume Setters ───────────────*/
        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        public void SetVFXVolume(float value)
        {
            vfxVolume = Mathf.Clamp01(value);
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        public void SetAmbienceVolume(float value)
        {
            ambienceVolume = Mathf.Clamp01(value);
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        /// <summary>Sets a custom tag volume (e.g. “ui”, “others”).</summary>
        public void SetCustomTagVolume(string tag, float value)
        {
            value = Mathf.Clamp01(value);
            for (int i = 0; i < customTagVolumes.Count; ++i)
                if (customTagVolumes[i].tag == tag)
                {
                    customTagVolumes[i] = new CustomAudioTag { tag = tag, volume = value };
                    SaveVolumeSettings();
                    return;
                }

            customTagVolumes.Add(new CustomAudioTag { tag = tag, volume = value });
            SaveVolumeSettings();
        }

        /*────────────────────────── Playback Helpers ──────────────────────────*/

        /// <summary>
        /// Plays an ambient audio clip in a loop with the specified tag.
        /// Does not affect the current audio count or maxConcurrentAudio.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="tag">The tag used to determine the volume.</param>
        public void PlayAmbientAudio(AudioClip clip, string tag = "ambient")
        {
            if (clip == null) return;
            loadingAudioSource.Stop();

            ambientAudioSource.Stop();
            ambientAudioSource.clip = clip;
            ambientAudioSource.volume = GetEffectiveVolume(tag);
            ambientAudioSource.Play();
        }

        /// <summary>
        /// Plays a loading menu audio clip and stops the ambient audio.
        /// The ambient audio will be stopped and prevented from playing again until reloaded.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="tag">The tag used to determine the volume.</param>
        public void PlayLoadingMenu(AudioClip clip, string tag = "master")
        {
            if (clip == null) return;
            ambientAudioSource.Stop();

            loadingAudioSource.volume = GetEffectiveVolume(tag);
            loadingAudioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// New signature – plays the clip at an arbitrary **world position**.
        /// Ideal para explosões, projéteis, etc.
        /// </summary>
        public void PlayAudio(AudioClip clip, string tag, Vector3 worldPos)
        {
            if (clip == null) return;
            if (maxConcurrentAudio > 0 && _currentAudioCount >= maxConcurrentAudio) return;

            float vol = GetEffectiveVolume(tag);
            SoundEffectsPool.PlayClip(clip, worldPos, vol);

            _currentAudioCount++;
            ReduceAudioCountAsync(clip.length).Forget();
        }

        /// <summary>
        /// Old signature – plays *at the listener* (2D).
        /// </summary>
        public void PlayAudio(AudioClip clip, string tag)
           => PlayAudio(clip, tag, Camera.main ? Camera.main.transform.position : Vector3.zero);

        /// <summary>
        /// Stops all currently playing audio, including ambient and loading audio.
        /// This function is useful when the game ends or the player dies,
        /// ensuring that no audio is left playing.
        /// </summary>
        public void StopAllAudioPlay()
        {
            masterAudioSource.Stop();
            ambientAudioSource.Stop();
            loadingAudioSource.Stop();
            _currentAudioCount = 0;
        }

        public void StopLoadingAudioPlay() => loadingAudioSource.Stop();

        public void StopAmbientAudio()
        {
            ambientAudioSource.Stop();
            ambientAudioSource.clip = null;
        }

        /// <summary>
        /// Gets the volume based on the tag.
        /// </summary>
        private float GetEffectiveVolume(string tag)
        {
            tag = tag.ToLowerInvariant();
            if (tag == "master") return masterVolume;

            float category = tag switch
            {
                "vfx" => vfxVolume,
                "ambient" => ambienceVolume,
                _ => GetCustomTag(tag)
            };
            return masterVolume * category;
        }

        private float GetCustomTag(string tag)
        {
            foreach (var t in customTagVolumes)
                if (t.tag == tag) return t.volume;
            Debug.LogWarning($"Audio tag '{tag}' not found – using 1.");
            return 1f;
        }

        /// <summary>
        /// Reduces the audio count after a certain amount of time (the clip length).
        /// </summary>
        private async UniTaskVoid ReduceAudioCountAsync(float seconds)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds));
            _currentAudioCount = Mathf.Max(0, _currentAudioCount - 1);
        }

        /*────────────────────────── Persistence ──────────────────────────*/
        private void LoadVolumeSettings()
        {
            // masterVolume = SecurePrefs.GetDecryptedFloatFromFile(KEY_MASTER, masterVolume);
            // vfxVolume = SecurePrefs.GetDecryptedFloatFromFile(KEY_VFX, vfxVolume);
            // ambienceVolume = SecurePrefs.GetDecryptedFloatFromFile(KEY_AMBIENCE, ambienceVolume);
        }

        private void SaveVolumeSettings()
        {
            // SecurePrefs.SetEncryptedFloatToFile(KEY_MASTER, masterVolume);
            // SecurePrefs.SetEncryptedFloatToFile(KEY_VFX, vfxVolume);
            // SecurePrefs.SetEncryptedFloatToFile(KEY_AMBIENCE, ambienceVolume);
        }

        private void ApplyVolumeSettings()
        {
            masterAudioSource.volume = masterVolume;
            ambientAudioSource.volume = masterVolume * ambienceVolume;
            loadingAudioSource.volume = masterVolume;
        }  
    }
    /// <summary>Simple pair (tag, volume) used by <see cref="AudioManager"/>.</summary>
    [System.Serializable]
    public struct CustomAudioTag
    {
        public string tag;
        [Range(0, 1)] public float volume;
    }

}
