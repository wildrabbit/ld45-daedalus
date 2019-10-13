using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndScreen: ShowConfirm
{
    [SerializeField] Image _panelBG;
    [SerializeField] GameObject _winObj;
    [SerializeField] GameObject _loseObj;
    [SerializeField] float hideDelay = 2.0f;

    public void  Setup(GameResult result, bool hidePanel)
    {
        _winObj.SetActive(result == GameResult.Won);
        _loseObj.SetActive(result == GameResult.Lost);

        if(result == GameResult.Won)
        {
            if (PlayState.Instance != null && PlayState.Instance.IsLastLevel)
            {
                _confirmLabel.text = "Thanks for playing! Press Confirm to restart the game!";
            }
            else
            {
                _confirmLabel.text = "Press Confirm to play the next level!";
            }
        }
        else if (result == GameResult.Lost)
        {
            _confirmLabel.text = "Press Confirm to restart the current level!";
        }

        if(hidePanel)
        {
            StartCoroutine(AutoHideScreen());
        }
    }

    IEnumerator AutoHideScreen()
    {
        yield return new WaitForSeconds(hideDelay);
        var color = _panelBG.color;
        color.a = 0;
        _panelBG.color = color;
        _winObj.gameObject.SetActive(false);
        _loseObj.gameObject.SetActive(false);
    }
    
}

