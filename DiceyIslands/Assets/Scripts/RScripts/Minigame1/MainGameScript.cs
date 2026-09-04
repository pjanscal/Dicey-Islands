using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MainGameScript : MonoBehaviour
{
    LokaalConnecter.PlayerController playerController;
    [SerializeField] int plrId;
    [SerializeField] TMP_Text timeText, countdown;
    [SerializeField] RawImage indicator;
    [HideInInspector] public float elapsed; //public so the cpu can check
    [SerializeField] bool isRunning, firstTime, ready;
    int seconds, centiseconds;
    TimeNeeded timeNeeded;

    void Start()
    {
        timeNeeded = FindFirstObjectByType<TimeNeeded>();
        firstTime = true;
        isRunning = false;
        playerController = LokaalConnecter.plrsController[plrId];
        countdown = GameObject.Find("Countdown").GetComponent<TMP_Text>();
        countdown.text = "";
    }

    void Update()
    {

        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
                if (playerController == null || !playerController.occuplied) return;

            if (!ready && firstTime && playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                firstTime = false;
                indicator.color = Color.green;

                foreach (var t in Object.FindObjectsByType<MainGameScript>(FindObjectsSortMode.None))
                {
                    if (t.playerController != null && t.playerController.occuplied && t.firstTime) return;
                }

                StartCoroutine(StartTimer());
            }

            if (ready && isRunning && playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                isRunning = false;
                indicator.color = Color.green;
                Winner();
            }

            if (isRunning) UpdateTimer();

            if (playerController.GetButtonDown(LokaalConnecter.InputType.y))
            {
                SceneManager.LoadScene("Minigame1");
            }  
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
            timeText.color = Color.black;
        }
    }

    System.Collections.IEnumerator StartTimer()
    {
        countdown.text = "3";
        yield return new WaitForSeconds(1f);
        countdown.text = "2";
        yield return new WaitForSeconds(1f);
        countdown.text = "1";
        yield return new WaitForSeconds(1f);
        countdown.text = "0";
        foreach (var t in Object.FindObjectsByType<MainGameScript>(FindObjectsSortMode.None))
        {
            if (t.playerController != null && t.playerController.occuplied)
            {
                t.ready = true;
                t.isRunning = true;
                t.indicator.color = Color.red;
            }
        }
        yield return new WaitForSeconds(1f);
        countdown.text = "";
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