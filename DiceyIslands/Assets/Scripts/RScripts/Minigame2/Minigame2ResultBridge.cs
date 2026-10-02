using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneManager =
    UnityEngine.SceneManagement.SceneManager;

public class Minigame2ResultBridge : MonoBehaviour
{
    [Header("Return To Board")]
    [SerializeField]
    private float returnToBoardDelay = 3f;

    [SerializeField]
    private string boardSceneName =
        "BoardTestScene";

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;

    [Header("Steal Sounds")]
    [SerializeField] private AudioClip stealPlus1Sound;
    [SerializeField] private AudioClip stealMinus1Sound;
    [SerializeField] private AudioClip stealPlus3Sound;
    [SerializeField] private AudioClip stealMinus3Sound;

    [Header("Other Sounds")]
    [SerializeField] private AudioClip pickupSpawnSound;
    [SerializeField] private AudioClip requiredPointsReachedSound;
    [SerializeField] private AudioClip backgroundMusic;

    private bool resultsProcessed = false;

    private void Start()
    {
        if (musicAudioSource != null &&
            backgroundMusic != null)
        {
            musicAudioSource.clip =
                backgroundMusic;

            musicAudioSource.loop = true;

            musicAudioSource.Play();
        }
    }

    public void PlayStealSound(int pointValue)
    {
        if (sfxAudioSource == null)
            return;

        AudioClip clip = null;

        switch (pointValue)
        {
            case 1:
                clip = stealPlus1Sound;
                break;

            case -1:
                clip = stealMinus1Sound;
                break;

            case 3:
                clip = stealPlus3Sound;
                break;

            case -3:
                clip = stealMinus3Sound;
                break;
        }

        if (clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayPickupSpawnSound()
    {
        if (sfxAudioSource == null ||
            pickupSpawnSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            pickupSpawnSound
        );
    }

    public void PlayRequiredPointsReachedSound()
    {
        if (sfxAudioSource == null ||
            requiredPointsReachedSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            requiredPointsReachedSound
        );
    }

    private void Update()
    {
        if (resultsProcessed)
            return;

        MaingameScript2[] allPlayers =
            Object.FindObjectsByType<MaingameScript2>(
                FindObjectsSortMode.None
            );

        List<MaingameScript2> activePlayers =
            new List<MaingameScript2>();

        foreach (
            MaingameScript2 player
            in allPlayers
        )
        {
            if (player == null)
                continue;

            if (!player.IsOccupied)
                continue;

            activePlayers.Add(player);
        }

        if (activePlayers.Count == 0)
            return;

        // =========================================
        // WAIT UNTIL SOMEONE WINS
        // =========================================

        bool someoneFinished = false;

        foreach (
            MaingameScript2 player
            in activePlayers
        )
        {
            if (player.IsGameFinished)
            {
                someoneFinished = true;
                break;
            }
        }

        if (!someoneFinished)
            return;

        resultsProcessed = true;

        StartCoroutine(
            ProcessResults(activePlayers)
        );
    }

    private IEnumerator ProcessResults(
        List<MaingameScript2> players
    )
    {
        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "MatchData does not exist!"
            );

            yield break;
        }

        // =========================================
        // SORT BY POINTS
        // =========================================

        players.Sort(
            (a, b) =>
            {
                // Higher points should come first.
                int comparison =
                    b.points.CompareTo(
                        a.points
                    );

                if (comparison != 0)
                    return comparison;

                // Tie breaker:
                // lower player number wins.
                return a.PlayerId.CompareTo(
                    b.PlayerId
                );
            }
        );


        MatchData.Instance
            .playerOrderNumbers.Clear();

        for (
            int i = 0;
            i < players.Count;
            i++
        )
        {
            MatchData.Instance
                .playerOrderNumbers.Add(
                    players[i].PlayerId
                );

            Debug.Log(
                "MINIGAME 2 RESULT - Place " +
                (i + 1) +
                ": Player " +
                players[i].PlayerId +
                " with " +
                players[i].points +
                " points."
            );
        }


        yield return new WaitForSecondsRealtime(
            returnToBoardDelay
        );

        // =========================================
        // SHOW RESULT
        // =========================================

        GameMangeren.MinigameWinner(
            MatchData.Instance.playerOrderNumbers
        );
    }
}