using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatCanvasController : MonoBehaviour
{
    [SerializeField] CombatSystem combatSystem;
    [SerializeField] Image attacker;
    [SerializeField] Image defender;
    [SerializeField] GameObject attackInfoWrapper;
    [SerializeField] TextMeshProUGUI hitChanceText;
    [SerializeField] TextMeshProUGUI rollText;
    [SerializeField] TextMeshProUGUI damageText;
    [SerializeField] float secondsToClose;

    private int hitChance;
    private int roll;
    private int damage;

    private AudioClip attackSFX;
    private AudioClip defendSFX;

    public int HitChance { get => hitChance; set => hitChance = value; }
    public int Roll { get => roll; set => roll = value; }
    public int Damage { get => damage; set => damage = value; }
    public float SecondsToClose { get => secondsToClose; set => secondsToClose = value; }

    public void StartAttackAnimations(CharacterUnit attackerUnit, CharacterUnit defenderUnit)
    {
        attacker.GetComponent<Image>().sprite = attackerUnit.AttackSprite;
        defender.GetComponent<Image>().sprite = defenderUnit.DefendSprite;
        attackSFX = attackerUnit.AttackSFX;
        defendSFX = defenderUnit.HurtSFX;
        GetComponent<Animator>().SetTrigger("Intro");
        /*AudioSource.PlayClipAtPoint(
            attackSFX,
            Camera.main.transform.position,
            PlayerPrefsController.GetMasterVolume());*/
    }

    public void ImagesIntroDone()
    {
        hitChanceText.text = hitChance.ToString();
        rollText.text = roll.ToString();
        damageText.text = damage.ToString();
        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(SecondsToClose);
        GetComponent<Animator>().SetTrigger("Outro");
    }

    public void EndAttack()
    {
        combatSystem.IsDisplayingAttack = false;
        if (!combatSystem.IsPlayerTurn) combatSystem.CheckAINextAction();
    }

    public void PlayAttackSFX()
    {
        AudioSource.PlayClipAtPoint(
            attackSFX,
            Camera.main.transform.position,
            PlayerPrefsController.GetMasterVolume());
    }

    public void PlayDefendSFX()
    {
        if(roll <= hitChance)
        {
            AudioSource.PlayClipAtPoint(
            defendSFX,
            Camera.main.transform.position,
            PlayerPrefsController.GetMasterVolume());
        }
    }
}
