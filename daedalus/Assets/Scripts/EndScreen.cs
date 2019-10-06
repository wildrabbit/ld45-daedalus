using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreen: ShowConfirm
{
    [SerializeField] GameObject _winObj;

    [SerializeField] GameObject _loseObj;

    public void  Setup(GameResult result)
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
    }
    
}

