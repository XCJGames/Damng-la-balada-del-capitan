using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueItem
{
    private Sprite background;
    private Sprite character;
    private string text;

    public DialogueItem(Sprite background, Sprite character, string text)
    {
        this.background = background;
        this.character = character;
        this.text = text;
    }

    public Sprite Background { get => background; set => background = value; }
    public Sprite Character { get => character; set => character = value; }
    public string Text { get => text; set => text = value; }
}
