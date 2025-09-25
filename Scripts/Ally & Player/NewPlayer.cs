using System;
using System.Linq;
using UnityEngine;
using VInspector;
using _Lofty.Hidden.Skill;
using _Lofty.Hidden.UI;
using _Lofty.Hidden;
using _Lofty.Hidden.Manager;
using _Lofty.Hidden.Interface;
using _Lofty.Hidden.Helpers;

public class NewPlayer : Character
{
    #region -Declared Variables-

    #region -Player Stats Variables-

    [Tab("Own Variables")]

    [SerializeField] private string name = "ROSSO";

    [Header("Default Stats")]
    [SerializeField] private int defaultMaxHealth;

    [Header("Skill Activated Check")]
    [SerializeField] private bool haveShield;

    [Header("Current Behavior Check")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isTakingDamage;

    public string Name { get => name; set => name = value; }
    public int DefaultMaxHealth { get => defaultMaxHealth; set => defaultMaxHealth = value; }
    public bool HaveShield { get => haveShield; set => haveShield = value; }
    public bool IsAttacking { get => isAttacking; set => isAttacking = value; }
    public bool IsTakingDamage { get => isTakingDamage; set => isTakingDamage = value; }

    #endregion

    [Space]
    [SerializeField] private Animator damageAnimator;
    public Animator DamageAnimator { get => damageAnimator; set => damageAnimator = value; }

    // [SerializeField] private TurnData turnData;

    private ClassPassive classPassive;

    public ClassPassive ClassPassive => classPassive;

    private Action<bool> alertAction;
    public Action<bool> AlertAction => alertAction;

    public LayerMask InteractableLayer;

    #endregion

    protected override void Start()
    {
        base.Start();

        DummyRossoUI.Instance.SetupActionPointUI(MaxActionPoint);

        print($"start player");
        ActionKeyManager.Instance.ActionKey.InteractAction.Interact.performed +=
            _ctx =>
            {
                CheckOverlapInteractable();
            };
    }

    protected override void SetData()
    {
        base.SetData();

        DefaultMaxHealth = Data.MaxHealth;
    }

    private void CheckOverlapInteractable()
    {
        // Use OverlapBox to detect colliders at the origin
        var hits = Physics.OverlapBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, InteractableLayer);
        foreach (var collider in hits)
        {
            if (collider.TryGetComponent<Interactable>(out var _interactable) && _interactable.InteractableObject.IsActive)
            {
                _interactable.Interact();
            }
        }
    }

    public override void ResetActionPoint()
    {
        base.ResetActionPoint();
        DummyRossoUI.Instance.UpdateCurrentActionPoint(ActionPoint);
    }

    public override void IncreaseActionPoint(int _amount)
    {
        base.IncreaseActionPoint(_amount);
        DummyRossoUI.Instance.UpdateCurrentActionPoint(ActionPoint);
    }

    public override void DecreaseActionPoint(int _amount)
    {
        base.DecreaseActionPoint(_amount);
        DummyRossoUI.Instance.UpdateCurrentActionPoint(ActionPoint);
    }

    public override void StartTurn()
    {
        base.StartTurn();

        if (!OnTurn) return;

        CheckMarkedVisible();

        DummyRossoUI.Instance.UpdateCurrentActionPoint(ActionPoint);

        // GridBehavior.SetAttackPattern();
        if (GridBehavior is not PlayerGridBehavior _playerGrid) return;
        if (_playerGrid.MoveMode != PlayerMoveMode.GridMove) return;

        _playerGrid.SetAttackPattern();
        _playerGrid.SetMoverPattern();

        SelectableObject.SetSelected(true);
    }

    public override void EndTurn()
    {
        base.EndTurn();

        CheckMarkedVisible();
        SelectableObject.SetSelected(false);
        // DummyRossoUI.Instance.UpdateCurrentActionPoint(ActionPoint);
    }

    #region # TakeDamage, Heal, and Special Attack Functions

    public override void Heal(int _amount)
    {
        base.Heal(_amount);
        VisualEffectManager.Instance.CallEffect(EffectName.Heal, transform, 1.5f);

        // if player health is greater or equal max health then can't heal
        if (CurrentHealth >= MaxHealth) return;

        // currentHealth += _amount + playerTarot.ArtifactHealMultiple;
        currentHealth += _amount;

        // var _limitTarotHp = MaxHealth + PlayerTarot.ArtifactHp + TempHealth;
        var _limitHeal = MaxHealth;

        if (CurrentHealth >= _limitHeal)
        {
            CurrentHealth = MaxHealth;
        }

        // update health UI
    }

    public void DamageCalculation(int _damage)
    {
        if (tempHealth > 0)
        {
            var _damageWithTemp = tempHealth - _damage;
            if (_damageWithTemp < 0)
            {
                currentHealth += _damageWithTemp;

                // deactivated the temp yellow heart display
            }
            else
            {
                // update temp heart display UI
                // update temp heart by code
                tempHealth -= _damage;
            }

            HealthUIManager.Instance.UpdateHealth();
        }
        else
        {
            currentHealth -= _damage;
        }

        // set damage animator set trigger to "TakeDamage"
        DamageAnimator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }

        // Update heart UI

        // HealthUIManager.Instance.UpdateHealth(currentHealth);
        HealthUIManager.Instance.OnHealthChanged?.Invoke(currentHealth);
    }


    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);

        CameraManager.Instance.TriggerShake();

        if (HaveShield)
        {
            HaveShield = false;
            // TurnManager.Instance.AddLog(Name, "", LogList.Block, true);
            return;
        }

        IsTakingDamage = true;

        // if (!ClassPassive.IsClassPassiveTwoActive(ClassType.BladeMaster))
        // {
        //     // if blade master passive 2 not active then just calculate the damage normal
        DamageCalculation(_damage);
        // }
        // else
        // {
        //     ClassPassive.ActivatePassive(_damage);
        // }

        IsTakingDamage = false;
    }

    #endregion

    public override void DeadHandle()
    {
        base.DeadHandle();

        IsDead = true;
        VisualEffectManager.Instance.CallEffect(EffectName.Dead, transform, 1f);

        if (!isDead) return;
        GameManager.Instance.GameOver();
        // game over
    }

    public override void AddDebuff(DebuffType _debuffType, int _turnTime)
    {
        base.AddDebuff(_debuffType, _turnTime);
    }

    public override void DebuffHandle()
    {
        base.DebuffHandle();
    }

    public override void DebuffUIUpdate()
    {
        if (DebuffHave.Count <= 0) return;

        foreach (var _debuff in debuffHave.Where(_debuff => !_debuff.Activated))
        {
            switch (_debuff.DebuffType)
            {
                case DebuffType.Stun:
                    break;
                case DebuffType.Burn:
                    break;
                case DebuffType.Bleed:
                    break;
                case DebuffType.Provoke:
                    break;
            }
        }
    }

    public void StatusHandle()
    {
        // upgrade all player stats
    }

    public void SaveStats()
    {
        ES3.Save("Hp", CurrentHealth);
        ES3.Save("Yellow_Hp", TempHealth); // yellow heart health
        ES3.Save("Dmg", Damage);
        ES3.Save("Speed", TurnSpeed);
        // ES3.Save("Pattern", );
    }

    public void LoadStats()
    {
        ES3.Load("Hp", CurrentHealth);
        ES3.Load("Yellow_Hp", TempHealth);
        ES3.Load("Dmg", Damage);
        ES3.Load("Speed", TurnSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}
