using System.Collections;
using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] GameInput _gameInput;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AwaitStart());

    }

    IEnumerator AwaitStart()
    {
        while (!ConfirmInput())
        {
            yield return null;
        }
        if(PlayState.Instance != null)
        {
            PlayState.Instance.PlayFirst();
        }
    }

    bool ConfirmInput()
    {
        return _gameInput.Confirmed;
    }
}
