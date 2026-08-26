using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text winnerText;

    [Header("Chart")]
    [SerializeField] private RectTransform chartArea;

    [Header("Chart Appearance")]
    [SerializeField] private float lineThickness = 5f;
    [SerializeField] private float pointSize = 12f;

    [SerializeField]
    private Color[] playerColors =
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow
    };

    private List<GameObject> generatedChartObjects =
        new List<GameObject>();

    public void ShowResults(
        PlayerPiece winner,
        List<PlayerPiece> players,
        int totalWaypoints
    )
    {
        resultsPanel.SetActive(true);

        winnerText.text =
            "Player " +
            winner.PlayerNumber +
            " is the winner!";

        ClearChart();

        DrawChart(
            players,
            totalWaypoints
        );
    }

    private void DrawChart(
        List<PlayerPiece> players,
        int totalWaypoints
    )
    {
        if (players.Count == 0)
            return;

        int roundCount = 0;

        foreach (PlayerPiece player in players)
        {
            roundCount = Mathf.Max(
                roundCount,
                player.roundWaypointHistory.Count
            );
        }

        if (roundCount == 0)
            return;

        float width = chartArea.rect.width;
        float height = chartArea.rect.height;

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

        // Draw labels.
        DrawRoundLabels(
            roundCount,
            paddingLeft,
            paddingBottom,
            graphWidth
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
                playerColors[
                    playerIndex % playerColors.Length
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

    private void DrawRoundLabels(
        int roundCount,
        float paddingLeft,
        float paddingBottom,
        float graphWidth
    )
    {
        for (int i = 0; i < roundCount; i++)
        {
            float percent;

            if (roundCount <= 1)
            {
                percent = 0.5f;
            }
            else
            {
                percent =
                    (float)i /
                    (roundCount - 1);
            }

            Vector2 position =
                new Vector2(
                    paddingLeft +
                    percent * graphWidth,
                    paddingBottom - 25f
                );

            CreateLabel(
                "Round " + (i + 1),
                position,
                18
            );
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
            chartArea,
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
            chartArea,
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
            chartArea,
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

    private void ClearChart()
    {
        foreach (
            GameObject obj
            in generatedChartObjects
        )
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        generatedChartObjects.Clear();
    }
}