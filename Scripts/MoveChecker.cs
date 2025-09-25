using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveChecker : MonoBehaviour
{
    public GridMover gridMover;
    public LayerMask gridLayer;
    public GridState gridCheck;

    public void SetAlliesMover()
    {
        gridMover = Pathfinding.GetOriginGrid(transform, gridLayer);

        if (gridMover == null) return;

        var _checkEnemyDieGrid = gridMover.CurrentState == GridState.OnEnemy
        && gridMover.EnemyActive && gridMover.EnemyDie
        || gridMover.CurrentState == GridState.OnEnemy
        && gridMover.TargetOnGrid is not Enemy or null;

        if (_checkEnemyDieGrid)
        {
            gridMover.CurrentState = gridMover.EnemyDie ? GridState.OnMove : GridState.Empty;
            gridMover.EnemyActive = false;
            gridMover.TargetOnGrid = null;
            gridMover.EnemyDie = false;
            gridMover.SetAttackField(false, GridOwner.None);
        }

        switch (gridMover.CurrentState)
        {
            case GridState.Empty:
                gridMover.CurrentState = GridState.OnMove;
                break;
            case GridState.OnEnemy:
                gridMover.ActiveEnemy();
                break;
            case GridState.OnEnemyMove:
                gridMover.CurrentState = GridState.OnMove;
                break;
            case GridState.OnPlayerAttackField:
                gridMover.CurrentState = GridState.OnMove;
                break;
        }

        gridCheck = gridMover.CurrentState;
    }

    public void SetEnemyMover(Enemy _enemy)
    {
        gridMover = Pathfinding.GetOriginGrid(transform, gridLayer);

        if (gridMover == null) return;

        switch (gridMover.CurrentState)
        {
            case GridState.OnPlayer:
                _enemy.DetectPlayer = gridMover.TargetOnGrid as NewPlayer;
                gridMover.CurrentState = GridState.OnEnemyTarget;
                break;
            case GridState.Empty or GridState.OnPlayerAttackField or GridState.OnEnemy or GridState.OnMove:
                gridMover.CurrentState = GridState.OnEnemyMove;
                break;
        }

        gridCheck = gridMover.CurrentState;
    }

    public void SetAttackPattern(bool _foundTargetBlock)
    {
        gridMover = Pathfinding.GetOriginGrid(transform, gridLayer);

        if (gridMover == null) return;

        if (gridMover.CurrentState == GridState.OnEnemy && gridMover.TargetOnGrid is not Enemy or null)
        {
            gridMover.CurrentState = GridState.Empty;
            gridMover.EnemyActive = false;
            gridMover.EnemyDie = false;
            gridMover.OnAttackField = false;
            gridMover.SetAttackField(false, GridOwner.None);
        }

        switch (gridMover.CurrentState)
        {
            case GridState.OnEnemy:
                if (!_foundTargetBlock)
                {
                    gridMover.ActiveEnemy();
                }
                else
                {
                    gridMover.ActiveAttackField();
                }
                break;
            case GridState.Empty:
                gridMover.ActiveAttackField();
                break;
        }

        gridCheck = gridMover.CurrentState;
    }

    public void SetEnemyAttackPattern(bool _foundTargetBlock, Enemy _enemy)
    {
        gridMover = Pathfinding.GetOriginGrid(transform, gridLayer);

        if (gridMover == null) return;

        if (gridMover.CurrentState == GridState.OnEnemy && gridMover.TargetOnGrid is not Enemy or null)
        {
            gridMover.CurrentState = GridState.Empty;
            gridMover.EnemyActive = false;
            gridMover.EnemyDie = false;
            gridMover.OnAttackField = false;
            gridMover.SetAttackField(false, GridOwner.None);
        }

        switch (gridMover.CurrentState)
        {
            case GridState.OnPlayer:
                _enemy.DetectPlayer = gridMover.TargetOnGrid as NewPlayer;
                if (!_foundTargetBlock)
                {
                    gridMover.CurrentState = GridState.OnEnemyTarget;
                }
                else
                {
                    gridMover.CurrentState = GridState.OnEnemyAttackField;
                }
                gridMover.SetAttackField(true, GridOwner.Enemy);
                break;
            case GridState.Empty:
                gridMover.CurrentState = GridState.OnEnemyAttackField;
                gridMover.SetAttackField(true, GridOwner.Enemy);
                break;
            case GridState.OnPlayerAttackField:
                gridMover.CurrentState = GridState.OnEnemyAttackField;
                gridMover.SetAttackField(true, GridOwner.Enemy);
                break;
            case GridState.OnMove:
                gridMover.CurrentState = GridState.OnEnemyAttackField;
                gridMover.SetAttackField(true, GridOwner.Enemy);
                break;
            case GridState.OnEnemy:
                gridMover.CurrentState = GridState.OnEnemyAttackField;
                break;
        }

        gridCheck = gridMover.CurrentState;
    }
}
