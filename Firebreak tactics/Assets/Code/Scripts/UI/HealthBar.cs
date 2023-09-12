using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private int totalTiles = 676;
    private int burntTiles;
    private int fireTiles;
    private int forest1Tiles;
    private int forest2Tiles;
    private int forest3Tiles;
    private int waterTiles;
    [SerializeField] public Image fireBar;
    
    private void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }
    
    private float calculateBarSize(int tiles) {
        // For testing
        tiles = UnityEngine.Random.Range(0, 600);
        Debug.Log(tiles);
        float percentage = (float)tiles/totalTiles;
        Debug.Log(percentage * 500);
        return (float)percentage * 500;
    }

    private void GameStateChanged(GameManager.GameState _state){
        float fireBarSize = calculateBarSize(fireTiles);
        changeImageSize(fireBar, new Vector2(fireBarSize, 30f));
    }
    
    private void changeImageSize(Image image, Vector2 size)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
    }
}
