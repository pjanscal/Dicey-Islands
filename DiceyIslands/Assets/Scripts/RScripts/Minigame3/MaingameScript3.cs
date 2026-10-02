using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaingameScript3 : MonoBehaviour
{
    [SerializeField] private int plrId;
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask raycastLayers = ~0;
    [SerializeField] private GameObject hitPrefab;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private RawImage dieIndicator;
    [SerializeField] private TargetScript target;

    private static readonly HashSet<MaingameScript3> activeMinigames = new();
    private static readonly HashSet<int> thrownPlayersThisRound = new();
    private static readonly List<int> eliminatedPlayers = new();
    private static bool gameEnded;
    private static bool resultsInitialized;

    private LokaalConnecter.PlayerController playerController;
    private GameObject swordObject;
    private bool swordHitBlocked;
    private readonly Color normalDieColor = Color.clear;

    public bool canShoot = true;
    public bool init;
    public bool hasThrownThisRound;

    private void OnEnable()
    {
        RemoveDestroyedInstances();
        if (activeMinigames.Count == 0)
        {
            ResetSharedState();
        }

        activeMinigames.Add(this);
        GameMangeren.startMiniGame += Init;
    }

    private void OnDisable()
    {
        GameMangeren.startMiniGame -= Init;
        activeMinigames.Remove(this);
    }

    private void Start()
    {
        if (!LokaalConnecter.plrsController.TryGetValue(plrId, out playerController))
        {
            Debug.LogError($"No player controller is registered for player {plrId}.", this);
        }

        if (target == null)
        {
            target = FindFirstObjectByType<TargetScript>();
        }

        swordObject = FindSwordGameObject();
        if (swordObject != null)
        {
            swordObject.SetActive(true);
        }

        UpdateDieIndicatorColor();
    }

    private void Update()
    {
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing ||
            !init || playerController == null || !playerController.occuplied ||
            swordHitBlocked || hasThrownThisRound || !canShoot || gameEnded)
        {
            return;
        }

        if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            ThrowSword();
        }
    }

    public void Init()
    {
        init = true;

        if (!resultsInitialized && MatchData.Instance != null)
        {
            MatchData.Instance.playerOrderNumbers.Clear();
            resultsInitialized = true;
        }
    }

    private void ThrowSword()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float distance = Mathf.Max(0f, rayDistance);
        bool foundHit = Physics.Raycast(origin, direction, out RaycastHit hit, distance, raycastLayers);

        if (foundHit && IsSwordHit(hit))
        {
            BlockPlayerFromThrowing();
            return;
        }

        Vector3 spawnPosition = foundHit
            ? hit.point
            : target != null ? target.transform.position : origin + direction * distance;
        Vector3 surfaceNormal = foundHit ? hit.normal : -direction;
        Transform spawnParent = foundHit ? hit.transform : target != null ? target.transform : null;

        if (hitPrefab != null)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.down, surfaceNormal) * Quaternion.Euler(rotationOffset);
            GameObject spawnedSword = Instantiate(hitPrefab, spawnPosition, rotation);
            if (spawnParent != null)
            {
                spawnedSword.transform.SetParent(spawnParent, true);
            }
        }
        else
        {
            Debug.LogError("Minigame 3 needs a sword prefab assigned to Hit Prefab.", this);
        }

        OnSwordThrown();
    }

    private void BlockPlayerFromThrowing()
    {
        if (swordHitBlocked || gameEnded)
        {
            return;
        }

        swordHitBlocked = true;
        hasThrownThisRound = true;
        canShoot = false;
        if (!eliminatedPlayers.Contains(plrId))
        {
            eliminatedPlayers.Add(plrId);
        }

        if (swordObject != null)
        {
            swordObject.SetActive(false);
        }

        UpdateDieIndicatorColor();

        if (GetEligiblePlayerCount() <= 1)
        {
            EndGame();
        }
        else
        {
            ResetRoundForEveryone();
        }
    }

    private void OnSwordThrown()
    {
        if (hasThrownThisRound || swordHitBlocked || gameEnded)
        {
            return;
        }

        hasThrownThisRound = true;
        canShoot = false;
        thrownPlayersThisRound.Add(plrId);

        if (swordObject != null)
        {
            swordObject.SetActive(false);
        }

        UpdateDieIndicatorColor();

        int eligibleCount = GetEligiblePlayerCount();
        if (eligibleCount <= 1)
        {
            EndGame();
        }
        else if (thrownPlayersThisRound.Count >= eligibleCount)
        {
            ResetRoundForEveryone();
        }
    }

    private void ResetRoundForEveryone()
    {
        if (gameEnded)
        {
            return;
        }

        thrownPlayersThisRound.Clear();
        RemoveDestroyedInstances();
        foreach (MaingameScript3 minigame in activeMinigames)
        {
            if (minigame.swordHitBlocked)
            {
                continue;
            }

            minigame.hasThrownThisRound = false;
            minigame.canShoot = true;
            if (minigame.swordObject != null)
            {
                minigame.swordObject.SetActive(true);
            }

            minigame.UpdateDieIndicatorColor();
        }
    }

    private void EndGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        List<int> places = new();
        RemoveDestroyedInstances();

        foreach (MaingameScript3 minigame in activeMinigames)
        {
            if (!minigame.swordHitBlocked && minigame.playerController != null && minigame.playerController.occuplied)
            {
                places.Add(minigame.plrId);
            }
        }

        for (int i = eliminatedPlayers.Count - 1; i >= 0; i--)
        {
            if (!places.Contains(eliminatedPlayers[i]))
            {
                places.Add(eliminatedPlayers[i]);
            }
        }

        if (places.Count == 0)
        {
            Debug.LogError("Minigame 3 ended without any registered players.");
            return;
        }

        if (MatchData.Instance != null)
        {
            MatchData.Instance.playerOrderNumbers.Clear();
            MatchData.Instance.playerOrderNumbers.AddRange(places);
        }
        else
        {
            Debug.LogError("Minigame 3 cannot save results because MatchData is missing.", this);
        }

        GameMangeren.MinigameWinner(places);
    }

    private bool IsSwordHit(RaycastHit hit)
    {
        Transform current = hit.transform;
        while (current != null)
        {
            if (current.tag == "Sword" || current.tag == "sword")
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void UpdateDieIndicatorColor()
    {
        if (dieIndicator != null)
        {
            dieIndicator.color = swordHitBlocked ? new Color(1f, 0f, 0f, 0.35f) : normalDieColor;
        }
    }

    private int GetEligiblePlayerCount()
    {
        RemoveDestroyedInstances();
        int count = 0;
        foreach (MaingameScript3 minigame in activeMinigames)
        {
            if (!minigame.swordHitBlocked && minigame.playerController != null && minigame.playerController.occuplied)
            {
                count++;
            }
        }

        return count;
    }

    private void ResetSharedState()
    {
        thrownPlayersThisRound.Clear();
        eliminatedPlayers.Clear();
        gameEnded = false;
        resultsInitialized = false;
    }

    private void RemoveDestroyedInstances()
    {
        activeMinigames.RemoveWhere(minigame => minigame == null);
    }

    private GameObject FindSwordGameObject()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Equals("sword", System.StringComparison.OrdinalIgnoreCase))
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
