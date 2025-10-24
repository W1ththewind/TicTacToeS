using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameMgr : Singleton<GameMgr>
{

    public delegate void ChangeLanguageCallback();
    public ChangeLanguageCallback changeLanguageCallback;

    int _turn = -1, _p1Score, _tScore, _p2Score;
    bool _player1AsO = false, _player1First = true, _aiMode = true, _settingChanged = false, _skillMode = true;
    [SerializeField]
    TMP_Text _player2AILabel, _player1Type, _player2Type, _player1Score, _tieScore, _player2Score;

    protected override void Awake()
    {
        base.Awake();
        if (Destroyed)
            return;

    }

    public int GetTurn()
    {
        return _turn;
    }

    public void SetTurn(int turn)
    {
        _turn = turn;
    }

    public bool GetAIMode()
    {
        return _aiMode;
    }

    public void SetAIMode(bool aIMode)
    {
        _aiMode = aIMode;
        _settingChanged = true;
    }

    public bool GetSkillMode()
    {
        return _skillMode;
    }

    public void SetSkillMode(bool skillMode)
    {
        _skillMode = skillMode;
        _settingChanged = true;
    }


    public bool GetPlayer1TypeAsO()
    {
        return _player1AsO;
    }

    public void SetPlayerType(bool player1AsO)
    {
        _player1AsO = player1AsO;
        _settingChanged = true;
    }


    public bool GetPlayer1First()
    {
        return _player1First;
    }

    public void SetPlayerFirst(bool player1First)
    {
        _player1First = player1First;
        _settingChanged = true;
    }

    public bool GetIsPlayer1Turn()
    {
        return (GameMgr.Instance.GetTurn() == 0 && GameMgr.Instance.GetPlayer1First())
                || (GameMgr.Instance.GetTurn() == 1 && !GameMgr.Instance.GetPlayer1First());
    }

    public void CheckSettingChanged()
    {
        if (_settingChanged)
        {
            _settingChanged = false;
            CardMgr.Instance.Hide();

            StartGame();
        }
    }

    public void ChangeLanguage(string languageCode)
    {
        SettingMgr.Instance.LanguageCode = languageCode;
        changeLanguageCallback?.Invoke();

    }

    public void StartGame()
    {
        _player1Type.text = _player1AsO ? "O" : "X";
        _player2Type.text = _player1AsO ? "X" : "O";

        _player2AILabel.text = _aiMode ? "AI" : "Player2";


        SetTurn(0);
        BoardMgr.Instance.Init();
        BoardMgr.Instance.Show();
    }

    public void AddP1Score()
    {
        _p1Score++;
        _player1Score.text = _p1Score.ToString();
    }

    public void AddTScore()
    {
        _tScore++;
        _tieScore.text = _tScore.ToString();
    }

    public void AddP2Score()
    {
        _p2Score++;
        _player2Score.text = _p2Score.ToString();
    }

    public void ClearScore()
    {
        _p1Score = _tScore = _p2Score = 0;
        _player1Score.text = _p1Score.ToString();
        _tieScore.text = _tScore.ToString();
        _player2Score.text = _p2Score.ToString();
    }
}
