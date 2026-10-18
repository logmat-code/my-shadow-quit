using UnityEngine;

namespace MyShadowQuit.Core
{
    public enum AudioMusicState
    {
        None,
        Menu,
        Gameplay_World1,
        Gameplay_World2,
        Gameplay_World3,
        Paused
    }

    /// <summary>
    /// Manages original procedural dark ambient music, torch crackling loop, and gameplay sound effects.
    /// Implements a state-based music architecture separating Menu from World Gameplay (Worlds 1, 2, 3),
    /// with unscaled crossfading, uninterrupted menu navigation, and a dB-calibrated 4-bus mix architecture.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PrefMasterVol = "MSQ_MasterVolume";
        private const string PrefMusicVol = "MSQ_MusicVolume";
        private const string PrefSfxVol = "MSQ_SfxVolume";
        private const string PrefAmbienceVol = "MSQ_AmbienceVolume";

        // Calibrated Reference Mix Bus Gains (dB -> Linear)
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

        private AudioMusicState _currentMusicState = AudioMusicState.None;
        private Coroutine _fadeCoroutine;
        private float _musicFadeMultiplier = 1f;

        public AudioMusicState CurrentMusicState => _currentMusicState;

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
            float musicLinear = HeadroomScale * DbToLinear(MusicBusDb) * _musicVolume * _endingMusicScale * _pauseMusicScale * _musicFadeMultiplier;
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
            // Default to menu music on initialization if started at MainMenu
            if (_currentMusicState == AudioMusicState.None)
            {
                PlayMenuMusic(fadeDuration: 0.5f);
            }
        }

        /// <summary>
        /// Plays or maintains the dark fantasy Menu Theme.
        /// Does NOT restart the track or recreate AudioSources when navigating between HOME, SHOP, WORLD SELECT, etc.
        /// </summary>
        public void PlayMenuMusic(float fadeDuration = 1.0f)
        {
            if (_currentMusicState == AudioMusicState.Menu && _musicSource != null && _musicSource.isPlaying && _musicSource.clip != null && _musicSource.clip.name == "menu_theme")
            {
                return;
            }

            TransitionToTrack("Music/menu_theme", AudioMusicState.Menu, fadeDuration, playFireCrackle: false);
        }

        /// <summary>
        /// Smoothly transitions to the distinct gameplay ambient music for the specified world (0: World 1, 1: World 2, 2: World 3).
        /// World 1: The Ruins of the Last Flame ("Music/dark_ambient_theme")
        /// World 2: The Inverted Forest ("Music/forest_ambient_theme")
        /// World 3: The Hollow Clocktower ("Music/clocktower_ambient_theme")
        /// </summary>
        public void PlayGameplayMusic(int worldIndex, float fadeDuration = 1.0f)
        {
            AudioMusicState targetState;
            string trackPath;

            switch (worldIndex)
            {
                case 1:
                    targetState = AudioMusicState.Gameplay_World2;
                    trackPath = "Music/forest_ambient_theme";
                    break;
                case 2:
                    targetState = AudioMusicState.Gameplay_World3;
                    trackPath = "Music/clocktower_ambient_theme";
                    break;
                default:
                    targetState = AudioMusicState.Gameplay_World1;
                    trackPath = "Music/dark_ambient_theme";
                    break;
            }

            if (_currentMusicState == targetState && _musicSource != null && _musicSource.isPlaying)
            {
                return;
            }

            TransitionToTrack(trackPath, targetState, fadeDuration, playFireCrackle: true);
        }

        private void TransitionToTrack(string trackPath, AudioMusicState newState, float fadeDuration, bool playFireCrackle)
        {
            if (_musicSource == null || _crackleSource == null)
            {
                SetupAudioSourcesSingleLifecycle();
            }

            _currentMusicState = newState;

            if (!playFireCrackle && _crackleSource != null && _crackleSource.isPlaying)
            {
                _crackleSource.Stop();
            }
            else if (playFireCrackle && _crackleSource != null)
            {
                AudioClip crackle = AssetRegistry.GetAudio("SFX/fire_crackle_loop");
                if (crackle != null)
                {
                    if (_crackleSource.clip != crackle) _crackleSource.clip = crackle;
                    if (!_crackleSource.isPlaying) _crackleSource.Play();
                }
            }

            AudioClip nextClip = AssetRegistry.GetAudio(trackPath) ?? AssetRegistry.GetAudio("Music/dark_ambient_theme");

            if (!Application.isPlaying)
            {
                _musicSource.clip = nextClip;
                _musicFadeMultiplier = 1f;
                ApplyBusVolumes();
                return;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeToTrackCoroutine(nextClip, fadeDuration));
        }

        private System.Collections.IEnumerator FadeToTrackCoroutine(AudioClip targetClip, float duration)
        {
            float halfDuration = Mathf.Max(0.12f, duration * 0.5f);

            // Phase 1: Fade out current track if active
            if (_musicSource.isPlaying && _musicSource.clip != null)
            {
                float elapsed = 0f;
                float startMult = _musicFadeMultiplier;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicFadeMultiplier = Mathf.Lerp(startMult, 0f, elapsed / halfDuration);
                    ApplyBusVolumes();
                    yield return null;
                }
            }

            _musicFadeMultiplier = 0f;
            ApplyBusVolumes();

            // Phase 2: Switch clip and begin playing
            _musicSource.clip = targetClip;
            if (targetClip != null)
            {
                _musicSource.Play();
            }
            else
            {
                _musicSource.Stop();
            }

            // Phase 3: Fade in target track
            if (targetClip != null)
            {
                float elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicFadeMultiplier = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
                    ApplyBusVolumes();
                    yield return null;
                }
            }

            _musicFadeMultiplier = 1f;
            ApplyBusVolumes();
            _fadeCoroutine = null;
        }

        public void StartAmbience(string musicTrackPath = null)
        {
            if (string.IsNullOrEmpty(musicTrackPath) || musicTrackPath == "Music/menu_theme")
            {
                PlayMenuMusic(0.8f);
            }
            else if (musicTrackPath.Contains("forest"))
            {
                PlayGameplayMusic(1, 0.8f);
            }
            else if (musicTrackPath.Contains("clocktower"))
            {
                PlayGameplayMusic(2, 0.8f);
            }
            else
            {
                PlayGameplayMusic(0, 0.8f);
            }
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
