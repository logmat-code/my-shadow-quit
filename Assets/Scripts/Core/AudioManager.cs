using UnityEngine;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Manages original procedural dark ambient music, torch crackling loop, and gameplay sound effects.
    /// Implements a dB-calibrated 4-bus mix architecture (MASTER, MUSIC, SFX, AMBIENCE/FIRE):
    /// - Music: 0 dB (reference dominant layer)
    /// - SFX: -8 dB
    /// - Ambience: -12 dB
    /// - Fire crackle: -18 dB relative to Music (soft, secondary texture that never masks music)
    /// Prevents duplicate AudioSources and persists independent user Music/SFX preferences via PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PrefMasterVol = "MSQ_MasterVolume";
        private const string PrefMusicVol = "MSQ_MusicVolume";
        private const string PrefSfxVol = "MSQ_SfxVolume";
        private const string PrefAmbienceVol = "MSQ_AmbienceVolume";

        // Calibrated Reference Mix Bus Gains (dB -> Linear)
        // Music: 0 dB -> 1.00f (scaled by headroom 0.72f so summed output never clips)
        // SFX: -8 dB -> 10^(-8/20) = 0.3981f
        // Ambience: -12 dB -> 10^(-12/20) = 0.2512f
        // Fire Crackle: -18 dB relative to Music -> 10^(-18/20) = 0.1259f
        private const float HeadroomScale = 0.72f;
        public const float MusicBusDb = 0f;
        public const float SfxBusDb = -8f;
        public const float AmbienceBusDb = -12f;
        public const float FireRelativeToMusicDb = -18f;

        private AudioSource _musicSource;
        private AudioSource _crackleSource;
        private AudioSource _sfxSource;

        private float _masterVolume = 0.90f;
        private float _musicVolume = 1.00f;
        private float _sfxVolume = 0.85f;
        private float _ambienceVolume = 0.80f;
        private float _torchProximityFactor = 0.65f;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                AudioListener.volume = _masterVolume;
                PlayerPrefs.SetFloat(PrefMasterVol, _masterVolume);
                PlayerPrefs.Save();
                ApplyBusVolumes();
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefMusicVol, _musicVolume);
                PlayerPrefs.Save();
                ApplyBusVolumes();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefSfxVol, _sfxVolume);
                PlayerPrefs.Save();
                ApplyBusVolumes();
            }
        }

        public float AmbienceVolume
        {
            get => _ambienceVolume;
            set
            {
                _ambienceVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PrefAmbienceVol, _ambienceVolume);
                PlayerPrefs.Save();
                ApplyBusVolumes();
            }
        }

        public static float DbToLinear(float dB)
        {
            return dB <= -80f ? 0f : Mathf.Pow(10f, dB / 20f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Destroy only the duplicate AudioManager component or child object, never the entire GameManager root
                if (GetComponent<GameManager>() != null)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            LoadSavedPreferences();
            SetupAudioSourcesSingleLifecycle();
            ApplyBusVolumes();
        }

        private void LoadSavedPreferences()
        {
            _masterVolume = PlayerPrefs.GetFloat(PrefMasterVol, 0.90f);
            _musicVolume = PlayerPrefs.GetFloat(PrefMusicVol, 1.00f);
            _sfxVolume = PlayerPrefs.GetFloat(PrefSfxVol, 0.85f);
            _ambienceVolume = PlayerPrefs.GetFloat(PrefAmbienceVol, 0.80f);
            AudioListener.volume = _masterVolume;
        }

        private void SetupAudioSourcesSingleLifecycle()
        {
            // Clean up any pre-existing duplicate AudioSources on this GameObject
            AudioSource[] existing = GetComponents<AudioSource>();
            for (int i = 0; i < existing.Length; i++)
            {
                if (Application.isPlaying) Destroy(existing[i]);
                else DestroyImmediate(existing[i]);
            }

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.priority = 32; // Highest priority for background music

            _crackleSource = gameObject.AddComponent<AudioSource>();
            _crackleSource.loop = true;
            _crackleSource.playOnAwake = false;
            _crackleSource.priority = 180; // Secondary low priority for subtle fire crackle

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.priority = 96;
        }

        private float _endingMusicScale = 1f;
        private float _endingSfxScale = 1f;
        private float _pauseMusicScale = 1f;
        private float _pauseSfxScale = 1f;

        public void ApplyBusVolumes()
        {
            float musicLinear = HeadroomScale * DbToLinear(MusicBusDb) * _musicVolume * _endingMusicScale * _pauseMusicScale;
            float sfxLinear = HeadroomScale * DbToLinear(SfxBusDb) * _sfxVolume * _endingSfxScale;
            // Fire crackle is -18 dB below Music reference, modulated by Ambience & SFX settings, torch proximity, and pause state
            float fireLinear = HeadroomScale * DbToLinear(FireRelativeToMusicDb) * _ambienceVolume * _sfxVolume * _torchProximityFactor * _endingSfxScale * _pauseSfxScale;

            if (_musicSource != null)
            {
                _musicSource.volume = Mathf.Clamp(musicLinear, 0f, 0.85f);
            }
            if (_crackleSource != null)
            {
                // Cap fire crackle strictly so it remains a subtle background warmth (~0.055 max)
                _crackleSource.volume = Mathf.Clamp(fireLinear, 0f, 0.085f);
            }
            if (_sfxSource != null)
            {
                _sfxSource.volume = Mathf.Clamp(sfxLinear, 0f, 0.65f);
            }
        }

        public void SetPauseDucking(bool isPaused)
        {
            _pauseMusicScale = isPaused ? 0.50f : 1f;
            _pauseSfxScale = isPaused ? 0f : 1f;
            ApplyBusVolumes();
        }

        public float PauseMusicScale => _pauseMusicScale;
        public float PauseSfxScale => _pauseSfxScale;

        public void SetEndingMixDucking(float musicScale, float sfxAndFireScale)
        {
            _endingMusicScale = Mathf.Clamp01(musicScale);
            _endingSfxScale = Mathf.Clamp01(sfxAndFireScale);
            ApplyBusVolumes();
        }

        public void RestoreNormalMix()
        {
            _endingMusicScale = 1f;
            _endingSfxScale = 1f;
            _pauseMusicScale = 1f;
            _pauseSfxScale = 1f;
            ApplyBusVolumes();
        }

        private void Start()
        {
            StartAmbience();
        }

        public void StartAmbience(string musicTrackPath = null)
        {
            if (_musicSource == null || _crackleSource == null)
            {
                SetupAudioSourcesSingleLifecycle();
            }

            if (string.IsNullOrEmpty(musicTrackPath))
            {
                musicTrackPath = "Music/dark_ambient_theme";
            }

            AudioClip music = AssetRegistry.GetAudio(musicTrackPath) ?? AssetRegistry.GetAudio("Music/dark_ambient_theme");
            if (music != null)
            {
                if (_musicSource.clip != music)
                {
                    _musicSource.clip = music;
                    _musicSource.Play();
                }
                else if (!_musicSource.isPlaying)
                {
                    _musicSource.Play();
                }
            }

            AudioClip crackle = AssetRegistry.GetAudio("SFX/fire_crackle_loop");
            if (crackle != null)
            {
                if (_crackleSource.clip != crackle)
                {
                    _crackleSource.clip = crackle;
                }
                if (!_crackleSource.isPlaying)
                {
                    _crackleSource.Play();
                }
            }

            ApplyBusVolumes();
        }

        public void SetTorchCrackleIntensity(float normalizedIntensity)
        {
            _torchProximityFactor = Mathf.Clamp(normalizedIntensity, 0.15f, 1.0f);
            ApplyBusVolumes();
        }

        public void PlaySFX(string sfxName, float volumeScale = 1f, float pitch = 1f)
        {
            if (_sfxSource == null) return;
            AudioClip clip = AssetRegistry.GetAudio("SFX/" + sfxName);
            if (clip != null)
            {
                // Attenuate torch_ignite slightly so repeated torch interactions remain warm and never harsh
                float clipTrim = sfxName == "torch_ignite" ? 0.65f : 1.0f;
                _sfxSource.pitch = Mathf.Clamp(pitch, 0.75f, 1.3f);
                _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale * clipTrim));
            }
        }
    }
}
