using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D.TopDownShooter;
using UnityEngine;

public enum BlockType
{
    HardBlock,
    SoftBlock
}

public enum RoomDirection
{
    None = 0,
    Front = 1,
    Back = 2,
    Left = 3,
    Right = 4,
}

public class RoomManager
{

    #region -Declared Variables-

    private int roomGridSize;
    private Room startRoom;

    private List<Room> allRooms = new();
    private Dictionary<Vector3Int, Room> hardBlockPositions = new();
    private Dictionary<Vector3Int, Room> softBlockPositions = new();

    public List<Room> AllRooms => allRooms;
    public Dictionary<Vector3Int, Room> HardBlockPositions => hardBlockPositions;
    public Dictionary<Vector3Int, Room> SoftBlockPositions => softBlockPositions;

    private bool initRoomDirections;
    private List<Vector3Int> roomSpawnDirections;

    public List<Vector3Int> RoomSpawnDirections
    {
        get
        {
            if (initRoomDirections) return roomSpawnDirections;

            initRoomDirections = true;
            // var _gridSpaceSize = RoomGenerator.Instance.RoomGridSize;
            return roomSpawnDirections = new List<Vector3Int>
            {
                new(0, 0, roomGridSize),   // Front
                new(0, 0, -roomGridSize),  // Back
                new(-roomGridSize, 0, 0),  // Left
                new(roomGridSize, 0, 0)    // Right
            };
        }
    }

    private bool initRoomDirectionsDict;
    private Dictionary<Vector3Int, RoomDirection> roomDirections = new();

    public Dictionary<Vector3Int, RoomDirection> RoomDirections
    {
        get
        {
            if (initRoomDirectionsDict) return roomDirections;

            initRoomDirectionsDict = true;

            return roomDirections = new Dictionary<Vector3Int, RoomDirection>
            {
                { new(0, 0, roomGridSize), RoomDirection.Front },
                { new(0, 0, -roomGridSize), RoomDirection.Back },
                { new(-roomGridSize, 0, 0), RoomDirection.Left },
                { new(roomGridSize, 0, 0), RoomDirection.Right },
            };
        }
    }

    #endregion

    public RoomManager(int roomGridSize)
    {
        this.roomGridSize = roomGridSize;
    }

    /// <summary>
    /// Connect the room to made the room room neighbor.
    /// </summary>
    /// <param name="latestRoom"></param>
    /// <param name="_previousRoom"></param>
    private void ConnectRoom(Room latestRoom, Room _previousRoom)
    {
        if (latestRoom == null || _previousRoom == null) return;

        // Set the new room as the based room for the previous room
        latestRoom.BasedRoom = _previousRoom;

        // Add the previous room as a neighbor to the new room
        if (!latestRoom.Neighbor.Contains(_previousRoom))
        {
            latestRoom.Neighbor.Add(_previousRoom);
        }

        // Add the new room as a neighbor to the previous room
        if (!_previousRoom.Neighbor.Contains(latestRoom))
        {
            _previousRoom.Neighbor.Add(latestRoom);
        }
    }

    /// <summary>
    /// Assign a new room to the manager and set it as the start room if it's the first room.
    /// This method also sets the new room as the based room for the previous room if it exists.
    /// If the previous room is not null, it will be added as a neighbor to the new room.
    /// </summary>
    /// <param name="_previouseRoom"></param> the temporary room to assign the new room based room
    /// <param name="_latestRoom"></param> the new room to assign the previous room as a neighbor, set the based room, and add to the hard block positions
    public void AssignRoom(ref Room _startRoom, ref Room _previouseRoom, Room _latestRoom)
    {
        AllRooms.Add(_latestRoom);

        // Add the room to the hard block positions
        HardBlockPositions.TryAdd(Vector3Int.FloorToInt(_latestRoom.transform.position), _latestRoom);

        if (_startRoom == null)
        {
            _startRoom = _latestRoom;
            startRoom = _startRoom;
        }

        ConnectRoom(_latestRoom, _previouseRoom);
    }

    private RoomInfo SetRoomType(List<RoomInfo> _roomTypeListData)
    {
        var _selectedRoom = _roomTypeListData[Random.Range(0, _roomTypeListData.Count)];

        foreach (var _roomInfo in _roomTypeListData)
        {
            var _randomRate = Random.Range(0f, 1f);

            if (_randomRate > _roomInfo.SpawnRate / 100) continue;
            _selectedRoom = _roomInfo;
            break; // Exit the loop once a valid room type is found
        }

        return _selectedRoom;
    }

