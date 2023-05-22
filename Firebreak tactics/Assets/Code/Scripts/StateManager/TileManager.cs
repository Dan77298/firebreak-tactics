using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] public GameObject boardGrid;
    private List<GameObject> tileList;

    void Awake()
    {
        tileList = new List<GameObject>();
        getTileObjects();
    }

    private void getTileObjects()
    {
        tileList.Clear();

        if (boardGrid != null)
        {
            foreach (Transform child in boardGrid.transform)
            {
                tileList.Add(child.gameObject);
            }
        }
    }
}