using UnityEngine;

public enum TileType
{
    Normal,
    MoveBack,
    MoveForward,
    SwapWithRandomPlayer,
    RollAgain,
    SkipNextTurn


}

public class Waypoint : MonoBehaviour
{
    [Header("waypoint Info")]
    public int waypointNumber;

    [Header("tile Effect")]
    public TileType tileType = TileType.Normal;

    [Tooltip("how many spaces this move the player")]
    public int effectAmount = 0;
    //===========================================================
    //MOVEMENT BASED TILES TYPES
    //===========================================================
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