    public void SetRoomData(Room _targetRoom, RoomTypeData _standbyRoomData, List<RoomInfo> roomTypeListData)
    {
        if (_targetRoom == startRoom)
        {
            _targetRoom.InitRoom(_standbyRoomData);
            _targetRoom.RoomState = RoomState.Completed;
        }
        else
        {
            // var _selectedRoom = roomTypeListData[Random.Range(0, roomTypeListData.Count)];

            // foreach (var _roomInfo in roomTypeListData)
            // {
            //     var _randomRate = Random.Range(0f, 1f);

            //     if (_randomRate > _roomInfo.SpawnRate / 100) continue;
            //     _selectedRoom = _roomInfo;
            //     break; // Exit the loop once a valid room type is found
            // }

            var _selectedRoom = SetRoomType(roomTypeListData);

            _targetRoom.InitRoom(_selectedRoom.RoomData);
            // if (_selectedRoom.RoomData.RoomType is RoomTypes.Shop or RoomTypes.Bonus)
            // {
            //     _targetRoom.RoomState = RoomState.Completed;
            // }
        }
    }

    /// <summary>
    /// to check if <param name="_blockedPosition"></param> is already contains in Hard Block Position or Soft Block Position
    /// </summary>
    /// <param name="_blockedPosition"></param> the target position to check if it contains
    /// <returns></returns>
    public bool IsPositionContains(Vector3 _blockedPosition)
    {
        var _checkPosition = Vector3Int.FloorToInt(_blockedPosition);
        return HardBlockPositions.ContainsKey(_checkPosition) || SoftBlockPositions.ContainsKey(_checkPosition);
    }

    /// <summary>
    /// the method to block the position around the <param name="_room"></param> variables use to block for making the room spacing
    /// </summary>
    /// <param name="_room"></param> the target center room block position around this room
    public void SoftBlockAroundRoomPosition(Room _room)
    {
        // var _directionList = RoomGenerator.Instance.RoomLogicHelpers.RoomDirections;

        foreach (var _direction in RoomSpawnDirections)
        {
            var _position = Vector3Int.FloorToInt(_room.transform.position + _direction);

            if (HardBlockPositions.ContainsKey(_position)) continue;
            if (!SoftBlockPositions.TryAdd(_position, _room)) continue;

            // successfully added soft block position around the room
        }
    }

    /// <summary>
    /// Clear all room data, including all rooms and their hard and soft block positions.
    /// </summary>
    public void ClearRoomData()
    {
        startRoom = null;

        foreach (var _room in AllRooms)
        {
            _room.ResetRoom();
        }

        AllRooms.Clear();

        HardBlockPositions.Clear();
        SoftBlockPositions.Clear();
    }

    /// <summary>
    /// the method to check if the target position is have block around
    /// </summary>
    /// <param name="_targetPosition"></param> the target position >> use to check is this position is have block around
    /// <param name="_baseRoom"></param> the based room of <param name="_targetPosition"></param> (but in this case <param name="_targetPosition"></param> is not create the room currently.)
    /// <param name="_blockType"></param> the enum of room block (hard or soft) to check if the position is blocked
    /// <returns></returns>
    public bool IsPositionHaveBlocked(Vector3 _targetPosition, Room _baseRoom, BlockType _blockType)
    {
        var _roomManager = RoomGenerator.Instance.RoomManager;
        foreach (var _direction in _roomManager.RoomSpawnDirections)
        {
            var _checkPosition = Vector3Int.FloorToInt(_targetPosition + _direction);

            if (_checkPosition == _targetPosition - _baseRoom.transform.position)
                continue; // Skip the base room position

            switch (_blockType)
            {
                case BlockType.HardBlock:
                    if (_roomManager.HardBlockPositions.TryGetValue(_checkPosition, out var _room) && _room != _baseRoom)
                    {
                        return true; // Position is blocked by hard blocks
                    }
                    break;
                case BlockType.SoftBlock:
                    if (_roomManager.SoftBlockPositions.TryGetValue(_checkPosition, out var _softRoom) && _softRoom != _baseRoom)
                    {
                        return true; // Position is blocked by soft blocks
                    }
                    break;
            }
        }

        // No blocks found around the target position
        return false;
    }

}
