using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Transform waypointParent;

    [Header("Players")]
    [Tooltip("You can have 1-4 players. Leave unused slots empty.")]
    [SerializeField] private List<PlayerPiece> playerSlots = new List<PlayerPiece>();

    [Header("Turn Order")]
    [Tooltip("Generated from the available players at the start of the game.")]
    [SerializeField] private List<PlayerPiece> turnOrder = new List<PlayerPiece>();

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [SerializeField] private float pauseBetweenSpaces = 0.08f;

    [Header("UI")]
    [SerializeField] private Button rollButton;

    [Header("Camera")]
    [SerializeField] private PlayerCamera playerCamera;

    private List<Waypoint> waypoints = new List<Waypoint>();

    private int currentTurnIndex = 0;

    private bool turnInProgress = false;

    public PlayerPiece CurrentPlayer
    {
        get
        {
            if (turnOrder.Count == 0)
                return null;

            return turnOrder[currentTurnIndex];
        }
    }

    private void Awake()
    {
        SetupWaypoints();
        SetupPlayers();
    }

    private void Start()
    {
        PlacePlayersOnStartingWaypoint();
        StartCurrentTurn();
    }

    // -------------------------------------------------------
    // THIS PARFT SETS STUFF UP
    // -------------------------------------------------------

    private void SetupWaypoints()
    {
        waypoints.Clear();

        for (int i = 0; i < waypointParent.childCount; i++)
        {
            Transform child = waypointParent.GetChild(i);

            Waypoint waypoint = child.GetComponent<Waypoint>();

            if (waypoint == null)
                continue;

            waypoint.waypointNumber = waypoints.Count + 1;

            waypoints.Add(waypoint);
        }

        Debug.Log("Board loaded " + waypoints.Count + " waypoints.");
    }

    private void SetupPlayers()
    {
        turnOrder.Clear();

        foreach (PlayerPiece player in playerSlots)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            turnOrder.Add(player);
        }

        Debug.Log("Game started with " + turnOrder.Count + " players.");
    }

    private void PlacePlayersOnStartingWaypoint()
    {
        if (waypoints.Count == 0)
            return;

        foreach (PlayerPiece player in turnOrder)
        {
            player.currentWaypointIndex = 0;

            player.transform.position =
                waypoints[0].transform.position +
                player.tileOffset;
        }
    }

    // -------------------------------------------------------
    // TURN SYSTEM!!!
    // -------------------------------------------------------

    private void StartCurrentTurn()
    {
        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("There are no players!!");
            return;
        }

        PlayerPiece player = CurrentPlayer;

        Debug.Log(player.name + "'s turn.");

        UpdatePlayerVisuals();

        if (playerCamera != null)
        {
            playerCamera.SetTarget(player.transform);
        }

        if (rollButton != null)
        {
            rollButton.interactable = true;
        }
    }

    private void FinishCurrentTurn()
    {
        currentTurnIndex++;

        // Reached the end of the turn order.
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;

            Debug.Log("New round started.");
        }

        StartCurrentTurn();
    }

    private void UpdatePlayerVisuals()
    {
        foreach (PlayerPiece player in turnOrder)
        {
            player.SetActiveTurnVisual(player == CurrentPlayer);
        }
    }

    // -------------------------------------------------------
    // DDICE SYSTEM OF DOOM AND DESPAIR
    // -------------------------------------------------------

    public void RollDice()
    {
        if (turnInProgress)
            return;

        if (CurrentPlayer == null)
            return;

        int roll = Random.Range(1, 7);

        Debug.Log(CurrentPlayer.name + " rolled " + roll);

        StartCoroutine(PlayTurn(CurrentPlayer, roll));
    }

    private IEnumerator PlayTurn(PlayerPiece player, int roll)
    {
        turnInProgress = true;

        if (rollButton != null)
        {
            rollButton.interactable = false;
        }

        // First movement caused by the dice.
        int targetIndex =
            player.currentWaypointIndex + roll;

        targetIndex = Mathf.Clamp(
            targetIndex,
            0,
            waypoints.Count - 1
        );

        yield return MovePlayerToWaypoint(
            player,
            targetIndex
        );

        // Dice movement has completely finished.
        // NOW resolve the tile we actually landed on.
        yield return ResolveTileEffects(player);

        // Entire player's turn is now finished.
        turnInProgress = false;

        FinishCurrentTurn();
    }

    // -------------------------------------------------------
    // PROTOTYPE ANIMATIONS (GARBAGE LATER ON!!!)
    // -------------------------------------------------------

    private IEnumerator MovePlayerToWaypoint(
        PlayerPiece player,
        int targetIndex
    )
    {
        if (targetIndex == player.currentWaypointIndex)
        {
            yield break;
        }

        int direction =
            targetIndex > player.currentWaypointIndex
            ? 1
            : -1;

        while (player.currentWaypointIndex != targetIndex)
        {
            int nextIndex =
                player.currentWaypointIndex + direction;

            Vector3 destination =
                waypoints[nextIndex].transform.position +
                player.tileOffset;

            while (
                Vector3.Distance(
                    player.transform.position,
                    destination
                ) > 0.01f
            )
            {
                player.transform.position =
                    Vector3.MoveTowards(
                        player.transform.position,
                        destination,
                        moveSpeed * Time.deltaTime
                    );

                yield return null;
            }

            player.transform.position = destination;

            player.currentWaypointIndex = nextIndex;

            // Purely visual pause.
            if (pauseBetweenSpaces > 0)
            {
                yield return new WaitForSeconds(
                    pauseBetweenSpaces
                );
            }
        }
    }

    // -------------------------------------------------------
    // THIS PART MANAGES TILE EFFECTS SO SPECIAL TILES BALRIGHT
    // -------------------------------------------------------

    private IEnumerator ResolveTileEffects(
        PlayerPiece player
    )
    {
        // Stops accidental infinite loops if two special
        // tiles keep sending the player to each other.
        int effectSafetyLimit = 20;

        int effectsResolved = 0;

        while (effectsResolved < effectSafetyLimit)
        {
            Waypoint landedWaypoint =
                waypoints[player.currentWaypointIndex];

            Debug.Log(
                player.name +
                " landed on Waypoint " +
                landedWaypoint.waypointNumber +
                " (" +
                landedWaypoint.tileType +
                ")"
            );

            int movement =
                landedWaypoint.GetMovementEffect();

            // Normal tile or effect with no movement.
            if (movement == 0)
            {
                break;
            }

            int targetIndex =
                player.currentWaypointIndex +
                movement;

            targetIndex = Mathf.Clamp(
                targetIndex,
                0,
                waypoints.Count - 1
            );

            // If the effect doesn't actually move us,
            // stop resolving.
            if (targetIndex == player.currentWaypointIndex)
            {
                break;
            }

            yield return MovePlayerToWaypoint(
                player,
                targetIndex
            );

            effectsResolved++;
        }

        if (effectsResolved >= effectSafetyLimit)
        {
            Debug.LogWarning(
                "Tile effect limit reached " +
                "Check for looping tile effects"
            );
        }
    }

    // -------------------------------------------------------
    // WINNER TO LOSERSS (TURN ORDER OR SOMETHING)
    // -------------------------------------------------------

    public void SetTurnOrder(
        List<PlayerPiece> newOrder
    )
    {
        turnOrder.Clear();

        foreach (PlayerPiece player in newOrder)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            if (!turnOrder.Contains(player))
            {
                turnOrder.Add(player);
            }
        }

        currentTurnIndex = 0;

        Debug.Log("Turn order changed");
    }
}