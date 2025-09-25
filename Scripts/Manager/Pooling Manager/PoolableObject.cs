using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    [HideInInspector]
    public GameObject prefab;
    
    [HideInInspector]
    public Transform DefaultParent;

    public void SetupPrefab(GameObject _prefab, Transform _parent)
    {
        prefab = _prefab;
        DefaultParent = _parent;
    }
}
