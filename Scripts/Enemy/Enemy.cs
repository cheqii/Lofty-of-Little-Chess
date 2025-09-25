using System.Linq;
using _Lofty.Hidden;
using _Lofty.Hidden.Utility;
using UnityEngine;
using VInspector;

public class Enemy : Character
{
    #region -Declared Variables-

    protected EnemyGridBehavior enemyGridBehavior;
    public EnemyGridBehavior EnemyGridBehavior => enemyGridBehavior;

    [Tab("Own Variables")]
    [SerializeField] protected NewPlayer detectPlayer;
    public NewPlayer DetectPlayer { get => detectPlayer; set => detectPlayer = value; }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        enemyGridBehavior = GetComponent<EnemyGridBehavior>();
    }

    private void Update()
    {
        // use for real quick test
    }

    public override void StartTurn()
    {
        base.StartTurn();

        if (GridBehavior is not EnemyGridBehavior _enemyGrid)
        {
            return;
        }

        CheckMarkedVisible();

        // Decrease action point
        // 1 points for 1 action (moving or attacking)
        DecreaseActionPoint(1);

        // Set enemy movement state to moving state

        DetectPlayer = null;

        _enemyGrid.SetAttackPattern();
        _enemyGrid.SetMoverPattern();

        if (!_enemyGrid.IsPlayerOnAttackRange())
        {
            print($"<color=yellow>{gameObject.name} moves! and not found player target in range</color>");

            _enemyGrid.CreatePath();
            _enemyGrid.MoveToPlayer();
        }
        else
        {
            print($"<color=red>{gameObject.name} attack player!</color>");
            _enemyGrid.Attack();
        }
    }

    public override void EndTurn()
    {
        base.EndTurn();

        CheckMarkedVisible();
    }

    public override void Heal(int _amount)
    {
        base.Heal(_amount);
    }

    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);

        Animator.SetTrigger("TakeDamage");
        CameraManager.Instance.TriggerShake();
        CurrentHealth -= _damage;

        if (CurrentHealth > 0) return;

        CurrentHealth = 0;
        DeadHandle();
    }

    public override void KnockBack(Transform _center, int _knockBackRange)
    {
        if (Data is EnemyData _enemyData && _enemyData.IsBoss) return;
        base.KnockBack(_center, _knockBackRange);
    }

    public override void DeadHandle()
    {
        base.DeadHandle();

        if (IsDead)
        {
            var _currentNode = Pathfinding.GetOriginGrid(transform, LayerMask.GetMask("GridMove"));

            _currentNode.EnemyDie = IsDead;

            if (_currentNode.TargetOnGrid == this)
            {
                _currentNode.TargetOnGrid = null;
            }
        }

        var _turnBasedManager = TurnBasedManager.Instance;
        _turnBasedManager.RemoveUnit(TurnData);
        _turnBasedManager.ClearQueueUnit(TurnData);
        EndTurn();

        PoolManager.Instance.DeActiveObject(gameObject);
    }

    public override void AddDebuff(DebuffType _debuffType, int _turnTime)
    {
        base.AddDebuff(_debuffType, _turnTime);
    }

    public override void DebuffHandle()
    {
        base.DebuffHandle();
    }

    // use in-case enemy is poolable object
    public virtual void ResetEnemy()
    {
        IsDead = false;
        IsImmortal = false;
        SetData();

        // change enemy movement state to idle state
        // EnemyGridBehavior.
    }
}
