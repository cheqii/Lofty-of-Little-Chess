using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    public static List<GridMover> GetPath(Character _character, GridMover _originNode, GridMover _targetNode) // path without pattern?
    {
        var _toSearch = new List<GridMover>() { _originNode }; // use for search and choose the node to find a way to target
        var _processed = new List<GridMover>(); // processed to check that which node is already search

        while (_toSearch.Any())
        {
            // start from origin node that player stand on and get check the value around origin node to get the cost
            var _current = _toSearch[0];

            // SEARCH_NODE:
            foreach (var _t in _toSearch.Where(_t => _t.F < _current.F || _t.F == _current.F && _t.H < _current.H))
            {
                _current = _t;
            }

            _processed.Add(_current);
            _toSearch.Remove(_current);

            if (_current == _targetNode)
            {
                var _currentPathTile = _targetNode;
                var _path = new List<GridMover>();

                while (_currentPathTile != _originNode) // just give the way back to the origin node with the shortest path
                {
                    _path.Add(_currentPathTile);

                    // after add the node from target then set it to the CONNECTION nodes to lead way back to the origin
                    _currentPathTile = _currentPathTile.Connection;
                }

                // if the character is enemy then set visible to the target node for enemy
                if (_character is Enemy)
                {
                    _targetNode.SetEnemyTarget(true);
                }

                // because the path is storing the path from end to start then need to reverse it to use in list
                _path.Reverse();
                return _path;
            }

            foreach (var _neighbor in _current.Neighbors.Where(_node => !_processed.Contains(_node)))
            {
                if (_neighbor.TargetOnGrid != null) continue;
                if (_neighbor.CurrentState is GridState.OnEnemy or GridState.OnObstacle or GridState.OnPlayer) continue;

                var _inSearch = _toSearch.Contains(_neighbor);

                var _costToNeighbor = _current.G + _current.GetDistance(_neighbor); // current get distance it's mean current find the H cost

                // if neighbor is not contain in the "Search list" AND "current F cost is less than neighbor G cost"
                // then set the G cost to neighbor by F cost of current (cause neighbor is never set the G cost before it's no way to greater than 0)
                // and set the neighbor connection with current
                if (_inSearch && !(_costToNeighbor < _neighbor.G)) continue;
                _neighbor.SetG(_costToNeighbor);
                _neighbor.SetConnection(_current);

                // if neighbor is not contain in the "Search list"
                // then set the H cost to neighbor by getting the distance from target node
                // and add neighbor to the next node to "Search list"
                if (_inSearch) continue;
                _neighbor.SetH(_neighbor.GetDistance(_targetNode));
                _toSearch.Add(_neighbor);

                // after end of this to set the G and H cost to neighbor of current the go back to the SEARCH_NODE:
                // to search the neighbor in the list to find the way node that have the less of F cost or H cost (in case F cost is equal to another nodes)
                // and loop until the current is equal to target
                // and that's mean we find the shortest way to get on the target in
            }
        }

        return null;
    }

    public static GridMover GetOriginGrid(Transform _origin, LayerMask _layer)
    {
        // get the origin grid or the current grid that the object have stand on
        if (!Physics.Raycast(_origin.position, Vector3.down, out RaycastHit _hit, Mathf.Infinity, _layer)) return null;
        // return _hit.collider == null ? null : _hit.collider.GetComponent<GridMover>();
        return _hit.transform.GetComponent<GridMover>();
    }

    public static GridMover GetPlayerTransform()
    {
        // get current enemy pos (currentGrid) to find the nearest player or ally (for the future if player have teammates)
        // find the player position from all grid
        // var _room = GetCurrentRoom(_origin, _layer);

        var _allGrids = GameController.Instance.CurrentRoom.AllGridsMover;

        var _playerNode = _allGrids.Find(_grid => _grid.TargetOnGrid is NewPlayer || _grid.CurrentState is GridState.OnPlayer);

        return _playerNode;
    }

    public static GridMover GetPlayerTransform(GridMover _currentGrid)
    {
        return null;
    }
}
