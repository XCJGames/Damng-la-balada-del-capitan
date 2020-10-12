using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using System;

public class CombatUIController : MonoBehaviour
{
    [Header("Left Menu")]
    [SerializeField] Image portrait;
    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider actionPointsBar;
    [SerializeField] TextMeshProUGUI actionPointsText;
    [SerializeField] TextMeshProUGUI movementRange;
    [SerializeField] TextMeshProUGUI attackRange;
    [SerializeField] TextMeshProUGUI attackCost;
    [SerializeField] TextMeshProUGUI attackDamage;
    [SerializeField] float intermitenceSpeed = 0.1f;

    [Header("Top Menu")]
    [SerializeField] TextMeshProUGUI round;
    [SerializeField] Image[] characterImages;

    private List<CharacterUnit> characterOrder;

    public void UpdateCharacterUI(CharacterUnit character)
    {
        if (character.IsPlayerControlled())
        {
            if (!actionPointsBar.IsActive()) actionPointsBar.gameObject.SetActive(true);
            UpdateActionPointsBar(character);
        }
        else
        {
            if(actionPointsBar.IsActive()) actionPointsBar.gameObject.SetActive(false);
        }
        portrait.sprite = character.GetComponent<SpriteRenderer>().sprite;
        UpdateHealthBar(character);
        movementRange.text = character.GetMovementCost().ToString();
        attackRange.text = character.GetAttackRange().ToString();
        attackCost.text = character.GetAttackCost().ToString();
        attackDamage.text = character.GetAttackDamage().ToString();
    }

    public void UpdateHealthBar(CharacterUnit character)
    {
        healthBar.maxValue = character.GetHealth();
        healthBar.value = character.GetCurrentHealth();
        healthText.text = character.GetCurrentHealth().ToString();
    }

    public void UpdateActionPointsBar(CharacterUnit character)
    {
        actionPointsBar.maxValue = character.GetMaxAP();
        actionPointsBar.value = character.GetCurrentAP();
        actionPointsText.text = character.GetCurrentAP().ToString();
    }

    public void UpdateRoundCounter(int round)
    {
        this.round.text = round.ToString();
    }

    internal void NotEnoughActionPoints()
    {
        Image image = actionPointsBar.GetComponentInChildren<Image>();
        StartCoroutine(IntermitentColor(image, image.color, Color.red));
    }

    private IEnumerator IntermitentColor(Image image, Color primaryColor, Color secondaryColor)
    {
        for(int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(intermitenceSpeed);
            if (i % 2 == 0) image.color = secondaryColor;
            else image.color = primaryColor;
        }
        image.color = primaryColor;
    }

    public void SetCharacterOrder(List<CharacterUnit> characterOrder)
    {
        this.characterOrder = characterOrder;
        UpdateCharacterOrder(0);
    }

    public void UpdateCharacterOrder(int currentCharacter)
    {
        for(int i = 0; i < 5; i++)
        {
            if(currentCharacter + i < characterOrder.Count)
            {
                if(characterOrder[currentCharacter + i].IsAlive)
                {
                    if (!characterImages[i].IsActive())
                    {
                        characterImages[i].gameObject.SetActive(true);
                    }
                    characterImages[i].sprite =
                        characterOrder[currentCharacter + i].
                        GetComponent<SpriteRenderer>().sprite;
                }
                else
                {
                    i--;
                    currentCharacter++;
                }
            }
            else
            {
                if (characterImages[i].IsActive())
                {
                    characterImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void CenterCameraOnCharacter(int index)
    {
        Camera.main.transform.position =
            new Vector3(characterOrder[index + CombatSystem.Instance.CurrentCharacter].transform.position.x,
            characterOrder[index + CombatSystem.Instance.CurrentCharacter].transform.position.y,
            Camera.main.transform.position.z);
    }
}
