using System.Collections.Generic;
using System.Linq;
using _Lofty.Hidden.Utility;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    // [SerializeField] private StageData stageData;
    // public StageData StageData => stageData;
    public List<PoolableList> PoolableLists;

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private bool initSpawningCost;

    private Dictionary<SpawningCostType, PoolableList> spawningCostsDict = new Dictionary<SpawningCostType, PoolableList>();
    public Dictionary<SpawningCostType, PoolableList> SpawningCostsDict
    {
        get
        {
            if (initSpawningCost) return spawningCostsDict;

            foreach (var _poolableObj in GameController.Instance.CurrentStage.PoolableLists)
            {
                if (_poolableObj.SpawningCostType == SpawningCostType.None) continue;
                spawningCostsDict.Add(_poolableObj.SpawningCostType, _poolableObj);
            }

            initSpawningCost = true;
            return spawningCostsDict;
        }
    }

    private void Start()
    {
        foreach (var _poolableList in GameController.Instance.CurrentStage.PoolableLists.SelectMany(_object => _object.ObjectSpawnList))
        {
            CreatePool(_poolableList.Prefab, _poolableList.PoolSize);
        }

        foreach (var _poolObj in PoolableLists.SelectMany(_object => _object.ObjectSpawnList))
        {
            CreatePool(_poolObj.Prefab, _poolObj.PoolSize, _poolObj.Parent);
        }
    }

    public void ClearPool()
    {
        pools.Clear();
    }

    public void CreatePool(GameObject _prefab, int _poolSize)
    {
        // protect to make sure that pools will no have the same key value
        if (pools.ContainsKey(_prefab)) return;

        var _newPool = new Queue<GameObject>();
        for (int i = 0; i < _poolSize; i++)
        {
            var _obj = Instantiate(_prefab, transform);

            // ??? why would PoolableObject do ??? //
            // we use PoolableObject to identify the prefab of each object to enqueue and dequeue object
            var _identify = _obj.GetOrCreate<PoolableObject>();
            _identify.SetupPrefab(_prefab, transform);
            _obj.SetActive(false);
            _newPool.Enqueue(_obj);
        }

        pools.Add(_prefab, _newPool);
    }

    public void CreatePool(GameObject _prefab, int _poolSize, Transform _parent)
    {
        // protect to make sure that pools will no have the same key value
        if (pools.ContainsKey(_prefab)) return;

        var _newPool = new Queue<GameObject>();
        for (int i = 0; i < _poolSize; i++)
        {
            var _obj = Instantiate(_prefab, _parent);

            // ??? why would PoolableObject do ??? //
            // we use PoolableObject to identify the prefab of each object to enqueue and dequeue object
            var _identify = _obj.GetOrCreate<PoolableObject>();
            _identify.SetupPrefab(_prefab, _parent);
            _obj.SetActive(false);
            _newPool.Enqueue(_obj);
        }

        pools.Add(_prefab, _newPool);
    }

    #region # Active Object Functions

    public void ActiveObject(List<GameObject> _objectList)
    {
        foreach (var _object in _objectList)
        {
            if (_object.activeInHierarchy) continue;

            if (!_object.TryGetComponent<PoolableObject>(out var _identifyPrefab)) continue;

            if (!pools.TryGetValue(_identifyPrefab.prefab, out var _pool)) continue;
            // _object.transform.SetParent(_identifyPrefab.DefaultParent);
            var _obj = _pool.Dequeue();
            _obj.SetActive(true);
        }
    }

    public void ActiveObject(GameObject _prefab, Vector3 _position, Quaternion _rotation)
    {
        // check if pools (Dictionary) have _prefab key then get value by _pool
        if (!pools.TryGetValue(_prefab, out var _pool))
        {
            Debug.LogError("No pool available for " + _prefab);
            return;
        }
        if (_pool.Count <= 0)
        {
            Debug.LogError($"{_prefab.name} pool is empty");
            return;
        }

        // remove object from pool and then set active the object and set position and rotation
        var _obj = _pool.Dequeue();
        _obj.SetActive(true);
        _obj.transform.position = _position;
        _obj.transform.rotation = _rotation;
    }

    public GameObject ActiveObjectReturn(GameObject _prefab, Vector3 _position, Quaternion _rotation)
    {
        // check if pools (Dictionary) have _prefab key then get value by _pool
        if (!pools.TryGetValue(_prefab, out var _pool))
        {
            Debug.LogError("No pool available for " + _prefab);
            return null;
        }
        if (_pool.Count <= 0)
        {
            Debug.LogError($"{_prefab.name} pool is empty");
            return null;
        }
        // remove object from pool and then set active the object and set position and rotation
        var _poolObj = _pool.Dequeue();
        _poolObj.SetActive(true);
        _poolObj.transform.position = _position;
        _poolObj.transform.rotation = _rotation;

        // print($"object active : {_poolObj.name}");

        return _poolObj;
    }

    public GameObject ActiveObjectReturn(GameObject _prefab, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        // check if pools (Dictionary) have _prefab key then get value by _pool
        if (!pools.TryGetValue(_prefab, out var _pool))
        {
            Debug.LogError("No pool available for " + _prefab);
            return null;
        }
        if (_pool.Count <= 0)
        {
            Debug.LogError($"{_prefab.name} pool is empty");
            return null;
        }
        // remove object from pool and then set active the object and set position and rotation
        var _poolObj = _pool.Dequeue();
        _poolObj.SetActive(true);
        _poolObj.transform.position = _position;
        _poolObj.transform.rotation = _rotation;
        _poolObj.transform.SetParent(_parent);

        // print($"object active : {_poolObj.name}");

        return _poolObj;
    }

    #endregion

    #region # Deactive Object Functions

    public void DeActiveObject(GameObject _objDeactive)
    {
        // set object to false and add object back to the pool by check identify from PoolPrefabIdentify
        // check pools (Dictionary) keys and add object back into the pool

        _objDeactive.SetActive(false);

        if (!_objDeactive.TryGetComponent<PoolableObject>(out var _identifyPrefab))
        {
            Debug.LogError($"{_objDeactive.name} is not a PoolableObject");
            return;
        }

        if (!pools.TryGetValue(_identifyPrefab.prefab, out var _pool))
        {
            Debug.LogError("No pool available for " + _identifyPrefab.prefab.name);
            return;
        }

        _objDeactive.transform.SetParent(_identifyPrefab.DefaultParent);
        _pool.Enqueue(_objDeactive);

        // print($"{_objDeactive.name} is enqueue");
    }

    public void DeActiveObject(List<GameObject> _objDeactiveList, bool _clearList = true)
    {
        if (_objDeactiveList == null || _objDeactiveList.Count <= 0) return;

        foreach (var _obj in _objDeactiveList)
        {
            if (!_obj.activeInHierarchy) continue;

            _obj.SetActive(false);

            if (!_obj.TryGetComponent<PoolableObject>(out var _identifyPrefab)) continue;

            if (!pools.TryGetValue(_identifyPrefab.prefab, out var _pool)) continue;
            _obj.transform.SetParent(_identifyPrefab.DefaultParent);
            _pool.Enqueue(_obj.gameObject);
            // print($"{_obj.name} is enqueue");
        }

        if (!_clearList) return;
        _objDeactiveList.Clear();
    }

    public void DeActiveObject<T>(List<T> _objDeactiveList) where T : MonoBehaviour
    {
        if (_objDeactiveList == null || _objDeactiveList.Count <= 0) return;

        foreach (var _obj in _objDeactiveList)
        {
            if (!_obj.gameObject.activeInHierarchy) continue;

            _obj.gameObject.SetActive(false);

            if (!_obj.TryGetComponent<PoolableObject>(out var _identifyPrefab)) return;

            if (!pools.TryGetValue(_identifyPrefab.prefab, out var _pool)) return;
            _obj.transform.SetParent(_identifyPrefab.DefaultParent);
            _pool.Enqueue(_obj.gameObject);
            // print($"{_obj.name} is enqueue");
        }

        _objDeactiveList.Clear();
    }

    #endregion
}
