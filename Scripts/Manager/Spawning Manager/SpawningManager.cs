using System;
using System.Collections.Generic;
using System.Linq;
using _Lofty.Hidden.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

public static class SpawningManager
{
    private static List<ObjectSpawnList> allEnemies;
    private static List<ObjectSpawnList> allObstacles;
    private static List<ObjectSpawnList> allTraps;

    public static void Setup()
    {
        foreach (SpawningCostType _type in Enum.GetValues(typeof(SpawningCostType)))
        {
            if (_type == SpawningCostType.None) continue;
            if (!PoolManager.Instance.SpawningCostsDict.TryGetValue(_type, out var _poolable)) return;
            switch (_type)
            {
                case SpawningCostType.Enemy:
                    allEnemies = _poolable.ObjectSpawnList.Where(_object => _object.Cost > 0).ToList();
                    break;
                case SpawningCostType.Obstacle:
                    allObstacles = _poolable.ObjectSpawnList;
                    break;
                case SpawningCostType.Trap:
                    allTraps = _poolable.ObjectSpawnList;
                    break;
            }
        }
    }

    public static GameObject GetEnemy()
    {
        // Debug.Log($"enemy count {allEnemies.Count}");

        var _enemyIndex = Random.Range(0, allEnemies.Count);
        var _current = allEnemies[_enemyIndex];
        if (PoolManager.Instance.SpawningCostsDict.TryGetValue(SpawningCostType.Enemy, out var _poolable))
        {
            _poolable.SpawningCost -= _current.Cost;
            allEnemies.ShuffleOrder(_poolable.ObjectSpawnList);
        }

        return _current.Prefab;
    }


    public static GameObject GetObstacle()
    {
        var _obstacleIndex = Random.Range(0, allObstacles.Count);
        var _current = allObstacles[_obstacleIndex];
        if (PoolManager.Instance.SpawningCostsDict.TryGetValue(SpawningCostType.Obstacle, out var _poolable))
        {
            _poolable.SpawningCost -= _current.Cost;
            allObstacles.ShuffleOrder(_poolable.ObjectSpawnList);
        }

        return _current.Prefab;
    }


    public static GameObject GetTrap()
    {
        var _trapIndex = Random.Range(0, allTraps.Count);
        var _current = allTraps[_trapIndex];
        if (PoolManager.Instance.SpawningCostsDict.TryGetValue(SpawningCostType.Trap, out var _poolable))
        {
            _poolable.SpawningCost -= _current.Cost;
            allTraps.ShuffleOrder(_poolable.ObjectSpawnList);
        }

        return _current.Prefab;
    }
    public static GameObject GetEnemy(Room _room)
    {
        var _enemyList = new List<ObjectSpawnList>(allEnemies);

        var _enemyIndex = Random.Range(0, _enemyList.Count);
        var _current = _enemyList[_enemyIndex];

        _room.SpawningCostsDict[SpawningCostType.Enemy] -= _current.Cost;
        allEnemies.ShuffleOrder();

        return _current.Prefab;
    }

    public static GameObject GetObstacle(Room _room)
    {
        var _obstacleIndex = Random.Range(0, allObstacles.Count);
        var _current = allObstacles[_obstacleIndex];

        _room.SpawningCostsDict[SpawningCostType.Obstacle] -= _current.Cost;
        allObstacles.ShuffleOrder();

        return _current.Prefab;
    }

    public static GameObject GetTrap(Room _room)
    {
        var _trapIndex = Random.Range(0, allTraps.Count);
        var _current = allTraps[_trapIndex];

        _room.SpawningCostsDict[SpawningCostType.Trap] -= _current.Cost;
        allTraps.ShuffleOrder();

        return _current.Prefab;
    }
}
