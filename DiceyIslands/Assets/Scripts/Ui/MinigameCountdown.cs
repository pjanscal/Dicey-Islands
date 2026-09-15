using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MinigameCountdown : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countdownText;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    //configs
    const float alphaTweenDur = .4f;
    const int secondsBeforeDone = 3;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        GameMangeren.minigameCountdown = this;
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        //set all default
        canvas.enabled = false;
        canvasGroup.alpha = 0;
    }

    public void Init()
    {
        StartCoroutine(StartCountDown());
    }

    IEnumerator StartCountDown()
    {
        canvas.enabled = true;
        canvasGroup.DOFade(1, alphaTweenDur)
         .SetEase(Ease.OutSine); //setting

        //count down
        for (int sec = secondsBeforeDone; sec > 0; sec--)
        {
            countdownText.text = $"{sec}";
            yield return new WaitForSeconds(1);
        }

        countdownText.text = "0";
        canvasGroup.DOFade(0, alphaTweenDur)
         .SetEase(Ease.OutSine) //setting
         .OnComplete(() =>
         {
            canvas.enabled = false;
            GameMangeren.startMiniGame?.Invoke();
         });
    }
}
