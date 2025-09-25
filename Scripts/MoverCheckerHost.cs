using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[Serializable]
public class CheckerData
{
    public bool dontStop;
    public bool checkSuccess;
    public bool foundTargetBlock;
    public List<MoveChecker> checkerOwn;
}
public class MoverCheckerHost : MonoBehaviour
{
    [Space(10)]
    [Header("Check Direction")]
    public List<CheckerData> checkerData;

    public List<CheckerData> GetCheckerData()
    {
        return checkerData;
    }

    public List<GridMover> GetMoveChecker()
    {
        return (from _checker in checkerData from _grid in _checker.checkerOwn where _grid.gridMover != null select _grid.gridMover).ToList();
    }

    public void SetCheckerData(bool _checkSuccess)
    {
        foreach (var _checker in checkerData)
        {
            _checker.checkSuccess = _checkSuccess;
        }
    }

    #region -> Player Checker Mover Functions
    public void CheckMove()
    {
        foreach (CheckerData checker in checkerData)
        {
            if (checker.checkSuccess)
            {
                continue;
            }

            foreach (MoveChecker mc in checker.checkerOwn.ToList())
            {
                mc.SetAlliesMover();

                // checker.checkSuccess = true;

                if (checker.dontStop == false)
                {
                    if (mc.gridCheck == GridState.OnEnemy || mc.gridCheck == GridState.OnObstacle || mc.gridCheck == GridState.Empty)
                    {
                        checker.checkSuccess = true;
                        break;
                    }
                }
            }
        }
    }

    #endregion

    #region -> Enemy Checker Mover Functions
    public void CheckEnemyMover(Enemy _enemy)
    {
        // print("Set Enemy Mover");
        foreach (CheckerData checker in checkerData)
        {
            if (checker.checkSuccess)
            {
                continue;
            }

            foreach (MoveChecker mc in checker.checkerOwn.ToList())
            {
                mc.SetEnemyMover(_enemy);
                checker.checkSuccess = true;
            }
        }
    }
    #endregion

    public void CheckAttackPattern()
    {
        foreach (CheckerData checker in checkerData)
        {
            checker.foundTargetBlock = false;
            if (checker.checkSuccess)
            {
                continue;
            }

            foreach (MoveChecker mc in checker.checkerOwn.ToList())
            {
                mc.SetAttackPattern(checker.foundTargetBlock);

                if (!checker.dontStop)
                {
                    if (mc.gridCheck == GridState.OnEnemy)
                    {
                        checker.foundTargetBlock = true;
                        checker.checkSuccess = true;
                    }
                    else if (mc.gridCheck == GridState.OnObstacle)
                    {
                        checker.checkSuccess = true;
                        break;
                    }
                }
            }
        }
    }

    public void CheckEnemyAttackPattern(Enemy _enemy)
    {
        foreach (CheckerData checker in checkerData)
        {
            checker.foundTargetBlock = false;
            if (checker.checkSuccess)
            {
                continue;
            }

            foreach (MoveChecker mc in checker.checkerOwn.ToList())
            {
                mc.SetEnemyAttackPattern(checker.foundTargetBlock, _enemy);

                if (!checker.dontStop)
                {
                    if (mc.gridCheck == GridState.OnEnemy)
                    {
                        checker.foundTargetBlock = true;
                        checker.checkSuccess = true;
                    }
                    else if (mc.gridCheck == GridState.OnObstacle)
                    {
                        checker.checkSuccess = true;
                        break;
                    }
                }
            }
        }
    }
}
