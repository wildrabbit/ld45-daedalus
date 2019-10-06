using UnityEngine;

public class Loader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(PlayState.Instance != null)
        {
            PlayState.Instance.PlayFirst();
        }
    }
}
