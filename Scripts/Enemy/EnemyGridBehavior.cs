using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes.Test;
using UnityEngine;
using VInspector;

public class EnemyGridBehavior : GridBehavior
{
    private Enemy enemyHost;
    public Enemy EnemyHost { get => enemyHost; set => enemyHost = value; }

    [Tab("Player Node & Move Path")]
    [SerializeField] protected GridMover playerNode;
    public GridMover PlayerNode { get => playerNode; set => playerNode = value; }

    [SerializeField] private bool isPatternVisible;
    [SerializeField] private List<GridMover> patternPath = new List<GridMover>();

    private void Start()
    {
        EnemyHost = GetComponent<Enemy>();
    }

    public void CreatePath()
    {
        OriginNode = Pathfinding.GetOriginGrid(transform, gridLayer);
        PlayerNode = Pathfinding.GetPlayerTransform();

        var _nearestTarget = GetNearestPatternToPlayer();

        movePaths.Clear();
        moveSuccess = false;
        movePaths = Pathfinding.GetPath(EnemyHost, OriginNode, _nearestTarget);

        if (_nearestTarget.Connection == null)
        {
            // nearest target is unreachable then stay in the same position
            _nearestTarget = OriginNode;
        }

        TargetNode = _nearestTarget;

        ClearAttackPattern();
        ClearMoverPattern();
    }

    public void Attack()
    {
        if (EnemyHost.IsDead)
        {
            print($"<color=red>Enemy is dead cannot attack</color>");
            return;
        }
        EnemyHost.DetectPlayer.TakeDamage(EnemyHost.Damage);
        ClearAttackPattern();
        ClearMoverPattern();
        TurnBasedManager.Instance.TurnBasedAction?.Invoke();
    }

    public void MoveToPlayer()
    {
        destinationNode = new Vector3(
            TargetNode.transform.parent.localPosition.x,
             0.5f,
             TargetNode.transform.parent.localPosition.z);

        StartMoving();
    }

