using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayState : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void CheckPlayState()
    {
        if (Instance == null)
        {
            var sceneInstance = FindObjectOfType<PlayState>();
            if (sceneInstance == null)
            {
                var obj = new GameObject("PlayState");
                var state = obj.AddComponent<PlayState>();
            }
        }
    }

    public static PlayState Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
    }

    [SerializeField] LevelList _levels;
    [SerializeField] int _startLevel;

    public int CurrentLevelIdx;
    public string CurrentScene => _levels == null ? SceneManager.GetActiveScene().name :_levels.Scenes[CurrentLevelIdx];

    public bool IsLastLevel => _levels == null ? true : (CurrentLevelIdx == _levels.Scenes.Length - 1);

    public void PlayFirst()
    {
        CurrentLevelIdx = _startLevel;
        LoadCurrentLevel();
    }

    public void NextScene()
    {
        if (_levels == null || _levels.Scenes.Length == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (CurrentLevelIdx < _levels.Scenes.Length - 1)
        {
            CurrentLevelIdx++;
            SceneManager.LoadScene(CurrentScene);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void LoadCurrentLevel()
    {
        if(_levels == null || _levels.Scenes.Length == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        SceneManager.LoadScene(CurrentScene);
    }
}
