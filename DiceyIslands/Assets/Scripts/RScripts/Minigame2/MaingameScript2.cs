using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;

public class MaingameScript2 : MonoBehaviour
{
    [SerializeField] int plrId;
    SpawnObject spawnObject;
    public int points;
    bool gameOver;
    bool ready;
    bool gameStarted;
    float nextPressTime;
    float lastPressTime = -1f;
    [SerializeField] TextMeshProUGUI pointsText;
    [SerializeField] RawImage readyColor;
    [SerializeField] TextMeshProUGUI winnerText;
    LokaalConnecter.PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnObject = FindFirstObjectByType<SpawnObject>();

        playerController = LokaalConnecter.plrsController[plrId];
    }

    void Update()
    {
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (playerController == null || !playerController.occuplied) return;

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
                if (!ready)
                {
                    readyColor.color = Color.green;
                    ready = true;
                    CheckIfEveryoneIsReady();
                    return;
                }

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

                    if (points >= 15)
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

    void CheckIfEveryoneIsReady()
    {
        foreach (MaingameScript2 player in FindObjectsByType<MaingameScript2>(FindObjectsSortMode.None))
        {
            if (player.playerController != null && player.playerController.occuplied && !player.ready) return;
        }

        foreach (MaingameScript2 player in FindObjectsByType<MaingameScript2>(FindObjectsSortMode.None))
        {
            if (player.playerController != null && player.playerController.occuplied)
            {
                player.gameStarted = true;
            }
        }

        spawnObject.StartSpawning();
        winnerText.text = "Everyone is ready. The game has started!";
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
