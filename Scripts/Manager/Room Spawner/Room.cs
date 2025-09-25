using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using VInspector;
using Random = UnityEngine.Random;

public class Room : MonoBehaviour
{
    #region -Declared Variables-
    [Header("Room Setup")]
    [SerializeField] private bool initialRoom;
    [SerializeField] private bool environmentComplete;
    [SerializeField] private Tilemap roomTilemap;
    [SerializeField] private RoomTypeData roomData;
    [SerializeField] private RoomState roomState = RoomState.NotStarted;

    public Action<RoomState> RoomStateChange;

    public bool InitialRoom { get => initialRoom; set => initialRoom = value; }
    public bool EnvironmentComplete { get => environmentComplete; set => environmentComplete = value; }
    public Tilemap RoomTilemap { get => roomTilemap; set => roomTilemap = value; }
    public RoomTypeData RoomData { get => roomData; set => roomData = value; }

    public RoomState RoomState
    {
        get => roomState;
        set
        {
            if (roomState == value) return;

            roomState = value;

            RoomStateChange?.Invoke(roomState);
        }
    }


    [SerializeField] private bool isCleared;
    [SerializeField] private bool isVisited;
    public bool IsCleared { get => isCleared; set => isCleared = value; }
    public bool IsVisited { get => isVisited; set => isVisited = value; }

    // all object that have spawn in room list
    #region -Object In Room Variables-

    [Header("Object In Room")]
    [SerializeField] private List<GameObject> enemies;
    [SerializeField] private List<GameObject> traps;
    [SerializeField] private List<GameObject> obstacles;

    public List<GameObject> Enemies { get => enemies; set => enemies = value; }
    public List<GameObject> Traps { get => traps; set => traps = value; }
    public List<GameObject> Obstacles { get => obstacles; set => obstacles = value; }

    #endregion

    [Header("Grid In Room")]
    [SerializeField] private List<GridMover> allGridsMover = new List<GridMover>();
    [SerializeField] private List<GridMover> emptyGrid = new List<GridMover>();
    public List<GridMover> AllGridsMover { get => allGridsMover; set => allGridsMover = value; }
    public List<GridMover> EmptyGrid { get => emptyGrid; set => emptyGrid = value; }

    #region -Connecting Room Variables-

    [Header("Connecting Room")]
    [SerializeField] private Room basedRoom;
    [SerializeField] private List<Room> neighbor = new List<Room>();
    public Room BasedRoom { get => basedRoom; set => basedRoom = value; }
    public List<Room> Neighbor { get => neighbor; set => neighbor = value; }

    [Header("Door In Room")]
    [SerializeField] private List<Gate> gateInRoom = new();
    public List<Gate> GateInRoom { get => gateInRoom; set => gateInRoom = value; }

    #endregion

    [Header("Room Collider")]
    private BoxCollider roomCollider;

    // each room will have their own spawning cost
    private bool initSpawningCost;
    private Dictionary<SpawningCostType, int> spawningCostsDict = new Dictionary<SpawningCostType, int>();
    public Dictionary<SpawningCostType, int> SpawningCostsDict
    {
        get
        {
            if (initSpawningCost) return spawningCostsDict;

            foreach (var _poolableList in PoolManager.Instance.SpawningCostsDict)
            {
                if (_poolableList.Value.SpawningCostType == SpawningCostType.None) continue;
                spawningCostsDict.Add(_poolableList.Key, _poolableList.Value.DefaultSpawningCost);
            }
            initSpawningCost = true;
            return spawningCostsDict;
        }
    }

    public GameObject dummyMonsterPrefab;

    #endregion

    private void OnEnable()
    {
        // PoolManager.Instance.ActiveObject(Enemies);
        PoolManager.Instance.ActiveObject(Obstacles);
        PoolManager.Instance.ActiveObject(Traps);
    }

    private void OnDisable()
    {
        PoolManager.Instance.DeActiveObject(Enemies);
        PoolManager.Instance.DeActiveObject(Obstacles, _clearList: false);
        PoolManager.Instance.DeActiveObject(Traps, _clearList: false);
    }

    private void Start()
    {
        RoomTilemap = GetComponentInChildren<Tilemap>();
        RoomStateChange += (_state) => RoomStateHandle();
    }

