using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardMgr : Singleton<CardMgr>
{
    TransparentOnDisableGroup cardGroup
    {
        get
        {
            if (_transparentOnDisableGroup == null)
                _transparentOnDisableGroup = GetComponent<TransparentOnDisableGroup>();
            return _transparentOnDisableGroup;
        }
    }
    TransparentOnDisableGroup _transparentOnDisableGroup;
    public enum CardType
    {
        None = 0,
        //随机将一个相邻对手棋子换为同种棋子
        RandOne = 1,
        //将棋盘以落点为中心移动
        ReCenter = 2,
        //本局内隐藏棋盘上所有棋子种类
        HideBoard = 3,
        //随机将一个相邻的空棋盘位锁定
        LockOne = 4,
        //将落点周围所有棋子以50%概率替换为相反种类的棋子
        RandAll = 5,
    }

    [SerializeField]
    Button _cardButton;

    CardType _currentCardType;

    [SerializeField]
    Image cardIcon;

    [SerializeField]
    TMP_Text cardTitle, cardDesc;

    [SerializeField]
    Sprite OIcon, XIcon;

    List<string> _cardTitle = new() {
        "偷天换日",
        "斗转星移",
        "飞沙走石",
        "画地为牢",
        "翻天覆地" };
    List<string> _cardDesc = new() {
        "随机将一个相邻对手棋子换为同种棋子",
        "将棋盘以落点为中心移动",
        "本局内隐藏棋盘上所有棋子种类",
        "随机将一个相邻的空棋盘位锁定",
        "将落点周围所有棋子以50%概率替换为相反种类的棋子" };

    List<string> _cardTitleEN = new() {
        "Swap Trick",
        "Stars Shift",
        "Sandstorm",
        "Imprisonment",
        "Cataclysmic Change" };
    List<string> _cardDescEN = new() {
        "Randomly change an adjacent opponent's piece to the same type",
        "Move the board centered on the landing point",
        "Hide all piece types on the board for this game",
        "Randomly lock an adjacent empty board position",
        "Replace all pieces around the landing point with the opposite type with a 50% probability" };

    void Start()
    {
        if (!Destroyed)
        {
            _cardButton.onClick.AddListener(CardChoosen);
        }
    }

    void CardChoosen()
    {
        cardGroup.interactable = cardGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        _currentCardType = CardType.None;
        cardGroup.interactable = cardGroup.blocksRaycasts = false;
    }


    public void CardChooseStart()
    {
        cardGroup.interactable = cardGroup.blocksRaycasts = true;
        _currentCardType = (CardType)Random.Range(1, 6);

        bool oIcon = false;
        if (GameMgr.Instance.GetPlayer1TypeAsO() && GameMgr.Instance.GetIsPlayer1Turn() ||
        ((!GameMgr.Instance.GetPlayer1TypeAsO()) && !GameMgr.Instance.GetIsPlayer1Turn()))
        {
            oIcon = true;
        }

        cardIcon.sprite = oIcon ? OIcon : XIcon;
        cardTitle.text = SettingMgr.Instance.LanguageCode == "en" ? _cardTitleEN[(int)_currentCardType - 1] : _cardTitle[(int)_currentCardType - 1];
        cardDesc.text = SettingMgr.Instance.LanguageCode == "en" ? _cardDescEN[(int)_currentCardType - 1] : _cardDesc[(int)_currentCardType - 1];

    }

    public void AIChoose()
    {
        _currentCardType = (CardType)Random.Range(0, 6);
    }

    public CardType GetCurrentCardType()
    {
        return _currentCardType;
    }



    public void CardEffect(Vector2Int slotIndex)
    {
        BoardMgr.Instance.ApplyCardEffect(_currentCardType, slotIndex);
    }

}
