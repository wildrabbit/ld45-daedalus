using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WaitInfo,
    WaitPlayerInput,
    WaitPlayerAction,
    Placement,
    Simulating,
    Ended
}

public enum GameResult
{
    None,
    Running,
    Won,
    Lost
}

[Flags]
public enum GoalType
{
    None = 0,
    Escape = 1,
    Treasure = 2,
    Monsters = 4
}

public class GameController : MonoBehaviour
{
    [SerializeField] Map _map;
    [SerializeField] Player _player;
    [SerializeField] Transform _scrollRoot;
    [SerializeField] Transform _pickablesRoot;
    [SerializeField] Transform _playerStart;
    [SerializeField] Transform _levelExit;

    [SerializeField] Color _validBlockTint;
    [SerializeField] Color _invalidBlockTint;

    [SerializeField] GameInput _gameInput;
    [SerializeField] GoalType _levelObjective;

    [SerializeField] Canvas _infoCanvas;
    [SerializeField] float _infoDelay = 3f;

    [SerializeField] Canvas _levelEndCanvas;
    [SerializeField] float _levelEndDelay = 1f;

    [SerializeField] float _moveSpeed = 3f;

    [SerializeField] TreasurePanel _treasurePanel;

    readonly Vector3Int[] _offsets = new Vector3Int[] {
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0)
    };

    Vector3Int _playerStartCoords;
    Vector3Int _playerEndCoords;
    List<Scroll> _scrolls;
    List<Pickable> _pickables;

    bool _playerActionFinished;
    GameState _gameState;
    GameResult _gameResult = GameResult.None;
    int _turnCount;

    PlaceableBlock _blockPrefab;
    PlaceableBlock _tempBlockInstance;

    int _totalTreasures;
    int _acquiredTreasures;

    bool _infoReady = false;
    bool _endReady = false;

    void Start()
    {
        if(_infoCanvas != null)
        _infoCanvas.gameObject.SetActive(false);
        if (_levelEndCanvas != null)
        _levelEndCanvas.gameObject.SetActive(false);

        StartGame();    
    }

    void StartGame()
    {
        _turnCount = 0;
        _playerActionFinished = false;
        _gameState = _infoCanvas != null ? GameState.WaitInfo : GameState.WaitPlayerInput;

        _pickables = new List<Pickable>();
        _totalTreasures = 0;
        _acquiredTreasures = 0;
        if(_pickablesRoot != null)
        {
            _pickables.AddRange(_pickablesRoot.GetComponentsInChildren<Pickable>());
            _totalTreasures = _pickables.FindAll(x => x.ItemType == PickableType.Treasure).Count;
        }

        _scrolls = new List<Scroll>(_scrollRoot.GetComponentsInChildren<Scroll>());
        if(_map != null)
        {
            _map.InitTilesFromView();
            _playerStartCoords = _map.CoordsFromWorld(_playerStart.position);
            if(_levelExit != null)
            {
                _playerEndCoords = _map.CoordsFromWorld(_levelExit.position);
            }            
        }

        _player.transform.position = _map.WorldFromCoords(_playerStartCoords, centered:true);

        _gameResult = NoPossiblePaths() ? GameResult.Lost : GameResult.Running;
        RefreshVisibility();

        PurgeOverlappingScrolls();
        Debug.Log($"Level {PlayState.Instance.CurrentScene} starts now");

        if(_gameState == GameState.WaitInfo)
        {
            _infoReady = false;
            StartCoroutine(ShowInfo());
        }
    }
    
    IEnumerator ShowInfo()
    {
        _infoCanvas.gameObject.SetActive(true);
        _infoCanvas.GetComponent<ShowConfirm>().SetConfirm(false);
        yield return new WaitForSeconds(_infoDelay);
        _infoReady = true;
        _infoCanvas.GetComponent<ShowConfirm>().SetConfirm(true);
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
                Debug.Log($"Found overlapping scroll {scroll.name} at {coords}");
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
        switch (_gameState)
        {
            case GameState.WaitInfo:
            {
                if(_infoReady && _gameInput.Confirmed)
                {
                    _infoCanvas.gameObject.SetActive(false);
                    _gameState = GameState.WaitPlayerInput;
                }
                break;
            }
            case GameState.Ended:
            {
                if (_endReady && _gameInput.Confirmed)
                {
                    if (_gameResult == GameResult.Won)
                    {
                        PlayState.Instance.NextScene();
                    }
                    else if (_gameResult == GameResult.Lost)
                    {
                        PlayState.Instance.LoadCurrentLevel();
                    }
                    
                }
                break;
            }
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
                    _gameResult = EvaluateVictory();
                    if(_gameResult == GameResult.Running)
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

        if(_gameState != GameState.Ended && _gameResult != GameResult.Running)
        {
            _gameState = GameState.Ended;
            _endReady = false;

            StartCoroutine(OnLevelEnd());
            Debug.LogFormat("{0}", _gameResult == GameResult.Won ? "won!" : "lost");
        }
    }

    IEnumerator OnLevelEnd()
    {
        if (_levelEndCanvas != null)
        {
            _levelEndCanvas.gameObject.SetActive(true);
            EndScreen endScreen = _levelEndCanvas.GetComponent<EndScreen>();
            endScreen.SetConfirm(false);
            endScreen.Setup(_gameResult);
        }
        // TODO: Show victory / defeat / last level logic
        yield return new WaitForSeconds(_levelEndDelay);
        if(_levelEndCanvas != null)
         _levelEndCanvas.GetComponent<ShowConfirm>().SetConfirm(true);
    
        _endReady = true;
       
    }

    private GameResult EvaluateVictory()
    {
        bool evaluateEscape = (_levelObjective & GoalType.Escape) != GoalType.None;
        if (evaluateEscape && !_map.CoordsFromWorld(_player.transform.position).Equals(_playerEndCoords))
        {
            return _gameResult;
        }

        bool evaluateTreasure = (_levelObjective & GoalType.Treasure) != GoalType.None;
        if(evaluateTreasure && _acquiredTreasures < _totalTreasures)
        {
            return _gameResult;
        }

        return GameResult.Won;
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
        return GetEntitiesAt<Scroll>(targetCoords, _scrolls);
    }

    IEnumerator MoveTo(Vector3 pos)
    {
        Vector3 playerPos = _player.transform.position;
        Vector3 direction = (pos - _player.transform.position).normalized;
        while(Vector3.Distance(playerPos, pos) > 0.01f)
        {
            playerPos += _moveSpeed * Time.deltaTime * direction; // TODO: Change to easing
            _player.transform.position = playerPos;
            yield return null;
            direction = (pos - playerPos).normalized;
        }

        _playerActionFinished = true;

        _player.transform.position = pos;
        var coords = _map.CoordsFromWorld(pos);

        CheckScrolls(coords);
        CheckPickables(coords);
    }

    void CheckPickables(Vector3Int coords)
    {
        Pickable pickable = GetPickableAt(coords);
        if(pickable != null)
        {
            _pickables.Remove(pickable);
            Destroy(pickable.gameObject);
            //_hud.AddCollectedPickable(pickable.ItemType);
            if (pickable.ItemType == PickableType.Treasure)
            {
                _acquiredTreasures++;
                if(_treasurePanel != null)
                {
                    _treasurePanel.AddPickablePanel(pickable.UIIcon);
                }
            }
        }
    }

    private Pickable GetPickableAt(Vector3Int coords)
    {
        return GetEntitiesAt<Pickable>(coords, _pickables);
    }

    private T GetEntitiesAt<T>(Vector3Int coords, List<T> list) where T: Entity
    {
        foreach(var entity in list)
        {
            if(_map.CoordsFromWorld(entity.transform.position).Equals(coords))
            {
                return entity;
            }
        }
        return null;
    }

    void CheckScrolls(Vector3Int coords)
    {
        var scroll = GetScrollAt(coords);

        if (scroll != null)
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
        bool evaluateExit = (_levelObjective & GoalType.Escape) != GoalType.None;
        bool evaluateTreasure = (_levelObjective & GoalType.Treasure) != GoalType.None;
        bool scrollsAvailable = ExistsPlayerWalkablePahToEntity(playerCoords, _scrolls);

        bool goalReachable = _map.ExistsPlayerWalkablePath(playerCoords, _playerEndCoords);
        bool treasuresReachable = ExistsPlayerWalkablePahToEntity(playerCoords, _pickables);

        if(scrollsAvailable)
        {
            return false;
        }

        return (evaluateExit && !goalReachable) ||  (evaluateTreasure && _acquiredTreasures < _totalTreasures && !treasuresReachable);
    }

    bool ExistsPlayerWalkablePahToEntity<T>(Vector3Int playerCoords, List<T> entities) where T:Entity
    {
        return entities.Exists(entity => _map.ExistsPlayerWalkablePath(playerCoords, _map.CoordsFromWorld(entity.transform.position)));
    }
}