    public Vector3 GetSpawnPoint()
    {
        var _random = Random.Range(0, emptyGrid.Count);
        var _target = emptyGrid[_random];

        emptyGrid.Remove(_target);

        var _spawnPoint = new Vector3(Mathf.Abs(_target.transform.parent.localPosition.x), 0.5f, Mathf.Abs(_target.transform.parent.localPosition.z));

        return _spawnPoint;
    }

    [Button("Spawn Enemy")]
    public void SpawnEnemy()
    {
        print("spawn enemy");
        // if (!PoolManager.Instance.SpawningCostsDict.TryGetValue(SpawningCostType.Enemy, out var _obj)) return;
        if (!SpawningCostsDict.TryGetValue(SpawningCostType.Enemy, out var _enemySpawnCost)) return;

        while (_enemySpawnCost-- > 0 && GameController.Instance.CurrentStage.MinMaxEnemyPerRoom.y > Enemies.Count)
        {
            var _spawnPoint = GetSpawnPoint();
            var _enemy = PoolManager.Instance.ActiveObjectReturn(SpawningManager.GetEnemy(this), _spawnPoint, Quaternion.identity, transform);
            _enemy.transform.SetLocalPositionAndRotation(_spawnPoint, Quaternion.identity);
            _enemy.name = "Enemy " + Enemies.Count;

            var _enemyComponent = _enemy.GetComponent<Enemy>();
            _enemyComponent.ResetEnemy();

            Enemies.Add(_enemy);
        }
    }

    [Button("Spawn Obstacle")]
    public void SpawnObstacle()
    {
        if (!SpawningCostsDict.TryGetValue(SpawningCostType.Obstacle, out var _obstacleSpawnCost)) return;

        while (_obstacleSpawnCost-- > 0 && GameController.Instance.CurrentStage.MinMaxObstaclePerRoom.y > Obstacles.Count)
        {
            var _spawnPoint = GetSpawnPoint();
            var _obstacle = PoolManager.Instance.ActiveObjectReturn(SpawningManager.GetObstacle(this), _spawnPoint, Quaternion.identity, transform);
            _obstacle.transform.SetLocalPositionAndRotation(_spawnPoint, Quaternion.identity);

            Obstacles.Add(_obstacle);
        }
    }

    [Button("Spawn Trap")]
    public void SpawnTrap()
    {
        if (!SpawningCostsDict.TryGetValue(SpawningCostType.Trap, out var _trapSpawnCost)) return;

        while (_trapSpawnCost-- > 0 && GameController.Instance.CurrentStage.MinMaxTrapPerRoom.y > Traps.Count)
        {
            var _spawnPoint = GetSpawnPoint();
            var _trap = PoolManager.Instance.ActiveObjectReturn(SpawningManager.GetTrap(this), _spawnPoint, Quaternion.identity, transform);
            _trap.transform.SetLocalPositionAndRotation(_spawnPoint, Quaternion.identity);


            Traps.Add(_trap);
        }
    }

    [Button("Spawn Dummy Monster")]
    public void SpawnDummyMonster()
    {
        if (dummyMonsterPrefab == null) return;

        var _spawnPoint = GetSpawnPoint();
        var _dummyMonster = Instantiate(dummyMonsterPrefab, _spawnPoint, Quaternion.identity, transform);
        _dummyMonster.name = "Dummy Monster " + Enemies.Count + 1;
        _dummyMonster.transform.SetLocalPositionAndRotation(_spawnPoint, Quaternion.identity);

        Enemies.Add(_dummyMonster);
    }

    /// <summary>
    /// Initialize the room with the given setup data.
    /// </summary>
    /// <param name="_roomSetupData"></param>
    public void InitRoom(RoomTypeData _roomSetupData)
    {
        roomData = _roomSetupData;

        if (roomCollider == null)
        {
            TryGetComponent(out roomCollider);
        }

        roomCollider.center = new Vector3((roomData.RoomSize / 2) - 0.5f, -0.5f, (roomData.RoomSize / 2) - 0.5f);
        roomCollider.size = new Vector3(roomData.RoomSize, 1, roomData.RoomSize);

        foreach (var _poolableList in GameController.Instance.CurrentStage.PoolableLists)
        {
            if (_poolableList.SpawningCostType == SpawningCostType.None) continue;
            if (!SpawningCostsDict.TryGetValue(_poolableList.SpawningCostType, out var _cost)) continue;
            print($"cost {_poolableList.SpawningCostType}: {_cost}");
            _poolableList.SetupCost(ref _cost);
        }

        if (roomData.RoomType is not RoomTypes.Combat or RoomTypes.Boss)
        {
            IsCleared = true;
            RoomState = RoomState.Completed;
        }

        InitialRoom = true;
    }

