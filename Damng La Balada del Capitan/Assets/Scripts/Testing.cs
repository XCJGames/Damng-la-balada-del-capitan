using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private Grid<GridCell> grid;

    [SerializeField] Sprite[] sprites;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid<GridCell>(24, 16, 1f, Vector2.zero, (Grid<GridCell> g, int x, int y) => new GridCell(g, x, y));

        GameObject obj = GameObject.FindGameObjectWithTag("Obstacle");
        Transform child;
        GridCell cell;
        for(int i = 0; i < obj.transform.childCount; i++)
        {
            child = obj.transform.GetChild(i);
            cell = grid.GetGridObject(new Vector2(child.transform.position.x,
                child.transform.position.y));
            //cell.SetIsObstacle(true);
            //cell.SetCover(child.gameObject.GetComponent<ObstacleStats>().GetCover());
            //cell.SetSprite(obj.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite);
        }
        //DebugGrid();
        //Debug.Log(grid);
        /*GridCell cell = grid.GetGridObject(4, 6);
        Debug.Log(cell);
        cell.SetSprite(sprites[Random.Range(0,sprites.Length - 1)]);*/
    }

    private void DebugGrid()
    {
        GridCell cell;
        for(int i = 0; i < grid.GetWidth(); i++)
        {
            for(int j = 0; j < grid.GetHeight(); j++)
            {
                cell = grid.GetGridObject(i, j);
                Debug.Log("x: " + cell.GetX() +
                    " y: " + cell.GetY() +
                    " isWalkable: " + cell.IsWalkable() +
                    " cover: " + cell.GetObstacle().GetCover());
            }
        }
    }
}