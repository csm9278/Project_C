using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill
{
    public EnemySkillData.EnemySkillType skillType;
    public EAttackSound soundType;
    public int dmgValue;
    public int attackTime = 1;
    List<CBuffValue> buffList;
    CBuffValue debuff;
    int getArmor;
    int getHealth;
    string info;

    public List<EnemyData> summon;


    public EnemySkill(EnemySkillData.EnemySkillType type, int dmgValue, int attackTime,
        int getArmor, int getHealth, string info, List<CBuffValue> buffValue,  CBuffValue debuffValue, EAttackSound soundType)
    {
        skillType = type;
        this.soundType = soundType;
        this.dmgValue = dmgValue;
        this.attackTime = attackTime;
        this.buffList = buffValue;
        this.debuff = debuffValue;
        this.getArmor = getArmor;
        this.getHealth = getHealth;
        this.info = info;
    }

    public EnemySkill(EnemySkillData.EnemySkillType type, List<EnemyData> summonEnemy, string info)
    {
        skillType = type;
        summon = summonEnemy;
        this.info = info;
    }

    public IEnumerator SkillEffect(Character caster)
    {
        PlayerManager.inst.TakeDamage(dmgValue, attackTime, caster, soundType);
        if(debuff != null)
            PlayerManager.inst.AddBuff(debuff);

        yield return new WaitForSeconds(0.3f * attackTime);

        if(buffList != null)
        {
            for(int i = 0; i < buffList.Count; i++)
            {
                if (!buffList[i].isAllTarget)
                {
                    Debug.Log(buffList[i].bType);
                    caster.AddBuff(buffList[i]);
                    SoundManager.instance.PlayEffSound(GlobalData.buffsounds[(int)buffList[i].bType]);
                }
                else
                    BattleManager.Inst.enemyManager.BuffAllEnemy(buffList[i]);
            }
        }

        caster.GetArmor(getArmor);
        caster.HealHp(getHealth);
    }

    public string GetInfo() { return info; }
}
