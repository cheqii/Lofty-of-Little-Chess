using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VInspector;
using VInspector.Libs;

public enum GridState
{
    Empty,
    OnMove,
    OnPlayer,
    OnEnemy,
    OnObstacle,
    OnEnemyMove,
    OnEnemyTarget, // for enemy attack player
    OnPlayerAttackField, // for player attack enemy
    OnEnemyAttackField // field check target to attack player
}

public enum GridOwner
{
    None, Ally, Enemy
}

public enum AttackFieldType
{
    None, Ally, Enemy
}

public enum Direction
{
    None, Forward, ForwardLeft, ForwardRight, Backward, BackwardLeft, BackwardRight, Left, Right
}

public class GridMover : MonoBehaviour
{
    #region -Grid Mover-

    [Tab("Grid Mover")]
    [Header("Ref")]
    [SerializeField] private bool enemyDie;
    [SerializeField] private bool isPortal;
    [SerializeField] private bool isAlert;
    [SerializeField] private Character targetOnGrid;

    public bool EnemyDie { get => enemyDie; set => enemyDie = value; }
    public bool IsPortal { get => isPortal; set => isPortal = value; }
    public bool IsAlert { get => isAlert; set => isAlert = value; }
    public Character TargetOnGrid { get => targetOnGrid; set => targetOnGrid = value; }

    [Space(10)]
    [Header("Checker")]
    [SerializeField] private bool onHover;
    [SerializeField] private GameObject selectedObject;
    private MeshRenderer selectedObjectMesh;

    [SerializeField] private GameObject enemyMoveTargetObject;
    [Space(10)]
    [SerializeField] private GridState currentState;
    [SerializeField] private GridState oldState;

    public bool OnHover
    {
        get => onHover;
        set
        {
            if (onHover == value) return;

            onHover = value;
            OnHoverAction?.Invoke(value);
        }
    }
    public GameObject SelectedObject { get => selectedObject; set => selectedObject = value; }
    public GridState CurrentState
    {
        get => currentState;
        set
        {
            OldState = currentState;
            if (currentState == value) return;
            currentState = value;
            OnStateChanged?.Invoke(value);
        }
    }

    public GridState OldState { get => oldState; set => oldState = value; }

    [SerializeField] private bool gridActive;

    public bool GridActive { get => gridActive; set => gridActive = value; }

    [Tab("Grid Material")]
    [Header("Grid Mover Material")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material moveableMat;
    [SerializeField] private Material attackFieldMat;

    [Header("Selected Material")]
    [SerializeField] private Material normalSelectMaterial;
    [SerializeField] private Material enemySelectMaterial;


    [Header("Enemy Grid Mover Material")]
    [SerializeField] private Material attackableMat;
    [SerializeField] private Material enemyMoveMat;
    [SerializeField] private Material onEnemyTargetMat;
    [SerializeField] private Material enemyAttackFieldMat;

    [Space(10)]
    [Header("Optional")]
    [SerializeField] private bool enemyActive;

    [Space]
    [SerializeField] private bool onAttackField;
    [SerializeField] private AttackFieldType attackFieldType;

    private MeshRenderer currentMesh;

    public bool EnemyActive { get => enemyActive; set => enemyActive = value; }
    public bool OnAttackField { get => onAttackField; set => onAttackField = value; }
    public AttackFieldType AttackFieldType { get => attackFieldType; set => attackFieldType = value; }

    public MeshRenderer CurrentMesh { get => currentMesh; set => currentMesh = value; }

    private Action<bool> OnHoverAction;

    // made this to be check the GridMover out of the script when state have changed
    public delegate void OnStateChangedDelegate(GridState _newState);
    public event OnStateChangedDelegate OnStateChanged;

    #endregion // variables for check the player and enemy on grid and grid state

    #region -Grid Node-

    [Tab("Grid Node")]
    [SerializeField] public List<GridMover> Neighbors;
    [SerializeField] private List<GridMover> neighbors => Neighbors;
    [SerializeField] public GridMover Connection;
    public float G { get; private set; } // G => distance from starting node
    public float H { get; private set; } // H => distance from target node

    public float F => G + H; // F

    private List<Vector3> allDirections = new List<Vector3>
    {
        Vector3.forward,
        new(-1, 0, 1), // forward left
        new(1, 0, 1), // forward right
        Vector3.back,
        new(-1, 0, -1), // backward left
        new(1, 0, -1), // backward right
        Vector3.left,
        Vector3.right
    };

    #endregion //  variables for check is grid is available for set the path for movement

    private void Awake()
    {
        CurrentMesh = GetComponent<MeshRenderer>();

        OldState = CurrentState;
    }

    private void Start()
    {
        selectedObjectMesh = SelectedObject.GetComponent<MeshRenderer>();
        SelectedObject.SetActive(false);

        OnStateChanged += _value =>
        {
            // register on state change event
            // print($"<color=green>Grid state changed to {_value}</color>");
            GridStateHandle();
        };

        OnHoverAction += _value =>
        {
            if (CurrentState == GridState.OnObstacle) return;
            SelectedObject.SetActive(_value);
            selectedObjectMesh.material = TargetOnGrid switch
            {
                Enemy => enemySelectMaterial,
                _ => normalSelectMaterial
            };
        };
    }

    #region # Grid Mover Functions

    /// <summary>
    /// *** use after set the material (based on AllIn1SpriteShader property name)***
    /// Set the alpha value of the material with duration
    /// </summary>
    /// <param name="_alphaValue"></param>
    /// <param name="_duration"></param>
    private void SetMaterialAlpha(float _alphaValue, float _duration)
    {
        CurrentMesh.material.DOFloat(_alphaValue, "_Alpha", _duration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            GridActive = true;
        });
    }

