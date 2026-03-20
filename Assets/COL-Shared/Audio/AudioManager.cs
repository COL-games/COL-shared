using UnityEngine;

namespace COLShared.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string MusicVolumeKey = "AudioManager_MusicVolume";
        private const string SFXVolumeKey = "AudioManager_SFXVolume";
        private const string MusicMuteKey = "AudioManager_MusicMute";
        private const string SFXMuteKey = "AudioManager_SFXMute";

        private AudioSource musicSource;
        private AudioSource sfxSource;

        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private bool musicMuted = false;
        private bool sfxMuted = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudioSources();
            LoadSettings();
        }

        private void InitAudioSources()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicMuted ? 0f : musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxMuted ? 0f : sfxVolume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicMuted ? 0f : musicVolume;
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxMuted ? 0f : sfxVolume;
            PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolume);
            PlayerPrefs.Save();
        }

        public void MuteMusic()
        {
            musicMuted = !musicMuted;
            musicSource.volume = musicMuted ? 0f : musicVolume;
            PlayerPrefs.SetInt(MusicMuteKey, musicMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void MuteSFX()
        {
            sfxMuted = !sfxMuted;
            sfxSource.volume = sfxMuted ? 0f : sfxVolume;
            PlayerPrefs.SetInt(SFXMuteKey, sfxMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
            musicMuted = PlayerPrefs.GetInt(MusicMuteKey, 0) == 1;
            sfxMuted = PlayerPrefs.GetInt(SFXMuteKey, 0) == 1;
            musicSource.volume = musicMuted ? 0f : musicVolume;
            sfxSource.volume = sfxMuted ? 0f : sfxVolume;
        }
    }
}