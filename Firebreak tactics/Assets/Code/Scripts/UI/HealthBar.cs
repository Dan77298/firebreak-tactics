using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    private int totalTiles = 676; // Replace with actual number of tiles
    private int burntTiles = 0; // Replace with global variables?
    private int fireTiles = 40;
    private int forest1Tiles = 200;
    private int forest2Tiles = 150;
    private int forest3Tiles = 100;
    private int waterTiles = 30;
    [SerializeField] public Image burntBar;
    [SerializeField] public Image fireBar;
    [SerializeField] public Image forest1Bar;
    [SerializeField] public Image forest2Bar;
    [SerializeField] public Image forest3Bar;
    [SerializeField] public Image waterBar;
    
    private void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }
    
    private float calculateBarSize(int tiles) {
        float percentage = (float) tiles/totalTiles;
        return (float) percentage * 500;
    }

    private void GameStateChanged(GameManager.GameState _state){
        float burntBarSize = calculateBarSize(tileManager.GetBurnt());
        changeImageSize(burntBar, new Vector2(burntBarSize, 30f));
        
        float fireBarSize = calculateBarSize(tileManager.GetFire());
        changeImageSize(fireBar, new Vector2(fireBarSize, 30f));
        
        float forest1BarSize = calculateBarSize(tileManager.GetForest1());
        changeImageSize(forest1Bar, new Vector2(forest1BarSize, 30f));
        
        float forest2BarSize = calculateBarSize(tileManager.GetForest2());
        changeImageSize(forest2Bar, new Vector2(forest2BarSize, 30f));
        
        float forest3BarSize = calculateBarSize(tileManager.GetForest3());
        changeImageSize(forest3Bar, new Vector2(forest3BarSize, 30f));
        
        float waterBarSize = calculateBarSize(tileManager.GetWater());
        changeImageSize(waterBar, new Vector2(waterBarSize, 30f));
    }
    
    private void changeImageSize(Image image, Vector2 size)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
    }
}
