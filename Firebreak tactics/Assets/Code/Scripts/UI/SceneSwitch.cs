using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
	public string scene;

	public void loadScene()
	{
		Debug.Log("loadScene");
		SceneManager.LoadScene(scene);
	}
}
