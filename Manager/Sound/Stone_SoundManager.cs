using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone_SoundManager : Stone_Manager
{
    private const float PlayTweenTime = 3f; //过渡时间（单位：S）
    private const float ChangeTweenTime = 4f; //过渡时间（单位：S）
    private const float StopTweenTime = 5f; //过渡时间（单位：S）

    public const string Name = "Stone_SoundManager";
    public override string GetName()
    {
        return Stone_SoundManager.Name;
    }

    private string m_ResourcePath;
    private string m_SoundPrefabPath;

    private Transform m_Root;
    private AudioSource m_BGMAudioSource;

    private string m_BGMSoundName;

    private int m_PlayTimer;
    private int m_StopTimer;

    private List<AudioSource> m_EffectAudioSourceList;
    private List<AudioSource> m_EffectAudioSourcePool;
    private List<int> m_EffectTimerList;

    private Stone_ResourceManager ResourceManager;
    private Stone_TimerManager TimerManager;
    public Stone_SoundManager(Stone_IManagerLifeControl stone_ManagerLifeControl) : base(stone_ManagerLifeControl)
    {
    }

    public override void Init()
    {
        GameObject root = new GameObject();
        root.name = "Sound";
        m_Root = root.transform;
        GameObject.DontDestroyOnLoad(m_Root);

        ResourceManager = Stone_RunTime.GetManager<Stone_ResourceManager>(Stone_ResourceManager.Name);
        TimerManager = Stone_RunTime.GetManager<Stone_TimerManager>(Stone_TimerManager.Name);

        m_PlayTimer = -1;
        m_StopTimer = -1;

        m_EffectAudioSourceList = new List<AudioSource>();
        m_EffectAudioSourcePool = new List<AudioSource>();
        m_EffectTimerList = new List<int>();
    }

    public override void UnInit()
    {
        if (m_PlayTimer != -1)
        {
            TimerManager.StopTimer(m_PlayTimer);
        }
        if (m_StopTimer != -1)
        {
            TimerManager.StopTimer(m_StopTimer);
        }
        for(int index = 0;index< m_EffectTimerList.Count;index++)
        {
            TimerManager.StopTimer(m_EffectTimerList[index]);
        }

        ResourceManager = null;
        TimerManager = null;
    }

    public void SetResourcePath(string resourcePath)
    {
        m_ResourcePath = resourcePath;
    }

    public void SetSoundPrefabPath(string path)
    {
        m_SoundPrefabPath = path;
    }

    public void PlayBGM(string soundName)
    {
        if (m_BGMSoundName == soundName)
        {
            return;
        }

        if (m_BGMAudioSource == null)
        {
            GameObject audioSoundGameObject = ResourceManager.Instance(m_SoundPrefabPath);
            audioSoundGameObject.name = "BGM";
            audioSoundGameObject.transform.SetParent(m_Root);
            m_BGMAudioSource = audioSoundGameObject.GetComponent<AudioSource>();
            m_BGMAudioSource.loop = true;
        }

        m_BGMSoundName = soundName;

        AudioClip audioClip = ResourceManager.LoadResource<AudioClip>(soundName, m_ResourcePath);

        if (m_BGMAudioSource.clip == null)
        {
            PlayNewSound();
        }
        else
        {
            ChangeSound();
        }

        m_BGMAudioSource.clip = audioClip;
        m_BGMAudioSource.Play();
    }

    public void StopBGM()
    {
        StopSound();
    }

    public void PlayEffectSound(string effectSoundName, float volume = 1)
    {
        AudioSource audioSource;

        if (m_EffectAudioSourcePool.Count == 0)
        {
            GameObject audioSoundGameObject = ResourceManager.Instance(m_SoundPrefabPath);
            audioSoundGameObject.name = "Effect";
            audioSoundGameObject.transform.SetParent(m_Root);
            audioSource = audioSoundGameObject.GetComponent<AudioSource>();
        }
        else
        {
            audioSource = m_EffectAudioSourcePool[0];
            m_EffectAudioSourcePool.RemoveAt(0);
        }

        AudioClip audioClip = ResourceManager.LoadResource<AudioClip>(effectSoundName, m_ResourcePath);
        audioSource.clip = audioClip;
        audioSource.volume = volume;

        audioSource.Play();

        int timer = 0;
        timer = TimerManager.StarTimer(null, (isError) =>
         {
             m_EffectAudioSourceList.Remove(audioSource);
             m_EffectTimerList.Remove(timer);

             m_EffectAudioSourcePool.Add(audioSource);
         }, updateTime: audioClip.length);

        m_EffectAudioSourceList.Add(audioSource);
        m_EffectTimerList.Add(timer);
    }

    private void PlayNewSound()
    {
        if(m_PlayTimer!=-1)
        {
            TimerManager.StopTimer(m_PlayTimer);
        }
        if (m_StopTimer != -1)
        {
            TimerManager.StopTimer(m_StopTimer);
        }

        float tweenVolume = 1 / PlayTweenTime;

        m_BGMAudioSource.volume = 0;

        m_PlayTimer = TimerManager.StarTimer(() =>
        {
            m_BGMAudioSource.volume += tweenVolume;
        }, updateTime: PlayTweenTime);
    }

    private void ChangeSound()
    {
        if (m_PlayTimer != -1)
        {
            TimerManager.StopTimer(m_PlayTimer);
        }
        if (m_StopTimer != -1)
        {
            TimerManager.StopTimer(m_StopTimer);
        }

        float tweenVolume = 1 / ChangeTweenTime;
        float updateCount = 0;

        m_PlayTimer = TimerManager.StarTimer(() =>
        {
            updateCount += Time.deltaTime;

            if (updateCount <= ChangeTweenTime)
            {
                m_BGMAudioSource.volume -= tweenVolume;
            }
            else
            {
                m_BGMAudioSource.volume += tweenVolume;
            }
        }, updateTime: ChangeTweenTime * 2);
    }

    private void StopSound()
    {
        if (m_PlayTimer != -1)
        {
            TimerManager.StopTimer(m_PlayTimer);
        }
        if (m_StopTimer != -1)
        {
            TimerManager.StopTimer(m_StopTimer);
        }

        float tweenVolume = 1 / PlayTweenTime;

        m_StopTimer = TimerManager.StarTimer(() =>
        {
            m_BGMAudioSource.volume -= tweenVolume;
        }, updateTime: StopTweenTime);

    }
}
