using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CharacterUnit : MonoBehaviour
{
    public enum Type
    {
        shooter,
        cqc
    }

    [SerializeField] bool isPlayerControlled = true;
    [SerializeField] int health = 150;
    [SerializeField] int currentHealth = 150;
    [SerializeField] int actionPoints = 10;
    [SerializeField] int currentAP = 10;
    [SerializeField] int attackRange = 15;
    [SerializeField] int attackDamage = 30;
    [SerializeField] int attackCost = 3;
    [SerializeField] int baseHitChance = 70;
    [SerializeField] int specialAttackCost = 6;
    [SerializeField] int initiative = 10;
    [SerializeField] int currentInitiative = 10;
    [SerializeField] int movementCost = 1;
    [SerializeField] int defenseModifierWithCover = 0;
    [SerializeField] int defenseModifierWithoutCover = 0;
    // Add special attack gameObject (script)

    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI currentHealthText;
    [SerializeField] TextMeshProUGUI maxHealthText;
    [SerializeField] Image hitChanceBackground;
    [SerializeField] TextMeshProUGUI hitChanceText;

    [SerializeField] Sprite attackSprite;
    [SerializeField] Sprite defendSprite;
    [SerializeField] Sprite specialAttackSprite;
    [SerializeField] AudioClip attackSFX;
    [SerializeField] AudioClip hurtSFX;

    [SerializeField] Type type;

    private bool isAlive = true;
    private bool isDisplayingInfo = false;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        RefreshHUD();
        isAlive = true;
    }

    private void RefreshHUD()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = health;
            healthSlider.value = currentHealth;
            currentHealthText.text = currentHealth.ToString();
            maxHealthText.text = health.ToString();
        }
    }

    public enum State
    {
        inactive,
        active,
        dead
    }

    private State state = State.inactive;

    public Sprite SpecialAttackSprite { get => specialAttackSprite; set => specialAttackSprite = value; }
    public Sprite AttackSprite { get => attackSprite; set => attackSprite = value; }
    public Sprite DefendSprite { get => defendSprite; set => defendSprite = value; }
    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public AudioClip AttackSFX { get => attackSFX; set => attackSFX = value; }
    public AudioClip HurtSFX { get => hurtSFX; set => hurtSFX = value; }

    public int GetHealth() => health;

    public int GetCurrentHealth() => currentHealth;

    public int GetInitiative() => initiative;

    public int GetCurrentInitiative() => currentInitiative;

    public bool IsPlayerControlled() => isPlayerControlled;

    public int GetCurrentAP() => currentAP;

    public int GetMovementCost() => movementCost;

    public int GetAttackRange() => attackRange;

    public int GetAttackCost() => attackCost;

    public int GetAttackDamage() => attackDamage;

    public int GetBaseHitChance()
    {
        if (isPlayerControlled)
        {
            return baseHitChance - (int)(PlayerPrefsController.GetDifficulty() * 10);
        }
        else
        {
            return baseHitChance + (int)(PlayerPrefsController.GetDifficulty() * 10);
        }
    }

    public int GetDefenseModifierWithCover() => defenseModifierWithCover;

    public int GetDefenseModifierWithoutCover() => defenseModifierWithoutCover;

    public void RemoveCurrentHealth(int damage)
    {
        currentHealth -= damage;
        RefreshHUD();
    }

    public void RestoreCurrentHealth(int heal)
    {
        currentHealth = Mathf.Min(health, currentHealth + heal);
        RefreshHUD();
    }

    public void SetState(State state)
    {
        switch (state)
        {
            case State.active:
                animator.SetBool("Idle", true);
                break;
            case State.inactive:
                animator.SetBool("Idle", false);
                break;
            case State.dead:
                animator.SetBool("Down", true);
                healthSlider.gameObject.SetActive(false);
                isAlive = false;
                GetComponent<SpriteRenderer>().sortingLayerName = "Character Down";
                break;
        }
        this.state = state;
    }

    internal int GetMaxAP()
    {
        return actionPoints;
    }

    internal void HandleAI()
    {
        if (type == Type.shooter)
        {
            GetComponent<AIShooter>().HandleTurn();
        }
        else
        {
            GetComponent<AICqc>().HandleTurn();
        }
    }

    public void SetAIGrid(Grid<GridCell> grid)
    {
        if (type == Type.shooter)
        {
            GetComponent<AIShooter>().Grid = grid;
        }
        else
        {
            GetComponent<AICqc>().Grid = grid;
        }
    }

    public void RemoveActionPoints(int actionPoints) => currentAP -= actionPoints;

    public void RestoreCurrentAP(int refund)
    {
        currentAP = Mathf.Min(actionPoints, currentAP + refund);
        RefreshHUD();
    }

    public void SetCurrentAP(int currentAP) => this.currentAP = currentAP;

    public void SetCurrentInitiative(int currentInitiative) => this.currentInitiative = currentInitiative;

    private void OnMouseEnter()
    {
        Debug.Log("on mouse enter");
        if (CombatSystem.Instance.IsPlayerTurn)
        {
            if (isAlive && !isPlayerControlled && CombatSystem.Instance.IsValidTarget(this))
            {
                hitChanceBackground.gameObject.SetActive(true);
                int hitChance = Mathf.Clamp(CombatSystem.Instance.GetHitChanceAgainstCharacter(this), 5, 95);
                hitChanceText.text = hitChance.ToString();
                isDisplayingInfo = true;
            }
        }
    }
    private void OnMouseExit()
    {
        Debug.Log("on mouse exit");
        if (isDisplayingInfo)
        {
            hitChanceBackground.gameObject.SetActive(false);
        }
    }

    internal void ResetCommands()
    {
        if (type == Type.shooter)
        {
            GetComponent<AIShooter>().ResetCommands();
        }
        else
        {
            GetComponent<AICqc>().ResetCommands();
        }
    }
}

