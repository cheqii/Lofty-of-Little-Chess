using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;

public class TurnBasedManager : Singleton<TurnBasedManager>
{
    #region -Declared Variables-

    #region -Turn Data-

    [Tab("Turn Data")]
    [Header("Turn Data")]
    [SerializeField] private List<GameObject> queueTransform = new();
    [SerializeField] private Queue<TurnData> turnDataQueue = new();

    public Queue<TurnData> TurnDataQueue { get => turnDataQueue; set => turnDataQueue = value; }
    public List<GameObject> QueueTransform { get => queueTransform; set => queueTransform = value; }

    [SerializeField] private bool onPlayerTurn;
    [SerializeField] private bool onEnemyTurn;

    public bool OnPlayerTurn { get => onPlayerTurn; set => onPlayerTurn = value; }
    public bool OnEnemyTurn { get => onEnemyTurn; set => onEnemyTurn = value; }

    [SerializeField] private TurnData focusTurn;

    #endregion

    #region -Canvas UI and Prefab-

    [Tab("UI Turn Slot")]
    [SerializeField] private Transform turnSlotCanvas;
    [SerializeField] private GameObject queuePrefab;

    private Dictionary<Character, TurnData> turnDataDict = new();

    private Dictionary<Character, UnitTurnSlot> unitTurnSlotDict = new();

    public Action TurnBasedAction { get; set; }

    #endregion

    #endregion

    private void Start()
    {
        TurnBasedAction += () =>
        {
            // if (GameController.Instance.CurrentRoom.RoomState != RoomState.InProgress) return;
            ActivateTurn();
        };
    }

    public void AddUnit(Character _unit, float _turnSpeed)
    {
        print($"add unit {_unit.gameObject.name}");

        var _turnData = new TurnData(_unit, _turnSpeed, null);

        TurnDataQueue.Enqueue(_turnData);
        // TurnDataList.Add(_turnData);

        turnDataDict.Add(_unit, _turnData);

        _unit.TurnData = _turnData;

        // set turn data by ordering by speed (max => min)
        TurnDataQueue.OrderByDescending(_unit => _unit.baseSpeed);
    }

    public void AddUnit(Room _targetRoom)
    {
        // add allies unit to the turn data queue
        foreach (var _unit in GameController.Instance.AlliesUnit)
        {
            var _turnData = new TurnData(_unit, _unit.TurnSpeed, null);

            TurnDataQueue.Enqueue(_turnData);
            turnDataDict.Add(_unit, _turnData);

            _unit.TurnData = _turnData;
            print($"add unit allies {_unit.gameObject.name}");
        }

        // add enemies unit to the turn data queue
        foreach (var _unit in _targetRoom.Enemies)
        {
            if (!_unit.TryGetComponent<Character>(out var _character)) continue;

            var _turnData = new TurnData(_character, _character.TurnSpeed, null);
            TurnDataQueue.Enqueue(_turnData);
            turnDataDict.Add(_character, _turnData);

            _character.TurnData = _turnData;

            print($"add unit enemies {_character.gameObject.name}");
        }

        TurnDataQueue.OrderByDescending(_unit => _unit.baseSpeed);
    }

    public void RemoveUnit(TurnData _unitTurn)
    {
        _unitTurn.unitSlot.ClearUnitSlot();
        unitTurnSlotDict.Remove(_unitTurn.unit);
    }

    public void ClearQueueUnit(TurnData _unitTurn)
    {
        if (TurnDataQueue.Contains(_unitTurn))
        {
            var _tempList = TurnDataQueue.ToList();
            _tempList.Remove(_unitTurn);
            TurnDataQueue.Clear();

            turnDataDict.Remove(_unitTurn.unit);

            foreach (var _turnData in _tempList)
            {
                TurnDataQueue.Enqueue(_turnData);
            }
            _unitTurn.unit.OnTurn = false;
        }
    }

    [VInspector.Button("Create Queue")]
    public void CreateTurnQueueUI()
    {
        foreach (var _turn in TurnDataQueue)
        {
            if (unitTurnSlotDict.ContainsKey(_turn.unit)) continue;

            var _slot = Instantiate(queuePrefab, turnSlotCanvas);
            var _unitSlot = _slot.GetComponent<UnitTurnSlot>();
            _unitSlot.SetUnitSlot(_turn.unit, _turn.unit.Data.Sprite);

            // set the unit slot in turn data
            _turn.unit.TurnData.unitSlot = _unitSlot;

            // add the unit slot to the dictionary
            unitTurnSlotDict.Add(_turn.unit, _unitSlot);
        }
    }

    private void ProcessTurnQueue()
    {
        if (GameController.Instance.CurrentRoom.RoomState == RoomState.Completed) return;

        if (focusTurn.unit.ActionPoint > 0)
        {
            focusTurn.unit.StartTurn();
        }
        else if (focusTurn.unit.ActionPoint <= 0)
        {
            RemoveUnit(focusTurn);
            TurnDataQueue.Enqueue(focusTurn);
            focusTurn.unit.EndTurn();
        }

        // After processing the turn queue
        // Check if current room is cleared or not to proceed and unlock the gates in room
        // GateManager.Instance.UnlockGatesInRoom();
        GameController.Instance.CurrentRoom.RoomStateHandle();
        if (GameController.Instance.CurrentRoom.RoomState == RoomState.Completed)
        {
            GameController.Instance.AlliesUnit.ForEach(_unit => _unit.GridBehavior.ClearAttackPattern());
            GameController.Instance.AlliesUnit.ForEach(_unit => _unit.GridBehavior.ClearMoverPattern());
        }
    }

    public void ActivateTurn()
    {
        var _checkChangeQueue =
            focusTurn.unit == null && GameController.Instance.CurrentRoom.RoomState != RoomState.Completed
            || focusTurn.unit != null
            && !focusTurn.unit.OnTurn;

        if (_checkChangeQueue)
        {
            CreateTurnQueueUI();
            focusTurn = TurnDataQueue.Dequeue();
            turnDataDict.Remove(focusTurn.unit);
            focusTurn.unit.OnTurn = true;
            print($"<color=yellow>change to {focusTurn.unit.gameObject.name} turn</color>");
        }

        ProcessTurnQueue();
    }

    public void ResetQueue()
    {
        TurnDataQueue.Clear();
        turnDataDict.Clear();
        unitTurnSlotDict.Clear();

        focusTurn.unit = null;

        if (turnSlotCanvas.childCount <= 0) return;
        foreach (Transform _turnSlot in turnSlotCanvas)
        {
            Destroy(_turnSlot.gameObject);
        }
    }

    public UnitTurnSlot GetFocusUnitSlot(Character _unit)
    {
        // if (_unit == null) return null;
        if (!unitTurnSlotDict.TryGetValue(_unit, out var _slot)) return null;
        return _slot;
    }
}
