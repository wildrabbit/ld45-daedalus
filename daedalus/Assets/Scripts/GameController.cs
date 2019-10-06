using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WaitPlayerInput,
    WaitPlayerAction,
    Placement,
    Simulating,
    // Tutorial??
}

public enum GameResult
{
    None,
    Running,
    Won,
    Lost
}

public class GameController : MonoBehaviour
{
    [SerializeField] Map _map;
    [SerializeField] Player _player;
    [SerializeField] Transform _scrollRoot;
    [SerializeField] Transform _playerStart;
    [SerializeField] Transform _levelExit;

    [SerializeField] Color _validBlockTint;
    [SerializeField] Color _invalidBlockTint;

    [SerializeField] GameInput _gameInput;

    readonly Vector3Int[] _offsets = new Vector3Int[] {
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0)
    };

    Vector3Int _playerStartCoords;
    Vector3Int _playerEndCoords;
    List<Scroll> _scrolls;

    bool _playerActionFinished;
    GameState _gameState;
    GameResult _gameResult = GameResult.None;
    int _turnCount;

    PlaceableBlock _blockPrefab;
    PlaceableBlock _tempBlockInstance;

    void Start()
    {
        StartGame();    
    }

    void StartGame()
    {
        _turnCount = 0;
        _playerActionFinished = false;
        _gameState = GameState.WaitPlayerInput;

        _scrolls = new List<Scroll>(_scrollRoot.GetComponentsInChildren<Scroll>());
        if(_map != null)
        {
            _map.InitTilesFromView();
            _playerStartCoords = _map.CoordsFromWorld(_playerStart.position);
            _playerEndCoords = _map.CoordsFromWorld(_levelExit.position);
        }

        _player.transform.position = _map.WorldFromCoords(_playerStartCoords, centered:true);

        _gameResult = NoPossiblePaths() ? GameResult.Lost : GameResult.Running;
        RefreshVisibility();

        PurgeOverlappingScrolls();
    }

    private void PurgeOverlappingScrolls()
    {
        HashSet<Vector3Int> foundCoords = new HashSet<Vector3Int>();
        List<Scroll> toRemove = new List<Scroll>();
        foreach(var scroll in _scrolls)
        {
            var coords = _map.CoordsFromWorld(scroll.transform.position);
            if (foundCoords.Contains(coords))
            {
                Debug.Log($"Found overlapping scroll {scroll.name} at {foundCoords}");
                toRemove.Add(scroll);
            }
            else
            {
                foundCoords.Add(coords);
            }
        }
        foreach(var remove in toRemove)
        {
            _scrolls.Remove(remove);
            Destroy(remove.gameObject);
        }
    }

    void RefreshVisibility()
    {
        Vector3Int playerCoords = _map.CoordsFromWorld(_player.transform.position);
        _map.RefreshHiddenTiles(playerCoords);

        foreach (var scroll in _scrolls)
        {
            scroll.SetVisible(IsVisibleEntity(scroll));
        }        
    }

    bool IsVisibleEntity(Entity entity)
    {
        Vector3Int coords = _map.CoordsFromWorld(entity.transform.position);
        return (_map.GetTileDataAt(coords)?.TileType ?? TileType.None) == TileType.Ground;
    }

    // Update is called once per frame
    void Update()
    {
        if(_gameResult != GameResult.Running)
        {
            // Do stuff (handle restarts)
            if(_gameResult == GameResult.Won && _gameInput.Confirmed)
            {
                PlayState.Instance.NextScene();
            }
            else if(_gameResult == GameResult.Lost && _gameInput.Confirmed)
            {
                PlayState.Instance.LoadCurrentLevel();
            }
            return;
        }

        switch (_gameState)
        {
            case GameState.WaitPlayerInput:
            {
                if(_gameInput.Direction != MoveDirection.None)
                {
                    Vector3Int coordOffset = _offsets[(int)_gameInput.Direction];
                    Vector3Int playerCoords = _map.CoordsFromWorld(_player.transform.position);
                    Vector3Int targetCoords = playerCoords + coordOffset;

                    if(_map.EntityCanMoveTo(targetCoords))
                    {
                        _gameState = GameState.WaitPlayerAction;
                        Vector3 targetWorld = _map.WorldFromCoords(targetCoords, centered:true);
                        StartCoroutine(MoveTo(targetWorld));
                    }
                }
                break;
            }
            case GameState.WaitPlayerAction:
            {
                if(_playerActionFinished)
                {
                    _playerActionFinished = false;
                    if(_map.CoordsFromWorld(_player.transform.position).Equals(_map.CoordsFromWorld(_levelExit.position)))
                    {
                        _gameResult = GameResult.Won;
                    }
                    else
                    {
                        _gameState = GameState.Simulating;
                    }                    
                }
                break;
            }
            case GameState.Placement:
            {
                if (_tempBlockInstance == null)
                {
                    Debug.LogError($"WAT");
                }
                else
                {
                    Vector3Int blockCoords = _map.CoordsFromWorld(_tempBlockInstance.transform.position);
                    bool validPos = _map.CanSetBlock(_tempBlockInstance, blockCoords);
                    if (_gameInput.Direction != MoveDirection.None)
                    {
                        Vector3Int coordOffset = _offsets[(int)_gameInput.Direction];
                        Vector3Int targetCoords = blockCoords + coordOffset;
                        BoundsInt bounds = _tempBlockInstance.Bounds;
                        int w = bounds.size.x;
                        int h = bounds.size.y;
                        bounds.xMin = targetCoords.x;
                        bounds.yMin = targetCoords.y;
                        bounds.xMax = targetCoords.x + w;
                        bounds.yMax = targetCoords.y + h;

                        targetCoords = _map.ClampBlockToFitBounds(bounds);

                        validPos = _map.CanSetBlock(_tempBlockInstance, targetCoords);
                        _tempBlockInstance.transform.position = _map.WorldFromCoords(targetCoords, centered: false);
                    }
                    else if(_gameInput.Rotation != RotateDirection.None)
                    {
                        _tempBlockInstance.Rotate(_gameInput.Rotation);
                        validPos = _map.CanSetBlock(_tempBlockInstance, blockCoords);
                    }
                    else if(_gameInput.Confirmed)
                    {
                        validPos = _map.TrySetBlock(_tempBlockInstance, blockCoords);
                        if(validPos)
                        {
                            Destroy(_tempBlockInstance.gameObject);
                            _tempBlockInstance = null;
                            RefreshVisibility();
                            _player.SetTint(Color.white);
                            if(NoPossiblePaths())
                            {
                                _gameResult = GameResult.Lost;
                            }
                            else
                            {
                                _gameState = GameState.Simulating;
                            }                           
                        }
                    }
                    else if(_gameInput.Cancelled)
                    {
                        Destroy(_tempBlockInstance.gameObject);
                        _tempBlockInstance = null;
                        _player.SetTint(Color.white);
                        if (NoPossiblePaths())
                        {
                            _gameResult = GameResult.Lost;
                        }
                        else
                        {
                            _gameState = GameState.Simulating;
                        }
                    }
                    if(_tempBlockInstance != null)
                    {
                        _tempBlockInstance.SetTint(validPos ? _validBlockTint : _invalidBlockTint);
                    }
                }
                break;
            }
            case GameState.Simulating:
            {
                // Stuff moves around
                _turnCount++;
                _playerActionFinished = false;
                _gameState = GameState.WaitPlayerInput;
                break;
            }
        }
        _gameInput.Reset();

        if(_gameResult != GameResult.Running)
        {
            Debug.LogFormat("{0}", _gameResult == GameResult.Won ? "won!" : "lost");
        }
    }

    private List<Scroll> GetScrollsAt(Vector3Int targetCoords)
    {
        List<Scroll> scrolls = new List<Scroll>();
        foreach (var scroll in _scrolls)
        {
            if (_map.CoordsFromWorld(scroll.transform.position).Equals(targetCoords))
            {
                scrolls.Add(scroll);
            }
        }

        return scrolls;
    }

    private Scroll GetScrollAt(Vector3Int targetCoords)
    {
        foreach(var scroll in _scrolls)
        {
            if(_map.CoordsFromWorld(scroll.transform.position).Equals(targetCoords))
            {
                return scroll;
            }
        }

        return null;
    }

    IEnumerator MoveTo(Vector3 pos)
    {
        Vector3 playerPos = _player.transform.position;
        Vector3 direction = (pos - _player.transform.position).normalized;
        while(Vector3.Distance(playerPos, pos) > 0.01f)
        {
            playerPos += 3 * Time.deltaTime * direction; // TODO: Change to easing
            _player.transform.position = playerPos;
            yield return null;
            direction = (pos - playerPos).normalized;
        }

        _playerActionFinished = true;

        _player.transform.position = pos;
        var coords = _map.CoordsFromWorld(pos);
        var scroll = GetScrollAt(coords);

        if(scroll != null)
        {
            _scrolls.Remove(scroll);
            Destroy(scroll.gameObject);
            _blockPrefab = scroll.BlockData;
            CreateBlock(coords);
            _player.SetTint(new Color(1, 1, 1, 0.4f));
            _gameState = GameState.Placement;
        }        
    }

    void CreateBlock(Vector3Int coords)
    {
        var pos = _map.WorldFromCoords(coords, centered: false);
        _tempBlockInstance = Instantiate(_blockPrefab);
        _tempBlockInstance.RefreshBounds();
        var xfm = _tempBlockInstance.transform;
        xfm.position = pos;

        bool validPos = _map.CanSetBlock(_tempBlockInstance, coords);
        _tempBlockInstance.SetTint(validPos ? _validBlockTint : _invalidBlockTint);
    }

    bool NoPossiblePaths()
    {
        var playerCoords = _map.CoordsFromWorld(_player.transform.position);
        bool pathToGoal = _map.ExistsPlayerWalkablePath(playerCoords, _map.CoordsFromWorld(_levelExit.position));
        if(pathToGoal)
        {
            return false;
        }

        foreach(var scroll in _scrolls)
        {
            if(_map.ExistsPlayerWalkablePath(playerCoords, _map.CoordsFromWorld(scroll.transform.position)))
            {
                return false;
            }
        }
        return true;
    }
}
