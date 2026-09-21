using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SceneManager = UnityEngine.SceneManagement.SceneManager;


public class MainGameScript : MonoBehaviour
{

    public LokaalConnecter.PlayerController playerController;
    [SerializeField] public int plrId;
    [SerializeField] TMP_Text timeText, countdown;
    [SerializeField] RawImage indicator;
    public float elapsed; //public so the cpu can check
    [SerializeField] public bool isRunning, firstTime, ready;
    bool init;
    int seconds, centiseconds;
    TimeNeeded timeNeeded;

    void Start()
    {
        timeNeeded = FindFirstObjectByType<TimeNeeded>();
        firstTime = true;
        isRunning = false;
        playerController = LokaalConnecter.plrsController[plrId];
        GameMangeren.startMiniGame += Init;
        countdown = GameObject.Find("Countdown").GetComponent<TMP_Text>();
        countdown.text = "";
    }

        public void Init()
    {
        init = true;
        StartCoroutine(StartTimer());
        if (MatchData.Instance != null) MatchData.Instance.playerOrderNumbers.Clear(); //clear it up and use this as a places var
    }

    void Update()
    {
        if (!init) return;

        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (playerController == null || !playerController.occuplied) return;

            if (ready && isRunning && playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                isRunning = false;
                indicator.color = Color.green;
                Winner();
            }

            if (isRunning) UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        elapsed += Time.deltaTime;
        seconds = (int)(elapsed % 60f);
        centiseconds = (int)((elapsed * 100f) % 100f);

        timeText.text = $"{seconds:00} : {centiseconds:00}";
        if (seconds == 3)
        {
            timeText.color = Color.clear;
        }
    }

    System.Collections.IEnumerator StartTimer()
    {
        foreach (var t in Object.FindObjectsByType<MainGameScript>(FindObjectsSortMode.None))
        {
            if (t.playerController != null && t.playerController.occuplied)
            {
                t.ready = true;
                t.isRunning = true;
                t.indicator.color = Color.red;
            }
        }

        countdown.text = "";
        yield break;
    }

    void Winner()
    {

        var all = Object.FindObjectsByType<MainGameScript>(FindObjectsSortMode.None);

        foreach (var t in all)
        {
            if (t.playerController != null && t.playerController.occuplied && t.isRunning)
            {
                Debug.Log("Winner: waiting for all players to stop");
                return;
            }
        }
        float bestDiff = float.MaxValue;
        int bestPlayer = -1;

        foreach (var t in all)
        {
            if (t.playerController == null || !t.playerController.occuplied) continue;

            float diff = Mathf.Abs(t.elapsed - timeNeeded.timeNeededSeconds);
            Debug.Log($"[Winner] player {t.plrId}: elapsed={t.elapsed:F2}s target={timeNeeded.timeNeededSeconds:F2}s diff={diff:F2}s isRunning={t.isRunning}");
            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestPlayer = t.plrId;
            }
            else if (Mathf.Approximately(diff, bestDiff))
            {
                if (t.plrId < bestPlayer) bestPlayer = t.plrId;
            }
        }

        if (bestPlayer >= 0)
            Debug.Log($"Winner: player {bestPlayer} (diff {bestDiff:F2}s)");
        else
        {
            Debug.Log("Winner: no players found");
        }

        foreach (var player in all)
        {
            if (player.playerController == null || !player.playerController.occuplied) continue;

            float playerDiff = Mathf.Abs(player.elapsed - timeNeeded.timeNeededSeconds);
            int place = 1;

            foreach (var otherPlayer in all)
            {
                if (otherPlayer.playerController == null || !otherPlayer.playerController.occuplied) continue;

                float otherDiff = Mathf.Abs(otherPlayer.elapsed - timeNeeded.timeNeededSeconds);
                if (otherDiff < playerDiff ||
                    (Mathf.Approximately(otherDiff, playerDiff) && otherPlayer.plrId < player.plrId))
                {
                    place++;
                }
            }

            Debug.Log($"Place {place}: Player {player.plrId}");
        }

        foreach (var t in all)
        {
            if (t.playerController != null && t.playerController.occuplied)
                t.timeText.color = t.plrId == bestPlayer ? Color.green : Color.red;
        }
    }
}