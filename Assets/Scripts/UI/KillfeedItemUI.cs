using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillfeedItemUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _playerKilledUsername;
    [SerializeField]
    private TMP_Text _killerUsername;
    [SerializeField]
    private Image _weaponImage;

    public void Setup(string playerKilledUsername, string killerUsername, string weaponSpritePath)
    {
        _playerKilledUsername.text = playerKilledUsername;

        if (weaponSpritePath != null)
            _weaponImage.sprite = Resources.Load<Sprite>(weaponSpritePath);

        if (killerUsername != null)
            _killerUsername.text = killerUsername;
        else
            _killerUsername.text = "";
    }
}
