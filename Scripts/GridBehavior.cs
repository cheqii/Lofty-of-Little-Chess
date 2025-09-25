using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Lofty;
using _Lofty.Hidden.Helpers;
using Com.LuisPedroFonseca.ProCamera2D.TopDownShooter;
using UnityEngine;
using VInspector;

public class GridBehavior : MonoBehaviour
{
    #region -Grid Movement Variables-

    [Tab("About Movement")]
    [SerializeField] protected BehaviorState currentState;

    [Space]
    [SerializeField] protected PatternType currentPattern;
    [SerializeField] protected Transform patternParent;
    [SerializeField] protected List<PatternData> patternData;

    public PatternType CurrentPattern { get => currentPattern; set => currentPattern = value; }
    public Transform PatternParent { get => patternParent; set => patternParent = value; }

    private bool initialPattern;
    protected Dictionary<PatternType, GameObject> patternDict;

    public Dictionary<PatternType, GameObject> PatternDict
    {
        get
        {
            if (initialPattern) return patternDict;

            patternDict = new Dictionary<PatternType, GameObject>();

            foreach (var _pattern in patternData)
            {
                patternDict.Add(_pattern.patternType, _pattern.patternPrefab.gameObject);
            }

            initialPattern = true;
            return patternDict;
        }
    }

    [SerializeField] protected GameObject ownPattern;
    public GameObject OwnPattern { get => ownPattern; set => ownPattern = value; }

    protected MoverCheckerHost moverChecker;

    [Space]
    [Header("Move Path Variables")]

    [SerializeField] protected LayerMask gridLayer;
    [SerializeField] protected GridMover originNode;
    [SerializeField] protected GridMover targetNode;
    public GridMover OriginNode { get => originNode; set => originNode = value; }
    public GridMover TargetNode { get => targetNode; set => targetNode = value; }

    [SerializeField] protected Vector3 destinationNode;
    [SerializeField] protected bool moveSuccess;

    [SerializeField] protected List<GridMover> movePaths = new List<GridMover>();
    protected Queue<GridMover> moveQueue = new Queue<GridMover>();

    protected Coroutine moveCoroutine;
    public Coroutine MoveCoroutine { get => moveCoroutine; set => moveCoroutine = value; }

    #endregion

    #region -Direction Variables-

    [Header("Check Directions")]
    protected bool forwardDirection;
    protected bool forwardLeftDirection;
    protected bool forwardRightDirection;
    protected bool backwardDirection;
    protected bool backwardLeftDirection;
    protected bool backwardRightDirection;
    protected bool leftDirection;
    protected bool rightDirection;

    protected List<bool> allDirections;

    [Space]
    [SerializeField] protected LayerMask blockedLayer;

    public bool ForwardDirection { get => forwardDirection; set => forwardDirection = value; }
    public bool ForwardLeftDirection { get => forwardLeftDirection; set => forwardLeftDirection = value; }
    public bool ForwardRightDirection { get => forwardRightDirection; set => forwardRightDirection = value; }
    public bool BackwardDirection { get => backwardDirection; set => backwardDirection = value; }
    public bool BackwardLeftDirection { get => backwardLeftDirection; set => backwardLeftDirection = value; }
    public bool BackwardRightDirection { get => backwardRightDirection; set => backwardRightDirection = value; }
    public bool LeftDirection { get => leftDirection; set => leftDirection = value; }
    public bool RightDirection { get => rightDirection; set => rightDirection = value; }
    public LayerMask BlockedLayer => blockedLayer;

    private bool initialDirection;
    protected List<bool> AllDirections
    {
        get
        {
            if (initialDirection) return allDirections;
            allDirections = new List<bool>
            {
                ForwardDirection,
                ForwardLeftDirection,
                ForwardRightDirection,
                BackwardDirection,
                BackwardLeftDirection,
                BackwardRightDirection,
                LeftDirection,
                RightDirection
            };
            initialPattern = true;
            return allDirections;
        }
    }

    [Space]
    [SerializeField] protected Direction currentDirection;

    protected Dictionary<Vector3, Direction> moveDirections;

    public Direction CurrentDirection { get => currentDirection; set => currentDirection = value; }

    private bool initialMoveDirections;

    public Dictionary<Vector3, Direction> MoveDirections
    {
        get
        {
            if (initialMoveDirections) return moveDirections;
            moveDirections = new Dictionary<Vector3, Direction>
            {
                {Vector3.zero, Direction.None},
                {Vector3.forward, Direction.Forward},
                {new Vector3(-1, 0, 1), Direction.ForwardLeft},
                {new Vector3(1, 0, 1), Direction.ForwardRight},
                {Vector3.back, Direction.Backward},
                {new Vector3(-1, 0, -1), Direction.BackwardLeft},
                {new Vector3(1, 0, -1), Direction.BackwardRight},
                {Vector3.left, Direction.Left},
                {Vector3.right, Direction.Right}
            };

            initialMoveDirections = true;
            return moveDirections;
        }
    }

    private Dictionary<Direction, Func<bool>> blockedDirections;

    private bool initialBlockedDirections;
    public Dictionary<Direction, Func<bool>> BlockedDirections
    {
        get
        {
            if (initialBlockedDirections) return blockedDirections;
            blockedDirections = new Dictionary<Direction, Func<bool>>
            {
                {Direction.Forward, () => ForwardDirection},
                {Direction.ForwardLeft, () => ForwardLeftDirection},
                {Direction.ForwardRight, () => ForwardRightDirection},
                {Direction.Backward, () => BackwardDirection},
                {Direction.BackwardLeft, () => BackwardLeftDirection},
                {Direction.BackwardRight, () => BackwardRightDirection},
                {Direction.Left, () => LeftDirection},
                {Direction.Right, () => RightDirection}
            };

            initialBlockedDirections = true;
            return blockedDirections;
        }
    }

