using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;

    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] float fadeDuration = 0.75f;

    public static AudioManager i { get; private set; }

    AudioClip currMusic;

    /// <summary>
    /// 原來的音量
    /// </summary>
    float originalMusicVol;

    Dictionary<AudioId, AudioData> sfxLookup;

    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        originalMusicVol = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip">音效</param>
    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null) return;

        if (pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }
        sfxPlayer.PlayOneShot(clip);
    }
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioid">id</param>
    public void PlaySfx(AudioId audioid, bool pauseMusic = false)
    {
        if (!sfxLookup.ContainsKey(audioid)) return;

        var audioData = sfxLookup[audioid];
        PlaySfx(audioData.clip, pauseMusic);
    }


    /// <summary>
    /// 播背景Muisc
    /// </summary>
    /// <param name="clip">聲音</param>
    /// <param name="loop">是否循環</param>
    /// <param name="fade">是否衰減</param>
    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if (clip == null || clip == currMusic) return;

        currMusic = clip;

        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        //衰減
        if (fade)
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        //正常播放
        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();
        //還原
        if (fade)
            yield return musicPlayer.DOFade(originalMusicVol, fadeDuration).WaitForCompletion();
    }


    /// <summary>
    /// 等待音效播放 重Set播放BGM
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVol, fadeDuration);
    }

}


public enum AudioId { UISelect, Hit, Faint, ExpGain, ItemObtained, PokemonObtained }

[System.Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
