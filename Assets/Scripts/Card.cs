using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    Vector2Int _cardSlotIndex;

    public delegate void CardAppear(Vector2Int slotIndex);
    public CardAppear CardAppearCallback;

    Button _cardButton;
    Animator _cardAnimator;

    void Awake()
    {
        _cardButton = GetComponent<Button>();
        _cardAnimator = GetComponent<Animator>();
    }

    public void SetCardSlot(Vector2Int slotIndex)
    {
        _cardSlotIndex = slotIndex;

    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if ((!GameMgr.Instance.GetAIMode()) ||
            GameMgr.Instance.GetTurn() == 0 && GameMgr.Instance.GetPlayer1First() ||
            GameMgr.Instance.GetTurn() == 1 && !GameMgr.Instance.GetPlayer1First())
            {
                SetCard();
            }
        });
    }

    public void SetCard()
    {
        _cardButton.interactable = false;

        if (BoardMgr.Instance.GetHideBoard())
        {
            Hide(true);
        }
        else
        {
            if (GameMgr.Instance.GetPlayer1TypeAsO() && GameMgr.Instance.GetIsPlayer1Turn() ||
            ((!GameMgr.Instance.GetPlayer1TypeAsO()) && !GameMgr.Instance.GetIsPlayer1Turn()))
            {
                OAppear(true);
            }
            else
            {
                XAppear(true);
            }
        }

        CardAppearCallback?.Invoke(_cardSlotIndex);
    }

    public void OAppear(bool playSound = false)
    {
        if (playSound)
            SoundMgr.Instance.PlaySoundEffect("OAppear");
        _cardAnimator.SetTrigger("O");
    }

    public void XAppear(bool playSound = false)
    {
        if (playSound)
            SoundMgr.Instance.PlaySoundEffect("XAppear");
        _cardAnimator.SetTrigger("X");
    }

    public void Hide(bool playSound = false)
    {
        if (playSound)
            SoundMgr.Instance.PlaySoundEffect("Hide");
        _cardAnimator.SetTrigger("Hide");
    }

    public void Lock(bool playSound = false)
    {
        if (playSound)
            SoundMgr.Instance.PlaySoundEffect("Hide");
        _cardAnimator.SetTrigger("Lock");
        _cardButton.interactable = false;
    }

    public void ResetCard()
    {
        _cardButton.interactable = true;
        _cardAnimator.Rebind();
    }

    public void SetInteractable(bool interactable)
    {
        _cardButton.interactable = interactable;
    }

}
