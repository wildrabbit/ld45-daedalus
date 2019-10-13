using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionDisplay : MonoBehaviour
{
    TMPro.TextMeshProUGUI _label;
    // Start is called before the first frame update
    void Start()
    {
        _label = GetComponent<TMPro.TextMeshProUGUI>();
        if(_label != null)
        {
            _label.text = $"v{Application.version}";
        }
    }
}
