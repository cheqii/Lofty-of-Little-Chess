using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Lofty.Hidden.Helpers;
using UnityEngine;
using VInspector;

public class PlayerGridBehavior : GridBehavior
{
    #region -Declared Variabled-

    private NewPlayer playerRosso;

    [Tab("Normal Setup")]
    [SerializeField] private Enemy targetEnemy;

    public Enemy TargetEnemy { get => targetEnemy; set => targetEnemy = value; }

    #region -Grid Movement Variables-

    [Space]
    private ActionKey moveAction;

    [Header("Navigation")]
    [SerializeField] private GameObject endNavigatorPrefab;
    [SerializeField] private GameObject arrowNavigatorPrefab;

    [Space]
    [SerializeField] private List<GameObject> arrowsPath = new List<GameObject>();
    [Space]
    [SerializeField] private GameObject endArrow;

    [SerializeField] private PlayerMoveMode moveMode = PlayerMoveMode.GridMove;

    public PlayerMoveMode MoveMode { get => moveMode; set => moveMode = value; }

    #endregion

    #region -Attack Action Behavior-

    [SerializeField] private AttackType attackType;
    public AttackType AttackType { get => attackType; set => attackType = value; }

    #endregion

    #endregion

    private void Awake()
    {
        moveAction = new ActionKey();
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Start()
    {
        playerRosso = GetComponent<NewPlayer>();
        moveAction.MouseAction.OnClick.performed += _ctx => OnMouseClickAction();
    }

    /// <summary>
    /// Handle the state of the action node based on the target node and hit information.
    /// </summary>
    /// <param name="_targetNode"></param>
    /// <param name="_hit"></param>
    private void ActionNodeState(GridMover _targetNode, RaycastHit _hit)
    {
        var _unCompleteRoomCheck = GameController.Instance.CurrentRoom.RoomData.RoomType is RoomTypes.Combat or RoomTypes.Boss
                && GameController.Instance.CurrentRoom.RoomState is RoomState.InProgress;

        // if (!_targetNode.GridActive) return;

        switch (_targetNode.CurrentState)
        {
            case GridState.OnMove: // to move to target action
                if (!_targetNode.GridActive) return;
                playerRosso.DecreaseActionPoint(1);

                FreeGridMovement(_targetNode);
                ClearAttackPattern();
                ClearMoverPattern();
                break;
            case GridState.OnEnemy: // to attack to enemy action

                var _pointerToAttack =
                _targetNode.TargetOnGrid is not Enemy or null
                || _targetNode.EnemyDie
                || _targetNode.TargetOnGrid is Enemy _enemy && _enemy.IsDead
                || !_targetNode.EnemyActive
                || !_targetNode.GridActive;

                if (_pointerToAttack) return;

                playerRosso.DecreaseActionPoint(1);
                TargetEnemy = _targetNode.TargetOnGrid as Enemy;

                SetAttackAnimation(_hit);
                break;
            case GridState.Empty: // to move everywhere after the room is clear
                // if the room is not complete or clear then return
                if (_unCompleteRoomCheck && moveMode == PlayerMoveMode.GridMove) return;
                ClearAttackPattern();
                ClearMoverPattern();
                FreeGridMovement(_targetNode);
                break;
        }
    }

    /// <summary>
    /// Handle the mouse click action for the player.
    /// </summary>
    private void OnMouseClickAction()
    {
        OriginNode = Pathfinding.GetOriginGrid(transform, gridLayer);

        var _ray = Camera.main!.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(_ray, out RaycastHit _hit, Mathf.Infinity, gridLayer)) return;
        if (_hit.collider == null) return;

        var _gridNode = _hit.collider.GetComponent<GridMover>();

        ActionNodeState(_gridNode, _hit);
    }

    #region # Grid Movement Functions

