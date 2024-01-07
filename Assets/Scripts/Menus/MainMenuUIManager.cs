using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject BaseMenu;
    public GameObject HostMenu;
    public GameObject JoinMenu;

    private void Start()
    {
        BaseMenu.SetActive(true);
        HostMenu.SetActive(false);
        JoinMenu.SetActive(false);
    }

    public void OpenHostMenu()
    {
        HostMenu.SetActive(true);
    }

    public void CloseHostMenu()
    {
        HostMenu.SetActive(false);
    }

    public void OpenJoinMenu()
    {
        JoinMenu.SetActive(true);
    }

    public void CloseJoinMenu()
    {
        JoinMenu.SetActive(false);
    }

    public void OpenFreeplayGame()
    {
        // Load the freeplay scene
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
