using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    private Grid<GridCell> grid;

    /*[SerializeField] bool isObstacle = false;
    [SerializeField] int cover = 0;*/
    //[SerializeField] Sprite obstacleSprite = null;
    // Diferenciamos entre si se puede andar por la casilla o no
    // Si se puede, es que no hay nada
    // Si no se puede, hay un personaje o un obstáculo
    // Si es personaje, guardamos el CharacterUnit
    // Si es obstáculo, guardamos el ObstacleStats
    [SerializeField] bool isWalkable = true;
    [SerializeField] CharacterUnit unit;
    [SerializeField] ObstacleStats obstacle;

    private int x;
    private int y;

    private bool isValidMovePosition = false;
    private bool isValidAttackTarget = false;

    public GridCell(Grid<GridCell> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    /*public GridCell(bool isObstacle, int cover, Sprite obstacleSprite, int x, int y)
    {
        this.isObstacle = isObstacle;
        this.cover = cover;
        //this.obstacleSprite = obstacleSprite;
        this.x = x;
        this.y = y;
    }*/

    public int GetX() => x;

    public int GetY() => y;

    public bool IsWalkable() => isWalkable;

    public bool IsValidMovePosition() => isValidMovePosition;

    public bool IsValidAttackTarget() => isValidAttackTarget;

    public bool HasUnit() => unit != null;

    public CharacterUnit GetUnit() => unit;

    public ObstacleStats GetObstacle() => obstacle;

    public void SetIsWalkable(bool isWalkable) => this.isWalkable = isWalkable;

    public void SetIsValidMovePosition(bool isValidMovePosition) => this.isValidMovePosition = isValidMovePosition;

    public void SetIsValidAttackTarget(bool isValidAttackTarget) => this.isValidAttackTarget = isValidAttackTarget;

    public void SetUnit(CharacterUnit unit) => this.unit = unit;

    public void SetObstacle(ObstacleStats obstacle) => this.obstacle = obstacle;

    //public int GetCover() => cover;

    //public bool IsObstacle() => isObstacle;

    //public Sprite GetSprite() => obstacleSprite;

    //public void SetIsObstacle(bool newIsObstacle) => isObstacle = newIsObstacle;

    //public void SetCover(int cover) => this.cover = cover;

    /*public void SetSprite(Sprite sprite)
    {
        obstacleSprite = sprite;
        //Sprite.Create(sprite.texture, sprite.rect, new Vector2(x, y));
        GameObject gameObject = new GameObject("Obst01");
        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        gameObject.transform.Translate(new Vector2(x + 0.5f, y + 0.5f));
    }*/
}
