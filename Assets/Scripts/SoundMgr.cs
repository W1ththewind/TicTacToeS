using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;

public class SoundMgr : Singleton<SoundMgr>
{


    public SoundDatabase SoundDB;
    public AudioMixer Mixer;
    AudioSource _soundEffectAudio;
    AudioSource _currentMusicTrack, _musicAudioTrack1, _musicAudioTrack2;
    IEnumerator _fadeInCo, _fadeOutCo;
    HashSet<string> _justInvokeSound = new HashSet<string>();

    protected override void Awake()
    {
        base.Awake();
        if (!Destroyed)
        {
            // _musicAudioTrack1 = gameObject.AddComponent<AudioSource>();
            // _musicAudioTrack2 = gameObject.AddComponent<AudioSource>();
            _soundEffectAudio = gameObject.AddComponent<AudioSource>();

            // _musicAudioTrack1.outputAudioMixerGroup = Mixer.FindMatchingGroups("Track1")[0];
            // _musicAudioTrack2.outputAudioMixerGroup = Mixer.FindMatchingGroups("Track2")[0];
            _soundEffectAudio.outputAudioMixerGroup = Mixer.FindMatchingGroups("SoundTrack")[0];
        }


    }


    public void PlaySoundEffect(AudioClip clip, float volume = 1.0f, float delayTime = 0.2f)
    {

        if (!_justInvokeSound.Contains(clip.name))
        {
            _soundEffectAudio.PlayOneShot(clip, volume);
            StartCoroutine(PlaySoundCoroutine(clip.name, delayTime));
        }
    }

    public void PlaySoundEffect(string clipName, float volume = 1.0f, float delayTime = 0.2f)
    {
        if (SettingMgr.Instance.Mute)
            return;

        AudioClip clip = SoundDB.Get(clipName);
        if (clip == null)
            return;
        if (!_justInvokeSound.Contains(clipName))
        {
            _soundEffectAudio.PlayOneShot(clip, volume);
            StartCoroutine(PlaySoundCoroutine(clipName, delayTime));
        }
    }

    IEnumerator PlaySoundCoroutine(string clipName, float delayTime = 0.2f)
    {
        _justInvokeSound.Add(clipName);
        yield return new WaitForSeconds(delayTime);
        _justInvokeSound.Remove(clipName);

    }

    public void PlayMusic(string clipName, bool fade = true, float volume = 1.0f, bool randomStart = false)
    {
        AudioClip clip = SoundDB.Get(clipName);
        if (clip == null)
            return;

        if (!fade)
        {
            if (_currentMusicTrack != null)
                _currentMusicTrack.Stop();
            _currentMusicTrack = _musicAudioTrack1;
            _musicAudioTrack1.loop = true;
            _musicAudioTrack1.clip = clip;
            _musicAudioTrack1.volume = volume;
            if (randomStart)
                _musicAudioTrack1.time = Random.Range(0, clip.length);

            _musicAudioTrack1.Play();
        }
        else
        {
            if (_currentMusicTrack != null)
            {
                _fadeOutCo = MusicFadeOutCoroutine();
                StartCoroutine(_fadeOutCo);
            }
            _fadeInCo = MusicFadeInCoroutine(clip, randomStart: randomStart);
            StartCoroutine(_fadeInCo);
        }
    }

    IEnumerator MusicFadeOutCoroutine(float fadeTime = 1f)
    {
        float timer = 0, initialVolume = _currentMusicTrack.volume;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            _currentMusicTrack.volume = Mathf.Lerp(initialVolume, 0, timer / fadeTime);
            yield return new WaitForEndOfFrame();
        }
        _currentMusicTrack.Stop();
    }

    IEnumerator MusicFadeInCoroutine(AudioClip clip, float volume = 1.0f, float fadeTime = 1f, bool randomStart = false)
    {
        yield return new WaitForSeconds(fadeTime);
        float timer = 0;
        AudioSource FadeInSource;
        if (_currentMusicTrack != null && _currentMusicTrack == _musicAudioTrack1)
            FadeInSource = _musicAudioTrack2;
        else
            FadeInSource = _musicAudioTrack1;

        FadeInSource.loop = true;
        FadeInSource.clip = clip;
        FadeInSource.volume = 0;
        if (randomStart)
            FadeInSource.time = Random.Range(0, clip.length);

        FadeInSource.Play();
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            FadeInSource.volume = Mathf.Lerp(0, volume, timer / fadeTime);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        _currentMusicTrack = FadeInSource;
    }

    public void StopMusic(bool fade = false)
    {
        StopAllCoroutines();
        if (_currentMusicTrack != null)
        {
            if (!fade)
            {
                _musicAudioTrack1.Stop();
                _musicAudioTrack2.Stop();
            }
            else
            {
                _fadeOutCo = MusicFadeOutCoroutine();
                StartCoroutine(_fadeOutCo);
            }
        }
    }



    // #if UNITY_EDITOR
    //     void OnDrawGizmos()
    //     {
    //         if (SoundDB == null)
    //         {
    //             SoundDB = AssetDatabase.LoadAssetAtPath<SoundDatabase>("Assets/Resources/Audio/SoundDB.asset");
    //             if (SoundDB == null)
    //             {
    //                 SoundDB = ScriptableObject.CreateInstance<SoundDatabase>();
    //                 AssetDatabase.CreateAsset(SoundDB, "Assets/Resources/Audio/SoundDB.asset");
    //                 EditorUtility.SetDirty(SoundDB);
    //                 AssetDatabase.SaveAssets();
    //             }
    //             EditorUtility.SetDirty(this);
    //             Debug.Log("Try to Find SoundDB");
    //         }
    //     }
    // #endif

}
