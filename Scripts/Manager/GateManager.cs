using System.Collections.Generic;
using System.Linq;
using Com.LuisPedroFonseca.ProCamera2D.TopDownShooter;
using UnityEngine;
using VInspector;

public class GateManager : Singleton<GateManager>
{
    #region -Declared Variables-
    [SerializeField] private GameObject gatePrefab;

    private RoomGenerator roomGenerator => RoomGenerator.Instance;

    private bool initGatePosition;
    private Dictionary<Vector3Int, RoomDirection> roomGatePositions = new Dictionary<Vector3Int, RoomDirection>();
    public Dictionary<Vector3Int, RoomDirection> RoomGatePositions => roomGatePositions;

    #endregion

    private void Start()
    {
        roomGatePositions = new Dictionary<Vector3Int, RoomDirection>()
        {
            {new (0, 0, roomGenerator.RoomGridSize), RoomDirection.Front},
            {new (0, 0, -roomGenerator.RoomGridSize), RoomDirection.Back},
            {new (-roomGenerator.RoomGridSize, 0, 0), RoomDirection.Left},
            {new (roomGenerator.RoomGridSize, 0, 0), RoomDirection.Right},
        };
    }

    /// <summary>
    /// Get the door spawn position based on the neighbor position.
    /// </summary>
    /// <param name="_targetRoom"></param> the target room to find the door spawn position
    /// <param name="_neighborRoomPosition"></param> the position of the neighbor room to find the door spawn position
    /// <param name="_gatePosition"></param> the position of the gate to be spawned
    /// <returns></returns>
    public bool GetGateSpawnPosition(Room _targetRoom, Vector3 _neighborRoomPosition, out Vector3 _gatePosition)
    {
        if (!RoomGatePositions.TryGetValue(Vector3Int.FloorToInt(_neighborRoomPosition), out RoomDirection _direction))
        {
            _gatePosition = Vector3.zero;
            return false;
        }

        // get gate spawn position calculated from specific room size and room neighbor directions
        var _roomSize = _targetRoom.RoomData.RoomSize;
        switch (_direction)
        {
            case RoomDirection.Front:
                _gatePosition = new Vector3(_roomSize / 2, 0, _roomSize - 1);
                break;
            case RoomDirection.Back:
                _gatePosition = new Vector3(_roomSize / 2, 0, 0);
                break;
            case RoomDirection.Left:
                _gatePosition = new Vector3(0, 0, (_roomSize / 2) - 1);
                break;
            case RoomDirection.Right:
                _gatePosition = new Vector3(_roomSize - 1, 0, (_roomSize / 2) - 1);
                break;
            default:
                _gatePosition = Vector3.zero;
                return false;
        }

        return true;
    }

    /// <summary>
    /// Set the gate object to the grid of the target room.
    /// to make the grid know where the gate is located and prevent enemies from blocking or spawning at the gate position.
    /// </summary>
    /// <param name="_targetRoom"></param> the target room to set the gate to the grid
    /// <param name="_gateObject"></param> the gate object to set to the grid
    public void SetGateToGrid(Room _targetRoom, GameObject _gateObject)
    {
        foreach (var _gridMover in _targetRoom.AllGridsMover)
        {
            var _gridBlockPos = new Vector3(_gridMover.transform.parent.localPosition.x, 0, _gridMover.transform.parent.localPosition.z);
            var _gatePos = new Vector3(_gateObject.transform.localPosition.x, 0, _gateObject.transform.localPosition.z);

            if (_gridBlockPos != _gatePos) continue;
            _gridMover.IsPortal = true;
        }
    }

    /// <summary>
    /// the method to create the room gate for the room
    /// This method will create the door for the room based on the neighbor of the room,
    /// and set the position of the door based on the direction of the neighbor.
    /// </summary>
    /// <param name="_targetRoom"></param> the target room to create the gates for
    public void CreateRoomGates(Room _targetRoom)
    {
        foreach (var _neighbor in _targetRoom.Neighbor)
        {
            // find the neighbor room offset to get gate position to spawn
            var _neighborRoomOffset = _neighbor.transform.position - _targetRoom.transform.position;

            if (!GetGateSpawnPosition(_targetRoom, _neighborRoomOffset, out Vector3 _gatePosition)) continue;

            // create the gate object from the pool manager
            var _gateObject = PoolManager.Instance.ActiveObjectReturn(gatePrefab, Vector3.zero, Quaternion.identity, _targetRoom.transform);
            _gateObject.transform.SetLocalPositionAndRotation(new Vector3(_gatePosition.x, _gateObject.transform.localPosition.y, _gatePosition.z), Quaternion.identity);

            if (!_gateObject.TryGetComponent<Gate>(out var _gateComponent)) continue;

            // Setup the gate component with the target room and neighbor room
            // and add gate to the target room
            _gateComponent.SetupGateRoom(_targetRoom, _neighbor);
            _targetRoom.GateInRoom.Add(_gateComponent);

            // Set the gate to the grid
            // Make the grid know where the gate is located so the enemy won't block or spawn at the gate position
            SetGateToGrid(_targetRoom, _gateObject);
        }
    }

    /// <summary>
    /// Unlock all gates in the target room after all enemies are defeated and room cleared.
    /// </summary>
    /// <param name="_targetRoom"></param> the target room to unlock the gates in
    public void UnlockGatesInRoom(Room _targetRoom)
    {
        // unlock gates in the target room if all enemies are dead or stage cleared

        if (!_targetRoom.IsCleared) return;

        foreach (var _gate in _targetRoom.GateInRoom)
        {
            if (_gate == null) continue;
            _gate.IsUnlocked = true;
        }
    }
}