    protected override IEnumerator MoveAlongPath()
    {
        while (moveQueue.Count > 0)
        {
            var _targetNode = moveQueue.Dequeue();

            // if (_targetNode.CurrentState != GridState.OnEnemyMove) continue;

            if (_targetNode == PlayerNode) continue;

            var _nextPoint = new Vector3(_targetNode.transform.parent.localPosition.x, transform.localPosition.y, _targetNode.transform.parent.localPosition.z);

            // remove target node that already point player to the position and focus on the others node left in list
            // and use the list for the path core to display it easily than queue
            // movePaths.Remove(_targetNode);

            // move direction animation change via direction of the target position

            // MoveHandle(_direction);

            while (Vector3.Distance(transform.localPosition, _nextPoint) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _nextPoint, EnemyHost.MoveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.localPosition = _nextPoint;
        }

        moveCoroutine = null;

        if (transform.localPosition == destinationNode)
        {
            print($"<color=green>move success</color>");
            moveSuccess = true;

            // Clear enemy target move material visual
            TargetNode.SetEnemyTarget(false);

            // Clear pattern after move complete
            ClearAttackPattern();
            ClearMoverPattern();

            TurnBasedManager.Instance.TurnBasedAction?.Invoke();
        }
    }

    #region # Mover & Attack Pattern Functions

    public override void SetMoverPattern()
    {
        base.SetMoverPattern();
        moverChecker?.CheckEnemyMover(EnemyHost);

        patternPath.Clear();
        patternPath = moverChecker?.GetMoveChecker();
        foreach (var _path in patternPath.ToList())
        {
            if (_path != null) continue;
            patternPath.Remove(_path);
        }
    }

    public override void ClearMoverPattern()
    {
        base.ClearMoverPattern();

        var _currentRoom = GameController.Instance.CurrentRoom;

        foreach (var _grid in _currentRoom.AllGridsMover)
        {
            if (_grid.CurrentState is not GridState.OnEnemyMove) continue;

            _grid.ClearGrid();
        }
    }

    public override void SetAttackPattern()
    {
        base.SetAttackPattern();

        attackCheckerHost?.CheckEnemyAttackPattern(EnemyHost);

        attackPatternPath.Clear();
        attackPatternPath = attackCheckerHost?.GetMoveChecker();
        foreach (var _path in attackPatternPath.ToList())
        {
            if (_path != null) continue;
            attackPatternPath.Remove(_path);
        }
    }

    public override void ClearAttackPattern()
    {
        base.ClearAttackPattern();

        var _currentRoom = GameController.Instance.CurrentRoom;

        foreach (var _grid in _currentRoom.AllGridsMover)
        {
            if (_grid.CurrentState is GridState.OnPlayerAttackField or GridState.OnEnemy or GridState.OnMove) continue;
            _grid.ClearGrid();
        }
    }

    #endregion

    // ===== For Checking and Finding the Grid That Player is On =====
    // ===== and Finding the Nearest Pattern Node to Attack Player =====
    // ===== Based on Attack Pattern Nodes to Find the Nearest Node to Attack Player =====
    #region # Attack Pattern Checker Functions

    public bool IsPlayerOnAttackRange()
    {
        var _inAttackRange = AttackPatternPath.Any(_node => _node.CurrentState == GridState.OnPlayer || _node.TargetOnGrid is NewPlayer);
        return _inAttackRange;
    }

    /// <summary>
    /// Find the nearest node to attack player from the candidate nodes that can attack player in range
    /// </summary>
    /// <param name="_nodesPath"></param> the list of nodes to check the nearest node to attack player
    /// <returns></returns>
    private GridMover FindNearestNodeToAttack(List<GridMover> _nodesPath)
    {
        PlayerNode = Pathfinding.GetPlayerTransform();

        if (_nodesPath.Count <= 0 || _nodesPath == null)
        {
            Debug.LogError("No candidate nodes in list available to find the nearest node to attack.");
            return null;
        }

        var _candidated = OriginNode;
        // if (_candidated.TargetOnGrid != null && _candidated.TargetOnGrid == EnemyHost)
        //     print($"<color=white>first candidate to attack player is {_candidated.transform.parent.name} and list count => {_nodesPath.Count}</color>");

        foreach (var _node in _nodesPath)
        {
            if (_node.GetDistance(PlayerNode) > _candidated.GetDistance(PlayerNode) || _node.TargetOnGrid != null) continue;
            _candidated = _node;
        }

        // if (_candidated == OriginNode)
        // {
        //     print($"<color=white>no available node to attack player</color>");
        // }

        return _candidated;
    }

    /// <summary>
    /// Get the target node to move to attack player
    /// If found candidate node that can attack player in range return the nearest node to player
    /// If not found candidate node that can attack player in range return the nearest pattern node to attack player in next turn or move
    /// </summary>
    /// <param name="_candidateNodes"></param> the list of candidate node that each node can attack player in range
    /// <returns></returns>
    private GridMover GetTargetNode(List<GridMover> _candidateNodes)
    {
        switch (_candidateNodes.Count)
        {
            case > 0: // found candidate node that can attack player in range for current turn
                {
                    // find the nearest node to attack player in range
                    // by checking from the candidate node that can attack player in range
                    print($"check from candidate node");
                    return FindNearestNodeToAttack(_candidateNodes);
                }

            case <= 0: // not found candidate node that can attack player in range for next turn
                {
                    // find the nearest node to attack player in next turn or move
                    // by checking from the mover pattern path to find the nearest node to player (based on which move can attack player in next turn or move)
                    print($"check from mover pattern");
                    return FindNearestNodeToAttack(patternPath);
                }
        }
    }

    /// <summary>
    /// Get the nearest pattern node to player that can attack player
    /// At first have to call the SetMoverPattern() to set the patternPath first
    /// Then call the SetAttackPattern() to set the in the move pattern to check if player is in attack range node by node
    /// return the nearest pattern node to player that can attack player or if not found player target
    /// return the nearest pattern node to attack player in next turn or move
    /// </summary>
    /// <returns></returns>
    public GridMover GetNearestPatternToPlayer()
    {
        var _candidateNodes = new List<GridMover>();

        foreach (var _moveNode in patternPath)
        {
            if (_moveNode.TargetOnGrid != null) continue;

            SetAttackPattern();

            if (IsPlayerOnAttackRange())
            {
                _candidateNodes.Add(_moveNode);
                ClearAttackPattern();
            }
        }

        var _targetNode = GetTargetNode(_candidateNodes);

        return _targetNode;
    }

    #endregion
}
