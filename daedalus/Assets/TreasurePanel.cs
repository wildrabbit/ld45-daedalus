using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasurePanel : MonoBehaviour
{
    [SerializeField] GameObject _panelPrefab;
    [SerializeField] RectTransform _root;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AddPickablePanel(Sprite icon)
    {
        var instance = Instantiate(_panelPrefab, _root);
        var iconImage = instance.transform.Find("Icon").GetComponent<Image>();
        if(iconImage != null)
        {
            iconImage.sprite = icon;
        }
    }
}
