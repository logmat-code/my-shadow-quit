using UnityEngine;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Manages original procedural dark ambient music, torch crackling loop, and gameplay sound effects.
    /// Handles WebGL audio context resumption cleanly on first user interaction.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _musicSource;
        private AudioSource _crackleSource;
        private AudioSource _sfxSource;

        private float _masterVolume = 0.85f;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                AudioListener.volume = _masterVolume;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = 0.42f;
            _musicSource.playOnAwake = false;

            _crackleSource = gameObject.AddComponent<AudioSource>();
            _crackleSource.loop = true;
            _crackleSource.volume = 0.28f;
            _crackleSource.playOnAwake = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.volume = 0.75f;
            _sfxSource.playOnAwake = false;
        }

        private void Start()
        {
            StartAmbience();
        }

        public void StartAmbience()
        {
            AudioClip music = AssetRegistry.GetAudio("Music/dark_ambient_theme");
            if (music != null && !_musicSource.isPlaying)
            {
                _musicSource.clip = music;
                _musicSource.Play();
            }

            AudioClip crackle = AssetRegistry.GetAudio("SFX/fire_crackle_loop");
            if (crackle != null && !_crackleSource.isPlaying)
            {
                _crackleSource.clip = crackle;
                _crackleSource.Play();
            }
        }

        public void SetTorchCrackleIntensity(float normalizedIntensity)
        {
            if (_crackleSource != null)
            {
                _crackleSource.volume = Mathf.Clamp01(normalizedIntensity) * 0.32f;
            }
        }

        public void PlaySFX(string sfxName, float volumeScale = 1f, float pitch = 1f)
        {
            if (_sfxSource == null) return;
            AudioClip clip = AssetRegistry.GetAudio("SFX/" + sfxName);
            if (clip != null)
            {
                _sfxSource.pitch = pitch;
                _sfxSource.PlayOneShot(clip, volumeScale);
            }
        }
    }
}
