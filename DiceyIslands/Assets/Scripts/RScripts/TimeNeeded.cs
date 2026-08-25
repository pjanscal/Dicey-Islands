using UnityEngine;
using TMPro;

public class TimeNeeded : MonoBehaviour
{
    public TMP_Text timeNeededText;
    public float timeNeededSeconds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       Awake();
    }

    public void Awake()
    {
        int needSec = Random.Range(5, 15);
        int needCenti = Random.Range(0, 99);
        if (timeNeededText != null)
        {
            timeNeededText.text = needSec.ToString("00") + " : " + needCenti.ToString("00");
        }
        timeNeededSeconds = needSec + needCenti / 100f; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
