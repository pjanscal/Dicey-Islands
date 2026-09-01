using UnityEngine;
using System.Collections.Generic;

public class PlayerPiece : MonoBehaviour
{
    [Header("Player Identity")]
    [SerializeField] private int playerNumber = 1;

    [SerializeField] private int controllerPlayerId = 1;

    public int PlayerNumber
    {
        get { return playerNumber; }
    }

    public int ControllerPlayerId
    {
        get { return controllerPlayerId; }
    }
    [Header("Board Position")]
    public int currentWaypointIndex = 0;

    [Tooltip("Offsets so players dont occupy the same point")]
    public Vector3 tileOffset = new Vector3(0, 0.5f, 0);

    [Header("Visuals")]
    [SerializeField] private Renderer[] playerRenderers;

    [Range(0f, 1f)]
    [SerializeField] private float inactiveAlpha = 0.35f;


    [Header("Match History")]
    public List<int> roundWaypointHistory = new List<int>();


    public void RecordRoundPosition()
    {
        int waypointNumber = currentWaypointIndex + 1;

        roundWaypointHistory.Add(waypointNumber);
    }

    public void ClearMatchHistory()
    {
        roundWaypointHistory.Clear();
    }
    private void Awake()
    {
        // finds render if unassigned
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = GetComponentsInChildren<Renderer>();
        }
    }

    public void SetActiveTurnVisual(bool isMyTurn)
    {
        float alpha = isMyTurn ? 1f : inactiveAlpha;

        foreach (Renderer rend in playerRenderers)
        {
            foreach (Material material in rend.materials)
            {
                Color color = material.color;
                color.a = alpha;
                material.color = color;
            }
        }
    }
}