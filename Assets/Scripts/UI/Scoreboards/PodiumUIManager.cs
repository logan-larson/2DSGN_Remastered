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

    public void SetPodium(string first, string second = "", string third = "")
    {
        _firstText.text = first;
        _secondText.text = second;
        _thirdText.text = third;
    }
}
