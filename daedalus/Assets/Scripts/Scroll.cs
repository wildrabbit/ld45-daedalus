using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Scroll : Entity
{
    [FormerlySerializedAs("_viewRoot")] [SerializeField] public Transform ViewRoot;
    [FormerlySerializedAs("_blockData")][SerializeField] public PlaceableBlock BlockData;
}
