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
    private bool extraRollGranted = false;

    [Header("Swap Tile")]
    [SerializeField] private float swapAnimationDuration = 1.2f;
    [SerializeField] private float swapNumberChangeSpeed = 0.1f;
    [SerializeField] private float swapResultDisplayTime = 0.8f;

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
    [SerializeField] private TMP_Text bonusRollNumberText;

    [SerializeField] private TMP_Text roundText;

    [SerializeField] private TMP_Text swapText;

    [SerializeField] private TMP_Text skippedText;

    [Header("Skip Turn")]
    [SerializeField] private float skippedDisplayDuration = 1.2f;




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

    [HideInInspector] public bool turnInProgress = false;
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

        StartCoroutine(ShowRoundTransition());

        if (skippedText != null)
            skippedText.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckCurrentPlayerRollInput();
    }

    private void CheckCurrentPlayerRollInput()
    {
        if (gameOver)
            return;

        if (turnInProgress)
            return;

        if (CurrentPlayer == null)
            return;

        int playerId =
            CurrentPlayer.ControllerPlayerId;

        if (playerId < 1 || playerId > 4)
        {
            Debug.LogWarning(
                "Invalid Controller Player ID: " +
                playerId
            );

            return;
        }

        if (!LokaalConnecter.plrsController.ContainsKey(playerId))
            return;

        LokaalConnecter.PlayerController controller =
            LokaalConnecter.plrsController[playerId];

        if (controller == null)
            return;

        if (!controller.occuplied)
            return;

        if (controller.GetButtonDown(
            LokaalConnecter.InputType.jump))
        {
            RollDice();
        }
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

        // First try to find players who actually
        // joined through matchmaking.
        foreach (PlayerPiece player in playerSlots)
        {
            if (player == null)
                continue;

            int playerId =
                player.ControllerPlayerId;

            if (playerId < 1 || playerId > 4)
            {
                Debug.LogWarning(
                    player.name +
                    " has an invalid Controller Player ID: " +
                    playerId
                );

                continue;
            }

            if (!LokaalConnecter.plrsController.ContainsKey(playerId))
                continue;

            LokaalConnecter.PlayerController controller =
                LokaalConnecter.plrsController[playerId];

            if (controller == null)
                continue;

            if (!controller.occuplied)
                continue;

            if (!turnOrder.Contains(player))
            {
                turnOrder.Add(player);
            }
        }

        /*
         * When testing the board scene directly
         * in the Unity Editor, matchmaking may
         * not have happened yet.
         *
         * In that case, use the Player Slots
         * from the Inspector instead.
         */
#if UNITY_EDITOR

        if (turnOrder.Count == 0)
        {
            Debug.LogWarning(
                "No matchmaking players found. " +
                "Using Player Slots for Editor testing."
            );

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
        }

#endif

        // Clear statistics from an old match.
        foreach (PlayerPiece player in turnOrder)
        {
            player.ClearMatchHistory();
        }

        Debug.Log(
            "Board game started with " +
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

        PlayerPiece player =
            CurrentPlayer;

        if (player == null)
            return;

        turnInProgress = false;

        Debug.Log(
            "Player " +
            player.PlayerNumber +
            "'s turn. Round " +
            currentRound
        );

        UpdatePlayerVisuals();

        if (playerCamera != null)
        {
            playerCamera.SetTarget(
                player.transform
            );
        }

        UpdateTilesLeftText();

        if (rollNumberText != null)
        {
            rollNumberText.gameObject
                .SetActive(false);

            if (bonusRollNumberText != null)
            {
                bonusRollNumberText.gameObject.SetActive(false);
            }
        }

        // =========================================
        // CHECK IF THIS PLAYER MUST BE SKIPPED
        // =========================================

        if (player.ConsumeSkipNextTurn())
        {
            Debug.Log(
                "Skip detected for Player " +
                player.PlayerNumber
            );

            StartCoroutine(
                ShowSkippedTurn(player)
            );

            return;
        }

        // =========================================
        // NORMAL TURN
        // =========================================

        if (rollButton != null)
        {
            rollButton.gameObject
                .SetActive(true);

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

    // ==============================================
    // BONUSDICE
    // ==============================================

    private int GetBonusDiceMax(
    PlayerPiece player
)
    {
        if (player == null)
            return 0;

        int place =
            turnOrder.IndexOf(player);

        switch (place)
        {
            // 1st place
            case 0:
                return 6;

            // 2nd place
            case 1:
                return 3;

            // 3rd place
            case 2:
                return 2;

            // 4th place
            default:
                return 0;
        }
    }

    private IEnumerator RollAndPlayTurn(
    PlayerPiece player
)
    {
        // =========================================
        // FIND THIS PLAYER'S BONUS DIE
        // =========================================

        int bonusDiceMax =
            GetBonusDiceMax(player);

        bool hasBonusDice =
            bonusDiceMax > 0;

        // =========================================
        // SHOW DICE UI
        // =========================================

        if (rollNumberText != null)
        {
            rollNumberText.gameObject
                .SetActive(true);
        }

        if (bonusRollNumberText != null)
        {
            bonusRollNumberText.gameObject
                .SetActive(hasBonusDice);
        }

        // =========================================
        // DICE ANIMATION
        // =========================================

        float elapsed = 0f;

        while (elapsed < diceAnimationDuration)
        {
            // Main die always rolls 1-6.
            int fakeMainRoll =
                Random.Range(1, 7);

            if (rollNumberText != null)
            {
                rollNumberText.text =
                    fakeMainRoll.ToString();
            }

            // Animate bonus die if this player has one.
            if (hasBonusDice)
            {
                int fakeBonusRoll =
                    Random.Range(
                        1,
                        bonusDiceMax + 1
                    );

                if (bonusRollNumberText != null)
                {
                    bonusRollNumberText.text =
                        fakeBonusRoll.ToString();
                }
            }

            yield return new WaitForSecondsRealtime(
                diceNumberChangeSpeed
            );

            elapsed +=
                diceNumberChangeSpeed;
        }

        // =========================================
        // FINAL MAIN DIE
        // =========================================

        int mainRoll =
            Random.Range(1, 7);

        // =========================================
        // FINAL BONUS DIE
        // =========================================

        int bonusRoll = 0;

        if (hasBonusDice)
        {
            bonusRoll =
                Random.Range(
                    1,
                    bonusDiceMax + 1
                );
        }

        // =========================================
        // DISPLAY FINAL RESULTS
        // =========================================

        if (rollNumberText != null)
        {
            rollNumberText.text =
                mainRoll.ToString();
        }

        if (bonusRollNumberText != null &&
            hasBonusDice)
        {
            bonusRollNumberText.text =
                bonusRoll.ToString();
        }

        // Add both dice together
        int totalRoll =
            mainRoll + bonusRoll;

        Debug.Log(
            "Player " +
            player.PlayerNumber +
            " rolled " +
            mainRoll +
            " + " +
            bonusRoll +
            " = " +
            totalRoll
        );

        // Let player see the final dice.
        yield return new WaitForSecondsRealtime(
            finalRollDisplayTime
        );

        // =========================================
        // MOVE PLAYER
        // =========================================

        int targetIndex =
            player.currentWaypointIndex +
            totalRoll;

        targetIndex = Mathf.Clamp(
            targetIndex,
            0,
            waypoints.Count - 1
        );

        yield return MovePlayerToWaypoint(
            player,
            targetIndex
        );

        // =========================================
        // CHECK FOR WIN
        // =========================================

        if (HasPlayerReachedEnd(player))
        {
            EndGame(player);
            yield break;
        }

        // =========================================
        // TILE EFFECTS
        // =========================================

        yield return ResolveTileEffects(
            player
        );

        // Tile effect may have moved them
        // onto the final waypoint.
        if (HasPlayerReachedEnd(player))
        {
            EndGame(player);
            yield break;
        }

        // =========================================
        // FINISH TURN
        // =========================================

        turnInProgress = false;

        // Roll Again tile support.
        if (extraRollGranted)
        {
            extraRollGranted = false;

            Debug.Log(
                "Player " +
                player.PlayerNumber +
                " gets to roll again!"
            );

            StartCurrentTurn();

            yield break;
        }

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
        const int effectSafetyLimit = 20;
        int effectsResolved = 0;

        while (effectsResolved < effectSafetyLimit)
        {
            if (HasPlayerReachedEnd(player))
                yield break;

            Waypoint landedWaypoint =
                waypoints[player.currentWaypointIndex];

            Debug.Log(
                "Player " +
                player.PlayerNumber +
                " landed on Waypoint " +
                landedWaypoint.waypointNumber +
                " (" +
                landedWaypoint.tileType +
                ")"
            );

            // =========================================
            // ROLL AGAIN
            // =========================================

            if (landedWaypoint.tileType ==
                TileType.RollAgain)
            {
                Debug.Log(
                    "Player " +
                    player.PlayerNumber +
                    " earned another roll!"
                );

                extraRollGranted = true;

                yield break;
            }

            // =========================================
            // SWAP WITH RANDOM PLAYER
            // =========================================

            if (landedWaypoint.tileType ==
                TileType.SwapWithRandomPlayer)
            {
                yield return SwapWithRandomPlayer(player);

                effectsResolved++;
                continue;
            }

            // =========================================
            // SKIP NEXT PLAYER
            // =========================================

            if (landedWaypoint.tileType ==
                TileType.SkipNextTurn)
            {
                GiveSkipToNextPlayer(player);

                yield break;
            }

            // =========================================
            // MOVE FORWARD / MOVE BACK
            // =========================================

            int movement =
                landedWaypoint.GetMovementEffect();

            // Normal tile.
            if (movement == 0)
                yield break;

            int targetIndex =
                player.currentWaypointIndex +
                movement;

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

            yield return MovePlayerToWaypoint(
                player,
                targetIndex
            );

            effectsResolved++;
        }

        Debug.LogWarning(
            "Tile effect safety limit reached. " +
            "Check for tile effects that loop forever."
        );
    }
    private IEnumerator SwapWithRandomPlayer(
     PlayerPiece currentPlayer
 )
    {
        List<PlayerPiece> possiblePlayers =
            new List<PlayerPiece>();

        foreach (PlayerPiece player in turnOrder)
        {
            if (player == null)
                continue;

            if (player == currentPlayer)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            possiblePlayers.Add(player);
        }

        // No other player exists.
        if (possiblePlayers.Count == 0)
        {
            Debug.Log(
                "No player available to swap with."
            );

            yield break;
        }

        // =========================================
        // SWAP ANIMATER
        // =========================================

        if (swapText != null)
        {
            swapText.gameObject.SetActive(true);
        }

        float elapsed = 0f;

        while (elapsed < swapAnimationDuration)
        {
            PlayerPiece fakePlayer =
                possiblePlayers[
                    Random.Range(
                        0,
                        possiblePlayers.Count
                    )
                ];

            if (swapText != null)
            {
                swapText.text =
                    "SWAPPING WITH...\n" +
                    "PLAYER " +
                    fakePlayer.PlayerNumber;
            }
            yield return new WaitForSecondsRealtime(
                swapNumberChangeSpeed
            );

            elapsed +=
                swapNumberChangeSpeed;
        }

        // =========================================
        // PICKS REAL PLAYERS
        // =========================================

        PlayerPiece otherPlayer =
            possiblePlayers[
                Random.Range(
                    0,
                    possiblePlayers.Count
                )
            ];

        if (swapText != null)
        {
            swapText.text =
                "SWAP WITH\nPLAYER " +
                otherPlayer.PlayerNumber +
                "!";
        }

        Debug.Log(
            "Player " +
            currentPlayer.PlayerNumber +
            " is swapping with Player " +
            otherPlayer.PlayerNumber
        );
        yield return new WaitForSecondsRealtime(
            swapResultDisplayTime
        );

        // =========================================
        // SAVE OLD POSITIONS
        // =========================================

        int currentPlayerOldIndex =
            currentPlayer.currentWaypointIndex;

        int otherPlayerOldIndex =
            otherPlayer.currentWaypointIndex;

        // =========================================
        // SWAP LOGICAL POSITIONS
        // =========================================

        currentPlayer.currentWaypointIndex =
            otherPlayerOldIndex;

        otherPlayer.currentWaypointIndex =
            currentPlayerOldIndex;

        // =========================================
        // SWAP VISUAL POSITIONS
        // =========================================

        Vector3 currentDestination =
            waypoints[
                currentPlayer.currentWaypointIndex
            ].transform.position +
            currentPlayer.tileOffset;

        Vector3 otherDestination =
            waypoints[
                otherPlayer.currentWaypointIndex
            ].transform.position +
            otherPlayer.tileOffset;

        currentPlayer.transform.position =
            currentDestination;

        otherPlayer.transform.position =
            otherDestination;

        UpdateTilesLeftText();

        if (swapText != null)
        {
            swapText.gameObject.SetActive(false);
        }

    }
    // =========================================
    // SKIP MANAGER
    // =========================================
    private void GiveSkipToNextPlayer(
    PlayerPiece player
)
    {
        if (player == null)
            return;

        int playerTurnIndex =
            turnOrder.IndexOf(player);

        if (playerTurnIndex < 0)
            return;

        if (playerTurnIndex >=
            turnOrder.Count - 1)
        {
            Debug.Log(
                "Player " +
                player.PlayerNumber +
                " landed on Skip Next Turn, " +
                "but they are last in the turn order.. " +
                "nothing happens."
            );

            return;
        }

        PlayerPiece playerToSkip =
            turnOrder[playerTurnIndex + 1];

        if (playerToSkip == null)
            return;

        playerToSkip.GiveSkipNextTurn();

        Debug.Log(
            "Player " +
            player.PlayerNumber +
            " caused Player " +
            playerToSkip.PlayerNumber +
            " to skip their next turn."
        );
    }
    // =========================================
    // SKIP UI ANIMATION (SUBJECT TO CHANGE/EVISCERATION)
    // =========================================
    private IEnumerator ShowSkippedTurn(
    PlayerPiece skippedPlayer
)
    {
        turnInProgress = true;

        if (rollButton != null)
        {
            rollButton.interactable = false;
        }

        if (rollNumberText != null)
        {
            rollNumberText.gameObject
                .SetActive(false);
        }

        if (playerCamera != null)
        {
            playerCamera.SetTarget(
                skippedPlayer.transform
            );
        }

        if (skippedText != null)
        {
            skippedText.text =
                "SKIPPED";

            skippedText.gameObject
                .SetActive(true);
        }

        Debug.Log(
            "Showing SKIPPED for Player " +
            skippedPlayer.PlayerNumber
        );

        yield return new WaitForSecondsRealtime(
            skippedDisplayDuration
        );

        if (skippedText != null)
        {
            skippedText.gameObject
                .SetActive(false);
        }

        turnInProgress = false;

        FinishCurrentTurn();
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

        // IMPORTANT:
        // Realtime means matchmaking's Time.timeScale = 0
        // cannot freeze the round screen.
        yield return new WaitForSecondsRealtime(
            roundTransitionDuration
        );

        if (roundText != null)
        {
            roundText.gameObject.SetActive(false);
        }

        turnInProgress = false;

        if (bonusRollNumberText != null)
        {
            bonusRollNumberText.gameObject.SetActive(false);
        }

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

        RecordFinalPositions();

        foreach (PlayerPiece player in turnOrder)
        {
            player.SetActiveTurnVisual(true);
        }

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
        if (bonusRollNumberText != null)
        {
            bonusRollNumberText.gameObject.SetActive(false);
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