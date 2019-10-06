using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Scroll : Entity
{
    [SerializeField] public Transform ViewRoot;
    [SerializeField] public PlaceableBlock BlockData;
}