    private void SetMaterialAlpha(float _alphaValue, float _duration, float _delay)
    {
        CurrentMesh.material.DOFloat(_alphaValue, "_Alpha", _duration).SetEase(Ease.InOutSine).SetDelay(_delay).OnComplete(() =>
        {
            GridActive = true;
        });
    }

    public void SetAttackField(bool _isOnAttackField, GridOwner _fieldOwner)
    {
        OnAttackField = _isOnAttackField;

        AttackFieldType = _fieldOwner switch
        {
            GridOwner.Ally => AttackFieldType.Ally,
            GridOwner.Enemy => AttackFieldType.Enemy,
            _ => AttackFieldType.None,
        };
    }

    private void GridStateHandle()
    {
        // if (CurrentState == OldState)
        // {
        //     return;
        // }

        CheckMoveType();
    }

    private void CheckMoveType()
    {
        CurrentMesh.enabled = true;
        // OldState = CurrentState;

        switch (CurrentState)
        {
            case GridState.OnMove:
                CurrentMesh.material = moveableMat;
                SetMaterialAlpha(0.4f, 0.5f);
                break;
            case GridState.OnEnemy:
                if (EnemyDie && !EnemyActive)
                {
                    // CurrentState = GridState.OnMove;
                    EnemyDie = false;
                }

                if (AttackFieldType == AttackFieldType.Ally && OnAttackField)
                {
                    ActiveEnemy();
                }
                break;
            case GridState.OnObstacle:
                CurrentMesh.enabled = false;
                break;
            case GridState.Empty:
                CurrentMesh.material = defaultMaterial;
                break;
            case GridState.OnPlayerAttackField:
                ActiveAttackField();
                break;
        }
    }

    public void VisibleEnemyGrid()
    {
        // visible the enemy grid while mouse is hovering
        CurrentMesh.enabled = true;

        switch (CurrentState)
        {
            case GridState.OnEnemyMove:
                CurrentMesh.material = enemyMoveMat;
                SetMaterialAlpha(0.4f, 0.5f);
                break;
            case GridState.OnEnemyTarget:
                CurrentMesh.material = onEnemyTargetMat;
                break;
            case GridState.OnEnemyAttackField:
                CurrentMesh.material = enemyAttackFieldMat;
                SetMaterialAlpha(0.4f, 0.5f, 0.25f);
                break;
            case GridState.OnObstacle:
                CurrentMesh.enabled = false;
                OldState = CurrentState;
                break;
            case GridState.Empty:
                // mesh.enabled = true;
                // CurrentMesh.material = OldMat;
                CurrentMesh.material = defaultMaterial;
                OldState = CurrentState;
                EnemyActive = false;
                break;
        }
    }

    public void ClearGrid()
    {
        isAlert = false;
        GridActive = false;

        CurrentMesh.material.DOKill();
        CurrentMesh.material = defaultMaterial;

        // SetAttackField(false, GridOwner.None);

        switch (CurrentState)
        {
            case GridState.OnMove:
                CurrentState = GridState.Empty;
                break;
            case GridState.OnEnemy:
                SetAttackField(false, GridOwner.None);

                if (TargetOnGrid as Enemy == null)
                {
                    CurrentState = OldState;
                }
                else
                {
                    CurrentState = GridState.OnEnemy;
                    if (OnAttackField)
                    {
                        ActiveEnemy();
                    }
                }
                break;
            case GridState.OnObstacle:
                CurrentMesh.enabled = false;
                break;
            case GridState.Empty:
                // CurrentMesh.material = defaultMaterial;
                break;
            case GridState.OnEnemyMove:
                // if (TargetOnGrid is not Enemy) return;
                // SetAttackField(false, GridOwner.None);
                if (TargetOnGrid as Enemy != null && EnemyActive && OnAttackField)
                {
                    ActiveEnemy();
                }
                else
                {
                    CurrentState = OldState;
                }
                break;
            case GridState.OnEnemyTarget:
                CurrentState = GridState.OnPlayer;
                break;
            case GridState.OnPlayerAttackField:
                SetAttackField(false, GridOwner.None);
                CurrentState = GridState.Empty;
                break;
            case GridState.OnEnemyAttackField:
                CurrentState = OldState;
                break;
        }

        CheckMoveType();
    }

