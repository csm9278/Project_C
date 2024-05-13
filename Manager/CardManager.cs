using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst;

    private void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;

    public DeckList deckList;
    public Button deckListBtn;
    public Text deckListText;
    public Button useDeckListBtn;
    public Text useDeckListText;

    public List<Item> allItemBuffer;
    public List<Item> itemBuffer;
    List<Item> useItemBuffer;
    [SerializeField] List<Card> myCards;

    int maxCardCount = 10;

    //Card Prefab
    public GameObject cardPrefab;
    public GameObject cardUIPrefab;

    //card Transform
    public Transform cardSpawnPoint;
    public Transform myCardPointLeft;
    public Transform myCardPointRight;

    bool widePos = false;
    bool isCardArea = false;
    bool cardClick = false;
    bool cardAreaOut = false;
    Card selectCard = null;
    RaycastHit2D hit;
    LayerMask areaLayer;

    public WaitForSeconds drawDelay = new WaitForSeconds(0.2f);

    private void Update() => UpdateFunc();

    private void UpdateFunc()
    {
        if (GameManager.inst.pause)
            return;

        if (BattleManager.Inst.turnManager.isLoading)
            return;

        DetectCardArea();
        CardDrag();
    }

    public void AddCard(int addCardCount = 1)
    {
        StartCoroutine(AddCardCo(addCardCount));
    }

    //�ʱ� ī�� ����
    public IEnumerator InitCardManager()
    {
        itemBuffer = new List<Item>();
        useItemBuffer = new List<Item>();

        switch (PlayerManager.inst.playerType)
        {
            case PlayerType.Barbarian:
                for (int i = 0; i < itemSO.barbarianDefault.Count; i++)
                {
                    if(i == 0 || i == 1)    //�⺻ ���ݰ� ���� 5�徿
                        for(int j = 0; j < 5; j++)
                        {
                            Item item = new Item(itemSO.barbarianDefault[i]);
                            itemBuffer.Add(item);
                        }
                    else
                    {
                        Item item = new Item(itemSO.barbarianDefault[i]);
                        itemBuffer.Add(item);
                    }
                }
                break;
        }
        allItemBuffer = new List<Item>(itemBuffer);
        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }

        deckListBtn.onClick.AddListener(() =>
        {
            BattleManager.Inst.battleStop = true;
            deckList.gameObject.SetActive(true);
            deckList.SetUpDeckList(itemBuffer);
        });

        useDeckListBtn.onClick.AddListener(() =>
        {
            BattleManager.Inst.battleStop = true;
            deckList.gameObject.SetActive(true);
            deckList.SetUpDeckList(useItemBuffer);
        });

        areaLayer = 1 << LayerMask.NameToLayer("CardArea");

        yield break;
    }

    /// <summary>
    /// �� ����� ��� ī�� ����
    /// </summary>
    public void RemoveAllCard()
    {
        for(int i = 0; i < myCards.Count; i++)
        {
            useItemBuffer.Add(myCards[i].originItem);

            CardTrail(myCards[i].transform.position, useDeckListBtn.transform.position, 1);
            myCards[i].ObjectReturn();
        }
        myCards.Clear();
        useDeckListText.text = useItemBuffer.Count.ToString();

    }

    /// <summary>
    /// ���� ����� �ʱ�ȭ
    /// </summary>
    public void ResetAllCard()
    {
        for (int i = 0; i < myCards.Count; i++)
        {
            useItemBuffer.Add(myCards[i].originItem);
            myCards[i].ObjectReturn();
        }
        myCards.Clear();

        for (int i = 0; i < useItemBuffer.Count; i++)
        {
            itemBuffer.Add(useItemBuffer[i]);
        }
        useItemBuffer.Clear();

        deckListText.text = itemBuffer.Count.ToString();
        useDeckListText.text = useItemBuffer.Count.ToString();

    }

    public IEnumerator AddCardCo(int addCardNum = 1)
    {
        for(int i = 0; i < addCardNum; i++)
        {
            if (myCards.Count >= maxCardCount)
            {
                yield break;
            }

            if (itemBuffer.Count <= 0)
                yield return StartCoroutine(ReChargeItemBuffer());

            GameObject c = MemoryPoolManager.instance.GetObject("Card");
            c.transform.position = cardSpawnPoint.position;
            Card card = c.GetComponent<Card>();
            card.Setup(PopItem());
            myCards.Add(card);
            SetOriginOrder();
            SetCardTransform();
            SoundManager.instance.PlayEffSound(GlobalData.effsounds[(int)EffSound.DrawCard]);
            yield return drawDelay;
        }
    }

    IEnumerator ReChargeItemBuffer()
    {
        for (int i = 0; i < useItemBuffer.Count; i++)
        {
            itemBuffer.Add(useItemBuffer[i]);
        }
        useItemBuffer.Clear();

        //���⿡ ����
        CardTrail(useDeckListBtn.transform.position, deckListBtn.transform.position, itemBuffer.Count);
        SoundManager.instance.PlayEffSound(GlobalData.effsounds[(int)EffSound.UseCardShake]);
        //���⿡ ����
        yield return new WaitForSeconds(1.0f);
        deckListText.text = itemBuffer.Count.ToString();
        useDeckListText.text = useItemBuffer.Count.ToString();
    }

    Item PopItem()
    {

        Item front = itemBuffer[0];
        itemBuffer.RemoveAt(0);

        deckListText.text = itemBuffer.Count.ToString();

        return front;
    }

    void SetCardTransform(bool widePos = false, int ignoreIdx = -1)
    {
        List<PRS> prs = GetAlignment(myCards.Count, 0.5f, widePos);
        for(int i = 0; i < myCards.Count; i++)
        {
            if (ignoreIdx == i)
                continue;
            myCards[i].MoveTransForm(prs[i], true, 0.2f);
            myCards[i].originPRS = prs[i];
        }
    }

    //ī�� Order ����
    void SetOriginOrder()
    {
        int count = myCards.Count;
        for (int i = 0; i < count; i++)
            myCards[i].GetComponent<Order>().SetOriginOrder(i);
    }

    //ī�� ��ġ Ž��
    List<PRS> GetAlignment(int cardCount, float height, bool widePos = false, int selectIdx = -1)
    {
        List<PRS> list = new List<PRS>(cardCount);
        float[] objLerps = new float[cardCount];

        //�߾�ī�尪�� ���� ����
        Vector3 tempRight = myCardPointRight.position;
        Vector3 tempLeft = myCardPointLeft.position;

        if(widePos)
        {
            tempLeft.x = -6.5f;
            tempRight.x = 6.5f;
        }

        switch (cardCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.4f, 0.6f }; break;
            case 3: objLerps = new float[] { 0.3f, 0.5f, 0.7f }; break;
            default:
                float interval = 1.0f / (cardCount - 1);
                for (int i = 0; i < cardCount; i++)
                    objLerps[i] = interval * i;
                //�߾Ӻ��� ī�尡 ���̰�
                float xlerp = (float)cardCount / maxCardCount;
                tempRight.x *= xlerp;
                tempLeft.x *= xlerp;
                break;
        }


        for(int i = 0; i < cardCount; i++)
        {
            if (selectIdx == i)
                continue;
            var targetPos = Vector3.Lerp(tempLeft, tempRight, objLerps[i]);
            var targetRot = Utill.QI;
            if (cardCount >= 4) //4������ ȸ����
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(myCardPointLeft.rotation, myCardPointRight.rotation, objLerps[i]);
            }
            list.Add(new PRS(targetPos, targetRot, Vector3.one * 4.0f));
        }

        return list;
    }

    #region ���콺 ����
    public void CardMouseOver(Card card)
    {
        //����� ȯ������ �Ϸ��� �ϴ� ��������
        return;


        if (BattleManager.Inst.battleStop)
            return;

        if (cardClick)
            return;
        //if(!widePos)
        //{
        //    widePos = true;
        //    int ignoreIdx = -1;
        //    for(int i = 0; i < myCards.Count; i++)
        //    {
        //        if (card == myCards[i])
        //            ignoreIdx = i;
        //    }
        //    SetCardTransform(true, ignoreIdx);
        //}
        LargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        //����� ȯ������ �Ϸ��� �ϴ� ��������
        return;

        if (cardClick)
            return;

        if(widePos)
        {
            widePos = false;
            SetCardTransform(false);
        }
        LargeCard(false, card);
    }

    public void CardMouseDown(Card card)
    {
        if (BattleManager.Inst.battleStop || GameManager.inst.pause)
            return;

        if (BattleManager.Inst.turnManager.isLoading)
            return;

        cardClick = true;
        selectCard = card;
        LargeCard(true, card);
        card.isChangeInfo = true;
    }

    public void CardMouseUp()
    {
        if (BattleManager.Inst.battleStop || GameManager.inst.pause)
            return;

        if (BattleManager.Inst.turnManager.isLoading)
            return;

        cardClick = false;
        //ī�� ����
        if(selectCard != null)
            UseCard();
    }

    /// <summary>
    /// īƮ �ؽ�Ʈ ���� ��
    /// </summary>
    public void RefreshCardText()
    {
        for (int i = 0; i < myCards.Count; i++)
            myCards[i].CheckText();
    }

    void UseCard()
    {
        //�ڽ�Ʈ ������ ����
        if(!isCardArea)
        if (!PlayerManager.inst.isEnoughCost(selectCard.originItem.GetItemValue().cost))
        {
            GameManager.inst.ShowGameMessage(GameMessageType.NotEnoughCost);
            LargeCard(false, selectCard);
            selectCard = null;
            return;
        }

        switch (selectCard.originItem.type)
        {
            case ItemType.Attack:
                int enemylayer = 1 << LayerMask.NameToLayer("Enemy");
                RaycastHit2D hit = Physics2D.Raycast(Utill.MousePos, Vector3.forward, 10, enemylayer);
                if (hit)
                {
                    Enemy enemy = hit.collider.GetComponent<Enemy>();

                    //���� ������ �ִ��� üũ
                    if(EnemyManager.inst.CheckEnemyBuff(BuffType.Taunt))
                    {
                        // ������ ������ ���� �ϼ����� �ƴϸ�
                        if (!EnemyManager.inst.CheckEnemyBuff(BuffType.Taunt, enemy))
                        {
                            GameManager.inst.ShowGameMessage(GameMessageType.AttackTauntUnit);
                            break;
                        }
                    }

                    //ī�� ����
                    selectCard.CardEffect(PlayerManager.inst, enemy);
                    CardTrail(Input.mousePosition, useDeckListBtn.transform.position, 1);
                    PlayerManager.inst.useCost(selectCard.originItem.GetItemValue().cost);

                    RemoveSelectCard();
                }
                selectCard.MoveTransForm(selectCard.originPRS, false);
                break;

            case ItemType.Guard:
                if (!isCardArea)
                {
                    selectCard.CardEffect(PlayerManager.inst, null);

                    CardTrail(Input.mousePosition, useDeckListBtn.transform.position, 1);
                    PlayerManager.inst.useCost(selectCard.originItem.GetItemValue().cost);

                    RemoveSelectCard();

                    selectCard.MoveTransForm(selectCard.originPRS, false);
                }
                else
                    LargeCard(false, selectCard);
                break;

            default:
                if (!isCardArea)
                {
                    selectCard.CardEffect(PlayerManager.inst, null);

                    CardTrail(Input.mousePosition, useDeckListBtn.transform.position, 1);
                    PlayerManager.inst.useCost(selectCard.originItem.GetItemValue().cost);

                    RemoveSelectCard();
                    selectCard.MoveTransForm(selectCard.originPRS, false);
                    selectCard = null;
                }
                else
                {
                    LargeCard(false, selectCard);
                    selectCard = null;
                }
                break;
        }

        if(selectCard != null)
        {
            selectCard.isChangeInfo = false;
            LargeCard(false, selectCard);
            selectCard = null;
        }

        cardAreaOut = false;
        useDeckListText.text = useItemBuffer.Count.ToString();
    }
    #endregion

    void RemoveSelectCard()
    {
        if(selectCard != null)
        {
            myCards.Remove(selectCard);
            useItemBuffer.Add(selectCard.originItem);
            selectCard.ObjectReturn();
            SetCardTransform();
        }
    }

    void CardTrail(Vector3 now, Vector3 target, int particleNum)
    {
        GameObject p = MemoryPoolManager.instance.GetObject("ParticleTrail");
        if (p.TryGetComponent(out ParticleTrail t))
        {
            //t.SetTrail(Utill.screenPos(now), Utill.screenPos(target), particleNum);
            t.SetTrail(now.x > 100 ? Utill.screenPos(now) : now, target.x > 100 ? Utill.screenPos(target) : target, particleNum);
        }
    }

    private void DetectCardArea()
    {
        hit = Physics2D.Raycast(Utill.MousePos, Vector3.forward, 200, areaLayer);
        isCardArea = hit ? true : false;
    }

    public void CardDrag()
    {
        if (selectCard == null)
            return;

        if(cardClick)
        {
            if(!isCardArea && !cardAreaOut)
                cardAreaOut = true;

            if(cardAreaOut)
                selectCard.MoveTransForm(new PRS(Utill.MousePos, Utill.QI, selectCard.originPRS.scale), false);
        }
    }

    void LargeCard(bool large, Card card)
    {
        if (large)
        {
            Vector3 largePos = new Vector3(card.originPRS.pos.x, card.originPRS.pos.y + 3.0f, -10.0f);
            card.MoveTransForm(new PRS(largePos, Utill.QI, Vector3.one * 7.0f), true, 0.2f);
        }
        else
            card.MoveTransForm(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(large);
    }

    /// <summary>
    /// ī�庸��ޱ�
    /// </summary>
    public void RewardCard(Item rewardItem)
    {
        Item item = new Item(rewardItem);
        itemBuffer.Add(item);
        allItemBuffer.Add(item);
    }

    public void ShowPlayerDeck()
    {
        deckList.gameObject.SetActive(true);
        deckList.SetUpDeckList(allItemBuffer, CardUIType.Info);
    }

    public void ShowUpgrade()
    {
        deckList.gameObject.SetActive(true);
        deckList.SetUpDeckList(itemBuffer, CardUIType.Upgrade);
        deckList.isUpgrade = true;
    }

    public void UpgradeItem(Item upgradeItem)
    {
        upgradeItem.isUpgraded = true;
    }
}