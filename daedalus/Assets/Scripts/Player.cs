using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    // Start is called before the first frame update
    public void SetTint(Color color)
    {
        _renderer.color = color;
    }
}
