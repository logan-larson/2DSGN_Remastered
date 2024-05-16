using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EmptyScreen : MonoBehaviour
{
    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    private StyleSheet _styleSheet;

    private void Start()
    {
        StartCoroutine(Generate());
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        Generate();
    }

    private IEnumerator Generate()
    {
        yield return null;

        var root = _document.rootVisualElement;
        root.Clear();

        root.styleSheets.Add(_styleSheet);



    }
}
