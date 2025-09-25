using System.Collections.Generic;
using UnityEngine;

public enum RoomTypes
{
    Standby = 0,
    Combat = 1,
    Bonus = 2,
    Shop = 3,
    Boss = 4
}

public enum RoomState
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}

[CreateAssetMenu(fileName = "RoomType", menuName = "Scriptable Objects/RoomTypeData")]
public class RoomTypeData : ScriptableObject
{
    public RoomTypes RoomType;
    [Space]
    public List<GridInfo> GridInfo;

    [Space, Tooltip("room size like 8x8, 10x10, or 12x12")]
    public int RoomSize;
}
