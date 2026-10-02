using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

//shortcuts
using AnimatorTriggerEvent = CharacterLoader.CharactersAnimationTriggerEvent;

public class MainGameScript : MonoBehaviour
{
    private MinigameResultBridge resultBridge;
    public LokaalConnecter.PlayerController playerController;
    [SerializeField] public int plrId;
    [SerializeField] TMP_Text timeText;
    [SerializeField] RawImage indicator;
    public float elapsed; //public so the cpu can check
    private CharacterLoader animatorController;
    [SerializeField] public bool isRunning, firstTime, ready;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip lockInSound;
    bool init;
    int seconds, centiseconds;
    TimeNeeded timeNeeded;

    void Start()
    {
        timeNeeded =
            FindFirstObjectByType<TimeNeeded>();

        resultBridge =
            FindFirstObjectByType<MinigameResultBridge>();

        firstTime = true;
        isRunning = false;

        playerController =
            LokaalConnecter.plrsController[plrId];

        GameMangeren.startMiniGame += Init;
        animatorController = GameMangeren.GetCharacterLoaderFromId(plrId);
    }

    public void Init()
    {
        init = true;
        StartCoroutine(StartTimer());
        if (MatchData.Instance != null) MatchData.Instance.playerOrderNumbers.Clear(); //clear it up and use this as a places var
    }

    private void PlayLockInSound()
    {
        if (sfxAudioSource == null ||
            lockInSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            lockInSound
        );
    }
    void Update()
    {
        if (!init) return;

        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (playerController == null || !playerController.occuplied) return;

            if (ready &&
     isRunning &&
     playerController.GetButtonDown(
         LokaalConnecter.InputType.x
     ))
                if (ready &&
         isRunning &&
         playerController.GetButtonDown(
             LokaalConnecter.InputType.x
         ))
                {
                    isRunning = false;
                    
                    //animation
                    animatorController.UseAnimation(AnimatorTriggerEvent.ActivePress);
                    //bugs u get the result before pressing

                    indicator.color =
                        new Color32(34, 139, 34, 255);

                    // Individual player lock-in sound.
                    if (resultBridge != null)
                    {
                        resultBridge.PlayPlayerLockedSound();
                    }

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
            }
        }

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