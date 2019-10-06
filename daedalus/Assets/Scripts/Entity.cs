using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    [SerializeField] protected Sprite _hiddenSprite;
    protected Sprite _defaultSprite;

    protected SpriteRenderer _renderer;

    // Use this for initialization
    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _defaultSprite = _renderer.sprite;
    }

    // Update is called once per frame
    public void SetVisible(bool isVisible)
    {
        _renderer.sprite = isVisible ? _defaultSprite : _hiddenSprite;        
    }
}
