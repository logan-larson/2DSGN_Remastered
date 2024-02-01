using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItemUIManager : MonoBehaviour
{
    [SerializeField]
    private Image _background;

    [SerializeField]
    private TMP_Text _placeText;

    [SerializeField]
    private TMP_Text _usernameText;

    [SerializeField]
    private TMP_Text _killsText;

    [SerializeField]
    private TMP_Text _deathsText;

    [SerializeField]
    private TMP_Text _assistsText;

    [SerializeField]
    private TMP_Text _scoreText;


    public void SetPlayer(Player player, int place)
    {
        if (_placeText != null)
            _placeText.text = place.ToString();

        if (_usernameText != null)
            _usernameText.text = player.Username;

        if (_killsText != null)
            _killsText.text = player.Kills.ToString();

        if (_deathsText != null)
            _deathsText.text = player.Deaths.ToString();

        if (_assistsText != null)
            _assistsText.text = player.Assists.ToString();

        if (_scoreText != null)
            _scoreText.text = player.Score.ToString();

        if (place % 2 == 0)
            _background.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        else
            _background.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    }
}
