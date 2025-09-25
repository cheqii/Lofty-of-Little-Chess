using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using VInspector;
using Random = UnityEngine.Random;

public class GridManager : Singleton<GridManager>
{
    [Tab("Grid Manager")]
    public GameObject tilePrefab;
    public List<GridMover> AllGrids;
    public List<GameObject> AllGridsObj;
    private Action<bool> OnCompleteGenerate;

    [Tooltip("to check if the grid generate complete in each room")]
    [SerializeField] private bool successGenerate;
    public bool SuccessGenerate => successGenerate;

    private float firstLineTimeCount = 0.035f;
    private float nextLineTimeCount = 0.005f;

    private float waitTimeBeforeComplete = 0.35f;

    private WaitForSeconds waitFirstLineCount;
    private WaitForSeconds waitNextLineCount;
    private WaitForSeconds waitBeforeComplete;

    private void Start()
    {
        waitFirstLineCount = new WaitForSeconds(firstLineTimeCount);
        waitNextLineCount = new WaitForSeconds(nextLineTimeCount);
        waitBeforeComplete = new WaitForSeconds(waitTimeBeforeComplete);

        OnCompleteGenerate += _value =>
        {
            print($"complete generate grid");
            if (!RoomGenerator.Instance.TestGenerate)
            {
                GameController.Instance.OnCompleteGenerateRoomSetup(_value);
            }
        };
    }

    [Button("Generate Grid")]
    public void GenerateGrid()
    {
        AllGrids.Clear();
        AllGridsObj.Clear();

        StartCoroutine(CreateAllGridTile());
    }

    // for testing generate all grid with room generate
    private IEnumerator CreateAllGridTile()
    {
        successGenerate = false;

        var _count = 0;

        foreach (var _room in RoomGenerator.Instance.RoomManager.AllRooms)
        {
            for (int i = 0; i < _room.RoomData.RoomSize; i++)
            {
                for (int j = 0; j < _room.RoomData.RoomSize; j++)
                {
                    if (_count <= 10)
                    {
                        yield return waitFirstLineCount;
                    }
                    else
                    {
                        yield return waitNextLineCount;
                    }

                    _count++;

                    RandomGridSpawn(i, j, _room, out var _gridBlock);

                    _gridBlock.name = _gridBlock.name + _count;
                    var _gridMover = _gridBlock.GetComponentInChildren<GridMover>();
                    var _tween = _gridBlock.transform.DOScale(Vector3.one, _count <= 10 ? 0.85f : 0.35f);

                    _tween.OnComplete(() =>
                    {
                        _tween.Kill();
                    });

                    AllGrids.Add(_gridMover);
                    AllGridsObj.Add(_gridBlock);

                    _room.AllGridsMover.Add(_gridMover);
                }
            }
        }

        foreach (var _grid in AllGrids)
        {
            var _tween = _grid.transform.DOLocalMoveY(0.55f, 0.25f);

            _grid.GetNeighbors();
        }

        successGenerate = true;
        OnCompleteGenerate?.Invoke(successGenerate);

        yield return null;
    }

    public IEnumerator CreateGridTileRoom(Room _room)
    {
        var _count = 0;
        var _roomGridMover = new List<GridMover>();

        for (int i = 0; i < _room.RoomData.RoomSize; i++)
        {
            for (int j = 0; j < _room.RoomData.RoomSize; j++)
            {
                if (_count <= 10)
                {
                    yield return waitFirstLineCount;
                }
                else
                {
                    yield return waitNextLineCount;
                }

                _count++;

                RandomGridSpawn(i, j, _room, out var _gridBlock);
                // print($"Grid Block: {_gridBlock.name}");
                var _gridMover = _gridBlock.GetComponentInChildren<GridMover>();
                _gridBlock.name = _gridBlock.name + i + j;
                _gridBlock.transform.SetParent(_room.RoomTilemap.transform);

                var _tween = _gridBlock.transform.DOScale(Vector3.one, _count <= 10 ? 0.85f : 0.35f);

                _tween.OnComplete(() =>
                {
                    _tween.Kill();
                });

                _roomGridMover.Add(_gridMover);
            }
        }

        _room.SetupGridsRoom(_roomGridMover);


        yield return waitBeforeComplete;

        successGenerate = true;
        OnCompleteGenerate?.Invoke(successGenerate);

        yield return null;
    }

    private void RandomGridSpawn(float _xPos, float _zPos, Room _room, out GameObject _gridObj)
    {
        var _gridIndex = Random.Range(0, _room.RoomData.GridInfo.Count);

        var _currentTile = _room.RoomData.GridInfo[_gridIndex];
        var _tileBase = _currentTile.GridData.TilePalette[Random.Range(0, _currentTile.GridData.TilePalette.Length)];

        foreach (var _tiles in _room.RoomData.GridInfo)
        {
            var _randomRate = Random.Range(0f, 1f);

            if (_randomRate <= _tiles.SpawnRate / 100)
            {
                _currentTile = _tiles;
                // _height = _tiles.Height;
                _tileBase = _tiles.GridData.TilePalette[Random.Range(0, _tiles.GridData.TilePalette.Length)];
            }
        }

        var _gridBlock = PoolManager.Instance.ActiveObjectReturn(_currentTile.GridData.GridPrefab, Vector3.zero, Quaternion.identity, _room.RoomTilemap.transform);
        _gridBlock.transform.SetLocalPositionAndRotation(new Vector3(_xPos, _currentTile.GridData.GridPrefab.transform.position.y + _currentTile.Height, _zPos), Quaternion.identity);
        _gridObj = _gridBlock;

        var _tilePos = new Vector3Int((int)_xPos - 1, (int)_zPos - 1, 0);

        var _worldPos = _room.RoomTilemap.GetCellCenterWorld(_tilePos);

        var _animBlock = Instantiate(tilePrefab, _worldPos, Quaternion.identity);

        _animBlock.transform.localScale = Vector3.zero;

        var _blockAnimTween = _animBlock.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        _blockAnimTween.OnComplete(() =>
        {
            _room.RoomTilemap.SetTile(_tilePos, _tileBase);
            _blockAnimTween.Kill();
            Destroy(_animBlock);
        });
    }
}
