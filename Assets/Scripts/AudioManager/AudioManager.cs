using UnityEngine;

public class AudioManager
{
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    public AudioManager(GameObject host)
    {
        _bgmSource = host.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;

        _sfxSource = host.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
    }

    // BGM
    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        _bgmSource.clip = clip;
        _bgmSource.volume = volume;
        _bgmSource.Play();
    }
    public void StopBGM() => _bgmSource.Stop();
    public void PauseBGM() => _bgmSource.Pause();
    public void ResumeBGM() => _bgmSource.UnPause();
    public void SetBGMVolume(float vol) => _bgmSource.volume = vol;

    // SFX
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        _sfxSource.PlayOneShot(clip, volume);
    }
    public void SetSFXVolume(float vol) => _sfxSource.volume = vol;
}
