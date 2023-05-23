using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private TileType defaultTile;
    private TileType current;

    public enum TileType
    {
        Grass1,
        Grass2,
        Grass3,
        Dirt,
        Fire,
        Water,
        Ember,
        Burned
    }

    private void Awake()
    {
        current = defaultTile; // Set the initial material option
        SetTile(current);
    }

    private void SetMaterial()
    {
        switch (current)
        {
            case TileType.Dirt:
                objectRenderer.material = GetMaterialFromName("Dirt");
                break;
            case TileType.Fire:
                objectRenderer.material = GetMaterialFromName("Fire");
                break;
            case TileType.Grass1:
                objectRenderer.material = GetMaterialFromName("Grass1");
                break;
            case TileType.Grass2:
                objectRenderer.material = GetMaterialFromName("Grass2");
                break;   
            case TileType.Grass3:
                objectRenderer.material = GetMaterialFromName("Grass3");
                break;  
            case TileType.Water:
                objectRenderer.material = GetMaterialFromName("Water");
                break; 
            case TileType.Ember:
                objectRenderer.material = GetMaterialFromName("Ember");
                break; 
            case TileType.Burned:
                objectRenderer.material = GetMaterialFromName("Burned");
                break; 
            // Add cases for other materials here

            default:
                break;
        }
    }

    private Material GetMaterialFromName(string materialName)
    {
        // Assuming the materials are stored in a "Materials" folder in the Resources folder
        string path = "Materials/" + materialName;
        return Resources.Load<Material>(path);
    }

    public void SetTile(TileType tile)
    {
        current = tile;
        SetMaterial();
    }

    public void SetDefault()
    {
        current = defaultTile;
        SetMaterial();
    }
}
