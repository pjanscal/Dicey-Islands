using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;

public class MaingameScript2 : MonoBehaviour
{
    [SerializeField] int plrId;
    SpawnObject spawnObject;
    public int points;
    bool gameOver, init;
    bool ready;
    bool gameStarted;
    float nextPressTime;
    float lastPressTime = -1f;
    [SerializeField] TextMeshProUGUI pointsText;
    [SerializeField] RawImage readyColor;
    [SerializeField] TextMeshProUGUI winnerText;
    LokaalConnecter.PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int PlayerId => plrId;

    public bool IsOccupied =>
        playerController != null &&
        playerController.occuplied;

    public bool IsGameFinished =>
        gameOver;
    void Start()
    {
        spawnObject = FindFirstObjectByType<SpawnObject>();

        playerController = LokaalConnecter.plrsController[plrId];
        GameMangeren.startMiniGame += Init;
    }

    public void Init()
    {
        init = true;
        gameStarted = true;

        if (spawnObject != null)
            spawnObject.StartSpawning();

        if (MatchData.Instance != null) MatchData.Instance.playerOrderNumbers.Clear(); //clear it up and use this as a places var
    }

    void Update()
    {
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (!init || playerController == null || !playerController.occuplied)
            {
                return;
            }

            if (playerController.GetButtonDown(LokaalConnecter.InputType.y) && IsGameOver())
            {
                RestartGame();
                return;
            }

            if (gameOver) return;

            if (gameStarted)
            {
                readyColor.color = Time.time < nextPressTime ? Color.red : Color.black;
            }

            if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
            
                if (!gameStarted) return;
                if (Time.time < nextPressTime) return;

                if (lastPressTime >= 0f && Time.time - lastPressTime < 0.5f)
                {
                    nextPressTime = Time.time + 5f;
                    readyColor.color = Color.red;
                    return;
                }

                lastPressTime = Time.time;
                if (!spawnObject.TryClaimObject(out GameObject claimedObject)) return;

                Match pointMatch = Regex.Match(claimedObject.name, @"-?\d+");
                if (pointMatch.Success)
                {
                    points += int.Parse(pointMatch.Value);

                    if (points >= 10)
                    {
                        gameOver = true;
                        spawnObject.StopSpawning();
                        winnerText.text = "Player " + plrId + " wins with " + points + " points!";
                    }
                }

            }

            pointsText.text = "Player " + plrId + " points: " + points;
        }
    }

    

    bool IsGameOver()
    {
        foreach (MaingameScript2 player in FindObjectsByType<MaingameScript2>(FindObjectsSortMode.None))
        {
            if (player.gameOver) return true;
        }

        return false;
    }

    void RestartGame()
    {
        spawnObject.StopSpawning();

        foreach (MaingameScript2 player in FindObjectsByType<MaingameScript2>(FindObjectsSortMode.None))
        {
            player.points = 0;
            player.gameOver = false;
            player.ready = false;
            player.gameStarted = false;
            player.nextPressTime = 0f;
            player.lastPressTime = -1f;
            player.winnerText.text = "";
            player.readyColor.color = Color.black;
        }
    }

}
