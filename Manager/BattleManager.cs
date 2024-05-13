using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Inst;

    private void Awake() => Inst = this;

    public TurnManager turnManager;
    public CardManager cardManager;
    public EnemyManager enemyManager;
    public RewardManager battleReward;

    public bool battleStop = false;
    public bool battleEnd = false;
    public bool eliteBattle = false;

    public Animator startBattleanimator;

    public IEnumerator InitBattleObjects()
    {
        yield return StartCoroutine(cardManager.InitCardManager());

        this.gameObject.SetActive(false);
    }

    public void StartBattle(bool isEliteBattle)
    {
        StartCoroutine(StartBattleCo(isEliteBattle));
    }

    IEnumerator StartBattleCo(bool isEliteBattle)
    {
        eliteBattle = isEliteBattle;
        if (!isEliteBattle)
            enemyManager.SpawnEnemy();
        else
            enemyManager.SpawnElite();

        yield return StartCoroutine(PlayerManager.inst.StartBattleCo());
        yield return StartCoroutine(turnManager.StartGameCo());
    }

    public IEnumerator EndBattle()
    {
        yield return StartCoroutine(GameManager.inst.FadeInCo());
        cardManager.ResetAllCard();
        battleReward.DeleteReward();
        RewardManager.inst.rewardObject.SetActive(false);
        PlayerManager.inst.EndBattleFunc();
        battleStop = false;
        battleEnd = false;

        yield return StartCoroutine(GameManager.inst.FadeOutCo(ObjectType.Map));
    }

}