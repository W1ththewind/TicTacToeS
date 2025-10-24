using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardMgr : Singleton<BoardMgr>
{
    int[,] _board;
    bool[,] _lockedSlots;
    bool _isBoardHidden = false, _finished = false;

    TransparentOnDisableGroup boardGroup
    {
        get
        {
            if (_transparentOnDisableGroup == null)
                _transparentOnDisableGroup = GetComponent<TransparentOnDisableGroup>();
            return _transparentOnDisableGroup;
        }
    }
    TransparentOnDisableGroup _transparentOnDisableGroup;

    [SerializeField]
    RectTransform SlotContent;

    public void Init()
    {
        _finished = false;
        _board = new int[3, 3];
        _lockedSlots = new bool[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _board[i, j] = -1;
                Card card = SlotContent.GetChild(i).GetChild(j).GetComponent<Card>();
                card.SetCardSlot(new Vector2Int(i, j));
                card.CardAppearCallback -= SetBoardSlot;
                card.CardAppearCallback += SetBoardSlot;
            }
        }

        UpdateAllCardsView();
    }

    public void Show()
    {
        boardGroup.interactable = boardGroup.blocksRaycasts = true;

        if (GameMgr.Instance.GetPlayer1First() || !GameMgr.Instance.GetAIMode())
        {
            if (GameMgr.Instance.GetSkillMode())
                CardMgr.Instance.CardChooseStart();
        }

        if (!GameMgr.Instance.GetPlayer1First() && GameMgr.Instance.GetAIMode())
        {
            StartCoroutine(DelayedAICoroutine());
        }
    }

    public void Hide()
    {
        boardGroup.interactable = boardGroup.blocksRaycasts = false;
    }

    public void SetBoardSlot(Vector2Int slotIndex)
    {
        int _turn = GameMgr.Instance.GetTurn();
        _board[slotIndex.x, slotIndex.y] = _turn;

        //S Card Effect
        CardMgr.Instance.CardEffect(slotIndex);

        GameMgr.Instance.SetTurn(_turn == 0 ? 1 : 0);

        int winTurn = CheckWinner();
        if (winTurn == 0)
        {
            if (GameMgr.Instance.GetPlayer1First())
                Player1Win();
            else
                OppositeWin();
        }
        else if (winTurn == 1)
        {
            if (GameMgr.Instance.GetPlayer1First())
                OppositeWin();
            else
                Player1Win();
        }
        else if (winTurn == -2)
        {
            Tie();
        }

        if (winTurn == -1 && ((_turn == 0 && GameMgr.Instance.GetPlayer1First()) || (_turn == 1 && !GameMgr.Instance.GetPlayer1First()) && GameMgr.Instance.GetAIMode()))
        {
            //AI WORK
            StartCoroutine(DelayedAICoroutine());
        }

        if (winTurn == -1 && ((_turn == 1 && GameMgr.Instance.GetPlayer1First()) || (_turn == 0 && !GameMgr.Instance.GetPlayer1First()) || !GameMgr.Instance.GetAIMode()))
        {
            if (GameMgr.Instance.GetSkillMode())
                CardMgr.Instance.CardChooseStart();
        }
    }

    IEnumerator DelayedAICoroutine()
    {
        if (GameMgr.Instance.GetSkillMode())
            CardMgr.Instance.AIChoose();
        Vector2Int bestMove = GetBestMove();
        yield return new WaitForSeconds(0.25f);
        Card card = SlotContent.GetChild(bestMove.x).GetChild(bestMove.y).GetComponent<Card>();
        card.SetCard();
    }

    public void ApplyCardEffect(CardMgr.CardType cardType, Vector2Int slotIndex)
    {
        switch (cardType)
        {
            case CardMgr.CardType.RandOne:
                RandOneEffect(slotIndex);
                break;
            case CardMgr.CardType.ReCenter:
                ReCenterEffect(slotIndex);
                break;
            case CardMgr.CardType.HideBoard:
                HideBoardEffect();
                break;
            case CardMgr.CardType.LockOne:
                LockOneEffect(slotIndex);
                break;
            case CardMgr.CardType.RandAll:
                RandAllEffect(slotIndex);
                break;
            default:
                break;
        }
    }

    public bool GetHideBoard()
    {
        return _isBoardHidden;
    }

    void RandOneEffect(Vector2Int slotIndex)
    {
        var adjacentOpponents = GetAdjacentSlots(slotIndex)
            .Where(pos => _board[pos.x, pos.y] >= 0 &&
                         _board[pos.x, pos.y] != GameMgr.Instance.GetTurn())
            .ToList();

        if (adjacentOpponents.Count > 0)
        {
            var targetPos = adjacentOpponents[Random.Range(0, adjacentOpponents.Count)];
            _board[targetPos.x, targetPos.y] = GameMgr.Instance.GetTurn();
            UpdateCardView(targetPos);
        }
    }

    void ReCenterEffect(Vector2Int slotIndex)
    {
        Vector2Int offset = new Vector2Int(1 - slotIndex.x, 1 - slotIndex.y);

        int[,] newBoard = new int[3, 3];
        bool[,] newLockedSlots = new bool[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                newBoard[i, j] = -1;
                newLockedSlots[i, j] = false;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int newX = i + offset.x;
                int newY = j + offset.y;

                // 只保留在棋盘边界内的棋子
                if (newX >= 0 && newX < 3 && newY >= 0 && newY < 3)
                {
                    newBoard[newX, newY] = _board[i, j];
                    newLockedSlots[newX, newY] = _lockedSlots[i, j];
                }
                // 超出边界的棋子直接消失（不复制到新棋盘）
            }
        }

        _board = newBoard;
        _lockedSlots = newLockedSlots;

        UpdateAllCardsView();
    }

    void HideBoardEffect()
    {
        _isBoardHidden = true;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (_board[i, j] >= 0)
                {
                    HideCardView(new Vector2Int(i, j));
                }
            }
        }
    }

    void LockOneEffect(Vector2Int slotIndex)
    {
        var adjacentEmpties = GetAdjacentSlots(slotIndex)
            .Where(pos => _board[pos.x, pos.y] == -1 && !_lockedSlots[pos.x, pos.y])
            .ToList();

        if (adjacentEmpties.Count > 0)
        {
            var targetPos = adjacentEmpties[Random.Range(0, adjacentEmpties.Count)];
            _lockedSlots[targetPos.x, targetPos.y] = true;
            LockCardView(targetPos);
        }
    }

    void RandAllEffect(Vector2Int slotIndex)
    {
        var surroundingSlots = GetAdjacentSlots(slotIndex);
        foreach (var pos in surroundingSlots)
        {
            if (_board[pos.x, pos.y] >= 0 && Random.Range(0f, 1f) > 0.5f)
            {
                _board[pos.x, pos.y] = 1 - _board[pos.x, pos.y];
                UpdateCardView(pos);
            }
        }
    }

    List<Vector2Int> GetAdjacentSlots(Vector2Int center)
    {
        List<Vector2Int> surrounding = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == j && i == 0)
                {
                    continue;
                }
                Vector2Int newPos = center + new Vector2Int(i, j);
                if (newPos.x >= 0 && newPos.x < 3 && newPos.y >= 0 && newPos.y < 3)
                {
                    surrounding.Add(newPos);
                }
            }
        }
        return surrounding;
    }

    void UpdateCardView(Vector2Int pos)
    {
        Card card = SlotContent.GetChild(pos.x).GetChild(pos.y).GetComponent<Card>();
        if (_board[pos.x, pos.y] == 0)
        {
            if (_isBoardHidden)
            {
                card.Hide();
            }
            else
            {
                if (isFirstTurnOCard())
                {
                    card.OAppear();
                }
                else
                {
                    card.XAppear();
                }
            }

        }
        else if (_board[pos.x, pos.y] == 1)
        {
            if (_isBoardHidden)
            {
                card.Hide();
            }
            else
            {
                if (isFirstTurnOCard())
                {
                    card.XAppear();
                }
                else
                {
                    card.OAppear();
                }
            }

        }
    }

    void UpdateAllCardsView()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Card card = SlotContent.GetChild(i).GetChild(j).GetComponent<Card>();

                card.ResetCard();

                if (_board[i, j] == 0)
                {
                    if (_isBoardHidden)
                    {
                        card.Hide();
                    }
                    else
                    {
                        if (isFirstTurnOCard())
                        {
                            card.OAppear();
                        }
                        else
                        {
                            card.XAppear();
                        }
                    }

                    card.SetInteractable(false);
                }
                else if (_board[i, j] == 1)
                {
                    if (_isBoardHidden)
                    {
                        card.Hide();
                    }
                    else
                    {
                        if (isFirstTurnOCard())
                        {
                            card.XAppear();
                        }
                        else
                        {
                            card.OAppear();
                        }
                    }

                    card.SetInteractable(false);
                }
                else if (_lockedSlots[i, j])
                {
                    card.Lock();
                    card.SetInteractable(false);
                }
                else if (_finished)
                {
                    card.SetInteractable(false);
                }

            }
        }
    }

    bool isFirstTurnOCard()
    {
        return GameMgr.Instance.GetPlayer1TypeAsO() && GameMgr.Instance.GetPlayer1First();
    }

    void HideCardView(Vector2Int pos)
    {
        Card card = SlotContent.GetChild(pos.x).GetChild(pos.y).GetComponent<Card>();
        card.Hide();
    }

    void LockCardView(Vector2Int pos)
    {
        Card card = SlotContent.GetChild(pos.x).GetChild(pos.y).GetComponent<Card>();
        card.Lock(true);
    }

    public int CheckWinner()
    {
        bool fullBoard = true;
        for (int i = 0; i < 3; i++)
        {
            //row
            if (_board[i, 0] == _board[i, 1] && _board[i, 0] == _board[i, 2] && _board[i, 0] >= 0)
            {
                return _board[i, 0];
            }

            //col
            if (_board[0, i] == _board[1, i] && _board[0, i] == _board[2, i] && _board[0, i] >= 0)
            {
                return _board[0, i];
            }


            for (int j = 0; j < 3; j++)
            {
                if (_board[i, j] == -1 && !_lockedSlots[i, j])
                {
                    fullBoard = false;
                }
            }
        }

        if (_board[0, 0] == _board[1, 1] && _board[0, 0] == _board[2, 2] && _board[0, 0] >= 0)
        {
            return _board[0, 0];
        }

        if (_board[0, 2] == _board[1, 1] && _board[0, 2] == _board[2, 0] && _board[0, 2] >= 0)
        {
            return _board[0, 2];
        }

        if (fullBoard)
        {
            return -2;
        }

        return -1;
    }

    void Player1Win()
    {
        SoundMgr.Instance.PlaySoundEffect("GameOver");
        GameMgr.Instance.AddP1Score();
        Debug.Log("1Win!");
        _isBoardHidden = false;
        _finished = true;
        UpdateAllCardsView();
    }

    void OppositeWin()
    {
        SoundMgr.Instance.PlaySoundEffect("GameOver");
        GameMgr.Instance.AddP2Score();
        Debug.Log("2Win!");
        _isBoardHidden = false;
        _finished = true;
        UpdateAllCardsView();
    }

    void Tie()
    {
        SoundMgr.Instance.PlaySoundEffect("Tie");
        GameMgr.Instance.AddTScore();
        Debug.Log("Tie!");
        _isBoardHidden = false;
        _finished = true;
        UpdateAllCardsView();
    }

    public Vector2Int GetBestMove()
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = new(-1, -1);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (_board[i, j] == -1)
                {
                    _board[i, j] = GameMgr.Instance.GetTurn();
                    int score = Minimax(0, false);
                    _board[i, j] = -1;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Vector2Int(i, j);
                    }
                }
            }
        }

        return bestMove;
    }

    int Minimax(int depth, bool isMaximizing)
    {
        int winTurn = CheckWinner();
        if (winTurn == 0)
        {
            if (GameMgr.Instance.GetPlayer1First())
                return depth - 10;
            else
                return 10 - depth;
        }
        else if (winTurn == 1)
        {
            if (GameMgr.Instance.GetPlayer1First())
                return 10 - depth;
            else
                return depth - 10;
        }
        else if (winTurn == -2)
        {
            return 0;
        }


        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_board[i, j] == -1)
                    {
                        _board[i, j] = GameMgr.Instance.GetPlayer1First() ? 1 : 0;
                        int score = Minimax(depth + 1, false);
                        _board[i, j] = -1;
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_board[i, j] == -1)
                    {
                        _board[i, j] = GameMgr.Instance.GetPlayer1First() ? 0 : 1;
                        int score = Minimax(depth + 1, true);
                        _board[i, j] = -1;
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }

            return bestScore;
        }
    }
}

