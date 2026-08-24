using UnityEngine;

public enum TileType
{
    Normal,
    MoveBack,
    MoveForward
}

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Info")]
    public int waypointNumber;

    [Header("Tile Effect")]
    public TileType tileType = TileType.Normal;

    [Tooltip("How many spaces this effect moves the player.")]
    public int effectAmount = 0;

    public int GetMovementEffect()
    {
        switch (tileType)
        {
            case TileType.MoveBack:
                return -effectAmount;

            case TileType.MoveForward:
                return effectAmount;

            default:
                return 0;
        }
    }
}