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

    private readonly List<Waypoint> waypoints =
        new List<Waypoint>();

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

    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("Tiny pause after reaching each waypoint.")]
    [SerializeField] private float pauseBetweenSpaces = 0.08f;
    [Header("Player Turn Positioning")]
    [SerializeField] private float inactivePlayerSideOffset = 0.6f;
    [SerializeField] private float playerShiftSpeed = 5f;

    // =========================================================
    // DICE
    // =========================================================

    [Header("Dice")]
    [SerializeField] private float diceAnimationDuration = 0.6f;
    [SerializeField] private float diceNumberChangeSpeed = 0.06f;

    [Tooltip("How long the final roll stays visible before movement begins.")]
    [SerializeField] private float finalRollDisplayTime = 0.5f;

    // =========================================================
    // SWAP TILE
    // =========================================================

    [Header("Swap Tile")]
    [SerializeField] private float swapAnimationDuration = 1.2f;
    [SerializeField] private float swapNumberChangeSpeed = 0.1f;
    [SerializeField] private float swapResultDisplayTime = 0.8f;

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
    [SerializeField] private TMP_Text turnOrderText;

    [Header("Skip Turn")]
    [SerializeField] private float skippedDisplayDuration = 1.5f;

    // =========================================================
    // MINIGAME
    // =========================================================

    [Header("Minigame")]
    [SerializeField] private TMP_Text minigameText;
    [SerializeField] private float minigameNameChangeSpeed = 0.1f;
    [SerializeField] private float minigameAnimationDuration = 1.5f;
    [SerializeField] private float minigameSelectedDisplayTime = 1.5f;

    private readonly List<string> availableMinigames =
        new List<string>();

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
            {
                return null;
            }

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
        FindMinigameScenes();
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

        HideGameplayUI();

        // Coming back from a minigame:
        // restore positions/history/order and begin this round's board turns.
        if (MatchData.Instance != null &&
            MatchData.Instance.returningFromMinigame)
        {
            Debug.Log(
                "Returning from minigame. Restoring board state."
            );

            RestoreBoardStateFromMinigame();

            MatchData.Instance.returningFromMinigame = false;
            MatchData.Instance.minigameTransitionStarted = false;

            StartCurrentTurn();
            return;
        }

        // Fresh match.
        currentRound = 1;
        currentTurnIndex = 0;

        if (MatchData.Instance != null)
        {
            MatchData.Instance.currentRound =
                currentRound;
        }

        PlacePlayersOnStartingWaypoint();

        if (rollButton != null)
            rollButton.interactable = false;

        // Every round, including Round 1, announces the round
        // and then launches a minigame before board turns begin.
        StartCoroutine(
            ShowRoundTransition()
        );
    }

    private void Update()
    {
        CheckCurrentPlayerRollInput();
    }

    private void HideGameplayUI()
    {
        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (roundText != null)
            roundText.gameObject.SetActive(false);

        if (minigameText != null)
            minigameText.gameObject.SetActive(false);

        if (swapText != null)
            swapText.gameObject.SetActive(false);

        if (skippedText != null)
            skippedText.gameObject.SetActive(false);
    }

    // =========================================================
    // CONTROLLER INPUT
    // =========================================================

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

        // Normal matchmaking path.
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
                turnOrder.Add(player);
        }

#if UNITY_EDITOR
        // Editor fallback when testing the board scene directly.
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
                    turnOrder.Add(player);
            }
        }
