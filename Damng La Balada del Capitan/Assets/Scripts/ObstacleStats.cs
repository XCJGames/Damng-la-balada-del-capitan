using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObstacleStats : MonoBehaviour
{
    [SerializeField] int cover = 20;
    [SerializeField] bool activeObstacle = true;
    [SerializeField] Image coverBackground;
    [SerializeField] TextMeshProUGUI coverText;

    private bool isDisplayingInfo = false;

    public int GetCover() => cover;

    public bool GetActiveObstacle() => activeObstacle;

    public void SetCover(int cover) => this.cover = cover;

    public void SetActiveObstacle(bool activeObstacle) => this.activeObstacle = activeObstacle;

    private void OnMouseEnter()
    {
        if (activeObstacle)
        {
            coverBackground.gameObject.SetActive(true);
            coverText.text = cover.ToString();
            isDisplayingInfo = true;
        }
    }

    private void OnMouseExit()
    {
        if (isDisplayingInfo)
        {
            coverBackground.gameObject.SetActive(false);
        }
    }
}
