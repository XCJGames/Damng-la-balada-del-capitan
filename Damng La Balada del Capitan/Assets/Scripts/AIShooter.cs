using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AIShooter : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;

    private Grid<GridCell> grid;
    private CharacterUnit stats;

    private CombatCanvasController combatCanvasController;

    private List<AICommand> commands;
    private int currentCommand;

    private bool isMoving = false;
    private List<Vector2> movePath;
    private int currentNode;

    public Grid<GridCell> Grid { get => grid; set => grid = value; }


    // Start is called before the first frame update
    void Start()
    {
        combatCanvasController = FindObjectOfType<CombatCanvasController>();
        commands = new List<AICommand>();
        stats = GetComponent<CharacterUnit>();
    }

    private void Update()
    {
        if (isMoving)
        {
            Move();
        }
    }

    public void HandleTurn()
    {
        if (commands.Count == 0)
        {

            Grid.GetXY(transform.position, out int unitX, out int unitY);
            List<CharacterUnit> playerCharacters = GetPlayerCharacters();
            if (HasCharacterInAttackRange(playerCharacters))
            {
                //Debug.Log("obj: " + gameObject + "character in range");
                //Debug.Log("is flanked: " + CheckIsFlanked(transform.position));
                if(CheckIsFlanked(transform.position))
                {
                    // Moverse a una mejor posición, si es posible, y atacar
                    //Debug.Log("flanqueado, reposicionando");
                    Vector2 newPos = CheckForFlankingPosition(stats.GetCurrentAP() / stats.GetMovementCost());
                    //Debug.Log("new pos" + newPos);
                    if (newPos.x != transform.position.x || newPos.y != transform.position.y)
                    {
                        Grid.GetXY(newPos, out int newX, out int newY);
                        commands.Add(new AICommand(AICommand.Commands.move, newX, newY));
                        int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, newPos);
                        stats.RemoveActionPoints(distance * stats.GetMovementCost());
                    }

                    AttackBetterTarget();
                }
                else
                {
                    if(UnityEngine.Random.Range(1, 100) <= 60)
                    {
                        //Debug.Log("reposicionarse y atacar");
                        Vector2 newPos = CheckForFlankingPosition((stats.GetCurrentAP()) / stats.GetMovementCost());
                        //Debug.Log("obj: " + gameObject + " new pos: " + newPos + " pos: " + transform.position);
                        if (newPos.x != transform.position.x || newPos.y != transform.position.y)
                        {
                            Grid.GetXY(newPos, out int newX, out int newY);
                            commands.Add(new AICommand(AICommand.Commands.move, newX, newY));
                            int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, newPos);
                            stats.RemoveActionPoints(distance * stats.GetMovementCost());
                        }

                        AttackBetterTarget();
                    }
                    else
                    {
                        //Debug.Log("quedarse y disparar");
                        // Permanecer en el sitio y disparar
                        AttackBetterTarget();
                    }
                }
            }
            else
            {
                //Debug.Log("obj: " + gameObject + "no character in range");
                Vector2 newPos = CheckForFlankingPosition(stats.GetCurrentAP() / stats.GetMovementCost());
                //Debug.Log("obj: " + gameObject + " new pos: " + newPos + " pos: " + transform.position);
                if (newPos.x != transform.position.x || newPos.y != transform.position.y)
                {
                    Grid.GetXY(newPos, out int newX, out int newY);
                    commands.Add(new AICommand(AICommand.Commands.move, newX, newY));
                    int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, newPos);
                    stats.RemoveActionPoints(distance * stats.GetMovementCost());
                }
                else
                {
                    newPos = AdvanceToCloserCharacter();
                    if (newPos.x != transform.position.x || newPos.y != transform.position.y)
                    {
                        Grid.GetXY(newPos, out int newX, out int newY);
                        commands.Add(new AICommand(AICommand.Commands.move, newX, newY));
                        int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, newPos);
                        stats.RemoveActionPoints(distance * stats.GetMovementCost());
                    }
                }
            }

            AttackBetterTarget();
            //List<ObstacleStats> osbtacles = GetObstaclesInRange();
            /*foreach(AICommand comm in commands)
            {
                Debug.Log("command: " + comm.Command + " target: " + comm.Target + " X: " + comm.X +
                    " Y: " + comm.Y);
            }*/
        }
        ExecuteNextCommand();
        if (CheckTurnDone() && commands.Count <= currentCommand)
        {
            commands.Clear();
            currentCommand = 0;
            CombatSystem.Instance.NextTurn();
        }
        //CombatSystem.Instance.NextTurn();
    }

    private void AttackBetterTarget()
    {
        List<CharacterUnit> playerCharacters = GetPlayerCharacters();
        while (!CheckTurnDone() && HasCharacterInAttackRange(playerCharacters))
        {
            CharacterUnit target = GetBetterTarget(playerCharacters);
            commands.Add(new AICommand(AICommand.Commands.attack, target));
            stats.RemoveActionPoints(stats.GetAttackCost());
        }
    }

    public void ExecuteNextCommand()
    {
        //Debug.Log("counter: " + currentCommand + " commands count" + commands.Count);
        if(currentCommand < commands.Count)
        {
            switch (commands[currentCommand].Command)
            {
                case AICommand.Commands.move:
                    Pathfinding pathfinding = Pathfinding.Instance;
                    movePath = pathfinding.FindPath(transform.position, 
                        new Vector2(commands[currentCommand].X, commands[currentCommand].Y));
                    isMoving = true;
                    currentNode = 0;
                    grid.GetGridObject(transform.position).SetIsWalkable(true);
                    grid.GetGridObject(transform.position).SetUnit(null);
                    break;
                case AICommand.Commands.attack:
                    if (commands[currentCommand].Target.IsAlive)
                    {
                        AttackCharacter(commands[currentCommand].Target);
                    }
                    else
                    {
                        stats.RestoreCurrentAP(stats.GetAttackCost());
                    }
                    break;
            }
            currentCommand++;
        }
        else
        {
            currentCommand = 0;
            commands.Clear();
            /*if (!CheckTurnDone()) HandleTurn();
            else CombatSystem.Instance.NextTurn();*/
        }
    }

    internal void ResetCommands()
    {
        commands.Clear();
        currentCommand = 0;
    }

    private Vector2 AdvanceToCloserCharacter()
    {
        Pathfinding pathfinding = Pathfinding.Instance;
        CharacterUnit character = GetCloserCharacter();
        List<Vector2> path = new List<Vector2>();
        List<Vector2> shortestPath = null;
        /*foreach(Vector2 point in path) Debug.Log("Advancing to closer character, path: " + point);*/
        GridCell cell;
        Vector2 pos = new Vector2((int)character.transform.position.x,
            (int)character.transform.position.y + 1);
        cell = Grid.GetGridObject(pos);
        if (cell != null && cell.IsWalkable())
        {
            path = pathfinding.FindPath(transform.position, pos);
            //Debug.Log("up path count:" + path.Count);
            //foreach (Vector2 point in path) Debug.Log("Advancing to closer character, path: " + point);
            if (path != null && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }
        pos.x = character.transform.position.x;
        pos.y = character.transform.position.y - 1;
        cell = Grid.GetGridObject(pos);
        if (cell != null && cell.IsWalkable())
        {
            path = pathfinding.FindPath(transform.position, pos);
            //Debug.Log("down path count:" + path.Count);
            //foreach (Vector2 point in path) Debug.Log("Advancing to closer character, path: " + point);
            if (path != null && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }
        pos.x = character.transform.position.x + 1;
        pos.y = character.transform.position.y;
        cell = Grid.GetGridObject(pos);
        if (cell != null && cell.IsWalkable())
        {
            path = pathfinding.FindPath(transform.position, pos);
            //Debug.Log("right path count:" + path.Count);
            //foreach (Vector2 point in path) Debug.Log("Advancing to closer character, path: " + point);
            if (path != null && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }
        pos.x = character.transform.position.x - 1;
        pos.y = character.transform.position.y;
        cell = Grid.GetGridObject(pos);
        if (cell != null && cell.IsWalkable())
        {
            path = pathfinding.FindPath(transform.position, pos);
            //Debug.Log("left path count:" + path.Count);
            //foreach (Vector2 point in path) Debug.Log("Advancing to closer character, path: " + point);
            if (path != null && (shortestPath == null || path.Count < shortestPath.Count))
            {
                shortestPath = path;
            }
        }
        if (shortestPath != null) return shortestPath[Mathf.Min(shortestPath.Count - 1, stats.GetCurrentAP()/stats.GetMovementCost())];
        return transform.position;
    }

    private CharacterUnit GetCloserCharacter()
    {
        List<CharacterUnit> playerCharacters = GetPlayerCharacters();
        Grid.GetXY(transform.position, out int unitX, out int unitY);
        CharacterUnit unit = playerCharacters[0];
        int minDistance = int.MaxValue;
        int distance;
        foreach(CharacterUnit character in playerCharacters)
        {
            distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, character.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                unit = character;
            }
        }

        return unit;
    }

    private Vector2 CheckForFlankingPosition(int range)
    {
        if (!FlankingFromPosition(transform.position) || CheckIsFlanked(transform.position))
        {
            Pathfinding pathfinding = Pathfinding.Instance;
            List<CharacterUnit> playerCharacters = GetPlayerCharacters();
            List<ObstacleStats> obstacles = GetObstaclesInRange(range);
            List<Vector2> potentialPositions = new List<Vector2>();
            GridCell cell;
            foreach(ObstacleStats obstacle in obstacles)
            {
                Grid.GetXY(obstacle.transform.position, out int obsX, out int obsY);
                cell = Grid.GetGridObject(obsX, obsY + 1);
                if (cell != null && cell.IsWalkable())
                {
                    Vector2 pos = new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1);
                    if (!CheckIsFlanked(pos) && HasCharacterInAttackRange(playerCharacters))
                    {
                        potentialPositions.Add(pos);
                        if(FlankingFromPosition(pos))
                        {
                            List<Vector2> path = pathfinding.FindPath(transform.position, pos);
                            if (path != null && path.Count - 1 <= range)
                            {
                                return pos;
                            }
                        }
                    }
                }
                cell = Grid.GetGridObject(obsX, obsY - 1);
                if (cell != null && cell.IsWalkable())
                {
                    Vector2 pos = new Vector2(obstacle.transform.position.x, obstacle.transform.position.y - 1);
                    if (!CheckIsFlanked(pos) && HasCharacterInAttackRange(playerCharacters))
                    {
                        potentialPositions.Add(pos);
                        if (FlankingFromPosition(pos))
                        {
                            List<Vector2> path = pathfinding.FindPath(transform.position, pos);
                            if (path != null && path.Count - 1 <= range)
                            {
                                return pos;
                            }
                        }
                    }
                }
                cell = Grid.GetGridObject(obsX + 1, obsY);
                if (cell != null && cell.IsWalkable())
                {
                    Vector2 pos = new Vector2(obstacle.transform.position.x + 1, obstacle.transform.position.y);
                    if (!CheckIsFlanked(pos) && HasCharacterInAttackRange(playerCharacters))
                    {
                        potentialPositions.Add(pos);
                        if (FlankingFromPosition(pos))
                        {
                            List<Vector2> path = pathfinding.FindPath(transform.position, pos);
                            if (path != null && path.Count - 1 <= range)
                            {
                                return pos;
                            }
                        }
                    }
                }
                cell = Grid.GetGridObject(obsX - 1, obsY);
                if (cell != null && cell.IsWalkable())
                {
                    Vector2 pos = new Vector2(obstacle.transform.position.x - 1, obstacle.transform.position.y);
                    if (!CheckIsFlanked(pos) && HasCharacterInAttackRange(playerCharacters))
                    {
                        potentialPositions.Add(pos);
                        if (FlankingFromPosition(pos))
                        {
                            List<Vector2> path = pathfinding.FindPath(transform.position, pos);
                            if (path != null && path.Count - 1 <= range)
                            {
                                return pos;
                            }
                        }
                    }
                }
            }
            Debug.Log("potential positions: " + potentialPositions.Count);
            if(potentialPositions.Count > 0)
            {
                return potentialPositions[UnityEngine.Random.Range(0, potentialPositions.Count - 1)];
            }
        }
        return transform.position;
    }

    private bool FlankingFromPosition(Vector2 position)
    {
        List<CharacterUnit> playerCharacters = GetPlayerCharacters();
        Grid.GetXY(new Vector2(transform.position.x, transform.position.y),
            out int unitX, out int unitY);
        foreach (CharacterUnit character in playerCharacters)
        {
            int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, character.transform.position);
            if(distance <= stats.GetAttackRange())
            {
                if (CheckCharacterIsFlankedFromPosition(character, transform.position)) return true;
            }
        }
        return false;
    }

    private bool CheckCharacterIsFlankedFromPosition(CharacterUnit character, Vector2 position)
    {
        bool coverUp = HasCoverUp(character.transform.position);
        bool coverRight = HasCoverRight(character.transform.position);
        bool coverDown = HasCoverDown(character.transform.position);
        bool coverLeft = HasCoverLeft(character.transform.position);

        if(character.transform.position.y > position.y && !coverDown)
        {
            if (character.transform.position.x == position.x) return true;
            if (character.transform.position.x < position.x && !coverRight) return true;
            if (character.transform.position.x > position.x && !coverLeft) return true;
        }
        else if(character.transform.position.y < position.y && !coverUp)
        {
            if (character.transform.position.x == position.x) return true;
            if (character.transform.position.x < position.x && !coverRight) return true;
            if (character.transform.position.x > position.x && !coverLeft) return true;
        }
        else
        {
            if (character.transform.position.x < position.x && !coverRight) return true;
            if (character.transform.position.x > position.x && !coverLeft) return true;
        }
        return false;
    }

    private bool CheckTurnDone()
    {
        if (stats.GetCurrentAP() > 0)
        {
            if (stats.GetCurrentAP() >= stats.GetAttackCost() &&
                HasCharacterInAttackRange(GetPlayerCharacters())) return false;
            else
            {
                /*if (stats.GetCurrentAP() >= stats.GetMovementCost())
                {
                    Vector2 newPos = CheckForBetterPositionWithinRange(stats.GetCurrentAP() / stats.GetMovementCost());
                    if (newPos.x != transform.position.x || newPos.y != transform.position.y) return false;
                    else return true;
                }
                else return true;*/
                return true;
            }
        }
        else return true;
    }

    private Vector2 CheckForBetterPositionWithinRange(int range)
    {
        if (CheckIsFlanked(transform.position))
        {
            Pathfinding pathfinding = Pathfinding.Instance;
            List<ObstacleStats> obstacles = GetObstaclesInRange(range);
            GridCell cell;
            foreach(ObstacleStats obstacle in obstacles)
            {
                Grid.GetXY(obstacle.transform.position, out int obsX, out int obsY);
                cell = Grid.GetGridObject(obsX, obsY + 1);
                if (cell != null && cell.IsWalkable())
                {
                    if (!CheckIsFlanked(new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1)))
                    {
                        List<Vector2> path = pathfinding.FindPath(transform.position,
                            new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1));
                        if(path != null && path.Count -1 <= range)
                        {
                            return new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1);
                        }
                    }
                }
                cell = Grid.GetGridObject(obsX, obsY - 1);
                if (cell != null && cell.IsWalkable())
                {
                    if (!CheckIsFlanked(new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1)))
                    {
                        List<Vector2> path = pathfinding.FindPath(transform.position,
                            new Vector2(obstacle.transform.position.x, obstacle.transform.position.y - 1));
                        if (path != null && path.Count - 1 <= range)
                        {
                            return new Vector2(obstacle.transform.position.x, obstacle.transform.position.y - 1);
                        }
                    }
                }
                cell = Grid.GetGridObject(obsX + 1, obsY);
                if (cell != null && cell.IsWalkable())
                {
                    if (!CheckIsFlanked(new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1)))
                    {
                        List<Vector2> path = pathfinding.FindPath(transform.position,
                            new Vector2(obstacle.transform.position.x + 1, obstacle.transform.position.y));
                        if (path != null && path.Count - 1 <= range)
                        {
                            return new Vector2(obstacle.transform.position.x + 1, obstacle.transform.position.y);
                        }
                    }
                }
                cell = Grid.GetGridObject(obsX - 1, obsY);
                if (cell != null && cell.IsWalkable())
                {
                    if (!CheckIsFlanked(new Vector2(obstacle.transform.position.x, obstacle.transform.position.y + 1)))
                    {
                        List<Vector2> path = pathfinding.FindPath(transform.position,
                            new Vector2(obstacle.transform.position.x - 1, obstacle.transform.position.y));
                        if (path != null && path.Count - 1 <= range)
                        {
                            return new Vector2(obstacle.transform.position.x - 1, obstacle.transform.position.y);
                        }
                    }
                }
            }
        }
        return transform.position;
    }

    private List<ObstacleStats> GetObstaclesInRange(int range)
    {
        List<ObstacleStats> list = new List<ObstacleStats>();
        ObstacleStats[] obstacles = FindObjectsOfType<ObstacleStats>();
        Grid.GetXY(new Vector2(transform.position.x, transform.position.y),
            out int unitX, out int unitY);
        foreach(ObstacleStats obstacle in obstacles)
        {
            int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, obstacle.transform.position);
            if (distance <= range + 1)
            {
                list.Add(obstacle);
            }
        }

        return list;
    }

    private int CalculateDistanceBetweenTwoPoints(int unitX, int unitY, Vector2 otherPos)
    {
        Grid.GetXY(new Vector2(otherPos.x, otherPos.y), out int otherX, out int otherY);
        int xDistance = Mathf.Abs(unitX - otherX);
        int yDistance = Mathf.Abs(unitY - otherY);
        return xDistance + yDistance;
    }

    private bool CheckIsFlanked(Vector2 position)
    {
        bool coverUp = HasCoverUp(position);
        bool coverRight = HasCoverRight(position);
        bool coverDown = HasCoverDown(position);
        bool coverLeft = HasCoverLeft(position);

        List<CharacterUnit> playerCharacters = GetPlayerCharacters();
        foreach(CharacterUnit character in playerCharacters)
        {
            if (IsInRangeOfPlayer(character))
            {
                if(character.transform.position.y > position.y && !coverUp)
                {
                    if (character.transform.position.x == position.x) return true;
                    else if (character.transform.position.x < position.x && !coverLeft) return true;
                    else if (character.transform.position.x > position.x && !coverRight) return true;
                }
                else if (character.transform.position.y < position.y && !coverDown)
                {
                    if (character.transform.position.x == position.x) return true;
                    else if (character.transform.position.x < position.x && !coverLeft) return true;
                    else if (character.transform.position.x > position.x && !coverRight) return true;
                }
                else
                {
                    if (character.transform.position.x < position.x && !coverLeft) return true;
                    else if (character.transform.position.x > position.x && !coverRight) return true;
                }
            }
        }

        return false;
    }

    private bool IsInRangeOfPlayer(CharacterUnit character)
    {
        int range = character.GetAttackRange();
        Grid.GetXY(new Vector2(transform.position.x, transform.position.y),
                out int unitX, out int unitY);
        int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, character.transform.position);
        if (distance <= range) return true;
        return false;
    }

    private bool HasCoverLeft(Vector2 position)
    {
        GridCell cell = Grid.GetGridObject(new Vector2(position.x - 1, position.y));
        if (cell != null)
        {
            if (!cell.IsWalkable() && !cell.HasUnit()) return true;
        }
        return false;
    }

    private bool HasCoverDown(Vector2 position)
    {
        GridCell cell = Grid.GetGridObject(new Vector2(position.x, position.y - 1));
        if (cell != null)
        {
            if (!cell.IsWalkable() && !cell.HasUnit()) return true;
        }
        return false;
    }

    private bool HasCoverRight(Vector2 position)
    {
        GridCell cell = Grid.GetGridObject(new Vector2(position.x + 1, position.y));
        if (cell != null)
        {
            if (!cell.IsWalkable() && !cell.HasUnit()) return true;
        }
        return false;
    }

    private bool HasCoverUp(Vector2 position)
    {
        GridCell cell = Grid.GetGridObject(new Vector2(position.x, position.y + 1));
        if (cell != null)
        {
            if (!cell.IsWalkable() && !cell.HasUnit()) return true;
        }
        return false;
    }

    private void AttackCharacter(CharacterUnit characterUnit)
    {
        //Debug.Log("Attack");
        int cover = CheckCover(stats, characterUnit);
        int hitChance = Mathf.Max(stats.GetBaseHitChance() - cover, 0);
        hitChance = Mathf.Clamp(hitChance, 5, 95);
        int roll = UnityEngine.Random.Range(1, 100);
        if (roll <= hitChance)
        {
            StartCoroutine(CombatSystem.Instance.UpdateUIAfterAttack(stats, characterUnit));
            /*characterUnit.RemoveCurrentHealth(stats.GetAttackDamage());
            if (characterUnit.GetCurrentHealth() <= 0)
            {
                characterUnit.SetState(CharacterUnit.State.dead);
                Grid.GetGridObject(characterUnit.transform.position).SetIsWalkable(true);
                Grid.GetGridObject(characterUnit.transform.position).SetUnit(null);
            }*/
        }
        combatCanvasController.HitChance = hitChance;
        combatCanvasController.Roll = roll;
        combatCanvasController.Damage = roll <= hitChance ? stats.GetAttackDamage() : 0;
        combatCanvasController.StartAttackAnimations(stats, characterUnit);
    }

    private CharacterUnit GetBetterTarget(List<CharacterUnit> playerCharacters)
    {
        int maxChance = 0, chance;
        CharacterUnit unit = playerCharacters[0];
        grid.GetXY(transform.position, out int unitX, out int unitY);
        foreach(CharacterUnit character in playerCharacters)
        {
            if (CalculateDistanceBetweenTwoPoints(unitX, unitY, character.transform.position) <= stats.GetAttackRange())
            {
                chance = stats.GetBaseHitChance() - CheckCover(stats, character);
                Debug.Log("char" + character.gameObject + "chance" + chance + "maxChance" + maxChance);
                if (chance > maxChance)
                {
                    unit = character;
                    maxChance = chance;
                }
            }
        }

        return unit;
    }

    private bool HasCharacterInAttackRange(List<CharacterUnit> playerCharacters)
    {
        int range = stats.GetAttackRange();
        Grid.GetXY(new Vector2(transform.position.x, transform.position.y), 
            out int unitX, out int unitY);
        foreach(CharacterUnit character in playerCharacters)
        {
            int distance = CalculateDistanceBetweenTwoPoints(unitX, unitY, character.transform.position);
            if (distance <= range) return true;
        }

        return false;
    }

    private List<CharacterUnit> GetPlayerCharacters()
    {
        CharacterUnit[] characters = FindObjectsOfType<CharacterUnit>();
        List<CharacterUnit> playerCharacters = new List<CharacterUnit>();
        foreach(CharacterUnit c in characters)
        {
            if (c.IsPlayerControlled() && c.IsAlive) playerCharacters.Add(c);
        }

        return playerCharacters;
    }

    private int CheckCover(CharacterUnit attacker, CharacterUnit defender)
    {
        int cover = 0;
        GridCell cell;
        if (attacker.transform.position.x < defender.transform.position.x)
        {
            cell = Grid.GetGridObject(new Vector2(
                defender.transform.position.x - 1,
                defender.transform.position.y));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        else if (attacker.transform.position.x > defender.transform.position.x)
        {
            cell = Grid.GetGridObject(new Vector2(
                defender.transform.position.x + 1,
                defender.transform.position.y));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        else
        {
            if (attacker.transform.position.y < defender.transform.position.y)
            {
                cell = Grid.GetGridObject(new Vector2(
                    defender.transform.position.x,
                    defender.transform.position.y - 1));
                if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
                {
                    cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
                }
            }
            else
            {
                cell = Grid.GetGridObject(new Vector2(
                    defender.transform.position.x,
                    defender.transform.position.y + 1));
                if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
                {
                    cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
                }
            }
        }
        if (attacker.transform.position.y < defender.transform.position.y)
        {
            cell = Grid.GetGridObject(new Vector2(
                defender.transform.position.x,
                defender.transform.position.y - 1));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        else if (attacker.transform.position.y > defender.transform.position.y)
        {
            cell = Grid.GetGridObject(new Vector2(
                defender.transform.position.x,
                defender.transform.position.y + 1));
            if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
            {
                cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
            }
        }
        else
        {
            if (attacker.transform.position.x < defender.transform.position.x)
            {
                cell = Grid.GetGridObject(new Vector2(
                    defender.transform.position.x - 1,
                    defender.transform.position.y));
                if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
                {
                    cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
                }
            }
            else
            {
                cell = Grid.GetGridObject(new Vector2(
                    defender.transform.position.x + 1,
                    defender.transform.position.y));
                if (cell != null && !cell.IsWalkable() && !cell.HasUnit())
                {
                    cover = Mathf.Max(cover, cell.GetObstacle().GetCover());
                }
            }
        }
        if (cover > 0) cover += defender.GetDefenseModifierWithCover();
        else cover += defender.GetDefenseModifierWithoutCover();
        return cover;
    }

    private void Move()
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        if (currentPos == movePath[currentNode])
        {
            currentNode++;
        }
        if (currentNode >= movePath.Count)
        {
            grid.GetGridObject(transform.position).SetIsWalkable(false);
            grid.GetGridObject(transform.position).SetUnit(stats);
            isMoving = false;
            ExecuteNextCommand();
        }
        else
        {
            Vector2 targetPosition = movePath[currentNode];
            var moveStep = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(currentPos, targetPosition, moveStep);
        }
    }
}
