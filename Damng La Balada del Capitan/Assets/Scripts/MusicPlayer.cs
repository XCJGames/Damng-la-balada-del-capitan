using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip mainTheme;
    [SerializeField] AudioClip mainIntro;
    [SerializeField] AudioClip level1Intro;
    [SerializeField] AudioClip combatTheme;
    [SerializeField] AudioClip duelTheme;
    [SerializeField] AudioClip loseTheme;
    [SerializeField] AudioClip winTheme;

    public enum Theme
    {
        mainTheme,
        mainIntro,
        level1Intro,
        combatTheme,
        duelTheme,
        loseTheme,
        winTheme
    }

    private Theme currentTheme;

    AudioSource audioSource;

    public Theme CurrentTheme { get => currentTheme; set => currentTheme = value; }

    // Start is called before the first frame update
    void Start()
    {
        SetUpSingleton();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = mainTheme;
        audioSource.Play();
        if (PlayerPrefsController.CheckIfPrefsExist())
        {
            audioSource.volume = PlayerPrefsController.GetMasterVolume();
        }
        else
        {
            audioSource.volume = 0.8f;
            PlayerPrefsController.SetMasterVolume(0.8f);
        }
    }

    private void SetUpSingleton()
    {
        int numberMusicPlayers = FindObjectsOfType<MusicPlayer>().Length;
        if (numberMusicPlayers > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void PlayTheme(Theme theme)
    {
        switch (theme)
        {
            case Theme.mainTheme:
                audioSource.clip = mainTheme;
                currentTheme = Theme.mainTheme;
                break;
            case Theme.mainIntro:
                audioSource.clip = mainIntro;
                currentTheme = Theme.mainIntro;
                break;
            case Theme.combatTheme:
                audioSource.clip = combatTheme;
                currentTheme = Theme.combatTheme;
                break;
            case Theme.duelTheme:
                audioSource.clip = duelTheme;
                currentTheme = Theme.duelTheme;
                break;
            case Theme.winTheme:
                audioSource.clip = winTheme;
                currentTheme = Theme.winTheme;
                break;
            case Theme.loseTheme:
                audioSource.clip = loseTheme;
                currentTheme = Theme.loseTheme;
                break;
            case Theme.level1Intro:
                audioSource.clip = level1Intro;
                currentTheme = Theme.level1Intro;
                break;
        }
        audioSource.Play();
    }
}
