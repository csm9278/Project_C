using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Enemy : Character
{
    public EnemyData data;
    public EnemySkillData.enemySkill skillEff;

    public GameObject targetImage;

    SpriteRenderer sprenderer;
    Collider2D coll;

    public TMP_Text actingText;
    public Image actingImage;

    EnemySkill skill;

    List<Enemy> minions = new List<Enemy>();

    [Header("--- Info ---")]
    public Image infoObject;
    public TMP_Text nameText;
    public TMP_Text actingInfoText;
    Tween infoTween;

    [Header("--- Buff Effect ---")]
    public SpriteRenderer buffImage;
    public ParticleSystem healParticle;

    private void Start() => StartFunc();

    private void StartFunc()
    {
        coll = GetComponent<Collider2D>();
    }

    public void SetUp(EnemyData data, bool setAcitng = true)
    {
        sprenderer = GetComponent<SpriteRenderer>();
        sprenderer.sprite = data.sprite;
        nameText.text = data.name;
        maxHp = data.hp;
        currentHp = data.hp;


        hpText.text = data.hp + "/" + data.hp;

        buffImage.sprite = data.sprite;

        skillEff = EnemySkillData.enemySkillData[data.name];
        if(setAcitng)
            skillEff(this);
    }

    public void Targeted(bool targeted)
    {
        if (targeted)
            targetImage.gameObject.SetActive(true);
        else
            targetImage.gameObject.SetActive(false);
    }

    public override void HitEffect()
    {
        this.transform.DOShakePosition(0.3f);
        sprenderer.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo);
    }

    public override void DieEffect()
    {
        currentHp = 0;
        this.transform.DOShakePosition(2.0f, 0.3f).OnComplete(() => Destroy(this.gameObject));
        coll.enabled = false;
        sprenderer.DOFade(0.0f, 2.0f);
        actingText.DOFade(0.0f, 2.0f);
        actingImage.DOFade(0.0f, 2.0f);
        hpText.DOFade(0.0f, 2.0f);
        buffTr.gameObject.SetActive(false);
        EnemyManager.inst.DieEnemy(this);
    }

    public void SetActing(EnemySkill enemySkill)
    {
        skill = enemySkill;
        actingInfoText.text = skill.GetInfo();

        if(enemySkill.skillType == EnemySkillData.EnemySkillType.Attack)
        {
            actingImage.sprite = Resources.Load<Sprite>("ActionImage/00.Attack");
            actingText.text = (skill.dmgValue + (buffDic.ContainsKey(BuffType.AtkUP) ? buffDic[BuffType.AtkUP].GetRange() : 0)).ToString();
            if (skill.attackTime > 1)
                actingText.text += "x" + skill.attackTime.ToString();
        }
        else
        {
            actingImage.sprite = Resources.Load<Sprite>("ActionImage/01.Skill");
            actingText.text = "";
        }

        actingText.DOFade(0.0f, 0);
        actingText.DOFade(1.0f, 1.0f);
        actingImage.DOFade(0.0f, 0);
        actingImage.DOFade(1.0f, 1.0f);
    }

    /// <summary>
    /// 행동하기
    /// </summary>
    public IEnumerator Acting()
    {
        if (skill == null)
            yield break;

        switch(skill.skillType)
        {
            case EnemySkillData.EnemySkillType.Attack:
                this.transform.DOMoveY(this.transform.position.y - 0.7f, 0.1f).SetLoops(2, LoopType.Yoyo);
                yield return StartCoroutine(skill.SkillEffect(this));
                break;

            case EnemySkillData.EnemySkillType.SKill:
                yield return StartCoroutine(skill.SkillEffect(this));
                break;

            case EnemySkillData.EnemySkillType.Summon:
                yield return StartCoroutine(EnemyManager.inst.SummonMinionCo(skill.summon, this));
                break;
        }

        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    /// 스킬 재설정
    /// </summary>
    public void SetActing()
    {
        skillEff(this);
    }

    void ShowInfo(bool isShow)
    {
        if(isShow)
        {
            nameText.DOFade(1.0f, 0.3f);
            actingInfoText.DOFade(1.0f, 0.3f);
            infoObject.DOFade(1.0f, 0.3f);
        }
        else
        {
            nameText.DOFade(0, 0.3f);
            actingInfoText.DOFade(0.0f, 0.3f);
            infoObject.DOFade(0.0f, 0.3f);
        }

    }

    private void OnMouseEnter()
    {
        ShowInfo(true);        
    }

    private void OnMouseExit()
    {
        ShowInfo(false);
    }

    public void BuffParticle(BuffType buffType)
    {
        switch(buffType)
        {
            case BuffType.AtkUP:
                buffImage.DOFade(0.5f, 0.3f).SetLoops(2, LoopType.Yoyo);
                break;
        }
    }

    public override void HealHp(int value)
    {
        base.HealHp(value);

        if(value > 0)
            healParticle.Play();
    }

    public override void AddBuff(CBuffValue buffValue, bool isStart = false)
    {
        BuffParticle(buffValue.bType);
        if (buffDic.ContainsKey(buffValue.bType))
        {
            if (buffDic[buffValue.bType].GetRange() > 0)
            {
                buffDic[buffValue.bType].AddRange(buffValue.range);
            }
        }
        else
        {
            GameObject obj = Instantiate(BattleManager.Inst.enemyManager.enemyBuff, buffTr);

            if (obj.TryGetComponent(out Buff b))
            {
                b.SetBuff(new CBuffValue(buffValue));

                buffDic[buffValue.bType] = b;
            }

            if(obj.TryGetComponent(out ShowInfo info))
            {
                info.SetInfo(GlobalData.buffInfo[(int)buffValue.bType] + GlobalData.buffRangeInfo[(int)buffValue.bReduce]);
            }
        }
        RefreshSkillValue();
    }

    /// <summary>
    /// 액팅이 세팅된 후 도중에 버프얻을 시 수치 변화
    /// </summary>
    void RefreshSkillValue()
    {
        if (skill.skillType == EnemySkillData.EnemySkillType.Attack)
        {
            Debug.Log("변화");
            actingText.text = (skill.dmgValue + (buffDic.ContainsKey(BuffType.AtkUP) ? buffDic[BuffType.AtkUP].GetRange() : 0)).ToString();
            if (skill.attackTime > 1)
                actingText.text += "x" + skill.attackTime.ToString();
        }
    }

    public void AddMinion(Enemy enemy)
    {
        minions.Add(enemy);
    }
}