#endif

        // New scene instance starts with empty history.
        // If this is a minigame return, RestoreBoardStateFromMinigame()
        // restores the saved history immediately afterward.
        foreach (PlayerPiece player in turnOrder)
            player.ClearMatchHistory();

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
        UpdatePlayerTurnPositions();
        UpdateTurnOrderText();

        if (playerCamera != null)
        {
            playerCamera.SetTarget(
                player.transform
            );
        }

        UpdateTilesLeftText();

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

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

        // Everyone has completed one board turn.
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;

            RecordRoundPositions();

            currentRound++;

            if (MatchData.Instance != null)
            {
                MatchData.Instance.currentRound =
                    currentRound;
            }

            StartCoroutine(
                ShowRoundTransition()
            );

            return;
        }

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
    private void UpdatePlayerTurnPositions()
    {
        foreach (PlayerPiece player in turnOrder)
        {
            if (player == null)
                continue;

            if (player.currentWaypointIndex < 0 ||
                player.currentWaypointIndex >= waypoints.Count)
            {
                continue;
            }

            Vector3 basePosition =
                waypoints[player.currentWaypointIndex]
                    .transform.position +
                player.tileOffset;

            // Current player stays centered.
            if (player == CurrentPlayer)
            {
                StartCoroutine(
                    MovePlayerVisualToPosition(
                        player,
                        basePosition
                    )
                );

                continue;
            }

            // Other players move slightly to the side.
            Vector3 sideOffset =
                Vector3.right *
                inactivePlayerSideOffset;

            StartCoroutine(
                MovePlayerVisualToPosition(
                    player,
                    basePosition + sideOffset
                )
            );
        }
    }
    private IEnumerator MovePlayerVisualToPosition(
    PlayerPiece player,
    Vector3 targetPosition
)
    {
        while (
            Vector3.Distance(
                player.transform.position,
                targetPosition
            ) > 0.01f
        )
        {
            player.transform.position =
                Vector3.MoveTowards(
                    player.transform.position,
                    targetPosition,
                    playerShiftSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        player.transform.position =
            targetPosition;
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

        turnInProgress = true;

        if (rollButton != null)
            rollButton.interactable = false;

        PlayerPiece rollingPlayer =
            CurrentPlayer;

        StartCoroutine(
            RollAndPlayTurn(rollingPlayer)
        );
    }

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
            case 0:
                return 6;

            case 1:
                return 3;

            case 2:
                return 2;

            default:
                return 0;
        }
    }

    private IEnumerator RollAndPlayTurn(
        PlayerPiece player
    )
    {
        int bonusDiceMax =
            GetBonusDiceMax(player);

        bool hasBonusDice =
            bonusDiceMax > 0;

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(true);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(hasBonusDice);

        float elapsed = 0f;

        while (elapsed < diceAnimationDuration)
        {
            int fakeMainRoll =
                Random.Range(1, 7);

            if (rollNumberText != null)
            {
                rollNumberText.text =
                    fakeMainRoll.ToString();
            }

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

        int mainRoll =
            Random.Range(1, 7);

        int bonusRoll = 0;

        if (hasBonusDice)
        {
            bonusRoll =
                Random.Range(
                    1,
                    bonusDiceMax + 1
                );
        }

        if (rollNumberText != null)
            rollNumberText.text = mainRoll.ToString();

        if (bonusRollNumberText != null &&
            hasBonusDice)
        {
            bonusRollNumberText.text =
                bonusRoll.ToString();
        }

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

        yield return new WaitForSecondsRealtime(
            finalRollDisplayTime
        );

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

        if (HasPlayerReachedEnd(player))
        {
            EndGame(player);
            yield break;
        }

        yield return ResolveTileEffects(
            player
        );

        if (HasPlayerReachedEnd(player))
        {
            EndGame(player);
            yield break;
        }

        turnInProgress = false;

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

            // =========================================
            // FACE THE NEXT TILE
            // =========================================

            Vector3 lookDirection =
                destination -
                player.transform.position;

            // Ignore vertical difference so the
            // character stays standing upright.
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                player.transform.rotation =
                    Quaternion.LookRotation(
                        lookDirection
                    );
            }

            // =========================================
            // MOVE TO NEXT TILE
            // =========================================

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

            // Snap exactly onto the waypoint.
            player.transform.position =
                destination;

            player.currentWaypointIndex =
                nextIndex;

            if (player == CurrentPlayer)
            {
                UpdateTilesLeftText();
            }

            if (pauseBetweenSpaces > 0f)
            {
                yield return new WaitForSecondsRealtime(
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

            // Roll again.
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

            // Swap with a random other player.
            if (landedWaypoint.tileType ==
                TileType.SwapWithRandomPlayer)
            {
                yield return SwapWithRandomPlayer(
                    player
                );

                effectsResolved++;
                continue;
            }

            // Skip the next player in the CURRENT turn order.
            // If this player is last, nothing happens.
            if (landedWaypoint.tileType ==
                TileType.SkipNextTurn)
            {
                GiveSkipToNextPlayer(player);
                yield break;
            }

            // Forward/backward tiles.
            int movement =
                landedWaypoint.GetMovementEffect();

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

        if (possiblePlayers.Count == 0)
        {
            Debug.Log(
                "No player available to swap with."
            );

            yield break;
        }

        if (swapText != null)
            swapText.gameObject.SetActive(true);

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
                    "SWAPPING WITH...\nPLAYER " +
                    fakePlayer.PlayerNumber;
            }

            yield return new WaitForSecondsRealtime(
                swapNumberChangeSpeed
            );

            elapsed +=
                swapNumberChangeSpeed;
        }

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

        int currentPlayerOldIndex =
            currentPlayer.currentWaypointIndex;

        int otherPlayerOldIndex =
            otherPlayer.currentWaypointIndex;

        currentPlayer.currentWaypointIndex =
            otherPlayerOldIndex;

        otherPlayer.currentWaypointIndex =
            currentPlayerOldIndex;

        currentPlayer.transform.position =
            waypoints[
                currentPlayer.currentWaypointIndex
            ].transform.position +
            currentPlayer.tileOffset;

        otherPlayer.transform.position =
            waypoints[
                otherPlayer.currentWaypointIndex
            ].transform.position +
            otherPlayer.tileOffset;

        UpdateTilesLeftText();

        if (swapText != null)
            swapText.gameObject.SetActive(false);
    }

    // =========================================================
    // SKIP TILE
    // =========================================================

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

        // Do not wrap to the first player.
        if (playerTurnIndex >=
            turnOrder.Count - 1)
        {
            Debug.Log(
                "Player " +
                player.PlayerNumber +
                " landed on Skip Next Turn, " +
                "but they are last in the turn order. " +
                "Nothing happens."
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

    private IEnumerator ShowSkippedTurn(
        PlayerPiece skippedPlayer
    )
    {
        turnInProgress = true;

        if (rollButton != null)
        {
            rollButton.interactable = false;
            rollButton.gameObject.SetActive(false);
        }

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

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

            skippedText.gameObject.SetActive(true);
        }

        Debug.Log(
            "Showing SKIPPED for Player " +
            skippedPlayer.PlayerNumber
        );

        yield return new WaitForSecondsRealtime(
            skippedDisplayDuration
        );

        if (skippedText != null)
            skippedText.gameObject.SetActive(false);

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
            CurrentPlayer.currentWaypointIndex;

        tilesLeft = Mathf.Max(
            tilesLeft,
            0
        );

        tilesLeftText.text =
            "Left: " +
            tilesLeft;
    }
    private void UpdateTurnOrderText()
    {
        if (turnOrderText == null)
            return;

        if (turnOrder.Count == 0)
        {
            turnOrderText.text = "";
            return;
        }

        string text =
            "ROUND " +
            currentRound +
            " TURN ORDER\n\n";

        for (int i = 0;
             i < turnOrder.Count;
             i++)
        {
            PlayerPiece player =
                turnOrder[i];

            if (player == null)
                continue;

            if (i == currentTurnIndex)
                text += "▶ ";
            else
                text += "   ";

            text +=
                (i + 1) +
                ". Player " +
                player.PlayerNumber;

            switch (i)
            {
                case 0:
                    text += "   +D6";
                    break;

                case 1:
                    text += "   +D3";
                    break;

                case 2:
                    text += "   +D2";
                    break;
            }

            if (i <
                turnOrder.Count - 1)
            {
                text += "\n";
            }
        }

        turnOrderText.text =
            text;
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
            rollButton.gameObject.SetActive(false);
        }

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (playerCamera != null &&
            topDownCameraPosition != null)
        {
            playerCamera.SetFixedPosition(
                topDownCameraPosition
            );
        }

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

        yield return new WaitForSecondsRealtime(
            roundTransitionDuration
        );

        if (roundText != null)
            roundText.gameObject.SetActive(false);

        turnInProgress = false;

        // Minigame happens before EVERY round,
        // including Round 1.
        StartCoroutine(
            StartMinigameSequence()
        );
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
                (player.currentWaypointIndex + 1)
            );
        }
    }

    private void RecordFinalPositions()
    {
        foreach (PlayerPiece player in turnOrder)
            player.RecordRoundPosition();
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
        if (gameOver)
            return;

        gameOver = true;
        turnInProgress = false;

        Debug.Log(
            "PLAYER " +
            winner.PlayerNumber +
            " IS THE WINNER!"
        );

        if (rollButton != null)
        {
            rollButton.interactable = false;
            rollButton.gameObject.SetActive(false);
        }

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (roundText != null)
            roundText.gameObject.SetActive(false);

        if (minigameText != null)
            minigameText.gameObject.SetActive(false);

        if (swapText != null)
            swapText.gameObject.SetActive(false);

        if (skippedText != null)
            skippedText.gameObject.SetActive(false);

        RecordFinalPositions();

        foreach (PlayerPiece player in turnOrder)
            player.SetActiveTurnVisual(true);

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
                "No MatchResultsUI has been assigned to BoardManager."
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

        List<PlayerPiece> validOrder =
            new List<PlayerPiece>();

        foreach (PlayerPiece player in newOrder)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            if (!validOrder.Contains(player))
                validOrder.Add(player);
        }

        foreach (PlayerPiece player in turnOrder)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            if (!validOrder.Contains(player))
                validOrder.Add(player);
        }

        turnOrder = validOrder;
        currentTurnIndex = 0;

        Debug.Log(
            "Turn order has been changed."
        );
    }

    // =========================================================
    // MINIGAMES
    // =========================================================

    private void FindMinigameScenes()
    {
        availableMinigames.Clear();

        int sceneCount =
            UnityEngine.SceneManagement
                .SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath =
                UnityEngine.SceneManagement
                    .SceneUtility
                    .GetScenePathByBuildIndex(i);

            if (string.IsNullOrEmpty(scenePath))
                continue;

            if (!scenePath.Contains(
                "Scenes/Minigame/"
            ))
            {
                continue;
            }

            string sceneName =
                System.IO.Path
                    .GetFileNameWithoutExtension(
                        scenePath
                    );

            if (!availableMinigames.Contains(sceneName))
                availableMinigames.Add(sceneName);
        }

        Debug.Log(
            "Found " +
            availableMinigames.Count +
            " minigames."
        );
    }

    /// <summary>
    /// Saves the board before loading a minigame.
    ///
    /// IMPORTANT:
    /// playerWaypointPositions is stored by STABLE PlayerNumber:
    /// index 0 = Player 1
    /// index 1 = Player 2
    /// index 2 = Player 3
    /// index 3 = Player 4
    ///
    /// We intentionally DO NOT overwrite playerOrderNumbers here,
    /// because MinigameResultBridge uses that list to store the
    /// new 1st/2nd/3rd/4th place order.
    /// </summary>
    private void SaveBoardState()
    {
        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "No MatchData object exists!"
            );

            return;
        }

        // -----------------------------------------
        // SAVE POSITIONS BY PLAYER NUMBER
        // -----------------------------------------

        MatchData.Instance.playerWaypointPositions.Clear();

        // Four fixed slots:
        // [0] P1, [1] P2, [2] P3, [3] P4
        for (int i = 0; i < 4; i++)
        {
            MatchData.Instance
                .playerWaypointPositions.Add(-1);
        }

        foreach (PlayerPiece player in playerSlots)
        {
            if (player == null)
                continue;

            int positionSlot =
                player.PlayerNumber - 1;

            if (positionSlot < 0 ||
                positionSlot >=
                MatchData.Instance
                    .playerWaypointPositions.Count)
            {
                continue;
            }

            MatchData.Instance
                .playerWaypointPositions[positionSlot] =
                player.currentWaypointIndex;
        }

        // -----------------------------------------
        // SAVE ROUND HISTORY
        // -----------------------------------------

        MatchData.Instance
            .historyPlayerNumbers.Clear();

        MatchData.Instance
            .playerRoundHistories.Clear();

        foreach (PlayerPiece player in playerSlots)
        {
            if (player == null)
                continue;

            MatchData.Instance
                .historyPlayerNumbers.Add(
                    player.PlayerNumber
                );

            MatchData.Instance
                .playerRoundHistories.Add(
                    new List<int>(
                        player.roundWaypointHistory
                    )
                );
        }

        MatchData.Instance.currentRound =
            currentRound;

        Debug.Log(
            "Saved board positions/history for minigame."
        );
    }

    private IEnumerator StartMinigameSequence()
    {
        if (availableMinigames.Count == 0)
        {
            Debug.LogError(
                "No minigame scenes were found. " +
                "Make sure they are in Assets/Scenes/Minigame " +
                "and included in Build Settings."
            );

            yield break;
        }

        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "MatchData is missing. Cannot start minigame."
            );

            yield break;
        }

        turnInProgress = true;

        if (rollButton != null)
        {
            rollButton.interactable = false;
            rollButton.gameObject.SetActive(false);
        }

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (minigameText != null)
        {
            minigameText.text =
                "MINIGAME";

            minigameText.gameObject.SetActive(true);
        }

        Debug.Log(
            "Minigame starting."
        );

        yield return new WaitForSecondsRealtime(
            1f
        );

        float elapsed = 0f;

        while (elapsed < minigameAnimationDuration)
        {
            string randomName =
                availableMinigames[
                    Random.Range(
                        0,
                        availableMinigames.Count
                    )
                ];

            if (minigameText != null)
                minigameText.text = randomName;

            yield return new WaitForSecondsRealtime(
                minigameNameChangeSpeed
            );

            elapsed +=
                minigameNameChangeSpeed;
        }

        string selectedMinigame =
            availableMinigames[
                Random.Range(
                    0,
                    availableMinigames.Count
                )
            ];

        Debug.Log(
            "Selected minigame: " +
            selectedMinigame
        );

        if (minigameText != null)
            minigameText.text = selectedMinigame;

        MatchData.Instance.selectedMinigameScene =
            selectedMinigame;

        // Save position/history BEFORE leaving board.
        SaveBoardState();

        yield return new WaitForSecondsRealtime(
            minigameSelectedDisplayTime
        );

        UnityEngine.SceneManagement
            .SceneManager.LoadScene(
                selectedMinigame
            );
    }

    /// <summary>
    /// Called after MinigameResultBridge loads the board again.
    /// Restores:
    /// - current round
    /// - board positions
    /// - chart history
    /// - turn order from the minigame placements
    private void RestoreBoardStateFromMinigame()
    {
        if (MatchData.Instance == null)
        {
            Debug.LogError(
                "MatchData does not exist."
            );

            return;
        }

        currentRound =
            MatchData.Instance.currentRound;

        // -----------------------------------------
        // RESTORE POSITIONS BY STABLE PLAYER NUMBER
        // -----------------------------------------

        foreach (PlayerPiece player in playerSlots)
        {
            if (player == null)
                continue;

            int positionSlot =
                player.PlayerNumber - 1;

            if (positionSlot >= 0 &&
                positionSlot <
                MatchData.Instance
                    .playerWaypointPositions.Count)
            {
                int savedPositionIndex =
                    MatchData.Instance
                        .playerWaypointPositions[positionSlot];

                if (savedPositionIndex >= 0)
                {
                    savedPositionIndex =
                        Mathf.Clamp(
                            savedPositionIndex,
                            0,
                            waypoints.Count - 1
                        );

                    player.currentWaypointIndex =
                        savedPositionIndex;

                    player.transform.position =
                        waypoints[savedPositionIndex]
                            .transform.position +
                        player.tileOffset;

                }
                UpdateTurnOrderText();
            }

            // -----------------------------------------
            // RESTORE CHART HISTORY
            // -----------------------------------------

            player.roundWaypointHistory.Clear();

            for (int i = 0;
                 i <
                 MatchData.Instance
                     .historyPlayerNumbers.Count;
                 i++)
            {
                if (
                    MatchData.Instance
                        .historyPlayerNumbers[i]
                    != player.PlayerNumber
                )
                {
                    continue;
                }

                if (
                    i <
                    MatchData.Instance
                        .playerRoundHistories.Count
                )
                {
                    player.roundWaypointHistory.AddRange(
                        MatchData.Instance
                            .playerRoundHistories[i]
                    );
                }

                break;
            }

            Debug.Log(
                "Restored Player " +
                player.PlayerNumber +
                " to Waypoint " +
                (player.currentWaypointIndex + 1) +
                " with " +
                player.roundWaypointHistory.Count +
                " history entries."
            );
        }

        // -----------------------------------------
        // REBUILD TURN ORDER FROM MINIGAME RESULTS
        // -----------------------------------------

        List<PlayerPiece> newOrder =
            new List<PlayerPiece>();

        foreach (
            int playerNumber
            in MatchData.Instance.playerOrderNumbers
        )
        {
            PlayerPiece matchingPlayer =
                FindPlayerByNumber(
                    playerNumber
                );

            if (matchingPlayer == null)
                continue;

            if (!matchingPlayer.gameObject.activeInHierarchy)
                continue;

            if (!newOrder.Contains(matchingPlayer))
                newOrder.Add(matchingPlayer);
        }

        foreach (PlayerPiece player in turnOrder)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            if (!newOrder.Contains(player))
                newOrder.Add(player);
        }

        turnOrder = newOrder;
        currentTurnIndex = 0;

        Debug.Log(
            "Restored turn order:"
        );

        for (int i = 0;
             i < turnOrder.Count;
             i++)
        {
            Debug.Log(
                (i + 1) +
                " place: Player " +
                turnOrder[i].PlayerNumber
            );
        }
    }

    private PlayerPiece FindPlayerByNumber(
        int playerNumber
    )
    {
        foreach (PlayerPiece player in playerSlots)
        {
            if (player == null)
                continue;

            if (player.PlayerNumber ==
                playerNumber)
            {
                return player;
            }
        }

        return null;
    }
}
