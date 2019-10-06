using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayState : MonoBehaviour
{
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
    public string CurrentScene => _levels.Scenes[CurrentLevelIdx];

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
