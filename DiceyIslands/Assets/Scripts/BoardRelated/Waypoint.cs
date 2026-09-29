using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


// =========================================
// TILE TYPES
// =========================================
// OUTSIDE the Waypoint class, but still
// inside Waypoint.cs

public enum TileType
{
    Normal,
    MoveForward,
    MoveBack,
    RollAgain,
    SwapWithRandomPlayer,
    SkipNextTurn
}


// =========================================
// WAYPOINT
// =========================================

public class Waypoint : MonoBehaviour
{
    [Header("Tile")]
    public TileType tileType = TileType.Normal;

    [Header("Movement Tile Settings")]
    [SerializeField] private int movementAmount = 3;


    // =========================================
    // SUB WAYPOINTS
    // =========================================

    [Header("Sub Waypoints")]
    [SerializeField]
    private List<Transform> subWaypoints =
        new List<Transform>();

    public IReadOnlyList<Transform> SubWaypoints
    {
        get { return subWaypoints; }
    }


    // =========================================
    // WAYPOINT NUMBER FROM NAME
    // =========================================

    public int WaypointNumber
    {
        get
        {
            Match match =
                Regex.Match(
                    gameObject.name,
                    @"\((\d+)\)"
                );

            if (match.Success &&
                int.TryParse(
                    match.Groups[1].Value,
                    out int number
                ))
            {
                return number;
            }

            Debug.LogError(
                "Could not find waypoint number in '" +
                gameObject.name +
                "'. Expected something like WayPoint (25).",
                gameObject
            );

            return -1;
        }
    }


    // =========================================
    // MOVEMENT TILE EFFECT
    // =========================================

    public int GetMovementEffect()
    {
        switch (tileType)
        {
            case TileType.MoveForward:
                return movementAmount;

            case TileType.MoveBack:
                return -movementAmount;

            default:
                return 0;
        }
    }
}