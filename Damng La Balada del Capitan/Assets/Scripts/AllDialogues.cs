using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AllDialogues : MonoBehaviour
{
    public enum Dialogue
    {
        mainIntro,
        level1
    }

    [SerializeField] List<Sprite> backgroundImages;
    [SerializeField] List<Sprite> characterImages;
    [SerializeField] List<string> texts;

    public List<DialogueItem> GetDialogue(Dialogue dialogue)
    {
        List<DialogueItem> list = new List<DialogueItem>();
        switch (dialogue)
        {
            case Dialogue.mainIntro:
                list.Add(new DialogueItem(backgroundImages[0], 
                    characterImages[2], 
                    texts[0]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[1]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[2]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[3]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[4]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[5]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[6]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[7]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[8]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[9]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[10]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[11]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[12]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[13]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[14]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[15]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[16]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[17]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[18]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[19]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[20]));
                break;
            case Dialogue.level1:
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[0]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[1]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[1],
                    texts[2]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[0],
                    texts[3]));
                list.Add(new DialogueItem(backgroundImages[0],
                    characterImages[2],
                    texts[4]));
                break;
        }
        return list;
    }
}
