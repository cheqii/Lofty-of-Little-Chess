using System;
using System.Collections.Generic;
using System.Linq;
using _Lofty.Hidden;
using UnityEngine;
using VInspector;

public class Character : MonoBehaviour
{
    #region -Declared Variables-

    #region -Character Variables-

    [Tab("Character Host")]
    [SerializeField] protected CharacterData data;

    [Space]
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int tempHealth; // yellow heart (like the shield heart but not always belong to player)

    [SerializeField] protected int maxActionPoint;
    [SerializeField] protected int actionPoint;

    [Space]
    [SerializeField] protected int damage;
    [SerializeField] protected int knockBackRange;

    [Space]
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float turnSpeed;

    [Space]
    [SerializeField] protected Animator animator;

    [Space]
    [SerializeField] protected bool isDead;
    [SerializeField] protected bool isImmortal;

    [Space]
    [SerializeField] protected GameObject focusArrow; // use for display which player currently focus (in future player have ally or teammates)

    public CharacterData Data => data;
    // public Transform TargetTransform { get => targetTransform; set => targetTransform = value;}
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int TempHealth { get => tempHealth; set => tempHealth = value; }
    public int MaxActionPoint { get => maxActionPoint; set => maxActionPoint = value; }
    public int ActionPoint { get => actionPoint; set => actionPoint = value; }
    public int Damage { get => damage; set => damage = value; }
    public int KnockBackRange { get => knockBackRange; set => knockBackRange = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float TurnSpeed { get => turnSpeed; set => turnSpeed = value; }
    public Animator Animator { get => animator; set => animator = value; }
    public bool IsDead { get => isDead; set => isDead = value; }
    public bool IsImmortal { get => isImmortal; set => isImmortal = value; }

    public GameObject FocusArrow { get => focusArrow; set => focusArrow = value; }

    #endregion

    #region -Buff & Debuff Variables-

    [Tab("Buff & Debuff")]
    [Header("Buff & Debuff Status")]
    [SerializeField] protected List<BuffInfo> buffHave;
    [SerializeField] protected GameObject buffUIPrefab;
    [SerializeField] protected Transform buffCanvas;

    public List<BuffInfo> BuffHave { get => buffHave; set => buffHave = value; }
    public GameObject BuffUIPrefab { get => buffUIPrefab; set => buffUIPrefab = value; }
    public Transform BuffCanvas { get => buffCanvas; set => buffCanvas = value; }

    [Space]
    [SerializeField] protected List<BuffInfo> debuffHave;
    [SerializeField] protected GameObject debuffUIPrefab;
    [SerializeField] protected Transform debuffCanvas;

    public List<BuffInfo> DebuffHave { get => debuffHave; set => debuffHave = value; }
    public GameObject DebuffUIPrefab { get => debuffUIPrefab; set => debuffUIPrefab = value; }
    public Transform DebuffCanvas { get => DebuffCanvas; set => DebuffCanvas = value; }

    #endregion

    #region -Turn Variables-


    [Tab("About Turn")]
    [Header("Turn Variables")]
    [SerializeField] protected TurnData turnData;

    [Space]
    [SerializeField] protected bool skipTurn;
    [SerializeField] protected bool autoSkip;
    [SerializeField] protected bool onTurn;

    public TurnData TurnData { get => turnData; set => turnData = value; }
    public bool SkipTurn { get => skipTurn; set => skipTurn = value; }
    public bool AutoSkip { get => autoSkip; set => autoSkip = value; }
    public bool OnTurn { get => onTurn; set => onTurn = value; }

    #endregion


    // # Component Variables
    private GridBehavior gridBehavior;
    private GridTargetMarked gridTargetMarked;
    private SelectableObject selectableObject;

    public GridBehavior GridBehavior => gridBehavior;

    public GridTargetMarked GridTargetMarked => gridTargetMarked;

    public SelectableObject SelectableObject => selectableObject;

    #endregion


    protected virtual void Awake()
    {
        gridBehavior = GetComponent<GridBehavior>();
        gridTargetMarked = GetComponentInChildren<GridTargetMarked>();
        selectableObject = GetComponent<SelectableObject>();

        SetData();
    }

    protected virtual void Start()
    {

    }

    protected virtual void SetData()
    {
        // setup stats variables from scriptable object
        MaxActionPoint = Data.MaxActionPoint;
        ActionPoint = MaxActionPoint;

        MaxHealth = Data.MaxHealth;
        CurrentHealth = MaxHealth;

        MoveSpeed = Data.MoveSpeed;
        TurnSpeed = Data.TurnSpeed;

        Damage = Data.Damage;

        GridBehavior.CurrentPattern = Data.MovePattern;
        GridBehavior.CurrentAttackPattern = Data.AttackPattern;
    }

    protected void CheckMarkedVisible()
    {
        if (GridTargetMarked != null)
        {
            GridTargetMarked.SetVisibleGridMarked(OnTurn);
        }
    }

    public virtual void StartTurn()
    {
        if (IsDead)
        {
            print($"<color=red>{gameObject.name} is dead so it can't start turn</color>");
            return;
        }
    }

    public virtual void EndTurn()
    {
        OnTurn = false;
        ResetActionPoint();

        TurnBasedManager.Instance.TurnBasedAction?.Invoke();
    }

    public virtual void Heal(int _amount)
    {
        if (CurrentHealth >= MaxHealth) return;
        CurrentHealth += _amount;

        var _limitHeal = MaxHealth;

        if (CurrentHealth >= _limitHeal)
        {
            CurrentHealth = _limitHeal;
        }
    }

    public virtual void TakeDamage(int _damage)
    {
        if (IsImmortal) return;
    }

    public virtual void KnockBack(Transform _center, int _knockBackRange)
    {
        var _direction = (transform.position - _center.position).normalized;
        var _knockBackOffset = new Vector3(
            Mathf.Round(_direction.x) * _knockBackRange,
            0,
            Mathf.Round(_direction.z) * _knockBackRange);

        var _knockBackDir = _knockBackOffset / _knockBackRange;

        if (!GridBehavior.BlockChecker(_knockBackDir))
        {
            transform.position += _knockBackOffset;
        }
    }

    public virtual void DeadHandle()
    {
        if (CurrentHealth > 0) return;
        IsDead = true;
    }

    public virtual void ResetActionPoint()
    {
        ActionPoint = MaxActionPoint;
    }

    public virtual void IncreaseActionPoint(int _amount)
    {
        ActionPoint += _amount;

        if (ActionPoint > MaxActionPoint)
        {
            ActionPoint = MaxActionPoint;
        }
    }

    public virtual void DecreaseActionPoint(int _amount)
    {
        ActionPoint -= _amount;

        if (ActionPoint < 0)
        {
            ActionPoint = 0;
        }
    }

    #region # Buff & Debuff Functions

    public virtual void AddDebuff(DebuffType _debuffType, int _turnTime)
    {

    }

    public virtual void DebuffHandle()
    {
        // if(BuffHave.Count <= 0) return;
        //
        // foreach (var _debuff in BuffHave.Where(_buff => !_buff.curseActivated))
        // {
        //     switch (_debuff.debuffType)
        //     {
        //         case DebuffType.Stun:
        //             // end turn the character whatever player or enemy
        //             break;
        //
        //         case DebuffType.Bleed:
        //             TakeDamage(1);
        //             break;
        //
        //         case DebuffType.Burn:
        //             TakeDamage(1);
        //             break;
        //
        //         case DebuffType.Provoke:
        //             break;
        //     }
        // }
    }

    public virtual void DebuffUIUpdate()
    {
        foreach (var _debuff in DebuffHave)
        {
            _debuff.UI.debuffType = _debuff.DebuffType;
            _debuff.UI.turnCount.text = _debuff.CurseTurn.ToString();
        }
    }

    #endregion
}
