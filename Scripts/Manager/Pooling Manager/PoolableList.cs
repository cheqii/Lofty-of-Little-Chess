using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public enum SpawningCostType
{
    None,
    Enemy,
    Obstacle,
    Trap
}

[Serializable]
public class PoolableList
{
    public string PoolName;
    public SpawningCostType SpawningCostType;

    [Space]
    [ShowIf(nameof(HasCost)), AllowNesting]
    public int DefaultSpawningCost;

    [ShowIf(nameof(HasCost)), AllowNesting]
    public int SpawningCost;

    public List<ObjectSpawnList> ObjectSpawnList = new List<ObjectSpawnList>();

    public void SetupCost(ref int _spawningCost)
    {
        _spawningCost = DefaultSpawningCost;
    }

    #region -for show & hide attribute-

    public bool HasCost()
    {
        return SpawningCostType != SpawningCostType.None;
    }

    #endregion
}
