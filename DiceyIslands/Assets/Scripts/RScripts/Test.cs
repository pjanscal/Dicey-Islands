using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    LokaalConnecter.PlayerController playerController;
    [SerializeField] int plrId;
    [SerializeField] TMP_Text timeText;
    float elapsed;
    [SerializeField] bool isRunning, firstTime;
    float playerTime;
    int seconds, centiseconds;
    TimeNeeded timeNeeded;

    void Start()
    {
        timeNeeded = FindObjectOfType<TimeNeeded>();
        firstTime = true;
        isRunning = false;
        playerController = LokaalConnecter.plrsController[plrId];
        
    }
    void Update()
    {
        if (playerController == null || !playerController.occuplied) return;

        if (firstTime == false)
        {
            if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                isRunning = false;
                Winner();
            }
        }

        if (firstTime == true)
        {
            if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                isRunning = true;
                firstTime = false;
            }
        }

        if (isRunning == true)
        {
            UpdateTimer();
        }

    }

    void UpdateTimer()
    {
        elapsed += Time.deltaTime;
           seconds = (int)(elapsed % 60f);
           centiseconds = (int)((elapsed * 100f) % 100f);

           timeText.text = seconds.ToString("00") + " : " + centiseconds.ToString("00");
        if (seconds == 3)
        {
            timeText.color = Color.black;
        }
    }

    void Winner()
    {
        timeText.color = Color.white;
        
        // Find the Test instance whose elapsed time is closest to its target (timeNeededSeconds)
        var all = Object.FindObjectsOfType<Test>();

        // Only decide winner when no player is still running
        foreach (var t in all)
        {
            if (t.isRunning)
            {
                Debug.Log("Winner: waiting for all players to stop");
                return;
            }
        }
        float bestDiff = float.MaxValue;
        int bestPlayer = -1;

        foreach (var t in all)
        {
            float diff = Mathf.Abs(t.elapsed - timeNeeded.timeNeededSeconds);
            Debug.Log($"[Winner] player {t.plrId}: elapsed={t.elapsed:F2}s target={timeNeeded.timeNeededSeconds:F2}s diff={diff:F2}s isRunning={t.isRunning}");
            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestPlayer = t.plrId;
            }
            else if (Mathf.Approximately(diff, bestDiff))
            {
                // tie-break: lower plrId wins
                if (t.plrId < bestPlayer) bestPlayer = t.plrId;
            }
        }

        if (bestPlayer >= 0)
            Debug.Log($"Winner: player {bestPlayer} (diff {bestDiff:F2}s)");
        else
            Debug.Log("Winner: no players found");
    }

}
