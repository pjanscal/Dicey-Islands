using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Colorhandler : MonoBehaviour
{
    [SerializeField] RawImage mainColor;

    void Start()
    {
        SetRandomColor();
    }

    void SetRandomColor()
    {
        Color randomColor = Random.ColorHSV();
        randomColor.r = Mathf.Max(randomColor.r, 40f / 255f);
        randomColor.g = Mathf.Max(randomColor.g, 40f / 255f);
        randomColor.b = Mathf.Max(randomColor.b, 40f / 255f);

        mainColor.color = randomColor;
        Debug.Log("Main color: " + mainColor.color);
    }
}
