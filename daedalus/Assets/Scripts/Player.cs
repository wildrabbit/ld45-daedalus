using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    [SerializeField] SpriteRenderer _renderer;
    // Start is called before the first frame update
    public void SetTint(Color color)
    {
        _renderer.color = color;
    }
}
