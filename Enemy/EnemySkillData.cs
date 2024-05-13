using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkillData
{
    public enum EnemySkillType
    {
        Attack,
        SKill,
        Guard,
        GuardAttack,
        Summon,
    }

    public delegate void enemySkill(Enemy enemy);
    public static Dictionary<string, enemySkill> enemySkillData = new Dictionary<string, enemySkill>();

    public static IEnumerator InitEnemySkill()
    {
        #region Normal

        #region level1
        enemySkillData.Add("늑대", WolfSkill);
        enemySkillData.Add("쥐", RatSkill);
        enemySkillData.Add("슬라임", SlimeSKill);
        enemySkillData.Add("역병 쥐", RatSkill);
        enemySkillData.Add("고블린 척후병", RatSkill);
        enemySkillData.Add("고블린 전사", Goblin_WarriorSkill);
        #endregion

        #region level2
        enemySkillData.Add("오스덱스", RatSkill);
        enemySkillData.Add("흑색 늑대", RatSkill);
        enemySkillData.Add("설원 늑대", RatSkill);
        enemySkillData.Add("슬레들", RatSkill);
        enemySkillData.Add("스피더", RatSkill);
        #endregion

        #region level3
        enemySkillData.Add("케르베로스", RatSkill);
        enemySkillData.Add("역병 케르베로스", RatSkill);
        enemySkillData.Add("지옥 케르베로스", RatSkill);
        enemySkillData.Add("혈 오스덱스", RatSkill);
        enemySkillData.Add("냉기 오스덱스", RatSkill);
        enemySkillData.Add("역병 오스덱스", RatSkill);
        #endregion
        #endregion

        #region Elite
        #region level1
        enemySkillData.Add("웨어울프", WareWolfSkill);
        enemySkillData.Add("그림자 전사", GhostWarriorSkill);
        enemySkillData.Add("현상금 사냥꾼", WareWolfSkill);

        #endregion

        #endregion

        yield break;
    }

    #region Level 1 Normal
    public static void WolfSkill(Enemy enemy)
    {
        int rand = Random.Range(0, 2);

        switch(rand)
        {
            case 0:
                List<CBuffValue> buffList = new List<CBuffValue>();
                buffList.Add(new CBuffValue(BuffType.AtkUP, BuffReduce.None, 1, true));
                enemy.SetActing(new EnemySkill(EnemySkillType.SKill, 0, 0, 0, 0, "이로운 효과를 사용하려 합니다.",
                    buffList, null, EAttackSound.None));
                break;

            case 1:
                enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 3, 1, 0, 0, "공격과 해로운 효과를 준비중입니다.",
                    null, new CBuffValue(BuffType.Bleeding, BuffReduce.None, 2), EAttackSound.Bite));
                break;
        }
    }
    public static void RatSkill(Enemy enemy)
    {
        int rand = Random.Range(0, 1);

        switch (rand)
        {
            case 0:
                List<CBuffValue> buffList = new List<CBuffValue>();
                buffList.Add(new CBuffValue(BuffType.AtkUP, BuffReduce.None, 1));
                enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, 3, 0, 0, "공격을 준비중입니다.",
                    buffList,  null, EAttackSound.Bite));
                break;

            case 1:
                enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 3, 1, 0, 0, "공격과 해로운 효과를 준비중입니다.",
                    null, new CBuffValue(BuffType.Poison, BuffReduce.JustOne, 2), EAttackSound.Bite));
                break;
        }
    }
    public static void Goblin_WarriorSkill(Enemy enemy)
    {
        if(enemy.buffDic.ContainsKey(BuffType.Taunt))
        {
            enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, 1, 0, 0, "공격을 준비중입니다.",
                null,  null, EAttackSound.Slash));
        }
        else
        {
            List<CBuffValue> buffList = new List<CBuffValue>();
            buffList.Add(new CBuffValue(BuffType.Taunt, BuffReduce.HitOne, 2));

            enemy.SetActing(new EnemySkill(EnemySkillType.SKill, 0, 0, 5, 0, "이로운 효과를 사용하려 합니다.",
                buffList,  null, EAttackSound.None));
        }
            
    }

    public static void SlimeSKill(Enemy enemy)
    {

    }
    #endregion


    #region Level 1 Elite

    #region 웨어울프
    static int wareWolfAttackNum = 2;
    static bool wareWolfAttack = true;
    static bool wareWolfSpawn = false;
    public static void WareWolfSkill(Enemy enemy)
    {
        if(!wareWolfSpawn)
        {
            List<EnemyData> spawnList = new List<EnemyData>();
            spawnList.Add(DataManager.inst.GetEnemyData(0, (int)LV1EnemyList.Wolf));
            spawnList.Add(DataManager.inst.GetEnemyData(0, (int)LV1EnemyList.Wolf));
            enemy.SetActing(new EnemySkill(EnemySkillType.Summon, spawnList, "털을 곤두세웁니다."));
            wareWolfSpawn = true;
            return;
        }

        if (wareWolfAttack)
        {
            enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, wareWolfAttackNum, 0, 0,
                "공격을 준비중입니다.", null, null, EAttackSound.Slash));
            wareWolfAttackNum++;
            wareWolfAttack = !wareWolfAttack;
        }
        else
        {
                List<CBuffValue> buffList = new List<CBuffValue>();
                buffList.Add(new CBuffValue(BuffType.AtkUP, BuffReduce.None, 1));

                enemy.SetActing(new EnemySkill(EnemySkillType.SKill, 0, 0, 0, 10, "이로운 효과를 사용하려 합니다.",
                buffList, null, EAttackSound.None));
            wareWolfAttack = !wareWolfAttack;
        }
    }
    #endregion

    #region 그림자전사
    public static void GhostWarriorSkill(Enemy enemy)
    {
        List<CBuffValue> buffList = new List<CBuffValue>();
        buffList.Add(new CBuffValue(BuffType.AtkUP, BuffReduce.None, 3));

        enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, 1, 0, 1, "공격을 준비중입니다.",
                        buffList, new CBuffValue(BuffType.Fire, BuffReduce.JustOne, 3), EAttackSound.HeavySlash));
    }
    #endregion

    #region 현상금 사냥꾼
    public static void BountyHunterSKill(Enemy enemy)
    {

    }

    #endregion


    #endregion
}