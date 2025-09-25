using System;
using NaughtyAttributes;
using UnityEngine;


[Serializable]
public class ObjectSpawnList
{
    public GameObject Prefab;
    [Range(0, 2000)]
    public int PoolSize = 5;

    [SerializeField] private bool HaveCost;
    [ShowIf("HaveCost"), AllowNesting]
    public int Cost;

    [Space]
    public Transform Parent;
}
