using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayMode
{
    TestMode,
    GameplayMode,
}

public class GameController : Singleton<GameController>
{
    #region -Declared Variables-

    [SerializeField] private PlayMode playMode;
    public PlayMode PlayMode => playMode;

    [ShowIf("PlayMode", PlayMode.TestMode), AllowNesting]
    [SerializeField] private GameObject testRoomPrefab;

    #region # Stage Data Variables
    [SerializeField] private List<StageData> stageDataList;
    [SerializeField] private StageData currentStage;

    public List<StageData> StageDataList => stageDataList;
    public StageData CurrentStage { get => currentStage; set => currentStage = value; }

    #endregion

    #region # Allies  Variables

    [SerializeField] private List<Character> alliesUnit;
    public List<Character> AlliesUnit => alliesUnit;

    #endregion

    #region # Room Variables

    [SerializeField] private Room currentRoom;
    [SerializeField] private List<Room> rooms; // all room that player have initial before

    private Action ChangeRoomAction;

    public Room CurrentRoom
    {
        get => currentRoom;
        set
        {
            currentRoom = value;
            ChangeRoomAction?.Invoke();
        }
    }

    public List<Room> Rooms { get => rooms; set => rooms = value; }

    #endregion

    #endregion

    public override void Awake()
    {
        base.Awake();
        CurrentStage = StageDataList[0];
    }

    private void Start()
    {
        SpawningManager.Setup();

        ChangeRoomAction += () =>
        {
            if (CurrentRoom.IsVisited || PlayMode == PlayMode.TestMode)
            {
                // If the room has been visited, we don't need to generate it again
                return;
            }

            StartCoroutine(GridManager.Instance.CreateGridTileRoom(CurrentRoom));
        };

        if (PlayMode != PlayMode.TestMode)
        {
            RoomGenerator.Instance.GenerateRoom();
            UpdateRoom(RoomGenerator.Instance.StartRoom);
        }
        else
        {
            Instantiate(testRoomPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    public void UpdateRoom(Room _newRoom)
    {
        CurrentRoom = _newRoom;
    }

    public void OnCompleteGenerateRoomSetup(bool _isComplete)
    {
        var _roomSize = CurrentRoom.RoomData.RoomSize;

        var _gateManager = GateManager.Instance;

        if (_isComplete)
        {
            _gateManager.CreateRoomGates(CurrentRoom);

            CurrentRoom.SetupGridsEmpty();

            Tweener _unitTween = null;

            foreach (var _unit in AlliesUnit)
            {
                _unit.gameObject.SetActive(true);
                _unit.transform.SetParent(CurrentRoom.transform);

                if (CurrentRoom.RoomData.RoomType == RoomTypes.Standby)
                {
                    _unit.transform.SetLocalPositionAndRotation(new Vector3((_roomSize / 2) - 1, 6f, (_roomSize / 2) - 1), Quaternion.identity);
                }
                else
                {
                    var _basedRoomOffset = CurrentRoom.BasedRoom.transform.position - CurrentRoom.transform.position;

                    if (_gateManager.GetGateSpawnPosition(CurrentRoom, _basedRoomOffset, out var _doorPosition))
                    {
                        _unit.transform.SetLocalPositionAndRotation(new Vector3(_doorPosition.x, 6f, _doorPosition.z), Quaternion.identity);
                    }
                }

                _unitTween = _unit.transform.DOLocalMoveY(0.5f, 0.75f).SetEase(Ease.OutElastic);
            }

            CurrentRoom.IsVisited = true;
            CurrentRoom.RoomStateHandle();

            _unitTween?.OnComplete(() =>
            {
                _unitTween.Kill();
                if (CurrentRoom.RoomData.RoomType == RoomTypes.Combat
                && CurrentRoom.RoomState is not RoomState.Completed or RoomState.Failed)
                {
                    // Turn based manager activated
                    // create turn queue by using the enemies and allies in the room
                    var _turnBasedManager = TurnBasedManager.Instance;

                    _turnBasedManager.ResetQueue();
                    _turnBasedManager.AddUnit(CurrentRoom);
                    _turnBasedManager.TurnBasedAction?.Invoke();
                }
            });
        }
    }

    public void SetAllies(Character _unit)
    {
        if (_unit is Enemy) return;

        if (!alliesUnit.Contains(_unit))
        {
            alliesUnit.Add(_unit);
        }
    }

    public void ClearEnemyGridInRoom()
    {
        print($"clear enemy grid in room");
        foreach (var _node in CurrentRoom.AllGridsMover)
        {
            _node.ClearGrid();
        }

        foreach (var _enemy in CurrentRoom.Enemies)
        {
            if (_enemy.TryGetComponent<Character>(out var _e))
            {
                _e.GridBehavior.ClearAttackPattern();
                _e.GridBehavior.ClearMoverPattern();
            }
        }
    }

    public void ResetAllyGridInRoom()
    {
        foreach (var _unit in AlliesUnit)
        {
            if (_unit.TryGetComponent<Character>(out var _c))
            {
                _c.GridBehavior.ClearAttackPattern();
                _c.GridBehavior.ClearMoverPattern();
            }
        }

        var _focusTarget = AlliesUnit.Find(_unit => _unit.OnTurn);

        if (_focusTarget != null)
        {
            _focusTarget.GridBehavior.SetAttackPattern();
            _focusTarget.GridBehavior.SetMoverPattern();
        }

    }

    public void AllyPatternVisibilityCheck()
    {
        var _focusAlly = AlliesUnit.Find(_unit => _unit.OnTurn);

        if (_focusAlly == null || !_focusAlly.OnTurn) return;
        var _isAllDenied = _focusAlly.GridBehavior.AttackPatternPath.Any(path => path.CurrentState is GridState.Empty);

        if (_isAllDenied)
        {
            print($"reset ally grid");
            ClearEnemyGridInRoom();
            ResetAllyGridInRoom();
        }
    }

    public void EnemyPatternVisible(Character _enemy, GridMover _selectedGrid)
    {
        _enemy.GridBehavior.SetMoverPattern();
        _enemy.GridBehavior.SetAttackPattern();


        var _gridInRoom = CurrentRoom.AllGridsMover;
        foreach (var _node in _gridInRoom)
        {
            _node.VisibleEnemyGrid();
        }

        if (_selectedGrid.EnemyActive)
            _selectedGrid.ActiveEnemy();
    }
}
