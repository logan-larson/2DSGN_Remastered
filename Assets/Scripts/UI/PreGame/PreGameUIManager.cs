using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PreGameUIManager : MonoBehaviour
{
    public UnityEvent OnStartGame = new UnityEvent();

    public UnityEvent<string> OnMapSelected = new UnityEvent<string>();

    public void OnStart()
    {
        OnStartGame.Invoke();
    }

    public void OnForestSelected()
    {
        OnMapSelected.Invoke("Forest");
    }

    public void OnArcticSelected()
    {
        OnMapSelected.Invoke("Arctic");
    }

    public void OnCaveSelected()
    {
        OnMapSelected.Invoke("Cave");
    }
}
