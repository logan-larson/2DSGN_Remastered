using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public List<TutorialItem> TutorialItems;

    public Image CurrentImage;
    public TMP_Text CurrentText;

    public Button NextButton;
    public Button PreviousButton;

    private int _currentIndex = 0;

    private void Start()
    {
        CheckDisable();

        if (TutorialItems.Count > 0)
        {
            Set(TutorialItems[_currentIndex]);
        }
    }

    public void Next()
    {
        if (_currentIndex < TutorialItems.Count - 1)
        {
            _currentIndex++;
            Set(TutorialItems[_currentIndex]);
        }

        CheckDisable();
    }

    public void Previous()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            Set(TutorialItems[_currentIndex]);
        }

        CheckDisable();
    }

    private void CheckDisable()
    {
        if (_currentIndex == 0)
        {
            PreviousButton.interactable = false;
        }
        else
        {
            PreviousButton.interactable = true;
        }

        if (_currentIndex == TutorialItems.Count - 1)
        {
            NextButton.interactable = false;
        }
        else
        {
            NextButton.interactable = true;
        }
    }

    private void Set(TutorialItem item)
    {
        if (item.Image != null)
            CurrentImage.sprite = item.Image;

        CurrentText.text = item.Text;
    }
}
