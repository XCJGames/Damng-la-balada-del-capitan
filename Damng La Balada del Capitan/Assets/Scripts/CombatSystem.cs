using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class CombatSystem : MonoBehaviour
{
    public enum combatThemes
    {
        combat,
        duel,
        win,
        lose
    }

    [SerializeField] int round = 1;
    [SerializeField] List<CharacterUnit> characterOrder;
    [SerializeField] int currentCharacter = 0;
    [SerializeField] float moveSpeed = 10f;

    [SerializeField] Tilemap movementTilemap;
    [SerializeField] Tilemap targetTilemap;
    [SerializeField] Tile movementTile;
    [SerializeField] Tile targetTile;

    [SerializeField] CombatCanvasController combatCanvasController;
    [SerializeField] CombatUIController combatUIController;
    [SerializeField] Canvas endCombatCanvas;

    [SerializeField] AudioClip notEnoughPointsSFX;

    public static CombatSystem Instance { get; private set; }

    private Grid<GridCell> grid;

    // Cuando se selecciona una acción, marcamos a true. Sirve para comprobaciones en Update
    private bool isActionSelected = false;
    private bool isDisplayingAttack = false;

    private bool isPlayerTurn = false;

    private bool isMoving = false;
    private List<Vector2> movePath;
    private int currentNode;

    private bool combatFinished = false;

    public bool IsDisplayingAttack { get => isDisplayingAttack; set => isDisplayingAttack = value; }
    public bool IsPlayerTurn { get => isPlayerTurn; set => isPlayerTurn = value; }
    public int CurrentCharacter { get => currentCharacter; set => currentCharacter = value; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        ChangeTheme(combatThemes.combat);
        OrderCharacters();
        SetUpGrid();
        HandleTurn();
    }

    private void ChangeTheme(combatThemes combatTheme)
    {
        if(FindObjectOfType<MusicPlayer>() != null)
        {
            switch(combatTheme)
            {
                case combatThemes.combat:
                    FindObjectOfType<MusicPlayer>().PlayTheme(MusicPlayer.Theme.combatTheme);
                    break;
                case combatThemes.duel:
                    FindObjectOfType<MusicPlayer>().PlayTheme(MusicPlayer.Theme.duelTheme);
                    break;
                case combatThemes.win:
                    FindObjectOfType<MusicPlayer>().PlayTheme(MusicPlayer.Theme.winTheme);
                    break;
                case combatThemes.lose:
                    FindObjectOfType<MusicPlayer>().PlayTheme(MusicPlayer.Theme.loseTheme);
                    break;
            }
        }

    }

    private void SetUpGrid()
    {
        grid = new Grid<GridCell>(24, 16, 1f, Vector2.zero, (Grid<GridCell> g, int x, int y) => new GridCell(g, x, y));
        Pathfinding pathfinding = new Pathfinding(grid.GetWidth(), grid.GetHeight());

        GameObject obj = GameObject.FindGameObjectWithTag("Obstacle");
        Transform child;
        GridCell cell;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            child = obj.transform.GetChild(i);
            cell = grid.GetGridObject(new Vector2(child.transform.position.x,
                child.transform.position.y));
            cell.SetIsWalkable(false);
            cell.SetObstacle(child.GetComponent<ObstacleStats>());
            pathfinding.SetCellNotWalkable(new Vector2(child.transform.position.x,
                child.transform.position.y));
        }

        foreach(CharacterUnit character in characterOrder)
        {
            cell = grid.GetGridObject(new Vector2(character.transform.position.x,
                character.transform.position.y));
            cell.SetIsWalkable(false);
            cell.SetUnit(character);
            if (!character.IsPlayerControlled())
            {
                character.SetAIGrid(grid);
            }
        }
    }

    public IEnumerator UpdateUIAfterAttack(CharacterUnit attacker, CharacterUnit defender)
    {
        yield return new WaitForSeconds(combatCanvasController.SecondsToClose);
        defender.RemoveCurrentHealth(attacker.GetAttackDamage());
        if (defender.GetCurrentHealth() <= 0)
        {
            defender.SetState(CharacterUnit.State.dead);
            grid.GetGridObject(defender.transform.position).SetIsValidAttackTarget(false);
            grid.GetGridObject(defender.transform.position).SetIsWalkable(true);
            grid.GetGridObject(defender.transform.position).SetUnit(null);
            if(attacker.IsPlayerControlled()) FillAttackableCellsForCharacter(attacker);
            CheckEndCombatConditions();
        }
    }

    private void CheckEndCombatConditions()
    {
        int allies = 0, enemies = 0;
        foreach(CharacterUnit character in characterOrder)
        {
            if (character.IsAlive)
            {
                if (character.IsPlayerControlled())
                {
                    allies++;
                }
                else
                {
                    enemies++;
                }
            }
        }
        if(allies == 0)
        {
            LoseWindow();
            combatFinished = true;
        }
        else if(enemies == 0)
        {
            WinWindow();
            combatFinished = true;
        }
    }

    private void WinWindow()
    {
        endCombatCanvas.GetComponent<Animator>().SetBool("Win", true);
        ChangeTheme(combatThemes.win);
    }

    private void LoseWindow()
    {
        endCombatCanvas.GetComponent<Animator>().SetBool("Lose", true);
        ChangeTheme(combatThemes.lose);
    }

    private void OrderCharacters()
    {
        characterOrder.Clear();
        CharacterUnit[] characters = FindObjectsOfType<CharacterUnit>();
        foreach (CharacterUnit character in characters)
        {
            if (character.IsAlive)
            {
                characterOrder.Add(character);
            }
        }
        characterOrder.Sort((a, b) =>
        {
            if(b.GetCurrentInitiative().CompareTo(a.GetCurrentInitiative()) == 0)
            {
                if (b.IsPlayerControlled()) return 1;
                else if (a.IsPlayerControlled()) return -1;
            }
            return b.GetCurrentInitiative().CompareTo(a.GetCurrentInitiative());
        });
        combatUIController.SetCharacterOrder(characterOrder);
        foreach (CharacterUnit character in characterOrder) Debug.Log(character.GetCurrentInitiative());
    }

    private void HandleTurn()
    {
        if (!combatFinished)
        {
            CharacterUnit character = characterOrder[CurrentCharacter];
            Debug.Log("character " + character);
            Camera.main.transform.position = new Vector3(character.transform.position.x,
                character.transform.position.y, Camera.main.transform.position.z);
            character.SetState(CharacterUnit.State.active);
            character.SetCurrentAP(character.GetMaxAP());
            combatUIController.UpdateCharacterUI(character);
            combatUIController.UpdateRoundCounter(round);
            combatUIController.UpdateCharacterOrder(CurrentCharacter);
            if (character.IsAlive)
            {
                if (character.IsPlayerControlled())
                {
                    IsPlayerTurn = true;
                    // Modificamos UI para enseñar las habilidades del personaje
                    // Los posibles movimientos los calculamos cuando el usuario pincha en mover
                    // Vamos a hacerlo ahora de primeras para desarrollarlo y luego ya lo cambiaremos
                    FillWalkableCellsForCharacter(character);
                    FillAttackableCellsForCharacter(character);
                }
                else
                {
                    IsPlayerTurn = false;
                    // IA del personaje
                    character.HandleAI();
                    //NextTurn();
                }
            }
            else
            {
                NextTurn();
            }
        }
    }

    internal bool IsValidTarget(CharacterUnit characterUnit)
    {
        return grid.GetGridObject((int)characterUnit.transform.position.x,
                        (int)characterUnit.transform.position.y).IsValidAttackTarget();
    }

    public int GetHitChanceAgainstCharacter(CharacterUnit defender)
    {
        int cover = CheckCover(characterOrder[CurrentCharacter], defender);
        int hitChance = Mathf.Max(characterOrder[CurrentCharacter].GetBaseHitChance() - cover, 0);
        return hitChance;
    }

    private void FillWalkableCellsForCharacter(CharacterUnit character)
    {
        Pathfinding pathfinding = Pathfinding.Instance;
        movementTilemap.ClearAllTiles();
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                grid.GetGridObject(x, y).SetIsValidMovePosition(false);
            }
        }
        grid.GetXY(new Vector2(character.transform.position.x, character.transform.position.y)
            , out int unitX, out int unitY);
        int range = character.GetCurrentAP() / character.GetMovementCost();
        for(int x = unitX - range; x <= unitX + range; x++)
        {
            for(int y = unitY - range; y <= unitY + range; y++)
            {
                int xDistance = Mathf.Abs(unitX - x);
                int yDistance = Mathf.Abs(unitY - y);
                if (x >= 0 && x < grid.GetWidth() && y >= 0 && y < grid.GetHeight() &&
                    (xDistance + yDistance) <= range)
                {
                    if(grid.GetGridObject(x, y).IsWalkable())
                    {
                        List<Vector2> list = pathfinding.FindPath(new Vector2(unitX, unitY), new Vector2(x, y));
                        if(list != null && list.Count > 0 && list.Count - 1 <= range)
                        {
                            movementTilemap.SetTile(new Vector3Int(x, y, 0), movementTile);
                            grid.GetGridObject(x, y).SetIsValidMovePosition(true);
                        }
                    }
                }
            }
        }
    }

    private void FillAttackableCellsForCharacter(CharacterUnit character)
    {
        targetTilemap.ClearAllTiles();
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                grid.GetGridObject(x, y).SetIsValidAttackTarget(false);
            }
        }
        int range = character.GetAttackRange();
        grid.GetXY(new Vector2(character.transform.position.x, character.transform.position.y)
            , out int unitX, out int unitY);
        foreach(CharacterUnit target in characterOrder)
        {
            if (!target.IsPlayerControlled() && target.IsAlive)
            {
                int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, target.transform.position);
                if(distance <= range)
                {
                    targetTilemap.SetTile(new Vector3Int(
                        (int)target.transform.position.x, 
                        (int)target.transform.position.y, 0), targetTile);
                    grid.GetGridObject((int)target.transform.position.x,
                        (int)target.transform.position.y).SetIsValidAttackTarget(true);
                }
            }
        }
    }

    private int CalculateDistanceBetweenTwoPoints(int unitX, int unitY, Vector2 otherPos)
    {
        grid.GetXY(new Vector2(otherPos.x, otherPos.y), out int otherX, out int otherY);
        int xDistance = Mathf.Abs(unitX - otherX);
        int yDistance = Mathf.Abs(unitY - otherY);
        return xDistance + yDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDisplayingAttack)
        {
            if (isMoving)
            {
                Move();
            }
            else if (IsPlayerTurn)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    NextTurn();
                }
                if (Input.GetMouseButtonUp(1))
                {

                    Vector2 clickPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    Vector2 worldPos = Camera.main.ScreenToWorldPoint(clickPos);
                    GridCell cell = grid.GetGridObject(worldPos);
                    CharacterUnit character = characterOrder[CurrentCharacter];
                    if (cell.IsValidAttackTarget())
                    {
                        if (character.GetAttackCost() <= character.GetCurrentAP())
                        {
                            AttackCharacter(character, cell.GetUnit());
                        }
                        else
                        {
                            Debug.Log("not enough Action Points!");
                            combatUIController.NotEnoughActionPoints();
                            AudioSource.PlayClipAtPoint(
                            notEnoughPointsSFX,
                            Camera.main.transform.position,
                            PlayerPrefsController.GetMasterVolume());
                        }
                    }
                    else if (cell.IsValidMovePosition())
                    {
                        cell.SetIsWalkable(false);
                        cell.SetUnit(character);
                        grid.GetGridObject(character.transform.position).SetUnit(null);
                        grid.GetGridObject(character.transform.position).SetIsWalkable(true);
                        Pathfinding pathfinding = Pathfinding.Instance;
                        movePath = pathfinding.FindPath(
                            new Vector2(character.transform.position.x, character.transform.position.y),
                            new Vector2(cell.GetX(), cell.GetY()));
                        isMoving = true;
                        currentNode = 0;
                    }
                }
            }
        }
    }

    private void AttackCharacter(CharacterUnit attacker, CharacterUnit defender)
    {
        int cover = CheckCover(attacker, defender);
        int hitChance = Mathf.Max(attacker.GetBaseHitChance() - cover, 0);
        hitChance = Mathf.Clamp(hitChance, 5, 95);
        int roll = UnityEngine.Random.Range(1, 100);
        if(roll <= hitChance)
        {
            StartCoroutine(UpdateUIAfterAttack(attacker, defender));
        }
        attacker.RemoveActionPoints(attacker.GetAttackCost());
        combatUIController.UpdateActionPointsBar(attacker);
        combatCanvasController.HitChance = hitChance;
        combatCanvasController.Roll = roll;
        combatCanvasController.Damage = roll <= hitChance ? attacker.GetAttackDamage() : 0;
        IsDisplayingAttack = true;
        combatCanvasController.StartAttackAnimations(attacker, defender);
        CheckCurrentCharacterActionsRemaining();
    }

    private int CheckCover(CharacterUnit attacker, CharacterUnit defender)
    {
        int cover = 0;
        GridCell cell;
        if(attacker.transform.position.x < defender.transform.position.x)
        {
            cell = grid.GetGridObject(new Vector2(
                defender.transform.position.x - 1,
                defender.transform.position.y));
            if(cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        else
        {
            cell = grid.GetGridObject(new Vector2(
                defender.transform.position.x + 1,
                defender.transform.position.y));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        if(attacker.transform.position.y < defender.transform.position.y)
        {
            cell = grid.GetGridObject(new Vector2(
                defender.transform.position.x,
                defender.transform.position.y - 1));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        else
        {
            cell = grid.GetGridObject(new Vector2(
                defender.transform.position.x,
                defender.transform.position.y + 1));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        if (cover > 0) cover += defender.GetDefenseModifierWithCover();
        else cover += defender.GetDefenseModifierWithoutCover();
        return cover;
    }

    private void Move()
    {
        Vector2 currentPos = new Vector2(characterOrder[CurrentCharacter].transform.position.x,
            characterOrder[CurrentCharacter].transform.position.y);
        if(currentPos == movePath[currentNode])
        {
            currentNode++;
        }
        if(currentNode >= movePath.Count)
        {
            isMoving = false;
            characterOrder[CurrentCharacter].RemoveActionPoints(movePath.Count - 1);
            combatUIController.UpdateActionPointsBar(characterOrder[CurrentCharacter]);
            CheckCurrentCharacterActionsRemaining();
        }
        else
        {
            Vector2 targetPosition = movePath[currentNode];
            var moveStep = moveSpeed * Time.deltaTime;
            characterOrder[CurrentCharacter].transform.position = Vector2.MoveTowards(currentPos, targetPosition, moveStep);
        }
    }

    private void CheckCurrentCharacterActionsRemaining()
    {
        if (characterOrder[CurrentCharacter].GetCurrentAP() > 0)
        {
            if (characterOrder[CurrentCharacter].IsPlayerControlled())
            {
                FillWalkableCellsForCharacter(characterOrder[CurrentCharacter]);
                FillAttackableCellsForCharacter(characterOrder[CurrentCharacter]);
            }
            else
            {
                characterOrder[CurrentCharacter].HandleAI();
            }
        }
        else NextTurn();
    }

    public void CheckAINextAction()
    {
        //characterOrder[CurrentCharacter].HandleAI();
        characterOrder[currentCharacter].ResetCommands();
        NextTurn();
    }

    public void NextTurnFromButton()
    {
        if (isPlayerTurn) NextTurn();
    }

    public void NextTurn()
    {
        movementTilemap.ClearAllTiles();
        targetTilemap.ClearAllTiles();
        characterOrder[CurrentCharacter].SetCurrentInitiative(
            characterOrder[CurrentCharacter].GetInitiative() +
            characterOrder[CurrentCharacter].GetCurrentAP());
        characterOrder[CurrentCharacter].SetState(CharacterUnit.State.inactive);
        CurrentCharacter++;
        if(CurrentCharacter >= characterOrder.Count)
        {
            round++;
            CurrentCharacter = 0;
            OrderCharacters();
        }
        HandleTurn();
    }
}