    /// <summary>
    /// Free grid movement use for the free movement while the room is clear or not a combat
    /// (not a movement that can move by pattern)
    /// </summary>
    /// <param name="_targetNode"></param>
    private void FreeGridMovement(GridMover _targetNode)
    {
        destinationNode = new Vector3(_targetNode.transform.position.x, transform.position.y, _targetNode.transform.position.z);
        TargetNode = _targetNode;
        movePaths?.Clear();
        movePaths = Pathfinding.GetPath(playerRosso, OriginNode, TargetNode);

        ClearNavigation();
        CreateNavigation();

        StartMoving();
    }

    /// <summary>
    /// Create navigation arrows between grid nodes.
    /// the arrow is show the path way to the target nodes
    /// </summary>
    private void CreateNavigation()
    {
        if (OriginNode == TargetNode) return;

        foreach (var _path in movePaths.ToList().Where(_path => _path != TargetNode))
        {
            Vector3 _direction;

            if (_path == TargetNode.Connection)
            {
                // if path is the arrow before the target path then set the before target path to the target node path direction
                _direction = TargetNode.transform.position - _path.transform.position;
            }
            else
            {
                // normal path direction if the arrow path is not node before the target node
                _direction = _path.transform.position - _path.Connection.transform.position;
            }

            // var _arrow = Instantiate(arrowNavigatorPrefab, _path.transform.position, Quaternion.identity);
            var _arrow = PoolManager.Instance.ActiveObjectReturn(arrowNavigatorPrefab, _path.transform.position, Quaternion.identity);

            _arrow.transform.rotation = GetDirection(_direction) switch
            {
                Direction.Forward => Quaternion.Euler(0, 0, 0),
                Direction.Backward => Quaternion.Euler(0, 180, 0),
                Direction.Left => Quaternion.Euler(0, -90, 0),
                Direction.Right => Quaternion.Euler(0, 90, 0),
                Direction.ForwardLeft => Quaternion.Euler(0, -45, 0),
                Direction.ForwardRight => Quaternion.Euler(0, 45, 0),
                Direction.BackwardLeft => Quaternion.Euler(0, -135, 0),
                Direction.BackwardRight => Quaternion.Euler(0, 135, 0),
                _ => _arrow.transform.rotation
            };

            arrowsPath.Add(_arrow);
        }

        // activated the end arrow at the end of destination of path
        // endArrow = Instantiate(endNavigatorPrefab, destinationNode, Quaternion.identity);
        endArrow = PoolManager.Instance.ActiveObjectReturn(endNavigatorPrefab, destinationNode, Quaternion.identity);
    }

    /// <summary>
    /// Clear all navigation arrows.
    /// </summary>
    public void ClearNavigation()
    {
        if (arrowsPath.Count > 0)
        {
            PoolManager.Instance.DeActiveObject(arrowsPath);
        }

        if (endArrow != null)
        {
            PoolManager.Instance.DeActiveObject(endArrow);
            endArrow = null;
        }
    }

