using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChampionGui : MinigameWinnerGUI
{
    [Header("UI")]
    [SerializeField] protected GameObject startNavigation;
    [SerializeField] protected RectTransform chartFrame;
    [SerializeField] private CanvasGroup infoOverlay;
    protected CanvasGroup canvasGroup;

    [Header("Chart Appearance")]
    [SerializeField] private float lineThickness = 5f;
    [SerializeField] private float pointSize = 12f;

    [SerializeField]
    private Color[] chartPlayerColors =
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow
    };

    private List<GameObject> generatedChartObjects =
        new List<GameObject>();

    //info for places
    [SerializeField] private List<PlayerPiece> playerPieces;
    [HideInInspector] public int totalWaypoints = 120;

    //configs
    const float guiSwitchingDur = 1.2f;
    const float lookTime = 2f;

    protected override IEnumerator DelayToTurnOffAndLoad()
    {
        yield return null;
        guiLighting.enabled = true;
        if (minigameLighting != null) minigameLighting.enabled = false;
        canvas.enabled = true; //make sure that it loaded
        
        yield return canvasGroup.DOFade(1, guiSwitchingDur)
        .WaitForCompletion();

        yield return new WaitForSecondsRealtime(lookTime);
        infoOverlay.DOFade(1, guiSwitchingDur); 
        EventSystem.current.SetSelectedGameObject(startNavigation); 
    }

    protected override void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        foreach (PlayerColorInfo playerColorInfo in playerColorInfos)
        {
            playerColors.Add(playerColorInfo.plrId, playerColorInfo.color);
        }

        GetSceneDirectionLighting();
    }

    public void Init()
    {
        //get the list of places
        playerPieces.Sort((a, b) => b.currentWaypointIndex.CompareTo(a.currentWaypointIndex));
        List<int> places = new();
        foreach (PlayerPiece playerPiece in playerPieces)
        {
            places.Add(playerPiece.PlayerNumber);
        }

        Toggle(true, places);
        DrawChart(playerPieces, totalWaypoints);
    }

    protected override void Testing()
    {
        if (!Input.GetKeyDown(KeyCode.Backspace)) return;

        List<int> places= new() {2, 1, 3, 4};
        Toggle(true, places);
    }

    #region ButtonsFunction
    public void Retry()
    {
        GameMangeren.Reset();
    }

    public void Exit()
    {
        GameMangeren.Exit();
    }
    #endregion

    #region chart
    void DrawChart(
        List<PlayerPiece> players,
        int totalWaypoints
    )
    {
        Debug.LogWarning("i am here");
        if (players.Count == 0)
            {Debug.LogError("no players"); return;}

        int roundCount = 0;

        foreach (PlayerPiece player in players)
        {
            roundCount = Mathf.Max(
                roundCount,
                player.roundWaypointHistory.Count
            );
        }

        if (roundCount == 0)
            {Debug.LogError("no History"); return;}
        Debug.LogWarning("bypass the check");

        float width = chartFrame.rect.width;
        float height = chartFrame.rect.height;

        // Leave some room around the edges.
        float paddingLeft = 50f;
        float paddingRight = 20f;
        float paddingTop = 20f;
        float paddingBottom = 40f;

        float graphWidth =
            width - paddingLeft - paddingRight;

        float graphHeight =
            height - paddingTop - paddingBottom;

        // Draw axes.
        DrawLine(
            new Vector2(
                paddingLeft,
                paddingBottom
            ),
            new Vector2(
                paddingLeft,
                paddingBottom + graphHeight
            ),
            3f,
            Color.white
        );

        DrawLine(
            new Vector2(
                paddingLeft,
                paddingBottom
            ),
            new Vector2(
                paddingLeft + graphWidth,
                paddingBottom
            ),
            3f,
            Color.white
        );

        DrawWaypointLabels(
            totalWaypoints,
            paddingLeft,
            paddingBottom,
            graphHeight
        );

        // Draw each player's performance line.
        for (int playerIndex = 0;
             playerIndex < players.Count;
             playerIndex++)
        {
            PlayerPiece player =
                players[playerIndex];

            Color playerColor =
                chartPlayerColors[
                    playerIndex % chartPlayerColors.Length
                ];

            DrawPlayerLine(
                player,
                roundCount,
                totalWaypoints,
                paddingLeft,
                paddingBottom,
                graphWidth,
                graphHeight,
                playerColor
            );
        }

        Debug.LogError("finish");
    }

    private void DrawPoint(
        Vector2 position,
        Color color
    )
    {
        GameObject point =
            new GameObject(
                "Chart Point",
                typeof(Image)
            );

        point.transform.SetParent(
            chartFrame,
            false
        );

        RectTransform rect =
            point.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;

        rect.sizeDelta =
            new Vector2(
                pointSize,
                pointSize
            );

        rect.anchoredPosition =
            position;

        Image image =
            point.GetComponent<Image>();

        image.color = color;

        generatedChartObjects.Add(point);
    }

    private void DrawLine(
        Vector2 start,
        Vector2 end,
        float thickness,
        Color color
    )
    {
        GameObject line =
            new GameObject(
                "Chart Line",
                typeof(Image)
            );

        line.transform.SetParent(
            chartFrame,
            false
        );

        RectTransform rect =
            line.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;

        Vector2 direction =
            end - start;

        float distance =
            direction.magnitude;

        rect.sizeDelta =
            new Vector2(
                distance,
                thickness
            );

        rect.pivot =
            new Vector2(0f, 0.5f);

        rect.anchoredPosition =
            start;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        rect.localRotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );

        Image image =
            line.GetComponent<Image>();

        image.color = color;

        generatedChartObjects.Add(line);
    }

    private void DrawPlayerLine(
        PlayerPiece player,
        int roundCount,
        int totalWaypoints,
        float paddingLeft,
        float paddingBottom,
        float graphWidth,
        float graphHeight,
        Color color
    )
    {
        List<int> history =
            player.roundWaypointHistory;

        if (history.Count == 0)
            return;

        Vector2? previousPoint = null;

        for (int i = 0; i < history.Count; i++)
        {
            int waypoint = history[i];

            float xPercent;

            if (roundCount <= 1)
            {
                xPercent = 0.5f;
            }
            else
            {
                xPercent =
                    (float)i /
                    (roundCount - 1);
            }

            float yPercent =
                (float)(waypoint - 1) /
                Mathf.Max(
                    1,
                    totalWaypoints - 1
                );

            float x =
                paddingLeft +
                xPercent * graphWidth;

            float y =
                paddingBottom +
                yPercent * graphHeight;

            Vector2 point =
                new Vector2(x, y);

            // Connect to previous round.
            if (previousPoint.HasValue)
            {
                DrawLine(
                    previousPoint.Value,
                    point,
                    lineThickness,
                    color
                );
            }

            DrawPoint(
                point,
                color
            );

            previousPoint = point;
        }
    }

    private void DrawWaypointLabels(
        int totalWaypoints,
        float paddingLeft,
        float paddingBottom,
        float graphHeight
    )
    {
        // We don't need to label every waypoint.
        // About five labels keeps the graph readable.
        int labelCount = Mathf.Min(
            5,
            totalWaypoints
        );

        for (int i = 0; i < labelCount; i++)
        {
            float percent =
                labelCount == 1
                ? 0f
                : (float)i /
                  (labelCount - 1);

            int waypoint =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        1,
                        totalWaypoints,
                        percent
                    )
                );

            Vector2 position =
                new Vector2(
                    paddingLeft - 25f,
                    paddingBottom +
                    percent * graphHeight
                );

            CreateLabel(
                waypoint.ToString(),
                position,
                18
            );
        }
    }

    private void CreateLabel(
        string text,
        Vector2 position,
        float fontSize
    )
    {
        GameObject label =
            new GameObject(
                "Chart Label",
                typeof(TextMeshProUGUI)
            );

        label.transform.SetParent(
            chartFrame,
            false
        );

        RectTransform rect =
            label.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;

        rect.sizeDelta =
            new Vector2(100, 30);

        rect.anchoredPosition =
            position;

        TMP_Text tmp =
            label.GetComponent<TMP_Text>();

        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment =
            TextAlignmentOptions.Center;

        generatedChartObjects.Add(label);
    }
    #endregion
}
