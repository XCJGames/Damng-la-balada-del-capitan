using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [SerializeField] Slider volumeSlider, difficultySlider;
    [SerializeField] float defaultVolume = 0.8f;
    [SerializeField] float defaultDifficulty = 0;
    [SerializeField] MusicPlayer musicPlayer;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefsController.CheckIfPrefsExist())
        {
            volumeSlider.value = PlayerPrefsController.GetMasterVolume();
            difficultySlider.value = PlayerPrefsController.GetDifficulty();
        }
        else
        {
            volumeSlider.value = defaultVolume;
            difficultySlider.value = defaultDifficulty;
        }
        musicPlayer = FindObjectOfType<MusicPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (musicPlayer)
        {
            musicPlayer.SetVolume(volumeSlider.value);
        }
        else
        {
            Debug.LogWarning("No music player found");
        }
    }

    public void SetDefaults()
    {
        volumeSlider.value = defaultVolume;
        difficultySlider.value = defaultDifficulty;
    }

    public void SaveAndExit()
    {
        PlayerPrefsController.SetMasterVolume(volumeSlider.value);
        PlayerPrefsController.SetDifficulty(difficultySlider.value);
        FindObjectOfType<LevelController>().LoadMainMenu();
    }
}
