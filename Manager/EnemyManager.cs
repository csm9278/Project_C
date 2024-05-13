using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int level = 0;
    
    public static EnemyManager inst;

    [SerializeField]
    List<TrVec> spawnPointList;

    public GameObject enemyPrefab;
    public GameObject enemyBuff;

    List<Enemy> enemyList = new List<Enemy>();


    public int rewardGold;

    private void Awake() => inst = this;

    public void SpawnEnemy()
    {
        switch (level)
        {
            case 0:
                Spawn(DataManager.inst.popSpawnData(level));
                break;
        }
    }

    public void SpawnElite()
    {
        switch (level)
        {
            case 0:
                Spawn(DataManager.inst.PopEliteData(level));
                break;
        }
    }

    /// <summary>
    /// Elite용
    /// </summary>
    /// <param name="spawn"></param>
    void Spawn(EnemyData spawn)
    {
        TrVec point = spawnPointList[0];

        GameObject enemyObj = Instantiate(enemyPrefab);
        enemyObj.transform.position = point.GetPos(0);
        if (enemyObj.TryGetComponent(out Enemy enemy))
        {
            enemy.SetUp(spawn);
            enemyList.Add(enemy);

            foreach(CBuffValue b in spawn.startBuff)
            {
                enemy.AddBuff(b, true);
            }
        }

        rewardGold = (level + 1) * Random.Range(50, 75);
    }

    void Spawn(EnemySpawn spawn)
    {
        int enemyNum = spawn.list.Count + spawn.list2.Count + spawn.list3.Count;

        TrVec point = spawnPointList[enemyNum-1];
        int pointIdx = 0;

        for (int j =0; j < spawn.list.Count; j++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab);
            enemyObj.transform.position = point.GetPos(pointIdx);
            if (enemyObj.TryGetComponent(out Enemy enemy))
            {
                EnemyData data = DataManager.inst.GetEnemyData(0, (int)spawn.list[j]) ;
                enemy.SetUp(data);
                enemy.GetComponentInChildren<Canvas>().sortingOrder = j;
                enemyList.Add(enemy);

                if (data.startBuff.Length > 0)
                    foreach (CBuffValue b in data.startBuff)
                    {
                        enemy.AddBuff(b, true);
                    }
            }
            pointIdx++;
        }

        for (int j = 0; j < spawn.list2.Count; j++)
        {

            GameObject enemyObj = Instantiate(enemyPrefab);
            enemyObj.transform.position = point.GetPos(pointIdx);
            if (enemyObj.TryGetComponent(out Enemy enemy))
            {
                EnemyData data = DataManager.inst.GetEnemyData(1, (int)spawn.list2[j]);
                enemy.SetUp(data);
                enemyList.Add(enemy);

                if(data.startBuff.Length > 0)
                foreach (CBuffValue b in data.startBuff)
                {
                    enemy.AddBuff(b, true);
                }
            }
            pointIdx++;
        }

        for (int j = 0; j < spawn.list3.Count; j++)
        {
            GameObject enemyObj = Instantiate(enemyPrefab);
            enemyObj.transform.position = point.GetPos(pointIdx);
            if (enemyObj.TryGetComponent(out Enemy enemy))
            {
                EnemyData data = DataManager.inst.GetEnemyData(2, (int)spawn.list3[j]);
                enemy.SetUp(data);
                enemyList.Add(enemy);

                if (data.startBuff.Length > 0)
                    foreach (CBuffValue b in data.startBuff)
                    {
                        enemy.AddBuff(b, true);
                    }
            }
            pointIdx++;
        }

        rewardGold = spawn.rewardGold;
    }

    public IEnumerator SummonMinionCo(List<EnemyData> summonData, Enemy summoner)
    {
        for(int i = 0; i < summonData.Count; i++)
        {
            TrVec spawnVec = spawnPointList[3];

            GameObject enemyObj = Instantiate(enemyPrefab);
            enemyObj.transform.position = spawnVec.GetPos(i);
            if(enemyObj.TryGetComponent(out Enemy enemy))
            {
                enemy.SetUp(summonData[i], false);
                SoundManager.instance.PlayEffSound(GlobalData.buffsounds[(int)EBuffSound.AtkUp]);
                summoner.AddMinion(enemy);
                enemyList.Add(enemy);
            }

            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator EnemyActing()
    {
        for(int i = 0; i < enemyList.Count; i++)
            enemyList[i].BuffEffect();

        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i].skillEff != null)
                yield return StartCoroutine(enemyList[i].Acting());
            else
                Debug.Log("스킬 없음");
        }

        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].SetActing();
        }
    }

    public void BuffAllEnemy(CBuffValue buffValue)
    {
        StartCoroutine(BuffAllEnemyCo(buffValue));
    }

    IEnumerator BuffAllEnemyCo(CBuffValue buffValue)
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].AddBuff(buffValue);
            SoundManager.instance.PlayEffSound(GlobalData.buffsounds[(int)buffValue.bType]);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void DieEnemy(Enemy dieEnemy)
    {
        enemyList.Remove(dieEnemy);

        if (enemyList.Count <= 0 && !BattleManager.Inst.battleEnd)
        {
            BattleManager.Inst.battleEnd = true;
            BattleManager.Inst.battleStop = true;
            RewardManager.inst.SetReward(BattleManager.Inst.eliteBattle ? ESetRewardType.EliteBattle : ESetRewardType.NormalBattle);
        }
    }

    public void AllEnemyKill()
    {
        for(int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].TakeDamage(999);
        }
    }
    public bool CheckEnemyBuff(BuffType Target, Enemy target)
    {
        if (target.buffDic.ContainsKey(Target))
        {
            return true;
        }
        return false;
    }
    public bool CheckEnemyBuff(BuffType Target)
    {
        for(int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i].buffDic.ContainsKey(Target))
            {
                return true;
            }
        }

        return false;
    }
}