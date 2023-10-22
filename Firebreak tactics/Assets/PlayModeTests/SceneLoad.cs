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

    [TearDown]
    public void TeardownTest()
    {
        SceneManager.UnloadSceneAsync("Mission1");
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
    public void TileMapSetup()
    {
        // Use the Assert class to test conditions
        GameObject go = GameObject.Find("State Manager");
        Assert.AreEqual(go.name, "State Manager");
        
        GridManager gm = go.GetComponent<GridManager>();

        Assert.IsNotNull(gm);

        //Assert.AreEqual(gm.gridXZ.Count, 25);
        //Assert.AreEqual(gm.gridXZ[0].Count, 24);

    }

    // A Test behaves as an ordinary method
    [Test]
    public void UnitsLoads()
    {
        // Use the Assert class to test conditions
        go = GameObject.Find("Units");

        Assert.IsNotNull(go);
    }
}
