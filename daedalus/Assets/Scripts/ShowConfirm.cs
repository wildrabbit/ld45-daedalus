using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowConfirm : MonoBehaviour
{
    [SerializeField] protected TMPro.TextMeshProUGUI _confirmLabel;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetConfirm(bool enabled)
    {
        _confirmLabel.gameObject.SetActive(enabled);
    }
}
