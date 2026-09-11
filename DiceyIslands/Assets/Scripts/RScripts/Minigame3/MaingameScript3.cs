using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MaingameScript3 : MonoBehaviour
{
    [SerializeField] int plrId;
    LokaalConnecter.PlayerController playerController;
    [SerializeField] float rayDistance = 10f;
    [SerializeField] LayerMask raycastLayers = ~0;
    [SerializeField] GameObject hitPrefab;
    [SerializeField] Vector3 rotationOffset;
    [SerializeField] RawImage dieIndicator;

    private static readonly HashSet<int> thrownPlayersThisRound = new();
    private static readonly Dictionary<int, int> swordsThrownByPlayer = new();
    private static bool gameEnded = false;

    GameObject spawnedSword;
    GameObject swordObject;
    bool canShoot = true, init;
    bool hasThrownThisRound = false;
    bool swordHitBlocked = false;
    readonly Color normalDieColor = Color.green;

    void Start()
    {
        playerController = LokaalConnecter.plrsController[plrId];
        GameMangeren.startMiniGame += Init;
        init = false;

        swordObject = FindSwordGameObject();
        if (swordObject != null)
        {
            swordObject.SetActive(true);
        }

        UpdateDieIndicatorColor();
    }

    void Update()
    {
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing)
        {
            if (!init || playerController == null || !playerController.occuplied)
            {
                return;
            }

            if (!swordHitBlocked && !hasThrownThisRound && canShoot && playerController.GetButtonDown(LokaalConnecter.InputType.x))
            {
                SpawnAtRaycastHit();
            }
        }
    }

    public void Init()
    {
        init = true;
        if (MatchData.Instance != null) MatchData.Instance.playerOrderNumbers.Clear(); //clear it up and use this as a places var
    }

    void SpawnAtRaycastHit()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float distance = Mathf.Max(0f, rayDistance);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, raycastLayers))
        {
            if (IsSwordHit(hit))
            {
                BlockPlayerFromThrowing();
                return;
            }

            if (hitPrefab == null)
            {
                return;
            }

            Quaternion rotation = Quaternion.FromToRotation(Vector3.down, hit.normal) * Quaternion.Euler(rotationOffset);
            spawnedSword = Instantiate(hitPrefab, hit.point, rotation);
            spawnedSword.transform.SetParent(hit.transform, true);

            OnSwordThrown();
        }
    }

    void BlockPlayerFromThrowing()
    {
        if (swordHitBlocked || gameEnded)
        {
            return;
        }

        swordHitBlocked = true;
        hasThrownThisRound = true;
        canShoot = false;

        if (swordObject != null)
        {
            swordObject.SetActive(false);
        }

        UpdateDieIndicatorColor();

        int eligibleCount = GetEligiblePlayerCount();
        if (eligibleCount <= 1)
        {
            EndGame();
            return;
        }

        ResetRoundForEveryone();
    }

    void OnSwordThrown()
    {
        if (hasThrownThisRound || swordHitBlocked || gameEnded)
        {
            return;
        }

        hasThrownThisRound = true;
        canShoot = false;
        thrownPlayersThisRound.Add(plrId);

        swordsThrownByPlayer[plrId] = swordsThrownByPlayer.TryGetValue(plrId, out int count) ? count + 1 : 1;

        if (swordObject != null)
        {
            swordObject.SetActive(false);
        }

        UpdateDieIndicatorColor();

        if (GetEligiblePlayerCount() == 1)
        {          
            EndGame();
        }
        else if (thrownPlayersThisRound.Count >= GetEligiblePlayerCount())
        {
            ResetRoundForEveryone();
        }
    }

    void ResetRoundForEveryone()
    {
        if (gameEnded)
        {
            return;
        }

        foreach (MaingameScript3 minigame in FindObjectsOfType<MaingameScript3>())
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

        thrownPlayersThisRound.Clear();
    }

    void EndGame()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        //--spefieck useless... by DDD this make it so 2 player can be draw so u can throw last without letting it change but debug--
        var orderedScores = FindObjectsOfType<MaingameScript3>()
            .Select(x => new
            {
                PlrId = x.plrId,
                SwordsThrown = swordsThrownByPlayer.TryGetValue(x.plrId, out int count) ? count : 0
            })
            .OrderByDescending(x => x.SwordsThrown)
            .ThenBy(x => x.PlrId)
            .ToList();

        int place = 1;
        int previousScore = int.MaxValue;
        for (int i = 0; i < orderedScores.Count; i++)
        {
            if (orderedScores[i].SwordsThrown != previousScore)
            {
                place = i + 1;
            }

            previousScore = orderedScores[i].SwordsThrown;
            Debug.Log($"Player {orderedScores[i].PlrId} Place {place} SwordsThrown {orderedScores[i].SwordsThrown}");
        }

        //--ended--

        if (MatchData.Instance == null) {Debug.LogError("there is no matchData"); return;}
        
        //get last plr
        for (int plrLeft = 1; plrLeft <= LokaalConnecter.maxPlr; plrLeft++)
        {
            if (MatchData.Instance.playerOrderNumbers.Contains(plrLeft)) continue;

            MatchData.Instance.playerOrderNumbers.Add(plrLeft);
            break;
        }

        //debug
        int places = 4;
        foreach (int placesPlr in MatchData.Instance.playerOrderNumbers)
        {
            print($"plr{placesPlr} is place {places} in result matchData");
            places -= 1;
        }

        //go back
        MatchData.Instance.returningFromMinigame = true;
        GameMangeren.SwitchScene("BoardTestScene"); //beta so we can have it in a mangeren we all can get
    }

    bool IsSwordHit(RaycastHit hit)
    {
        Transform current = hit.transform;
        while (current != null)
        {
            if (current.CompareTag("Sword") || current.CompareTag("sword"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    void UpdateDieIndicatorColor()
    {
        if (dieIndicator == null)
        {
            return;
        }

        if (swordHitBlocked)
        {
            if (MatchData.Instance != null) MatchData.Instance.playerOrderNumbers.Add(plrId); //use this as a global mangener
            dieIndicator.color = Color.red;
        }
        else
        {
            dieIndicator.color = normalDieColor;
        }
    }

    int GetEligiblePlayerCount()
    {
        int count = 0;
        foreach (MaingameScript3 minigame in FindObjectsOfType<MaingameScript3>())
        {
            if (!minigame.swordHitBlocked && minigame.playerController != null && minigame.playerController.occuplied)
            {
                count++;
            }
        }

        return count;
    }

    GameObject FindSwordGameObject()
    {
        Transform swordTransform = transform.Find("sword");
        if (swordTransform != null)
        {
            return swordTransform.gameObject;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Equals("sword", StringComparison.OrdinalIgnoreCase))
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
