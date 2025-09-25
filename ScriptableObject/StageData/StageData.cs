using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [Header("Stage Settings")]
    [Header("Poolable Objects")]
    [Tooltip("List of poolable objects (enemies, obstacles, traps) that can be spawned in each stage.")]
    public List<PoolableList> PoolableLists = new List<PoolableList>();

    // [Range(1, 10)]
    [MinMaxSlider(1, 10), AllowNesting] // use for index min and max but store as Vector2
    public Vector2 MinMaxEnemyPerRoom;

    // [Range(1, 10)]
    [MinMaxSlider(1, 10), AllowNesting] // use for index min and max but store as Vector2
    public Vector2 MinMaxObstaclePerRoom;

    [MinMaxSlider(1, 10), AllowNesting] // use for index min and max but store as Vector2
    public Vector2 MinMaxTrapPerRoom;

    [Header("Room Settings")]
    [Tooltip("List of room type that can be spawned and rate to spawn in each stage.")]
    public List<RoomInfo> RoomInfos = new List<RoomInfo>();

    [MinMaxSlider(5, 10), AllowNesting]
    public Vector2 MinMaxRoomInStage;
}
