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
        enemySkillData.Add("����", WolfSkill);
        enemySkillData.Add("��", RatSkill);
        enemySkillData.Add("������", SlimeSKill);
        enemySkillData.Add("���� ��", RatSkill);
        enemySkillData.Add("��� ô�ĺ�", RatSkill);
        enemySkillData.Add("��� ����", Goblin_WarriorSkill);
        #endregion

        #region level2
        enemySkillData.Add("��������", RatSkill);
        enemySkillData.Add("��� ����", RatSkill);
        enemySkillData.Add("���� ����", RatSkill);
        enemySkillData.Add("������", RatSkill);
        enemySkillData.Add("���Ǵ�", RatSkill);
        #endregion

        #region level3
        enemySkillData.Add("�ɸ����ν�", RatSkill);
        enemySkillData.Add("���� �ɸ����ν�", RatSkill);
        enemySkillData.Add("���� �ɸ����ν�", RatSkill);
        enemySkillData.Add("�� ��������", RatSkill);
        enemySkillData.Add("�ñ� ��������", RatSkill);
        enemySkillData.Add("���� ��������", RatSkill);
        #endregion
        #endregion

        #region Elite
        #region level1
        enemySkillData.Add("�������", WareWolfSkill);
        enemySkillData.Add("�׸��� ����", GhostWarriorSkill);
        enemySkillData.Add("����� ��ɲ�", WareWolfSkill);

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
                enemy.SetActing(new EnemySkill(EnemySkillType.SKill, 0, 0, 0, 0, "�̷ο� ȿ���� ����Ϸ� �մϴ�.",
                    buffList, null, EAttackSound.None));
                break;

            case 1:
                enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 3, 1, 0, 0, "���ݰ� �طο� ȿ���� �غ����Դϴ�.",
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
                enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, 3, 0, 0, "������ �غ����Դϴ�.",
                    buffList,  null, EAttackSound.Bite));
                break;

            case 1:
                enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 3, 1, 0, 0, "���ݰ� �طο� ȿ���� �غ����Դϴ�.",
                    null, new CBuffValue(BuffType.Poison, BuffReduce.JustOne, 2), EAttackSound.Bite));
                break;
        }
    }
    public static void Goblin_WarriorSkill(Enemy enemy)
    {
        if(enemy.buffDic.ContainsKey(BuffType.Taunt))
        {
            enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, 1, 0, 0, "������ �غ����Դϴ�.",
                null,  null, EAttackSound.Slash));
        }
        else
        {
            List<CBuffValue> buffList = new List<CBuffValue>();
            buffList.Add(new CBuffValue(BuffType.Taunt, BuffReduce.HitOne, 2));

            enemy.SetActing(new EnemySkill(EnemySkillType.SKill, 0, 0, 5, 0, "�̷ο� ȿ���� ����Ϸ� �մϴ�.",
                buffList,  null, EAttackSound.None));
        }
            
    }

    public static void SlimeSKill(Enemy enemy)
    {

    }
    #endregion


    #region Level 1 Elite

    #region �������
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
            enemy.SetActing(new EnemySkill(EnemySkillType.Summon, spawnList, "���� ��μ���ϴ�."));
            wareWolfSpawn = true;
            return;
        }

        if (wareWolfAttack)
        {
            enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, wareWolfAttackNum, 0, 0,
                "������ �غ����Դϴ�.", null, null, EAttackSound.Slash));
            wareWolfAttackNum++;
            wareWolfAttack = !wareWolfAttack;
        }
        else
        {
                List<CBuffValue> buffList = new List<CBuffValue>();
                buffList.Add(new CBuffValue(BuffType.AtkUP, BuffReduce.None, 1));

                enemy.SetActing(new EnemySkill(EnemySkillType.SKill, 0, 0, 0, 10, "�̷ο� ȿ���� ����Ϸ� �մϴ�.",
                buffList, null, EAttackSound.None));
            wareWolfAttack = !wareWolfAttack;
        }
    }
    #endregion

    #region �׸�������
    public static void GhostWarriorSkill(Enemy enemy)
    {
        List<CBuffValue> buffList = new List<CBuffValue>();
        buffList.Add(new CBuffValue(BuffType.AtkUP, BuffReduce.None, 3));

        enemy.SetActing(new EnemySkill(EnemySkillType.Attack, 5, 1, 0, 1, "������ �غ����Դϴ�.",
                        buffList, new CBuffValue(BuffType.Fire, BuffReduce.JustOne, 3), EAttackSound.HeavySlash));
    }
    #endregion

    #region ����� ��ɲ�
    public static void BountyHunterSKill(Enemy enemy)
    {

    }

    #endregion


    #endregion
}