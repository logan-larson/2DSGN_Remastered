using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _countdownText;

    [SerializeField]
    private Image _backgroundImage;

    private void Start()
    {
        GameManager.Instance.OnCountdown.AddListener(OnCountdown);

        _backgroundImage.gameObject.SetActive(GameManager.Instance.GameState == GameState.PreGame);
        _countdownText.gameObject.SetActive(GameManager.Instance.GameState == GameState.PreGame);
    }

    private void OnCountdown(int num)
    {
        _countdownText.text = num.ToString();

        if (num == 0)
        {
            _countdownText.text = "GO NUTS!";
            _countdownText.fontSize = 100;
            _backgroundImage.gameObject.SetActive(false);
            StartCoroutine(GoNuts());
        }
        else
        {
            StartCoroutine(CountdownTick(num));
        }
    }

    private IEnumerator GoNuts()
    {
        float timer = 2f;

        while (timer > 0f)
        {
            // Update the timer.
            timer -= Time.deltaTime;

            // Calculate the alpha value based on the remaining time.
            float alpha = Mathf.Lerp(0.0f, 1.0f, timer / 2f);

            _countdownText.alpha = alpha; // Fade the text each countdown tick.

            yield return null; // Wait for the next frame.
        }

        // Countdown has finished - disable the background and text.
        _countdownText.gameObject.SetActive(false);
        _backgroundImage.gameObject.SetActive(false);
    }

    private IEnumerator CountdownTick(int num)
    {
        float timer = 1f;

        while (timer > 0f)
        {
            // Update the timer.
            timer -= Time.deltaTime;

            // Calculate the alpha value based on the remaining time.
            float alpha = Mathf.Lerp(0.0f, 1.0f, timer);

            // On the last tick, we want to fade out the background and text.
            if (num == 1)
            {
                _backgroundImage.color = new Color(0f, 0f, 0f, alpha); // Fade the background each countdown tick.
            }

            _countdownText.alpha = alpha; // Fade the text each countdown tick.

            yield return null; // Wait for the next frame.
        }
    }
}
