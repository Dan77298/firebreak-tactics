using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SceneLoad
{
    private GameObject mainCam;
    private GameObject go;

    [OneTimeSetUp]
    public void LoadTestScene()
    {
        SceneManager.LoadScene("Mission1");

    }

    // A Test behaves as an ordinary method
    [UnityTest]
    public IEnumerator CameraLoads()
    {
        yield return new WaitForSeconds(0.5f) ;

        Assert.That(Object.FindObjectOfType<Camera>(), Is.Not.Null);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void StateManagerLoads()
    {
        // Use the Assert class to test conditions
        go =  GameObject.Find("State Manager");

        Assert.IsNotNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void PlayerInputLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("PlayerInput");

        Assert.IsNotNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void TutorialLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("Tutorial");

        Assert.IsNotNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void TutorialSkips()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("skip");

        Assert.IsNotNull(go);

        go.GetComponent<ScreenManager>().closeTutorial();

        go = GameObject.Find("Tutorial");

        Assert.IsNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void TutorialNext()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("next");

        Assert.IsNotNull(go);

        go.GetComponent<ScreenManager>().NextScreen();

        go = GameObject.Find("page2");

        Assert.IsNotNull(go);

        go = GameObject.Find("next");

        go.GetComponent<ScreenManager>().NextScreen();

        go = GameObject.Find("page3");

        Assert.IsNotNull(go);

        go = GameObject.Find("next");

        go.GetComponent<ScreenManager>().NextScreen();

        go = GameObject.Find("page4");

        Assert.IsNotNull(go);

        go = GameObject.Find("next");

        go.GetComponent<ScreenManager>().NextScreen();

        go = GameObject.Find("page5");

        Assert.IsNotNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void TutorialPrev()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("back");

        Assert.IsNotNull(go);

        go.GetComponent<ScreenManager>().PrevScreen();

        go = GameObject.Find("page4");

        Assert.IsNotNull(go);

        go = GameObject.Find("back");

        go.GetComponent<ScreenManager>().PrevScreen();

        go = GameObject.Find("page3");

        Assert.IsNotNull(go);

        go = GameObject.Find("back");

        go.GetComponent<ScreenManager>().PrevScreen();

        go = GameObject.Find("page2");

        Assert.IsNotNull(go);

        go = GameObject.Find("back");

        go.GetComponent<ScreenManager>().PrevScreen();

        go = GameObject.Find("page1");

        Assert.IsNotNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void VictoryUILoads()
    {
        GameObject.Find("State Manager").GetComponent<GameManager>().Victory();

        // Use the Assert class to test conditions
        go = GameObject.Find("VictoryUI");

        Assert.IsNotNull(go);

        GameObject.Find("State Manager").GetComponent<GameManager>().UnVictory();
    }

    // A Test behaves as an ordinary method
    [Test]
    public void VictoryUIFailsLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("VictoryUI");

        Assert.IsNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void LoseUILoads()
    {

        GameObject.Find("State Manager").GetComponent<GameManager>().Lose();

        // Use the Assert class to test conditions
        go = GameObject.Find("LoseUI");

        Assert.IsNotNull(go);

        GameObject.Find("State Manager").GetComponent<GameManager>().UnLose();
    }

    // A Test behaves as an ordinary method
    [Test]
    public void LoseUIFailsLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("LoseUI");

        Assert.IsNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void PlayerUI()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("PlayerUI");

        Assert.IsNotNull(go);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void TileMapLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("Tiles");

        Assert.IsNotNull(go);
    }

    //Validate tile map
    [Test]
    public void GridManagerSetupValidation()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");
        
        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        //26 columns, 25 rows
        Assert.AreEqual(gm.gridXZ.Count, 26);
        Assert.AreEqual(gm.gridXZ[0].Count, 25);
        
        //validate tile coordinates
        for (int i = 0; i < gm.gridXZ.Count; i++)
        {
            foreach (GameObject tile in gm.gridXZ[i])
            {
                if (tile != null)
                {
                    Assert.IsNotNull(tile.GetComponent<TileBehaviour>());
                    Assert.AreEqual(tile.GetComponent<TileBehaviour>().getCellPos(), gm.grid.WorldToCell(new Vector3(tile.transform.position.x, 0, tile.transform.position.z)));
                }
            }
        }

    }

    //test pathfinding
    [UnityTest]
    public IEnumerator AStarValidateGround() 
    {
        yield return new WaitForSeconds(0.5f);

        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        TileBehaviour startTile = gm.getTile(new Vector3Int(15, 13, 0)).GetComponent<TileBehaviour>();
        TileBehaviour endTile = gm.getTile(new Vector3Int(21, 12, 0)).GetComponent<TileBehaviour>();

        List<Vector2Int> path = gm.FindPath(startTile, endTile, 0);

        Assert.IsNotNull(path);

        Assert.AreEqual(8, path.Count);

        Assert.AreEqual(new Vector2Int(15, 13), path[0]);
        Assert.AreEqual(new Vector2Int(16, 13), path[1]);
        Assert.AreEqual(new Vector2Int(17, 13), path[2]);
        Assert.AreEqual(new Vector2Int(18, 13), path[3]);
        Assert.AreEqual(new Vector2Int(19, 14), path[4]);
        Assert.AreEqual(new Vector2Int(20, 14), path[5]);
        Assert.AreEqual(new Vector2Int(20, 13), path[6]);
        Assert.AreEqual(new Vector2Int(21, 12), path[7]);

    }

    //test pathfinding
    [UnityTest]
    public IEnumerator AStarValidateAir()
    {
        yield return new WaitForSeconds(1.0f);

        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        TileBehaviour startTile = gm.getTile(new Vector3Int(19, 11, 0)).GetComponent<TileBehaviour>();
        TileBehaviour endTile = gm.getTile(new Vector3Int(21, 12, 0)).GetComponent<TileBehaviour>();

        List<Vector2Int> path = gm.FindPath(startTile, endTile, 1);

        Assert.IsNotNull(path);

        Assert.AreEqual(3, path.Count);

        Assert.AreEqual(new Vector2Int(19, 11), path[0]);
        Assert.AreEqual(new Vector2Int(20, 11), path[1]);
        Assert.AreEqual(new Vector2Int(21, 12), path[2]);

    }

    //test get tile
    [Test]
    public void GetTileLowerBound()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        Assert.IsNull(gm.getTile(new Vector3Int(-1, -1, 0)));
        Assert.IsNull(gm.getTile(new Vector3Int(-1, 15, 0)));
        Assert.IsNull(gm.getTile(new Vector3Int(15, -1, 0)));
    }

    [Test]
    public void GetValidTile()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        Assert.IsNotNull(gm.getTile(new Vector3Int(15, 15, 0)));
    }

    [Test]
    public void GetTileUpperBound() 
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        Assert.IsNull(gm.getTile(new Vector3Int(28, 42, 0)));
        Assert.IsNull(gm.getTile(new Vector3Int(28, 15, 0)));
        Assert.IsNull(gm.getTile(new Vector3Int(15, 42, 0)));

    }

    // A Test behaves as an ordinary method
    [Test]
    public void UnitsLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("Units");

        Assert.IsNotNull(go);
    }

    [Test]
    public void ValidateUnitOnTile()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        GridManager gm = go.GetComponent<GridManager>();
        UnitManager um = go.GetComponent<UnitManager>();

        Assert.IsNotNull(gm);
        Assert.IsNotNull(um);

        Assert.IsNotNull(um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 4, 0))));
        Assert.AreEqual(um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 4, 0))).name, "Foam");
    }


    [Test]
    public void ValidateUnitOnEmptyTile()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        GridManager gm = go.GetComponent<GridManager>();
        UnitManager um = go.GetComponent<UnitManager>();

        Assert.IsNotNull(gm);
        Assert.IsNotNull(um);

        Assert.IsNull(um.GetUnitOnTile(gm.getTile(new Vector3Int(20, 17, 0))));
    }

    [Test]
    public void UnitsValidate()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("Units");

        foreach (UnitBehaviour ub in go.transform.GetComponentsInChildren<UnitBehaviour>())
        {

            if (ub.gameObject.name == "Striker")
            {
                Assert.AreEqual(ub.GetRange(), 3);
                Assert.AreEqual(ub.GetMaxMovements(), 4);
                Assert.AreEqual(ub.getActions(), 2);
                Assert.AreEqual(ub.getCapacity(), 6);
                Assert.AreEqual(ub.getSupport(), false);
                Assert.AreEqual(ub.getPreventative(), true);
                Assert.AreEqual(ub.getExtinguish(), true);
                Assert.AreEqual(ub.GetTraversalType(), 0);
            }
            else if (ub.gameObject.name == "Foam")
            {
                Assert.AreEqual(ub.GetRange(), 2);
                Assert.AreEqual(ub.GetMaxMovements(), 3);
                Assert.AreEqual(ub.getActions(), 2);
                Assert.AreEqual(ub.getCapacity(), 6);
                Assert.AreEqual(ub.getSupport(), false);
                Assert.AreEqual(ub.getPreventative(), true);
                Assert.AreEqual(ub.getExtinguish(), false);
                Assert.AreEqual(ub.GetTraversalType(), 0);

            }
            else if (ub.gameObject.name == "Scout")
            {
                Assert.AreEqual(ub.GetRange(), 0);
                Assert.AreEqual(ub.GetMaxMovements(), 7);
                Assert.AreEqual(ub.getActions(), 0);
                Assert.AreEqual(ub.getCapacity(), 0);
                Assert.AreEqual(ub.getSupport(), false);
                Assert.AreEqual(ub.getPreventative(), false);
                Assert.AreEqual(ub.getExtinguish(), false);
                Assert.AreEqual(ub.GetTraversalType(), 0);

            }
        }
    }

    [Test]
    public void ValidateMove()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        GameObject destTile = gm.getTile(new Vector3Int(17,11, 0));

        GameObject unitToMove = um.GetUnitOnTile(gm.getTile(new Vector3Int(21, 4, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(21, 4, 0)));

        Assert.IsNotNull(unitToMove);
        Assert.AreEqual(unitToMove.name, "Scout");

        um.moveUnitToTile(unitToMove, destTile);
        um.CenterUnitToTile(unitToMove, destTile);

        GameObject movedUnit = um.GetUnitOnTile(destTile);
    
        Assert.AreSame(unitToMove, movedUnit);
    }

    [Test]
    public void ValidateSpawnStriker()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        bm.CheckSpawn(3);

        GameObject destTile = gm.getTile(new Vector3Int(0, 0, 0));

        GameObject newUnit = um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 3, 0)));

        newUnit.GetComponent<UnitBehaviour>().SetOccupyingTile(gm.getTile(new Vector3Int(22, 3, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(22, 3, 0)));

        Assert.IsNotNull(newUnit);
        Assert.AreEqual(newUnit.name, "Striker");

        um.moveUnitToTile(newUnit, destTile);
        um.CenterUnitToTile(newUnit, destTile);
    }

    [Test]
    public void ValidateSpawnFoam()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        bm.CheckSpawn(5);

        GameObject destTile = gm.getTile(new Vector3Int(0, 1, 0));

        GameObject newUnit = um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 3, 0)));

        newUnit.GetComponent<UnitBehaviour>().SetOccupyingTile(gm.getTile(new Vector3Int(22, 3, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(22, 3, 0)));

        Assert.IsNotNull(newUnit);
        Assert.AreEqual(newUnit.name, "Foam");

        um.moveUnitToTile(newUnit, destTile);
        um.CenterUnitToTile(newUnit, destTile);
    }

    [Test]
    public void ValidateSpawnScout()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        bm.CheckSpawn(10);

        GameObject destTile = gm.getTile(new Vector3Int(0, 2, 0));

        GameObject newUnit = um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 3, 0)));

        newUnit.GetComponent<UnitBehaviour>().SetOccupyingTile(gm.getTile(new Vector3Int(22, 3, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(22, 3, 0)));

        Assert.IsNotNull(newUnit);
        Assert.AreEqual(newUnit.name, "Scout");

        um.moveUnitToTile(newUnit, destTile);
        um.CenterUnitToTile(newUnit, destTile);
    }

    [Test]
    public void ValidateSpawnSpotter()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        bm.CheckSpawn(8);

        GameObject destTile = gm.getTile(new Vector3Int(0, 4, 0));

        GameObject newUnit = um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 3, 0)));

        newUnit.GetComponent<UnitBehaviour>().SetOccupyingTile(gm.getTile(new Vector3Int(22, 3, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(22, 3, 0)));

        Assert.IsNotNull(newUnit);
        Assert.AreEqual(newUnit.name, "Spotter");

        um.moveUnitToTile(newUnit, destTile);
        um.CenterUnitToTile(newUnit, destTile);
    }

    [Test]
    public void ValidateSpawnTransport()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        bm.CheckSpawn(12);

        GameObject destTile = gm.getTile(new Vector3Int(0, 5, 0));

        GameObject newUnit = um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 3, 0)));

        newUnit.GetComponent<UnitBehaviour>().SetOccupyingTile(gm.getTile(new Vector3Int(22, 3, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(22, 3, 0)));

        Assert.IsNotNull(newUnit);
        Assert.AreEqual(newUnit.name, "Transport");

        um.moveUnitToTile(newUnit, destTile);
        um.CenterUnitToTile(newUnit, destTile);
    }

    [Test]
    public void ValidateSpawnTanker()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");

        BaseManager bm = go.GetComponent<BaseManager>();

        UnitManager um = go.GetComponent<UnitManager>();

        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(bm);
        Assert.IsNotNull(um);
        Assert.IsNotNull(gm);

        bm.CheckSpawn(6);

        GameObject destTile = gm.getTile(new Vector3Int(0, 6, 0));

        GameObject newUnit = um.GetUnitOnTile(gm.getTile(new Vector3Int(22, 3, 0)));

        newUnit.GetComponent<UnitBehaviour>().SetOccupyingTile(gm.getTile(new Vector3Int(22, 3, 0)));

        um.requestCancel(gm.getTile(new Vector3Int(22, 3, 0)));

        Assert.IsNotNull(newUnit);
        Assert.AreEqual(newUnit.name, "Tanker");

        um.moveUnitToTile(newUnit, destTile);
        um.CenterUnitToTile(newUnit, destTile);
    }
}
