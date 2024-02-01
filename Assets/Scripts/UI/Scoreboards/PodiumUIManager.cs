using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PodiumUIManager : MonoBehaviour
{

    [SerializeField]
    private TMP_Text _firstText;

    [SerializeField]
    private TMP_Text _secondText;

    [SerializeField]
    private TMP_Text _thirdText;

    private void Start()
    {
        _firstText.text = "";
        _secondText.text = "";
        _thirdText.text = "";
    }

    public void ResetPodium()
    {
        _firstText.text = "";
        _secondText.text = "";
        _thirdText.text = "";
    }

    public void SetPlace(int place, string username)
    {
        switch (place)
        {
            case 1:
                _firstText.text = username;
                break;
            case 2:
                _secondText.text = username;
                break;
            case 3:
                _thirdText.text = username;
                break;
        }

    }
}
