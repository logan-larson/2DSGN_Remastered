using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListItemUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _usernameText;

    public void SetPlayer(Player player)
    {
        _usernameText.text = player.Username;
    }
}
