using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager inst;

    public Button nextTurnBtn;

    [Header("---- Develop ---")]
    [SerializeField] [Tooltip("카드 배분이 빨라집니다.")] bool fastMode;
    [SerializeField] [Tooltip("시작 카드개수를 정합니다")] int startCardCount;

    [Header("---- Properties ----")]
    public bool isLoading = false;
    public bool isMyTurn = false;

    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    private void Awake() => inst = this;

    private void Start() => StartFunc();

    private void StartFunc()
    {
        nextTurnBtn.onClick.AddListener(() =>
        {
            if(!isLoading)
                StartCoroutine(NextTurnCo());
        });
    }

    public IEnumerator StartGameCo()
    {
        isLoading = true;
        BattleManager.Inst.startBattleanimator.SetTrigger("StartAnim");
        SoundManager.instance.PlayEffSound(GlobalData.effsounds[(int)EffSound.BattleStart]);
        yield return new WaitForSeconds(1.5f);

        if (fastMode)
            CardManager.Inst.drawDelay = new WaitForSeconds(0.05f);

        yield return StartCoroutine(CardManager.Inst.AddCardCo(startCardCount));

        isLoading = false;
    }

    public IEnumerator NextTurnCo()
    {
        isLoading = true;

        CardManager.Inst.RemoveAllCard();

        yield return delay07;
        PlayerManager.inst.BuffEffect();

        yield return delay07;
        //상대의 행동
        yield return StartCoroutine(EnemyManager.inst.EnemyActing());

        if (BattleManager.Inst.battleEnd)
            yield break;

        yield return StartCoroutine(CardManager.Inst.AddCardCo(startCardCount));
        PlayerManager.inst.EndTurnFunc();

        isLoading = false;
    }
}