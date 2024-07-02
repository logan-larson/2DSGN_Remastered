using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EmptyScreen : MonoBehaviour
{
    [SerializeField]
    private UIDocument _document;

    [SerializeField]
    private List<StyleSheet> _styleSheets;

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

        foreach (var sheet in _styleSheets)
        {
            root.styleSheets.Add(sheet);
        }
    }

    private T Create<T>(params string[] classList) where T : VisualElement, new()
    {
        var element = new T();
        for (int i = 0; i < classList.Length; i++)
        {
            element.AddToClassList(classList[i]);
        }
        return element;
    }
}
