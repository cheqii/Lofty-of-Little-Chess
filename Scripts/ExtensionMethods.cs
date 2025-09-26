using System.Collections.Generic;
using UnityEngine;

namespace _Lofty.Hidden.Utility
{
    public static class ExtensionMethods
    {
        public static T GetOrCreate<T>(this GameObject _object) where T : Component
        {
            return _object.TryGetComponent<T>(out var _component) ? _component : _object.AddComponent<T>();
        }

        public static T GetOrCreate<T>(this GameObject _object, out T _result) where T : Component
        {
            if (_object.TryGetComponent<T>(out var _tempResult))
            {
                _result = _tempResult;
                return _tempResult;
            }

            _result = _object.AddComponent<T>();
            return _result;
        }

        public static void ShuffleOrder<T>(this List<T> _shuffleOrder)
        {
            for (int i = 0; i < _shuffleOrder.Count; i++)
            {
                var _random = Random.Range(0, _shuffleOrder.Count);
                var _tempOrder = _shuffleOrder[i];
                _shuffleOrder[i] = _shuffleOrder[_random];
                _shuffleOrder[_random] = _tempOrder;
            }
        }

        public static void ShuffleOrder<T>(this List<T> _shuffleOrder, List<T> _originOrder)
        {
            // _shuffleOrder.Clear();
            _shuffleOrder = _originOrder;

            for(int i = 0; i < _shuffleOrder.Count; i++)
            {
                var _random = Random.Range(0, _shuffleOrder.Count);
                var _tempOrder = _shuffleOrder[i];
                _shuffleOrder[i] = _shuffleOrder[_random];
                _shuffleOrder[_random] = _tempOrder;
            }
        }

        // public static List<T> ShuffleOrder<T>(List<T> _shuffleOrder, List<T> _originOrder)
        // {
        //     _shuffleOrder.Clear();
        //     _shuffleOrder.AddRange(_originOrder);
        //
        //     for (int i = 0; i < _shuffleOrder.Count; i++)
        //     {
        //         var _random = Random.Range(0, _shuffleOrder.Count);
        //         var _tempOrder = _shuffleOrder[i];
        //         _shuffleOrder[i] = _shuffleOrder[_random];
        //         _shuffleOrder[_random] = _tempOrder;
        //     }
        //
        //     return _shuffleOrder;
        // }
    }
}