    /// <summary>
    /// Clear all enemies and clear the room function
    /// </summary>
    [Button("Clear Room")]
    public void ClearRoom()
    {
        foreach (var _enemy in Enemies)
        {
            var _enemyComponent = _enemy.GetComponent<Enemy>();
            _enemyComponent.IsDead = true;
        }

        PoolManager.Instance.DeActiveObject(Enemies);

        RoomStateHandle();
    }

    /// <summary>
    /// Reset the room to its initial state.
    /// </summary>
    public void ResetRoom()
    {
        InitialRoom = false;
        EnvironmentComplete = false;
        RoomData = null;
        BasedRoom = null;
        IsVisited = false;

        AllGridsMover.Clear();
        EmptyGrid.Clear();
        Neighbor.Clear();
        RoomTilemap.ClearAllTiles();

        PoolManager.Instance.DeActiveObject(Enemies);
        PoolManager.Instance.DeActiveObject(Traps);
        PoolManager.Instance.DeActiveObject(Obstacles);
        PoolManager.Instance.DeActiveObject(GateInRoom);
        // PoolManager.Instance.DeActiveObject(AllGridsObject);
        PoolManager.Instance.DeActiveObject(gameObject);
    }

    /// <summary>
    /// Setup the grid spaces in the room.
    /// </summary>
    /// <param name="_gridMoversList"></param>
    public void SetupGridsRoom(List<GridMover> _gridMoversList)
    {
        foreach (var _gridMover in _gridMoversList)
        {
            if (AllGridsMover.Contains(_gridMover)) continue;

            AllGridsMover.Add(_gridMover);
        }

        foreach (var _gridMover in _gridMoversList)
        {
            _gridMover.GetNeighbors(this);
        }
    }


    /// <summary>
    /// Setup the empty grid spaces in the room.
    /// </summary>
    public void SetupGridsEmpty()
    {
        foreach (var _grid in allGridsMover)
        {
            if (_grid.CurrentState != GridState.Empty || _grid.IsPortal)
                continue;

            EmptyGrid.Add(_grid);
        }
    }

    /// <summary>
    /// Handle the state of the room based on the initial setup and environment completion.
    /// </summary>
    public void RoomStateHandle()
    {
        if (InitialRoom && !EnvironmentComplete && RoomState == RoomState.NotStarted)
        {
            RoomState = RoomState.InProgress;

            // set room to in progress and setup the room
            // Spawn enemies, obstacles, traps, etc.
            SpawnEnemy();
            // SpawnObstacle();
            // SpawnTrap();

            EnvironmentComplete = true;
        }

        if (!InitialRoom || !EnvironmentComplete) return;

        switch (RoomState)
        {
            case RoomState.NotStarted:
                // Handle not started state
                RoomState = RoomState.InProgress;
                break;
            case RoomState.InProgress:
                // Handle in progress state
                var _isAllEnemiesDead = Enemies.All(enemy => enemy.GetComponent<Enemy>().IsDead);

                if (!_isAllEnemiesDead) return;
                IsCleared = true;
                RoomState = RoomState.Completed;
                break;
            case RoomState.Completed:
                // Handle completed state
                // all gate in room should be unlocked
                // player should get rewards

                AllGridsMover.ForEach(_grid => _grid.GridActive = true);

                GateManager.Instance.UnlockGatesInRoom(this);

                // after completed or cleared the room then reset the action point for allies!
                var _allies = GameController.Instance.AlliesUnit;
                foreach (var _ally in _allies)
                {
                    if (_ally.ActionPoint >= _ally.MaxActionPoint) continue;
                    _ally.ResetActionPoint();
                }

                // reset the turn queue
                TurnBasedManager.Instance.ResetQueue();
                break;
            case RoomState.Failed:
                // Handle failed state
                // player should lose rewards or get punishment
                break;
        }
    }
}