    /// <summary>
    /// Move the player along the calculated path.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator MoveAlongPath()
    {
        while (moveQueue.Count > 0)
        {
            var _targetNode = moveQueue.Dequeue();
            var _nextPoint = new Vector3(_targetNode.transform.position.x, transform.position.y, _targetNode.transform.position.z);

            // remove target node that already point player to the position and focus on the others node left in list
            // and use the list for the path core to display it easily than queue
            movePaths.Remove(_targetNode);

            // move direction animation change via direction of the target position
            var _direction = (_nextPoint - transform.position);
            MoveHandle(_direction);

            while (Vector3.Distance(transform.position, _nextPoint) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _nextPoint, playerRosso.MoveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = _nextPoint;
        }

        moveCoroutine = null;

        if (transform.position == destinationNode)
        {
            moveSuccess = true;
            MoveHandle(Vector3.zero); // set animation to default idle after player go through the destination Vector.zero => None Direction

            // ClearMoverPattern();

            // SetMoverPattern();
            if (GameController.Instance.CurrentRoom.RoomData.RoomType is RoomTypes.Combat or RoomTypes.Boss
                && GameController.Instance.CurrentRoom.RoomState is RoomState.InProgress)
            {
                if (moveMode == PlayerMoveMode.GridMove)
                    TurnBasedManager.Instance.TurnBasedAction?.Invoke();
                else
                {
                    playerRosso.DecreaseActionPoint(playerRosso.MaxActionPoint);
                    TurnBasedManager.Instance.TurnBasedAction?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// Set the movement pattern for the player.
    /// </summary>
    public override void SetMoverPattern()
    {
        base.SetMoverPattern();
        moverChecker?.CheckMove();
    }

    /// <summary>
    /// Clear the movement pattern for the player.
    /// </summary>
    public override void ClearMoverPattern()
    {
        base.ClearMoverPattern();
        // GridSpawnManager.Instance.ClearMover();
        var _currentRoom = GameController.Instance.CurrentRoom;
        foreach (var _grid in _currentRoom.AllGridsMover)
        {
            if (_grid.CurrentState is GridState.OnPlayerAttackField) continue;
            _grid.ClearGrid();
        }
    }

    public override void SetAttackPattern()
    {
        base.SetAttackPattern();
        attackCheckerHost?.CheckAttackPattern();

        AttackPatternPath.Clear();
        AttackPatternPath = attackCheckerHost?.GetMoveChecker();
        foreach (var _path in AttackPatternPath.ToList())
        {
            if (_path != null) continue;
            AttackPatternPath.Remove(_path);
        }
    }

    public override void ClearAttackPattern()
    {
        base.ClearAttackPattern();

        var _currentRoom = GameController.Instance.CurrentRoom;

        foreach (var _grid in _currentRoom.AllGridsMover)
        {
            // if (_grid.CurrentState is GridState.OnEnemyMove) continue;
            _grid.ClearGrid();
        }
    }

    /// <summary>
    /// Set the animation move direction for the player.
    /// </summary>
    /// <param name="_direction"></param>
    private void SetAnimationMoveDirection(Vector3 _direction)
    {
        // if direction is none or Vector3 is zero then Set animation onMove => false
        playerRosso.Animator.SetBool("OnMove", _direction != Vector3.zero);

        playerRosso.Animator.SetFloat("X", _direction.x);
        playerRosso.Animator.SetFloat("Z", _direction.z);
    }


    /// <summary>
    /// Handle the movement direction for the player.
    /// </summary>
    /// <param name="_direction"></param>
    public void MoveHandle(Vector3 _direction)
    {
        if (!MoveDirections.TryGetValue(_direction, out var _targetDirection)) return;
        CurrentDirection = _targetDirection;

        SetAnimationMoveDirection(_direction);
    }

    #endregion

    /// <summary>
    /// call this function with attack animation to the action match with animation!
    /// </summary>
    public void AttackAction()
    {
        if (targetEnemy == null) return;

        switch (AttackType)
        {
            case AttackType.NormalAttack:
                TargetEnemy.TakeDamage(playerRosso.Damage);
                if (TargetEnemy.IsDead)
                {
                    TargetEnemy = null;
                }
                break;
            case AttackType.FightingStyleAttack:
                break;
        }

        ClearAttackPattern();
        ClearMoverPattern();
        TurnBasedManager.Instance.TurnBasedAction?.Invoke();
    }

    /// <summary>
    /// Set the attack animation based on the hit information.
    /// </summary>
    /// <param name="_hit"></param>
    private void SetAttackAnimation(RaycastHit _hit)
    {
        if (_hit.transform.position.x < transform.position.x
            || _hit.transform.position.z > transform.position.z)
        {
            playerRosso.Animator.SetTrigger("CloseAttackLeft");
        }
        else if (_hit.transform.position.x > transform.position.x
                 || _hit.transform.position.z < transform.position.z)
        {
            playerRosso.Animator.SetTrigger("CloseAttackRight");
        }
    }
}
