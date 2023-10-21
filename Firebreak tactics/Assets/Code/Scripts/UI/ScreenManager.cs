using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject Tutorial;
    [SerializeField] private GameObject Settings;

    [SerializeField] private GameObject current;
    [SerializeField] private GameObject next;
    [SerializeField] private GameObject prev;

    public void NextScreen()
    {
        current.SetActive(false);
        next.SetActive(true);
    }

    public void PrevScreen()
    {
        current.SetActive(false);
        prev.SetActive(true);
    }

    public void closeTutorial(){
        PlayerUI.SetActive(true);
        Tutorial.SetActive(false);
        if (Settings){
            Settings.SetActive(false);
        }
    }

    public void beginGame()
    {
        closeTutorial();
    }
}