    [Tab("Attack Pattern")]
    [SerializeField] protected AttackPattern currentAttackPattern;
    [SerializeField] protected Transform attackPatternParent;

    [Space]
    [SerializeField] protected List<AttackData> attackData;

    [Space]
    [SerializeField] protected GameObject ownAttackPattern;

    public AttackPattern CurrentAttackPattern { get => currentAttackPattern; set => currentAttackPattern = value; }
    public Transform AttackPatternParent => attackPatternParent;

    public GameObject OwnAttackPattern { get => ownAttackPattern; set => ownAttackPattern = value; }


    private bool initialAttackPattern;
    protected Dictionary<AttackPattern, GameObject> attackPatternDict;
    public Dictionary<AttackPattern, GameObject> AttackPatternDict
    {
        get
        {
            if (initialAttackPattern) return attackPatternDict;

            attackPatternDict = new Dictionary<AttackPattern, GameObject>();

            foreach (var _data in attackData)
            {
                attackPatternDict.Add(_data.AttackPattern, _data.PatternPrefab);
            }

            initialAttackPattern = true;
            return attackPatternDict;
        }
    }

    protected MoverCheckerHost attackCheckerHost;

    [SerializeField] protected List<GridMover> attackPatternPath = new List<GridMover>();

    public List<GridMover> AttackPatternPath { get => attackPatternPath; set => attackPatternPath = value; }

    #endregion

    private void OnEnable()
    {
        // Subscribe to events or initialize variables
        OriginNode = Pathfinding.GetOriginGrid(transform, gridLayer);
    }

    public Direction GetDirection(Vector3 _direction)
    {
        return MoveDirections.GetValueOrDefault(_direction, Direction.None);
    }

    public void CheckBlockDirectionCast()
    {
        //Forward Check
        ForwardDirection = Physics.Raycast(transform.position, Vector3.forward, 1, BlockedLayer);
        ForwardLeftDirection = Physics.Raycast(transform.position, new Vector3(-1, 0, 1), 1, BlockedLayer);
        ForwardRightDirection = Physics.Raycast(transform.position, new Vector3(1, 0, 1), 1, BlockedLayer);

        //Backward Check
        BackwardDirection = Physics.Raycast(transform.position, Vector3.back, 1, BlockedLayer);
        BackwardLeftDirection = Physics.Raycast(transform.position, new Vector3(-1, 0, -1), 1, BlockedLayer);
        BackwardRightDirection = Physics.Raycast(transform.position, new Vector3(1, 0, -1), 1, BlockedLayer);

        //Left & Right
        LeftDirection = Physics.Raycast(transform.position, Vector3.left, 1, BlockedLayer);
        RightDirection = Physics.Raycast(transform.position, Vector3.right, 1, BlockedLayer);
    }

    public bool BlockChecker()
    {
        CheckBlockDirectionCast();

        // check around the character directions
        // to check enemy nearby which directions
        // to check if obstacle nearby and can't move to which directions

        var _isBlocked = AllDirections.All(_value => _value);
        return _isBlocked;
    }

    public bool BlockChecker(Vector3 _direction)
    {
        // check is the direction through params get blocked?
        // ** Call CheckBlockDirectionCast before use this function for casting the block direction first ** //

        CheckBlockDirectionCast();

        if (!MoveDirections.TryGetValue(_direction, out var _checkDirection)) return false;
        return BlockedDirections.TryGetValue(_checkDirection, out var _blockedDirection) && _blockedDirection();
    }

    /// <summary>
    /// Start moving the player along the calculated path.
    /// </summary>
    protected virtual void StartMoving()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveQueue.Clear();

        foreach (var _node in movePaths)
        {
            moveQueue.Enqueue(_node);
        }

        moveCoroutine = StartCoroutine(MoveAlongPath());
    }

    protected virtual IEnumerator MoveAlongPath()
    {
        moveSuccess = false;
        yield return null;
    }

    #region > Move Pattern

    public virtual void SetMoverPattern()
    {
        if (!PatternDict.TryGetValue(CurrentPattern, out var _pattern)) return;
        if (OwnPattern != null) ClearMoverPattern();
        OwnPattern = PoolManager.Instance.ActiveObjectReturn(_pattern, PatternParent.position, Quaternion.identity, patternParent);

        moverChecker = OwnPattern.GetComponent<MoverCheckerHost>();

        moverChecker.SetCheckerData(false);

        // check move (use override functions for check move player or enemy after this)
    }

    public virtual void ClearMoverPattern()
    {
        if (OwnPattern == null) return;
        PoolManager.Instance.DeActiveObject(OwnPattern);
        OwnPattern = null;

        // clear mover base on player or enemy condition
        // write the code in their own script
    }

    #endregion

    public virtual void SetAttackPattern()
    {
        if (!AttackPatternDict.TryGetValue(CurrentAttackPattern, out var _pattern)) return;
        if (OwnAttackPattern != null) ClearAttackPattern();

        OwnAttackPattern = PoolManager.Instance.ActiveObjectReturn(_pattern, AttackPatternParent.position, Quaternion.identity, attackPatternParent);

        attackCheckerHost = OwnAttackPattern.GetComponent<MoverCheckerHost>();
        attackCheckerHost.SetCheckerData(false);
    }

    public virtual void ClearAttackPattern()
    {
        if (OwnAttackPattern == null) return;
        PoolManager.Instance.DeActiveObject(OwnAttackPattern);
        OwnAttackPattern = null;
    }
}
