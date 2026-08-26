using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    // =========================================================
    // BOARD
    // =========================================================

    [Header("Board")]
    [SerializeField] private Transform waypointParent;

    private List<Waypoint> waypoints = new List<Waypoint>();


    // =========================================================
    // PLAYERS
    // =========================================================

    [Header("Players")]
    [Tooltip("Add up to 4 players here. Empty/inactive players are ignored.")]
    [SerializeField]
    private List<PlayerPiece> playerSlots =
        new List<PlayerPiece>();

    [Header("Turn Order")]
    [Tooltip("Automatically generated from the available players.")]
    [SerializeField]
    private List<PlayerPiece> turnOrder =
        new List<PlayerPiece>();

    private int currentTurnIndex = 0;


    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("Tiny pause after reaching each waypoint.")]
    [SerializeField] private float pauseBetweenSpaces = 0.08f;


    // =========================================================
    // DICE
    // =========================================================

    [Header("Dice")]
    [SerializeField] private float diceAnimationDuration = 0.6f;

    [SerializeField] private float diceNumberChangeSpeed = 0.06f;

    [Tooltip("How long the final roll stays visible before movement begins.")]
    [SerializeField] private float finalRollDisplayTime = 0.5f;


    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private Button rollButton;

    [SerializeField] private TMP_Text tilesLeftText;

    [SerializeField] private TMP_Text rollNumberText;

    [SerializeField] private TMP_Text roundText;


    // =========================================================
    // CAMERA
    // =========================================================

    [Header("Camera")]
    [SerializeField] private PlayerCamera playerCamera;

    [SerializeField] private Transform topDownCameraPosition;


    // =========================================================
    // ROUND SYSTEM
    // =========================================================

    [Header("Round System")]
    [SerializeField] private float roundTransitionDuration = 2f;

    private int currentRound = 1;


    // =========================================================
    // RESULTS
    // =========================================================

    [Header("Match Results")]
    [SerializeField] private MatchResultsUI matchResultsUI;


    // =========================================================
    // STATE
    // =========================================================

    private bool turnInProgress = false;
    private bool gameOver = false;


    // =========================================================
    // PUBLIC PROPERTIES
    // =========================================================

    public PlayerPiece CurrentPlayer
    {
        get
        {
            if (turnOrder.Count == 0)
                return null;

            if (currentTurnIndex < 0 ||
                currentTurnIndex >= turnOrder.Count)
                return null;

            return turnOrder[currentTurnIndex];
        }
    }

    public int CurrentRound
    {
        get { return currentRound; }
    }

    public bool GameOver
    {
        get { return gameOver; }
    }


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        SetupWaypoints();
        SetupPlayers();
    }

    private void Start()
    {
        if (turnOrder.Count == 0)
        {
            Debug.LogWarning(
                "No players were found. Add PlayerPiece objects " +
                "to the Player Slots list."
            );

            if (rollButton != null)
                rollButton.interactable = false;

            return;
        }

        if (waypoints.Count == 0)
        {
            Debug.LogWarning(
                "No waypoints were found."
            );

            if (rollButton != null)
                rollButton.interactable = false;

            return;
        }

        gameOver = false;
        currentRound = 1;
        currentTurnIndex = 0;

        PlacePlayersOnStartingWaypoint();

        // Hide UI that shouldn't initially be visible.
        if (rollNumberText != null)
        {
            rollNumberText.gameObject.SetActive(false);
        }

        if (roundText != null)
        {
            roundText.gameObject.SetActive(false);
        }

        if (rollButton != null)
        {
            rollButton.interactable = false;
        }

        // Show ROUND 1 before the first turn.
        StartCoroutine(ShowRoundTransition());
    }


    // =========================================================
    // SETUP
    // =========================================================

    private void SetupWaypoints()
    {
        waypoints.Clear();

        if (waypointParent == null)
        {
            Debug.LogError(
                "Waypoint Parent has not been assigned!"
            );

            return;
        }

        for (int i = 0;
             i < waypointParent.childCount;
             i++)
        {
            Transform child =
                waypointParent.GetChild(i);

            Waypoint waypoint =
                child.GetComponent<Waypoint>();

            if (waypoint == null)
                continue;

            // Child order determines waypoint number.
            waypoint.waypointNumber =
                waypoints.Count + 1;

            waypoints.Add(waypoint);
        }

        Debug.Log(
            "Loaded " +
            waypoints.Count +
            " waypoints."
        );
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

            if (!turnOrder.Contains(player))
            {
                turnOrder.Add(player);
            }
        }

        // Clear statistics from any previous match.
        foreach (PlayerPiece player in turnOrder)
        {
            player.ClearMatchHistory();
        }

        Debug.Log(
            "Game started with " +
            turnOrder.Count +
            " players."
        );
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


    // =========================================================
    // TURN SYSTEM
    // =========================================================

    private void StartCurrentTurn()
    {
        if (gameOver)
            return;

        if (turnOrder.Count == 0)
            return;

        PlayerPiece player = CurrentPlayer;

        if (player == null)
            return;

        turnInProgress = false;

        Debug.Log(
            "Player " +
            player.PlayerNumber +
            "'s turn. Round " +
            currentRound
        );

        // Current player opaque,
        // everybody else transparent.
        UpdatePlayerVisuals();

        // Camera now follows the current player.
        if (playerCamera != null)
        {
            playerCamera.SetTarget(
                player.transform
            );
        }

        UpdateTilesLeftText();

        // Hide previous player's dice result.
        if (rollNumberText != null)
        {
            rollNumberText.gameObject.SetActive(false);
        }

        // Player may now roll.
        if (rollButton != null)
        {
            rollButton.gameObject.SetActive(true);
            rollButton.interactable = true;
        }
    }

    private void FinishCurrentTurn()
    {
        if (gameOver)
            return;

        currentTurnIndex++;

        // Everyone has now had their turn.
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;

            // Record where everybody ended this round.
            RecordRoundPositions();

            currentRound++;

            StartCoroutine(
                ShowRoundTransition()
            );

            return;
        }

        // Otherwise move immediately to next player.
        StartCurrentTurn();
    }

    private void UpdatePlayerVisuals()
    {
        foreach (PlayerPiece player in turnOrder)
        {
            player.SetActiveTurnVisual(
                player == CurrentPlayer
            );
        }
    }


    // =========================================================
    // DICE
    // =========================================================

    public void RollDice()
    {
        if (gameOver)
            return;

        if (turnInProgress)
            return;

        if (CurrentPlayer == null)
            return;

        // Lock immediately so the button cannot
        // be spammed before the coroutine starts.
        turnInProgress = true;

        if (rollButton != null)
        {
            rollButton.interactable = false;
        }

        PlayerPiece rollingPlayer =
            CurrentPlayer;

        StartCoroutine(
            RollAndPlayTurn(rollingPlayer)
        );
    }

    private IEnumerator RollAndPlayTurn(
        PlayerPiece player
    )
    {
        // -----------------------------------------------------
        // DICE ANIMATION
        // -----------------------------------------------------

        if (rollNumberText != null)
        {
            rollNumberText.gameObject.SetActive(true);
        }

        float elapsed = 0f;

        while (elapsed < diceAnimationDuration)
        {
            int fakeRoll =
                Random.Range(1, 7);

            if (rollNumberText != null)
            {
                rollNumberText.text =
                    fakeRoll.ToString();
            }

            yield return new WaitForSeconds(
                diceNumberChangeSpeed
            );

            elapsed += diceNumberChangeSpeed;
        }

        // The real roll.
        int roll =
            Random.Range(1, 7);

        if (rollNumberText != null)
        {
            rollNumberText.text =
                roll.ToString();
        }

        Debug.Log(
            "Player " +
            player.PlayerNumber +
            " rolled " +
            roll
        );

        yield return new WaitForSeconds(
            finalRollDisplayTime
        );


        // -----------------------------------------------------
        // NORMAL DICE MOVEMENT
        // -----------------------------------------------------

        int targetIndex =
            player.currentWaypointIndex +
            roll;

        // Player cannot move beyond EndWaypoint.
        targetIndex = Mathf.Clamp(
            targetIndex,
            0,
            waypoints.Count - 1
        );

        yield return MovePlayerToWaypoint(
            player,
            targetIndex
        );


        // -----------------------------------------------------
        // CHECK FOR WIN
        // -----------------------------------------------------

        // Reaching the final waypoint immediately wins.
        if (HasPlayerReachedEnd(player))
        {
            EndGame(player);
            yield break;
        }


        // -----------------------------------------------------
        // TILE EFFECTS
        // -----------------------------------------------------

        yield return ResolveTileEffects(
            player
        );


        // Tile effects may have moved the player
        // onto the final waypoint.
        if (HasPlayerReachedEnd(player))
        {
            EndGame(player);
            yield break;
        }


        // -----------------------------------------------------
        // TURN COMPLETE
        // -----------------------------------------------------

        turnInProgress = false;

        FinishCurrentTurn();
    }


    // =========================================================
    // MOVEMENT
    // =========================================================

    private IEnumerator MovePlayerToWaypoint(
        PlayerPiece player,
        int targetIndex
    )
    {
        targetIndex = Mathf.Clamp(
            targetIndex,
            0,
            waypoints.Count - 1
        );

        if (targetIndex ==
            player.currentWaypointIndex)
        {
            yield break;
        }

        int direction =
            targetIndex >
            player.currentWaypointIndex
            ? 1
            : -1;

        while (
            player.currentWaypointIndex !=
            targetIndex
        )
        {
            int nextIndex =
                player.currentWaypointIndex +
                direction;

            Vector3 destination =
                waypoints[nextIndex]
                .transform.position +
                player.tileOffset;

            // Move toward next tile.
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
                        moveSpeed *
                        Time.deltaTime
                    );

                yield return null;
            }

            // Snap exactly onto it.
            player.transform.position =
                destination;

            player.currentWaypointIndex =
                nextIndex;

            // Update Tiles Left as we travel.
            if (player == CurrentPlayer)
            {
                UpdateTilesLeftText();
            }

            // Important:
            // We DO NOT activate a waypoint here.
            // Passing through a special tile does nothing.

            if (pauseBetweenSpaces > 0f)
            {
                yield return new WaitForSeconds(
                    pauseBetweenSpaces
                );
            }
        }
    }


    // =========================================================
    // TILE EFFECTS
    // =========================================================

    private IEnumerator ResolveTileEffects(
        PlayerPiece player
    )
    {
        /*
         * Prevents an infinite loop like:
         *
         * Tile 5 -> Move Forward 2
         * Tile 7 -> Move Back 2
         *
         * forever.
         */
        const int effectSafetyLimit = 20;

        int effectsResolved = 0;

        while (
            effectsResolved <
            effectSafetyLimit
        )
        {
            // Winning always takes priority.
            if (HasPlayerReachedEnd(player))
            {
                yield break;
            }

            Waypoint landedWaypoint =
                waypoints[
                    player.currentWaypointIndex
                ];

            Debug.Log(
                "Player " +
                player.PlayerNumber +
                " landed on Waypoint " +
                landedWaypoint.waypointNumber +
                " (" +
                landedWaypoint.tileType +
                ")"
            );

            int movement =
                landedWaypoint
                .GetMovementEffect();

            // Normal tile.
            if (movement == 0)
            {
                yield break;
            }

            int targetIndex =
                player.currentWaypointIndex +
                movement;

            targetIndex = Mathf.Clamp(
                targetIndex,
                0,
                waypoints.Count - 1
            );

            // Effect cannot actually move player.
            if (targetIndex ==
                player.currentWaypointIndex)
            {
                yield break;
            }

            // Move through the tiles visually.
            yield return MovePlayerToWaypoint(
                player,
                targetIndex
            );

            effectsResolved++;

            /*
             * Loop runs again.
             *
             * This means if one special tile
             * MOVES you onto another special tile,
             * the new destination can trigger too.
             *
             * Tiles merely PASSED THROUGH still
             * never trigger.
             */
        }

        Debug.LogWarning(
            "Tile effect safety limit reached. " +
            "Check for tile effects that loop forever."
        );
    }


    // =========================================================
    // TILES LEFT UI
    // =========================================================

    private void UpdateTilesLeftText()
    {
        if (tilesLeftText == null)
            return;

        if (CurrentPlayer == null ||
            waypoints.Count == 0)
        {
            tilesLeftText.text = "";
            return;
        }

        int tilesLeft =
            (waypoints.Count - 1) -
            CurrentPlayer
            .currentWaypointIndex;

        tilesLeft = Mathf.Max(
            tilesLeft,
            0
        );

        tilesLeftText.text =
            "Left: " +
            tilesLeft;
    }


    // =========================================================
    // ROUND SYSTEM
    // =========================================================

    private IEnumerator ShowRoundTransition()
    {
        if (gameOver)
            yield break;

        turnInProgress = true;

        if (rollButton != null)
        {
            rollButton.interactable = false;
        }

        if (rollNumberText != null)
        {
            rollNumberText.gameObject.SetActive(false);
        }

        // Move camera above whole map.
        if (playerCamera != null &&
            topDownCameraPosition != null)
        {
            playerCamera.SetFixedPosition(
                topDownCameraPosition
            );
        }

        // Show "ROUND X"
        if (roundText != null)
        {
            roundText.text =
                "ROUND " +
                currentRound;

            roundText.gameObject.SetActive(true);
        }

        Debug.Log(
            "Round " +
            currentRound +
            " started."
        );

        yield return new WaitForSeconds(
            roundTransitionDuration
        );

        if (roundText != null)
        {
            roundText.gameObject.SetActive(false);
        }

        turnInProgress = false;

        // Start first player of this round.
        StartCurrentTurn();
    }


    // =========================================================
    // ROUND STATISTICS
    // =========================================================

    private void RecordRoundPositions()
    {
        foreach (PlayerPiece player in turnOrder)
        {
            player.RecordRoundPosition();

            Debug.Log(
                "Round " +
                currentRound +
                " result: Player " +
                player.PlayerNumber +
                " = Waypoint " +
                (
                    player.currentWaypointIndex +
                    1
                )
            );
        }
    }

    private void RecordFinalPositions()
    {
        /*
         * The game may end halfway through a round.
         *
         * We still save one final snapshot so that
         * the graph shows where everybody was when
         * the winner finished.
         */

        foreach (PlayerPiece player in turnOrder)
        {
            player.RecordRoundPosition();
        }
    }


    // =========================================================
    // WIN / GAME OVER
    // =========================================================

    private bool HasPlayerReachedEnd(
        PlayerPiece player
    )
    {
        if (player == null)
            return false;

        if (waypoints.Count == 0)
            return false;

        return
            player.currentWaypointIndex >=
            waypoints.Count - 1;
    }

    private void EndGame(
        PlayerPiece winner
    )
    {
        // Prevent EndGame from running twice.
        if (gameOver)
            return;

        gameOver = true;
        turnInProgress = false;

        Debug.Log(
            "PLAYER " +
            winner.PlayerNumber +
            " IS THE WINNER!"
        );

        // No more rolling.
        if (rollButton != null)
        {
            rollButton.interactable = false;
            rollButton.gameObject.SetActive(false);
        }

        if (rollNumberText != null)
        {
            rollNumberText.gameObject.SetActive(false);
        }

        if (roundText != null)
        {
            roundText.gameObject.SetActive(false);
        }

        // Save board positions at the moment
        // the match ended.
        RecordFinalPositions();

        // Make everybody fully visible again.
        foreach (PlayerPiece player in turnOrder)
        {
            player.SetActiveTurnVisual(true);
        }

        // Open winner/results/chart screen.
        if (matchResultsUI != null)
        {
            matchResultsUI.ShowResults(
                winner,
                turnOrder,
                waypoints.Count
            );
        }
        else
        {
            Debug.LogWarning(
                "No MatchResultsUI has been " +
                "assigned to BoardManager."
            );
        }
    }


    // =========================================================
    // TURN ORDER CHANGING
    // =========================================================

    public void SetTurnOrder(
        List<PlayerPiece> newOrder
    )
    {
        if (newOrder == null)
            return;

        List<PlayerPiece>
            validOrder =
            new List<PlayerPiece>();

        foreach (
            PlayerPiece player
            in newOrder
        )
        {
            if (player == null)
                continue;

            if (!player.gameObject
                .activeInHierarchy)
                continue;

            if (!validOrder.Contains(player))
            {
                validOrder.Add(player);
            }
        }

        /*
         * Make sure a minigame didn't accidentally
         * forget one of the participating players.
         */
        foreach (
            PlayerPiece player
            in turnOrder
        )
        {
            if (player == null)
                continue;

            if (!player.gameObject
                .activeInHierarchy)
                continue;

            if (!validOrder.Contains(player))
            {
                validOrder.Add(player);
            }
        }

        turnOrder = validOrder;

        currentTurnIndex = 0;

        Debug.Log(
            "Turn order has been changed."
        );
    }
}