    public void ActiveEnemy()
    {
        if (CurrentState != GridState.OnEnemy)
            CurrentState = GridState.OnEnemy;

        // CurrentMesh.enabled = true;
        EnemyDie = false;
        EnemyActive = true;
        CurrentMesh.material = attackableMat;
        SetMaterialAlpha(0.6f, 0.5f, 0.25f);

        // OnAttackField = true;
        // enemy will active the attack field because player cast the enemy on this grid
        SetAttackField(true, GridOwner.Ally);
    }

    public void ActiveAttackField()
    {
        if (CurrentState != GridState.OnEnemy)
            CurrentState = GridState.OnPlayerAttackField;

        CurrentMesh.material = attackFieldMat;

        SetMaterialAlpha(0.4f, 0.5f, 0.25f);
        SetAttackField(true, GridOwner.Ally);
    }

    public void SetEnemyTarget(bool _isActive)
    {
        enemyMoveTargetObject.SetActive(_isActive);
    }

    #endregion


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            CurrentState = GridState.OnObstacle;
        }

        if (other.TryGetComponent<Character>(out var _character))
        {
            if (TargetOnGrid != null) return;

            if (_character is NewPlayer)
            {
                // if (NewPlayer == null)
                //     NewPlayer = _character as NewPlayer;

                TargetOnGrid = _character;

                if (CurrentState == GridState.Empty)
                    CurrentState = GridState.OnPlayer;
            }
            else if (_character is Enemy)
            {
                TargetOnGrid = _character;

                CurrentState = GridState.OnEnemy;

                if (CurrentState == GridState.OnMove)
                {
                    ActiveEnemy();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            CurrentState = GridState.Empty;
        }

        if (other.TryGetComponent<Character>(out var _character))
        {
            if (TargetOnGrid != _character) return;

            if (_character is NewPlayer)
            {
                if (CurrentState != GridState.OnEnemy)
                {
                    CurrentState = GridState.Empty;
                }
                // NewPlayer = null;
                TargetOnGrid = null;
            }
            else if (_character is Enemy)
            {
                // if condition to protect misunderstanding grid state from the overlap collider while another enemy is pass the grid
                if (CurrentState == GridState.OnPlayerAttackField) return;

                if (CurrentState == GridState.OnEnemy && !OnAttackField)
                {
                    CurrentState = GridState.Empty;
                }

                // FocusEnemy = null;
                EnemyActive = false;
                TargetOnGrid = null;
            }
        }
    }

    #region # Node Functions

    public void SetConnection(GridMover _node)
    {
        // print($"<color=yellow>Set connection {transform.parent.name} to { _node.transform.parent.name}</color>");
        Connection = _node;
    }

    public void SetG(float _origin) => G = _origin;

    public void SetH(float _target) => H = _target;

    public List<GridMover> GetNeighbors()
    {
        Neighbors = new List<GridMover>();
        var _neighborPositions = new List<Vector3>();

        foreach (var _direction in allDirections)
        {
            var _tempPos = transform.parent.localPosition;
            _tempPos += _direction;
            _neighborPositions.Add(_tempPos);
        }

        foreach (var _grid in GridManager.Instance.AllGrids)
        {
            foreach (var _direction in _neighborPositions)
            {
                if (_grid.transform.parent.localPosition != _direction) continue;
                Neighbors.Add(_grid);
            }
        }

        return Neighbors;
    }

    public List<GridMover> GetNeighbors(Room _room)
    {
        Neighbors = new List<GridMover>();
        var _neighborPositions = new List<Vector3>();

        foreach (var _direction in allDirections)
        {
            var _tempPos = transform.parent.localPosition;
            _tempPos += _direction;
            _neighborPositions.Add(_tempPos);
        }

        foreach (var _grid in _room.AllGridsMover)
        {
            foreach (var _direction in _neighborPositions)
            {
                if (_grid.transform.parent.localPosition != _direction) continue;
                Neighbors.Add(_grid);
            }
        }

        return Neighbors;
    }

    public float GetDistance(GridMover _node)
    {
        var _dist = new Vector3(
            Mathf.Abs((int)transform.parent.localPosition.x - (int)_node.transform.parent.localPosition.x),
            0,
            Mathf.Abs((int)transform.parent.localPosition.z - (int)_node.transform.parent.localPosition.z));

        var lowest = Mathf.Min(_dist.x, _dist.z);
        var highest = Mathf.Max(_dist.x, _dist.z);

        var horizontalMovesRequired = highest - lowest;

        // print($"{gameObject.transform.parent.name} distance to {_node.transform.parent.name} is {lowest * 14 + horizontalMovesRequired * 10}");

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    #endregion // functions that use for calculate A* pathfinding
}
