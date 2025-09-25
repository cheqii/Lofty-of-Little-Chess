using System;
using UnityEngine;

[Serializable]
public class RoomInfo
{
    public RoomTypeData RoomData;

    [Range(0, 100)]
    public float SpawnRate;
}
