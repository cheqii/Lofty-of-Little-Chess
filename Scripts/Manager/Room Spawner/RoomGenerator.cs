using System;
using System.Collections.Generic;
using System.Linq;
using _Lofty.Hidden.Helpers;
using _Lofty.Hidden.Utility;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGenerator : Singleton<RoomGenerator>
{
    #region -Declared Variables-

    #region -Room Spawn Variables-

    [Header("Room Spawn Grid Range")]
    [Tooltip("the size of the grid that the room will spawn in for on draw gizmos")]
    [SerializeField] private int roomGridSize;
    public int RoomGridSize => roomGridSize;

    [SerializeField] private int gridCountX;
    [SerializeField] private int gridCountZ;

    private Vector3 offset;

    [Header("Room Spawn Manager Variables"), Space]
    [SerializeField] private GameObject roomPrefab;

    [SerializeField] private float defaultRoomMoveOnRate;
    [SerializeField] private float increaseRoomMoveOnRate;
    private float roomMoveOnRate;

    [Space]
    [SerializeField] private int maxRoomGenerateLoops;

    [SerializeField] private bool testGenerate;
    public bool TestGenerate => testGenerate;

    [ShowIf("testGenerate")]
    [MinMaxSlider(5, 1000)]
    [SerializeField] private Vector2 minMaxRoomInTest;

    private Room tempRoom;
    private Room startRoom;

    public Room StartRoom => startRoom;

    private RoomManager roomManager;

    public RoomManager RoomManager
    {
        get
        {
            if (roomManager != null)
            {
                return roomManager;
            }

            roomManager = new RoomManager(roomGridSize);
            return roomManager;
        }
        set => roomManager = value;
    }
    #endregion

    [Space]
    [SerializeField] private RoomTypeData standbyRoomData;

    #endregion


    private void Start()
    {
        // roomManager = new RoomManager(roomGridSize);

        offset = new Vector3(-((RoomGridSize / 2) - 0.5f), 0, -((RoomGridSize / 2) - 0.5f));
    }

    /// <summary>
    /// the method that get position based on grid position index
    /// </summary>
    /// <param name="_gridIndex"></param>
    /// <returns></returns>
    private Vector3 GetPositionGridIndex(Vector3Int _gridIndex)
    {
        var _gridX = _gridIndex.x;
        var _gridZ = _gridIndex.y;
        return new Vector3(roomGridSize * (_gridX - gridCountX / 2), -0.5f, roomGridSize * (_gridZ - gridCountZ / 2));
    }

    /// <summary>
    /// the method to get room spawn position and return the position that the room could be spawn
    /// </summary>
    /// <param name="_directionIndex"></param>
    /// <returns>return Vector3 (position) to spawning the room</returns>
    private Vector3 GetRoomSpawnPositions(int _directionIndex)
    {
        // temp room will not be null after the first room have init
        // if temp room is not null the return position that base on temp room
        // return the position around the temp room
        if (tempRoom)
        {
            return tempRoom.transform.position + roomManager.RoomSpawnDirections[_directionIndex];
        }

        // first room will get center position of the grid index
        return GetPositionGridIndex(new Vector3Int(gridCountX / 2, gridCountZ / 2, 0));
    }

    /// <summary>
    /// this method will check if the target position is blocked by hard or soft blocks
    /// and find the fallback room path to find the new position to spawn the room without blocking
    /// but if the target position is not blocked then spawn the room at the position based on the temp room
    /// </summary>
    /// <param name="_targetPosition"></param>
    private void FindRoomPosition(out Vector3 _targetPosition)
    {
        // if temp room is not null then check if the room is fully blocked or not
        // if the room is fully blocked then find the fallback path to find the new position to spawn the room

        if (tempRoom && RoomLogicHelpers.IsRoomFullyBlocked(tempRoom, roomManager))
        {
            if (RoomLogicHelpers.GetFallbackRoomPath(ref tempRoom, roomManager, out var _fallbackPosition))
            {
                // remove the soft block at the target position to spawn the room to that new position
                roomManager.SoftBlockPositions.Remove(Vector3Int.FloorToInt(_fallbackPosition));
                _targetPosition = _fallbackPosition; // set the target position to the fallback position
                return;
            }
        }

        roomManager.RoomSpawnDirections.ShuffleOrder(); // shuffle the room direction orders to make it more randomly
        var _directionIndex = Random.Range(0, roomManager.RoomSpawnDirections.Count);

        // while the room is not getting block then the target room will get the position randomly base on temp room to spawn a new room
        _targetPosition = GetRoomSpawnPositions(_directionIndex);
    }

    private void TargetSpawningMoveOn(ref Room _targetRoom, Room _newRoom)
    {
        var _newRoomRate = Random.Range(0f, 1f); // declared random rate to random the chance to move on the spawn room neighbor or not
        var _shouldMoveOn =
                _newRoomRate <= roomMoveOnRate
                || !_targetRoom
                || RoomLogicHelpers.IsRoomFullyBlocked(_targetRoom, roomManager)
                || _targetRoom == startRoom
                || _targetRoom.Neighbor.Count >= roomManager.RoomSpawnDirections.Count;

        if (_shouldMoveOn)
        {
            // *** block position before change the room target to new room *** //
            // temp room is will not null if the first has init
            // soft block temp room neighbor (not the latest room) and block soft at the free space around temp room
            if (_targetRoom)
            {

                foreach (var _neighbor in _targetRoom!.Neighbor)
                {
                    if (_neighbor == _newRoom) continue;
                    roomManager.SoftBlockAroundRoomPosition(_neighbor);
                }

                roomManager.SoftBlockAroundRoomPosition(_targetRoom);
            }

            // assign temp room with the new room or latest room and reset room rate to default value
            _targetRoom = _newRoom;

            roomMoveOnRate = defaultRoomMoveOnRate; // after the chance successfully reach then set back the rate to origin value

        }
        else if (_newRoomRate > roomMoveOnRate) // if rate fail then increase rate percent
        {
            roomMoveOnRate += increaseRoomMoveOnRate;
        }
    }

    [VInspector.Button("Generate Room")]
    public void GenerateRoom()
    {
        var _gameController = GameController.Instance;
        var _triesCount = 0;
        var _targetRoomPosition = Vector3.zero;

        roomMoveOnRate = defaultRoomMoveOnRate; // spawn room rate to check the chance to spawn the new room nearby the current room

        var _minRooms = (int)_gameController.CurrentStage.MinMaxRoomInStage.x;
        var _maxRooms = (int)_gameController.CurrentStage.MinMaxRoomInStage.y;

        var _randomRoomCount = Random.Range(_minRooms, _maxRooms + 1);

        if (testGenerate)
        {
            _randomRoomCount = Random.Range((int)minMaxRoomInTest.x, (int)minMaxRoomInTest.y + 1);
        }

#if UNITY_EDITOR
        print($"Generating room {_randomRoomCount + 1}...");
#endif

        while (RoomManager.AllRooms.Count < _randomRoomCount + 1 && _triesCount < maxRoomGenerateLoops)
        {
            _triesCount++;

            FindRoomPosition(out _targetRoomPosition); // find the room position to spawn the room

            if (RoomManager.IsPositionContains(_targetRoomPosition)) continue;

            // var _newRoom = Instantiate(roomPrefab, _targetRoomPosition, Quaternion.identity, roomParent).GetComponent<Room>(); // spawning the room and set the position to the target (will make use with pool in the future)
            var _newRoom = PoolManager.Instance.ActiveObjectReturn(roomPrefab, _targetRoomPosition, Quaternion.identity).GetComponent<Room>();
            _newRoom.name = $"Room {RoomManager.AllRooms.Count + 1}";

            RoomManager.AssignRoom(ref startRoom, ref tempRoom, _newRoom); // assign the room to the manager and set the based room
            RoomManager.SetRoomData(_newRoom, standbyRoomData, _gameController.CurrentStage.RoomInfos); // set the room data to the new room

            TargetSpawningMoveOn(ref tempRoom, _newRoom);
        }

#if UNITY_EDITOR
        if (_triesCount >= maxRoomGenerateLoops)
        {
            Debug.LogError($"Generation stopped early! Only {roomManager.AllRooms.Count}/{_randomRoomCount} rooms created due to blocked space.");
        }
        else
        {
            Debug.Log($"<color=green>[RoomGen] Generation complete! Created {roomManager.AllRooms.Count} rooms.</color>");
        }
#endif
        GameController.Instance.Rooms = roomManager.AllRooms; // assign the all rooms to the game controller
    }

    [VInspector.Button("Clear Room")]
    public void ClearRoom()
    {
        startRoom = null;
        tempRoom = null;

        if (roomManager != null && roomManager.AllRooms.Count > 0)
        {
            roomManager.ClearRoomData();
        }
    }

#if UNITY_EDITOR

    // for visualize the room connection, room block position and grid
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < gridCountX; i++)
        {
            for (int j = 0; j < gridCountZ; j++)
            {
                var _position = GetPositionGridIndex(new Vector3Int(i, j, 0));
                Gizmos.DrawWireCube(_position - offset, new Vector3(roomGridSize, 1, roomGridSize));
            }
        }

        if (roomManager == null || roomManager.AllRooms.Count <= 0) return;

        // Draw actual rooms as solid green cubes

        foreach (var room in roomManager.AllRooms)
        {
            if (room == null) continue;
            if (room.IsCleared)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawCube(room.transform.position + Vector3.up * 0.5f, Vector3.one * 3f);
        }

        // Draw hard blocked (actual room position) positions (takenPositions keys without a real room) as red wire cubes
        Gizmos.color = Color.red;
        if (roomManager.HardBlockPositions != null)
        {
            foreach (var _hard in roomManager.HardBlockPositions)
            {
                Gizmos.DrawWireCube(_hard.Key + Vector3.up * 0.5f, Vector3.one * 2);

            }
        }

        // Draw soft block (the blocking for spacing with the room) as a magenta wire cubes
        Gizmos.color = Color.magenta;
        if (roomManager.SoftBlockPositions != null)
        {
            foreach (var _soft in roomManager.SoftBlockPositions)
            {
                Gizmos.DrawWireCube(_soft.Key + Vector3.up * 0.5f, Vector3.one);
            }
        }

        // Draw blue lines between neighbors to visualize connections
        Gizmos.color = Color.cyan;
        foreach (var room in roomManager.AllRooms)
        {
            if (room == null) continue;
            foreach (var neighbor in room.Neighbor)
            {
                if (neighbor == null) continue;
                Gizmos.DrawLine(room.transform.position + Vector3.up * 2f, neighbor.transform.position + Vector3.up * 2f);
            }
        }
    }
#endif
}
