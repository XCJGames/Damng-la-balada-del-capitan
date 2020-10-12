using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Image character;
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] AllDialogues.Dialogue thisDialogue;

    private List<DialogueItem> dialogueList;
    private AllDialogues dialogues;
    private int dialogueIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        dialogues = GetComponent<AllDialogues>();
        dialogueList = dialogues.GetDialogue(thisDialogue);
        UpdateUI(dialogueIndex);
        Music();
    }

    private void Music()
    {
        switch (thisDialogue)
        {
            case AllDialogues.Dialogue.mainIntro:
                FindObjectOfType<MusicPlayer>().PlayTheme(MusicPlayer.Theme.mainIntro);
                break;
            case AllDialogues.Dialogue.level1:
                FindObjectOfType<MusicPlayer>().PlayTheme(MusicPlayer.Theme.level1Intro);
                break;
        }
    }

    private void UpdateUI(int index)
    {
        background.sprite = dialogueList[index].Background;
        character.sprite = dialogueList[index].Character;
        text.text = dialogueList[index].Text;
        dialogueIndex++;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            NextText();
        }
    }

    private void NextText()
    {
        if(dialogueIndex < dialogueList.Count)
        {
            UpdateUI(dialogueIndex);
        }
        else
        {
            FindObjectOfType<LevelController>().LoadNextScene();
        }
    }
}
