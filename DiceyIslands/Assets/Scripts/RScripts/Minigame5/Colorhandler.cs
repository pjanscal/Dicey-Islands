using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Colorhandler : MonoBehaviour
{
    public static Colorhandler instance;
    public RawImage[] mainColors;

    void Awake()
    {
        instance = this;
    }

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

        foreach (RawImage mainColor in mainColors)
        {
            mainColor.color = randomColor;
        }
        Debug.Log("Main color: " + randomColor);
    }
}
