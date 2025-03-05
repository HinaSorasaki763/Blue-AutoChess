
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CharacterCTRL : MonoBehaviour
{
    #region 屬性變數宣告
    // Grid and Target-related Fields
    public HexNode CurrentHex;
    public HexNode HexWhenBattleStart;
    public GameObject Target;
    public GameObject PreTarget;

    // Team and Layer-related Fields
    public bool IsAlly;
    public LayerMask allyLayer;
    public LayerMask enemyLayer;
    public LayerMask GridLayer;

    // Character and Stats-related Fields
    public Character characterStats;
    public CharacterBars characterBars;
    public CharacterParent allyParent;
    public CharacterParent enemyParent;
    public StatsContainer stats;
    public int star;
    public ModifierCTRL modifierCTRL;
    public EffectCTRL effectCTRL;
    private List<Shield> shields = new List<Shield>(); // 存儲所有護盾
    // Combat-related Fields
    public bool IsHeroicEnhanced = false;
    public bool FaceDirectionLock = false;
    private bool tempDirectionLock = false;
    public bool ManaLock = false;
    public bool enterBattle = false;
    public bool isAlive = true;
    public bool IsCastingAbility = false;
    public bool isWalking = false;
    public bool IsDying = false;
    public bool isMarked = false;
    public bool isCCImmune = false;
    public bool Invincible = false;
    public bool AntiHeal = false;
    public bool Overtime = false;
    public float AntiHealRatio = 0;
    public bool Taunted = false;
    private bool isFindingPath = false;
    public float attackRate = 5.0f;
    private float attackTimer = 0f;
    private float attackSpeed = 1.0f;
    public bool WakamoMark = false;
    private float wakamoMarkRatio;
    private int dmgRecivedOnWakamoMarked;
    private CharacterCTRL WakamoMarkParent;
    public bool stunned = false;
    public Transform FirePoint;
    public Transform GetHitPoint;
    public CharacterSkillBase ActiveSkill;
    public Vector3 DetectedPos = Vector3.zero;
    public SkillContext SkillContext;
    public CharacterAudioManager AudioManager;
    // Traits and Effects-related Fields
    public TraitController traitController;
    private TraitEffectApplier traitEffectApplier = new TraitEffectApplier();
    public List<CharacterObserverBase> observers = new List<CharacterObserverBase>();
    // Animator-related Fields
    public CustomAnimatorController customAnimator;
    private int Amulet_WatchObserverManaRestoreAmount;
    // Targeting and Grid Events-related Fields
    public bool isTargetable = true;
    public delegate void GridChanged(HexNode oldTile, HexNode newTile);
    public event GridChanged OnGridChanged;

    private readonly Vector3 offset = new Vector3(0, 0.14f, 0);
    public bool isShirokoTerror;
    public Shiroko_Terror_DroneCTRL droneCTRL;
    public bool isObj;
    public GameObject Logistic_dummy;
    public CharacterEquipmentManager equipmentManager;
    public int critTransferAmount;
    readonly int crtiTranferRatio = 70;
    public int StealManaCount;
    public bool IsFeared { get; set; }
    private CharacterCTRL fearSource = null;      // 恐懼來源
    private float fearDuration = 0;            // 恐懼持續秒數
    public Coroutine fearCorutine;
    public int DealtDamageThisRound;
    public int TakeDamageThisRound;
    #endregion
    #region Unity Lifecycle Methods
    public void ResetToBeforeBattle()
    {
        if (fearCorutine != null)
        {
            StopCoroutine(fearCorutine);
        }

        if (customAnimator!= null)
        {
            customAnimator.ForceIdle();
        }

        enterBattle = false;
        FaceDirectionLock = false;
        tempDirectionLock = false;
        Overtime = false;
        ManaLock = false;
        isAlive = true;
        IsCastingAbility = false;
        isWalking = false;
        IsDying = false;
        isMarked = false;
        isCCImmune = false;
        Invincible = false;
        AntiHeal = false;
        Taunted = false;
        isFindingPath = false;
        attackRate = 5.0f;
        attackTimer = 0f;
        WakamoMark = false;
        stunned = false;
        isTargetable = true;
        fearSource = null;
        fearDuration = 0;
        effectCTRL.ClearAllEffect();
    }
    public virtual void OnEnable()
    {
        star = 1;
        stats = characterStats.Stats.Clone();
        ResetStats();

        allyParent = ResourcePool.Instance.ally;
        enemyParent = ResourcePool.Instance.enemy;
        modifierCTRL = GetComponent<ModifierCTRL>();
        effectCTRL = GetComponent<EffectCTRL>();
        effectCTRL.characterCTRL = GetComponent<CharacterCTRL>();
        traitController = GetComponent<TraitController>();
        IsDying = false;
        if (characterBars != null)
        {
            characterBars.gameObject.SetActive(true);
        }

        int characterId = characterStats.CharacterId;
        isShirokoTerror = characterId == 31;
        if (characterBehaviors.TryGetValue(characterId, out var characterBehaviorFunc))
        {
            var observer = characterBehaviorFunc();
            AddObserver(observer);
        }
        GlobalBaseObserver globalObserver = new GlobalBaseObserver();
        AddObserver(globalObserver);
        if (characterSkills.TryGetValue(characterId, out var characterSkillFunc))
        {
            CustomLogger.Log(this, $"characterId {characterId} getting {characterSkillFunc} TestEnhanceSkill");
            var baseSkill = characterSkillFunc();
            if (characterStats.TestEnhanceSkill || characterId >= 500)
            {
                CustomLogger.Log(this, $"characterId {characterId} getting {characterSkillFunc} TestEnhanceSkill");
                ActiveSkill = baseSkill.GetHeroicEnhancedSkill();
            }
            else
            {
                ActiveSkill = baseSkill;
            }
        }

        if (characterStats.logistics)
        {
            SetStat(StatsType.Range, 20);
        }
        equipmentManager = GetComponent<CharacterEquipmentManager>();
        equipmentManager.SetParent(this);
        AudioManager = GetComponent<CharacterAudioManager>();

    }
    public void OnDisable()
    {
        foreach (var item in observers)
        {
            CustomLogger.Log(this, $"character {characterStats.name} disabled");
            item.OnCharacterDisabled(this);
        }
    }
    public void LockDirection(Vector3 PosLookAt)
    {
        Quaternion targetRotation = Quaternion.LookRotation(PosLookAt);
        transform.rotation = targetRotation;
        FaceDirectionLock = true;

    }
    private bool CanTakeDamage()
    {
        return isAlive
            && !IsDying
            && !(characterStats.TestEnhanceSkill)
            && !characterStats.TestBuildInvinvicble;
    }
    private IEnumerator CheckEverySecond()
    {
        while (true)
        {
            if (enterBattle)
            {
                CustomLogger.Log(this, "enterBattle");
                if (CurrentHex.isDesertified)
                {
                    CustomLogger.Log(this, "CurrentHex.isDesertified");
                    bool isAbydos = traitController.GetAcademy() == Traits.Abydos;
                    Effect effect = EffectFactory.CreateAbydosEffect(isAbydos, AbydosManager.Instance.level);
                    CustomLogger.Log(this, $"isabydos = {isAbydos}, effect = {effect.Source}");

                    effectCTRL.AddEffect(effect);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
    public void ReleaseLockDirection()
    {
        if (Target == null)
        {
            FaceDirectionLock = false;
            return;
        }
        transform.LookAt(Target.transform.position);
        FaceDirectionLock = false;
        CustomLogger.Log(this, $"character {characterStats.name} released ,time = {Time.time}");
    }
    public void ResetStats()
    {
        SetStat(StatsType.Health, characterStats.Health[star - 1]);
        SetStat(StatsType.Attack, characterStats.Attack[star - 1]);
        SetStat(StatsType.currHealth, GetStat(StatsType.Health));
        SetStat(StatsType.Mana, 0);
        CustomLogger.Log(this, $"character {characterStats.name} , health = {characterStats.Health[star - 1]} , attack = {characterStats.Attack[star - 1]}");
    }
    private void Start()
    {
        allyLayer = 1 << LayerMask.NameToLayer("Ally");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        GridLayer = 1 << LayerMask.NameToLayer("Grid");
        customAnimator = GetComponent<CustomAnimatorController>();
        TriggerCharacterStart();
        TriggerManualUpdate();
        StartCoroutine(CheckEverySecond());
    }

    void Awake()
    {

    }

    public virtual void Update()
    {
        if (tempDirectionLock != FaceDirectionLock)
        {
            CustomLogger.Log(this, $"character {characterStats.name} lockDirection = {FaceDirectionLock} , time = {Time.time}");
            tempDirectionLock = FaceDirectionLock;
        }
        if (customAnimator != null)
        {
            characterBars.UpdateText(customAnimator.GetState().Item1.ToString());
            bool canAttack = !customAnimator.animator.GetBool("CastSkill") && !isWalking;

            if (enterBattle)
            {
                DetectedPos = CurrentHex.transform.position + new Vector3(0, 0.3f, 0);

                if (!(stunned || IsFeared))
                {
                    HandleTargetFinding();
                    HandleAttack(canAttack);
                }
                CheckEveryThing();
                foreach (var hitCollider in Physics.OverlapSphere(transform.position, 0.2f, GridLayer))
                {
                    CurrentHex = hitCollider.GetComponent<HexNode>();
                    CurrentHex.SetOccupyingCharacter(this);
                }
            }
        }

        if (Target != null && (Target.GetComponent<CharacterCTRL>().IsDying || !Target.GetComponent<CharacterCTRL>().isAlive || !Target.activeInHierarchy))
        {
            Target = null;
        }

        for (int i = shields.Count - 1; i >= 0; i--)
        {
            shields[i].remainingTime -= Time.deltaTime;
            if (shields[i].remainingTime <= 0)
            {
                RemoveShield(shields[i]);
                shields.RemoveAt(i);
            }
        }

        if (characterStats.CharacterId == 28)
        {
            //FindTarget();
        }
        CharacterUpdate();
    }
    public void CharacterUpdate()
    {
        foreach (var item in observers)
        {
            item.CharacterUpdate();
        }
        if (traitController!= null)
        {
            traitController.CharacterUpdate();
        }
        if (equipmentManager!= null)
        {
            equipmentManager.CharacterUpdate();
        }

    }
    #endregion

    #region Combat Logic
    private void RemoveShield(Shield shield)
    {
        AddStat(StatsType.Shield, -shield.amount);
    }
    public void EnterBattle() => enterBattle = true;

    private void HandleAttack(bool canAttack)
    {
        if (!canAttack || IsCastingAbility || Target == null || !Target.activeInHierarchy)
        {
            if (Target == null && !isWalking && !isFindingPath)
            {
                bool foundTarget = FindTarget();

                if (!foundTarget)
                {
                    customAnimator.ChangeState(CharacterState.Idling);
                    customAnimator.HaveTarget(false);
                    if ((IsAlly && enemyParent.GetActiveCharacter() == 0) || (!IsAlly && allyParent.GetActiveCharacter() == 0))
                    {
                        enterBattle = false;
                        customAnimator.HaveTarget(false);
                    }
                }
            }
            return;
        }

        SetAnimatorStateToAttack();
        if (!FaceDirectionLock)
        {
            transform.LookAt(Target.transform);
        }


        int animationIndex = 4;
        if (isShirokoTerror)
        {
            animationIndex = 3;
        }
        attackTimer += Time.deltaTime;

        float time = characterStats.logistics ? 1f : customAnimator.GetAnimationClipInfo(animationIndex).Item2 / attackSpeed;
        if (attackTimer >= time)
        {
            attackTimer = 0f;
        }
    }
    public void SetAnimatorStateToAttack()
    {
        if (IsCastingAbility || isWalking || isFindingPath)
        {
            return;
        }
        customAnimator.ChangeState(CharacterState.Attacking);
    }
    public int ExecuteActiveSkill()
    {
        CharacterCTRL target = null;
        if (Target != null)
        {
            target = GetTarget().GetComponent<CharacterCTRL>();
        }
        else
        {
            CustomLogger.LogWarning(this, $"character {name} using empty target");
        }
        var skillContext = new SkillContext
        {
            Parent = this,
            Enemies = GetEnemies(),
            Allies = GetAllies(),
            hexMap = SpawnGrid.Instance.hexNodes.Values.ToList(),
            Range = 5,
            duration = 2f,
            currHex = CurrentHex,
            CurrentTarget = target,
            CharacterLevel = 1,
            statsByStarLevel = ActiveSkill.GetCharacterLevel()
        };
        int i = 0;
        if (isShirokoTerror)
        {
            Shiroko_Terror_SkillCTRL shiroko_Terror_SkillCTRL = GetComponent<Shiroko_Terror_SkillCTRL>();
            skillContext.shirokoTerror_SkillID = shiroko_Terror_SkillCTRL.ChooseSkill(skillContext);
            i = skillContext.shirokoTerror_SkillID;
        }
        SkillContext = skillContext;
        ActiveSkill.ExecuteSkill(skillContext);
        traitController.CastedSkill();
        equipmentManager.OnParentCastedSkill();
        foreach (var item in observers)
        {
            item.OnCastedSkill(this);
        }
        return i;

    }
    public (bool, int) CalculateCrit(int dmg)
    {
        bool iscrit = false;
        if (characterStats.CharacterId == 27)
        {
            dmg = (int)(dmg * (1 + ((GetStat(StatsType.CritRatio) + GetStat(StatsType.CritChance)) * 0.01f)));
            iscrit = true;
            return (iscrit, dmg);
        }

        if (Utility.Iscrit(GetStat(StatsType.CritChance),this))
        {
            dmg = (int)(dmg * (1 + GetStat(StatsType.CritRatio) * 0.01f));
            CustomLogger.Log(this, $"character {name} crit");
            iscrit = true;
        }
        return (iscrit, dmg);
    }
    public void Attack()
    {
        if (Target != null && Vector3.Distance(Target.transform.position, transform.position) > GetStat(StatsType.Range) + 0.1f)
        {
            FindTarget();
            return;
        }
        if (IsDying || IsCastingAbility) return;
        if (characterStats.logistics)
        {
            logistics();
            HandleAttacking();
            return;
        }
        HandleAttacking();


        SetAnimatorStateToAttack();
        if (characterStats.CharacterId != 34)
        {
            var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, FirePoint.position, Quaternion.identity);
            var bulletComponent = bullet.GetComponent<NormalBullet>();
            if (!Target) return;
            int damage = (int)(GetStat(StatsType.Attack) * (1 + modifierCTRL.GetTotalStatModifierValue(ModifierType.DamageDealt) * 0.01f));
            (bool, int) tuple = CalculateCrit(damage);
            bool iscrit = tuple.Item1;
            damage = tuple.Item2;
            bulletComponent.Initialize(damage, GetTargetLayer(), this, 20, Target, false, iscrit);

        }

        transform.LookAt(Target.transform);
    }
    public IEnumerator Explosion(int dmg, int range, HexNode centerNode, CharacterCTRL sourceCharacter, float waitforSec, string sources, bool iscrit)
    {
        yield return new WaitForSeconds(waitforSec);
        Utility.DealDamageInRange(centerNode, range, sourceCharacter, dmg, sources, iscrit);
        yield return null;
    }
    public int GetAttack()
    {
        return (int)GetStat(StatsType.Attack);
    }
    private void HandleTargetFinding()
    {
        if (Target == null && !FindTarget())
        {
            Debug.Log($"{characterStats.CharacterName} 未找到有效目標，狀態變更為 Idling");
            customAnimator.ChangeState(CharacterState.Idling);
            if ((IsAlly && enemyParent.GetActiveCharacter() == 0) || (!IsAlly && allyParent.GetActiveCharacter() == 0))
            {
                enterBattle = false;
            }
        }
    }


    private void HandleAttacking()
    {
        customAnimator.animator.speed = GetStat(StatsType.AttackSpeed);
        if (!ManaLock)
        {
            Addmana(10);
        }
        foreach (var item in observers)
        {
            item.OnAttacking(this);
        }
        traitController.Attacking();
        equipmentManager.OnParentAttack();
        AugmentEventHandler.Instance.Attacking(this);
    }
    public (float, float) BeforeApplyingNegetiveEffect(float length, float effectiveness)
    {
        float finallength = length;
        float finaleffectiveness = effectiveness;
        foreach (var item in observers)
        {
            (finallength, finaleffectiveness) = item.AdjustNegetiveEffect(finallength, finaleffectiveness);
        }
        (finallength, finaleffectiveness) = traitController.BeforeApplyingNegetiveEffect(finallength, finaleffectiveness);
        (finallength, finaleffectiveness) = equipmentManager.BeforeApplyingNegetiveEffect(finallength, finaleffectiveness);
        CustomLogger.Log(this, $"from ({length},{effectiveness}) to ({finallength},{finaleffectiveness})");
        return (finallength, finaleffectiveness);
    }
    public int BeforeHealing(int amount, CharacterCTRL source)
    {
        int final = amount;
        foreach (var item in observers)
        {
            final = item.BeforeHealing(source, final);
        }
        final = traitController.BeforeHealing(source, final);
        final = equipmentManager.BeforeHealing(source, final);
        return final;
    }
    public void Heal(int amount, CharacterCTRL source)
    {
        amount = BeforeHealing(amount, source);
        if (AntiHeal)
        {
            amount = (int)(amount * (1 - AntiHealRatio));
        }
        if (amount <= 1)
        {
            return;
        }
        CustomLogger.Log(this, $"{source} heal {name} of {amount}");
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.None, amount, screenPos, false, true);
        if (GetStat(StatsType.currHealth) + amount >= GetStat(StatsType.Health))
        {
            SetStat(StatsType.currHealth, GetStat(StatsType.Health));
            if (equipmentManager.HaveSpecificEquipment(11))
            {
                int manashouldRestore = (int)(GetStat(StatsType.currHealth) + amount - GetStat(StatsType.Health)) / 20;
                int manaRestoreAmount = Math.Min((int)GetStat(StatsType.MaxMana) / 5 - Amulet_WatchObserverManaRestoreAmount, manashouldRestore);
                Amulet_WatchObserverManaRestoreAmount += manaRestoreAmount;
                Addmana(manaRestoreAmount);
            }
            return;
        }
        AudioManager.PlayHpRestoredSound();
        AddStat(StatsType.currHealth, amount);
    }
    private Vector3 GetRandomDeviation(float deviation)
    {
        return new Vector3(
            UnityEngine.Random.Range(-deviation, deviation),
            UnityEngine.Random.Range(-deviation, deviation),
            UnityEngine.Random.Range(-deviation, deviation)
        );
    }
    public void SetUnTargetable(bool b)
    {
        isTargetable = b;
    }
    public void SetInvincible(bool b)
    {
        Invincible = b;
    }
    public void SetAntiHeal(bool b)
    {
        AntiHeal = b;
        if (b && !Overtime)
        {
            AntiHealRatio = 0.4f;
        }
    }
    public void SetTaunt(bool b)
    {
        Taunted = b;
    }
    public void AbydosBuff(bool isAbydos, int data2, int data3, bool start)
    {
        AddAttackSpeed(data2 * 0.01f);
        if (start)
        {
            int health = (int)(GetStat(StatsType.Health) * (isAbydos ? 1 : -1) * data3 * 0.01f);
            AddStat(StatsType.currHealth, health);
        }

    }
    public void RemoveHealth(int amount, string source)
    {
        if (IsDying || !isAlive) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        if (amount <= 0)
        {
            amount = 1;
        }
        CustomLogger.Log(this, $"{name} losing {amount} health by  source {source} ");
        while (amount > 0 && shields.Count > 0)
        {
            Shield shield = shields[0];
            if (shield.amount >= amount)
            {
                TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, amount, screenPos, false);
                shield.amount -= amount;
                AddStat(StatsType.Shield, -amount);
                amount = 0;
            }
            else
            {
                TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, shield.amount, screenPos, false);
                amount -= shield.amount;
                AddStat(StatsType.Shield, -shield.amount);
                shields.RemoveAt(0);
            }
        }

        if (amount > 0)
        {
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.None, amount, screenPos, true);
            AddStat(StatsType.currHealth, -amount);
        }
    }
    public void StealMana(CharacterCTRL stolen)
    {
        stolen.ManaStolen(this);
    }
    private void ManaStolen(CharacterCTRL stealer)
    {
        if (StealManaCount >= 5)
        {
            return;
        }
        StealManaCount++;
        int amount = 10;
        Addmana(-amount);
        stealer.Addmana(amount);
    }
    public void EmptyEffectFunction()
    {
        //不能移除

    }
    public void OnKillEnemy(string source, CharacterCTRL characterDies)
    {
        foreach (var item in observers)
        {
            item.OnKilledEnemy(this, source, characterDies);
        }
    }
    public void logistics()
    {
        foreach (var item in observers)
        {
            item.OnLogistic(this);
        }
    }


    #endregion


    #region Property Handling and Updates
    public int GetEquippedItemCount() => 0;

    public void SetBarChild(CharacterBars bar) => characterBars = bar;
    #endregion

    #region Skill and Observer Pattern
    public void AddObserver(CharacterObserverBase observer)
    {
        if (!observers.Contains(observer)) observers.Add(observer);
    }

    public void RemoveObserver(CharacterObserverBase observer) => observers.Remove(observer);

    public void CheckEveryThing()
    {
        if (CheckDeath() && !IsDying)
        {
            Die();
            return;
        }
        if (GetStat(StatsType.Mana) >= GetStat(StatsType.MaxMana) && !IsCasting() && !isWalking && !isObj && !Taunted)
        {
            Addmana(-(int)GetStat(StatsType.MaxMana));
            SwapToCastSkill();
        }
    }
    public void SwapToCastSkill()
    {
        customAnimator.ChangeState(CharacterState.CastSkill);
        AudioManager.PlayCastExSkillSound();
        CustomLogger.Log(this, $"{characterStats.CharacterName}CastSkill()");
        IsCastingAbility = true;
        int i = GetSkillID();
        int animationIndex = 6;
        if (isShirokoTerror)
        {
            animationIndex = i + 8;
        }
        float sec1 = customAnimator.GetAnimationClipInfo(animationIndex).Item2;
        if (characterStats.CharacterId == 16)
        {
            sec1 /= GetStat(StatsType.AttackSpeed);
        }
        float sec = sec1 / 3f;
        Debug.Log($"[CharacterCTRL] index = {animationIndex} name = {customAnimator.GetAnimationClipInfo(animationIndex).Item1}  length = {customAnimator.GetAnimationClipInfo(animationIndex).Item2}");
        StartCoroutine(CastSkillWaitTime(sec));
    }
    public IEnumerator CastSkillWaitTime(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (enterBattle)
        {
            SetAnimatorStateToAttack();
            customAnimator.animator.SetBool("CastSkill", false);
        }
        yield return new WaitForSeconds(sec * 2 - 0.1f);
        AfterCastSkill();
        yield return new WaitForSeconds(0.1f);
    }
    public float GetHealthPercentage() => GetStat(StatsType.currHealth) / (float)GetStat(StatsType.Health);

    public float GetStat(StatsType statsType) => stats.GetStat(statsType);
    public void CritCorrection()
    {
        int currentCritChance = (int)GetStat(StatsType.CritChance);
        float ratio = crtiTranferRatio * 0.01f; // 轉換因子

        if (currentCritChance > 100)
        {
            // 有溢出：計算超出部分
            int overflow = currentCritChance - 100;
            // 將 overflow 部分轉換成額外爆擊傷害
            float addedCritRatio = overflow * ratio;
            AddStat(StatsType.CritRatio, addedCritRatio);
            SetStat(StatsType.CritChance, 100);
            critTransferAmount = overflow;

            CustomLogger.Log(this, $"Applied overflow: {overflow} overflow converted to {addedCritRatio} extra CritRatio.");
        }
        else // currentCritChance <= 100
        {
            // 當前爆擊率低於上限，檢查是否有先前轉換的溢出需要還原
            int deficit = 100 - currentCritChance;
            // 要還原的爆擊傷害數值，也就是如果要補足至 100，理論上需要扣除的 CritRatio
            float neededCritRatio = deficit * ratio;

            if (critTransferAmount > 0)
            {
                // 先計算從目前已轉換的 critTransferAmount 能還原多少爆擊率
                // 反向公式：還原的爆擊率 = 扣除的 CritRatio / ratio
                // 如果 critTransferAmount（以爆擊率點數計）足夠填補 deficit，則可全部還原
                if (critTransferAmount >= deficit)
                {
                    float removalCritRatio = deficit * ratio;
                    AddStat(StatsType.CritRatio, -removalCritRatio);
                    SetStat(StatsType.CritChance, 100);
                    critTransferAmount -= deficit;
                    CustomLogger.Log(this, $"Restored full deficit: restored {deficit} CritChance by removing {removalCritRatio} CritRatio.");
                }
                else
                {
                    // 如果不足，則部分還原：依據現有轉換量還原對應的爆擊率
                    int restoredCritChance = critTransferAmount; // 已有的全部轉換點數都能還原
                    float removalCritRatio = restoredCritChance * ratio;
                    AddStat(StatsType.CritRatio, -removalCritRatio);
                    SetStat(StatsType.CritChance, currentCritChance + restoredCritChance);
                    critTransferAmount = 0;
                    CustomLogger.Log(this, $"Partially restored: restored {restoredCritChance} CritChance by removing {removalCritRatio} CritRatio.");
                }
            }
            else
            {
                CustomLogger.Log(this, "No overflow correction needed.");
            }
        }
    }
    public void AddStat(StatsType statsType, float amount)
    {
        if (statsType == StatsType.Mana)
        {
            Addmana((int)amount);
            return;
        }
        if (statsType == StatsType.AttackSpeed)
        {
            AddAttackSpeed(amount);
            return;
        }
        SetStat(statsType, GetStat(statsType) + amount);
    }
    public void AddAttackSpeed(float amount)
    {
        if (characterStats.CharacterId == 34)
        {
            AddStat(StatsType.Attack, amount * 100);
            return;
        }
        SetStat(StatsType.AttackSpeed, GetStat(StatsType.AttackSpeed) + amount);
    }
    public void Addmana(int amount)
    {
        if (characterStats.CharacterId == 16)
        {
            return;
        }
        SetStat(StatsType.Mana, GetStat(StatsType.Mana) + amount);
    }

    public void SetStat(StatsType statsType, float amount) => stats.SetStat(statsType, amount);
    private int GetSkillID()
    {
        CharacterCTRL c = null;
        if (Target != null)
        {
            c = Target.GetComponent<CharacterCTRL>();
        }
        var skillContext = new SkillContext
        {
            Parent = this,
            Enemies = GetEnemies(),
            Allies = GetAllies(),
            hexMap = SpawnGrid.Instance.hexNodes.Values.ToList(),
            Range = 5,
            duration = 2f,
            currHex = CurrentHex,
            CurrentTarget = GetTargetCTRL(),
            CharacterLevel = 1
        };
        int i = 0;
        if (isShirokoTerror)
        {
            Shiroko_Terror_SkillCTRL shiroko_Terror_SkillCTRL = GetComponent<Shiroko_Terror_SkillCTRL>();
            skillContext.shirokoTerror_SkillID = shiroko_Terror_SkillCTRL.ChooseSkill(skillContext);
            i = skillContext.shirokoTerror_SkillID;
        }
        return i;
    }
    public void CastSkill()
    {
        ExecuteActiveSkill();
    }
    public CharacterCTRL GetTargetCTRL()
    {
        if (Target != null && Target.activeInHierarchy)
        {
            return Target.GetComponent<CharacterCTRL>();
        }
        else
        {
            CustomLogger.LogWarning(this, "Getting null target");
            return null;
        }

    }
    public GameObject GetTarget()
    {
        if (Target != null && Target.activeInHierarchy)
        {
            return Target;
        }
        else
        {
            CustomLogger.LogWarning(this, "Getting null target");
            return PreTarget;
        }
    }
    public void AfterCastSkillAnimatorEvent()
    {
        IsCastingAbility = false;
        if (Target == null)
        {
            return;
        }
        if ((Target.transform.position - transform.position).magnitude >= GetStat(StatsType.Range) + 0.1f)
        {
            Target = null;
            PreTarget = null;
        }
    }
    public void AfterCastSkill()
    {
        Amulet_WatchObserverManaRestoreAmount = 0;
        foreach (var item in observers)
        {
            item.OnSkillFinished(this);

        }
        ReleaseLockDirection();
        ManaLock = false;
        customAnimator.AfterCastSkill();
        effectCTRL.OnParentCastSkillFinished();
        IsCastingAbility = false;
        StealManaCount = 0;
    }
    public bool IsCasting() => customAnimator.animator.GetBool("CastSkill");
    public bool EquipItem(IEquipment equipment)
    {
        CustomLogger.Log(this, $"EquipItem {equipment.EquipmentName}");
        if (equipment is SpecialEquipment specialEquipment)
        {
            Traits traits = specialEquipment.trait;
            foreach (var item in equipmentManager.GetEquippedItems())
            {
                if (item is SpecialEquipment)
                {
                    PopupManager.Instance.CreatePopup("已經有一個轉學證明了", 2);
                    return false;
                }
            }
            if (traitController.HasTrait(traits))
            {
                PopupManager.Instance.CreatePopup("本校學生無法配戴", 2);
                return false;
            }
        }
        if (equipment is ConsumableItem consumable)
        {
            CustomLogger.Log(this, $"{consumable} working");
            consumable.OnActivated();
            PopupManager.Instance.CreatePopup("觸發成功", 2);
            return false;
        }
        bool result = equipmentManager.EquipItem(equipment);

        if (result)
        {
            UpdateEquipmentUI();
            ResourcePool.Instance.ally.UpdateTraitEffects();
        }
        return result;
    }
    public void UpdateEquipmentUI()
    {
        characterBars.UpdateEquipmentDisplay(equipmentManager.GetEquippedItems());
    }
    #endregion

    #region Targeting and Pathfinding
    public bool FindTarget()
    {
        var hitColliders = Physics.OverlapSphere(transform.position, 50, GetTargetLayer());
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.TryGetComponent(out CharacterCTRL C) || !C.CurrentHex.IsBattlefield) continue;
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            if (C.isTargetable && !C.characterStats.logistics && distance < closestDistance)
            {
                closestTarget = hitCollider.gameObject;
                closestDistance = distance;
            }
        }
        bool found = UpdateTarget(closestTarget, closestDistance);
        return found;
    }
    public bool CheckEnemyIsInrange(CharacterCTRL enemy)
    {
        if (!enemy.gameObject.TryGetComponent(out CharacterCTRL C) || !C.CurrentHex.IsBattlefield) return false;
        float distance = Vector3.Distance(transform.position, enemy.transform.position);
        if (distance < GetStat(StatsType.Range) + 0.1f)
        {
            return true;
        }
        return false;
    }
    private bool UpdateTarget(GameObject closestTarget, float closestDistance)
    {
        if (closestTarget != null)
        {
            if (closestDistance <= GetStat(StatsType.Range) + 0.1f)
            {
                PreTarget = null;
                Target = closestTarget;
            }
            else
            {
                Target = null;
                PreTarget = closestTarget;
                if (!isWalking && !isFindingPath && !IsCastingAbility)
                {
                    TargetFinder();
                }
            }
            return true;
        }
        return false;
    }
    public void TargetFinder()
    {
        if (isFindingPath)
        {
            return;
        }
        isFindingPath = true;
        HexNode targetNode = PreTarget?.GetComponent<CharacterCTRL>().CurrentHex ??
                             Target?.GetComponent<CharacterCTRL>().CurrentHex;
        HexNode startNode = CurrentHex;
        if (startNode == null)
        {
            isFindingPath = false;
            return;
        }
        if (targetNode == null)
        {
            isFindingPath = false;
            return;
        }
        PathRequestManager.Instance.RequestPath(this, startNode, targetNode, OnPathFound, (int)stats.GetStat(StatsType.Range));
    }

    private void OnPathFound(List<HexNode> path)
    {
        isFindingPath = false;
        if (Target == null && PreTarget != null && !isWalking)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }


    private IEnumerator MoveAlongPath(List<HexNode> path)
    {
        isWalking = true;
        customAnimator.ChangeState(CharacterState.Moving);
        HexNode previousNode = null;
        int currentNodeIndex = -1;
        for (int i = 0; i < path.Count; i++)
        {
            var node = path[i];
            Vector3 targetPos = node.Position + offset;
            yield return MoveTowardsPosition(targetPos);
            if (previousNode != null)
            {
                previousNode.SetOccupyingCharacter(null);
                previousNode.Release();
            }
            previousNode = CurrentHex;
            CurrentHex = node;
            CurrentHex.SetOccupyingCharacter(this);
            currentNodeIndex = i;
            if (IsTargetInRange())
            {
                currentNodeIndex = i;
                break;
            }
        }
        PathRequestManager.Instance.ReleaseCharacterReservations(this);
        isWalking = false;
        FindTarget();
    }





    public IEnumerator MoveTowardsPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 1f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPos - transform.position), Time.deltaTime * 5);

            HexNode newGrid = SpawnGrid.Instance.GetHexNodeByPosition(targetPos - offset);
            if (newGrid != null)
            {
                MoveToNewGrid(newGrid);
            }

            yield return null;
        }
        transform.position = targetPos;
    }

    public void ForceChangeTarget(CharacterCTRL newTarget)
    {
        if (Taunted) return;
        Target = newTarget.gameObject;
        PreTarget = null;
        transform.LookAt(Target.transform.position);
    }
    public void MoveToNewGrid(HexNode newGrid)
    {
        OnGridChanged?.Invoke(CurrentHex, newGrid);
        CurrentHex = newGrid;
    }


    private bool IsTargetInRange()
    {
        if (GetEnemies().Count == 0) return false;
        float closestDistance = Mathf.Min(GetEnemies().Min(e => Vector3.Distance(transform.position, e.transform.position)), int.MaxValue);
        bool inRange = closestDistance <= GetStat(StatsType.Range) + 0.1f ||
                       (PreTarget != null && Vector3.Distance(transform.position, PreTarget.transform.position) <= GetStat(StatsType.Range) + 0.1f);
        return inRange;
    }

    #endregion

    #region Damage and Death
    public virtual void Executed(CharacterCTRL sourceCharacter, string detailedSource)
    {
        if (IsDying) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Weak, 9999, screenPos, false);
        IsDying = true;
        sourceCharacter.traitController.NotifyOnKilledEnemy(detailedSource, this);
        Die();

    }
    public virtual void GetHitByTrueDamage(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {

        if (!CanTakeDamage()) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        if (isCrit)
        {
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Weak, amount, screenPos, false);
        }
        if (WakamoMark)
        {
            dmgRecivedOnWakamoMarked += (int)(amount * wakamoMarkRatio);
        }
        CustomLogger.Log(this, $"{name} get hit by {sourceCharacter}'s {detailedSource} with {amount} iscrit:{isCrit}");
        while (amount > 0 && shields.Count > 0)
        {
            Shield shield = shields[0];
            if (shield.amount >= amount)
            {
                TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, amount, screenPos, false);
                shield.amount -= amount;
                AddStat(StatsType.Shield, -amount);
                amount = 0;
            }
            else
            {
                TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, shield.amount, screenPos, false);
                amount -= shield.amount;
                AddStat(StatsType.Shield, -shield.amount);
                shields.RemoveAt(0);
            }
        }

        if (amount > 0)
        {
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.None, amount, screenPos, true);
            AddStat(StatsType.currHealth, -amount);
        }
        foreach (var item in observers)
        {
            item.GetHit(this, sourceCharacter, amount, isCrit, detailedSource);
        }
        traitController.NotifyGetHit(this, sourceCharacter, amount, isCrit, detailedSource);
        equipmentManager.OnParentGethit(this, sourceCharacter, amount, isCrit, detailedSource);
        if (CheckDeath() && !IsDying)
        {
            sourceCharacter.traitController.NotifyOnKilledEnemy(detailedSource, this);
            Die();

        }
    }
    public bool Dragable()
    {
        return !(enterBattle||GameStageManager.Instance.CurrGamePhase == GamePhase.Battling);
    }
    public virtual void GetHit(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {
        if (IsDying || (characterStats.TestEnhanceSkill && IsAlly) || characterStats.TestBuildInvinvicble) return;//TODO:正式版記得拔掉
        if (!isAlive) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        int rand = UnityEngine.Random.Range(1, 100);
        if (GetStat(StatsType.DodgeChance) >= rand)
        {
            AudioManager.PlayDodgedSound();
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Miss, 0, screenPos, false);
            CustomLogger.Log(this, $"{sourceCharacter} attack got dodged");
            return;
        }
        int finalAmount = ObserverDamageModifier(amount, sourceCharacter, detailedSource, isCrit);
        finalAmount = BeforeDealtDmg(finalAmount, sourceCharacter, detailedSource, isCrit);
        float r = GetStat(StatsType.Resistence);
        float ratio = r / (100 + r);
        finalAmount = (int)(finalAmount * (1 - ratio));
        
        finalAmount = traitController.ModifyDamageTaken(finalAmount, sourceCharacter, detailedSource, isCrit);
        finalAmount = (int)(finalAmount * (1 - GetStat(StatsType.PercentageResistence) / 100f));
        finalAmount = (int)MathF.Max(finalAmount, 1);
        bool getEffect = false;
        if (isCrit)
        {
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Weak, finalAmount, screenPos, false);
            getEffect = true;
        }
        if (WakamoMark)
        {
            dmgRecivedOnWakamoMarked += (int)(finalAmount * wakamoMarkRatio);
        }
        CustomLogger.Log(this, $"{name} get hit by {sourceCharacter}'s {detailedSource} with {amount} iscrit:{isCrit}");
        while (finalAmount > 0 && shields.Count > 0)
        {
            Shield shield = shields[0];
            if (shield.amount >= finalAmount)
            {
                if (!getEffect) TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, finalAmount, screenPos, false);
                shield.amount -= finalAmount;
                AddStat(StatsType.Shield, -finalAmount);
                finalAmount = 0;
            }
            else
            {
                if (!getEffect) TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, shield.amount, screenPos, false);
                finalAmount -= shield.amount;
                AddStat(StatsType.Shield, -shield.amount);
                shields.RemoveAt(0);
            }
        }

        if (finalAmount > 0)
        {
            if (!getEffect) TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.None, finalAmount, screenPos, true);
            AddStat(StatsType.currHealth, -finalAmount);
        }
        foreach (var item in observers)
        {
            item.GetHit(this, sourceCharacter, finalAmount, isCrit, detailedSource);
        }
        traitController.NotifyGetHit(this, sourceCharacter, finalAmount, isCrit, detailedSource);
        equipmentManager.OnParentGethit(this, sourceCharacter, finalAmount, isCrit, detailedSource);
        if (CheckDeath() && !IsDying)
        {
            sourceCharacter.traitController.NotifyOnKilledEnemy(detailedSource, this);
            Die();

        }
    }
    public int ObserverDamageModifier(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {
        foreach (var item in observers)
        {
            amount = item.DamageModifier(sourceCharacter, this, amount, detailedSource, isCrit);
        }
        amount = sourceCharacter.traitController.ObserverDamageModifier(sourceCharacter, this, amount, detailedSource, isCrit);
        amount = sourceCharacter.equipmentManager.ObserverDamageModifier(sourceCharacter, this, amount, detailedSource, isCrit);
        if (amount <= 0)
        {
            amount = 0;
        }
        return amount;
    }

    public int BeforeDealtDmg(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {
        int totalAdjustedAmount = 0;
        foreach (var item in observers)
        {
            totalAdjustedAmount += item.BeforeDealtDmg(sourceCharacter, this, amount, detailedSource, isCrit);
        }
        totalAdjustedAmount += sourceCharacter.traitController.BeforeDealtDamage(sourceCharacter, this, amount, detailedSource, isCrit);
        totalAdjustedAmount += sourceCharacter.equipmentManager.BeforeDealtDamage(sourceCharacter, this, amount, detailedSource, isCrit);
        amount -= totalAdjustedAmount;
        if (amount <= 0)
        {
            amount = 0;
        }
        return amount;
    }
    public void AddShield(int amount, float duration, CharacterCTRL source)
    {
        Shield newShield = new Shield(amount, duration);
        shields.Add(newShield);
        shields.Sort((a, b) => a.remainingTime.CompareTo(b.remainingTime));
        CustomLogger.Log(this, $"{source} Shield {name} of {amount}");
        AddStat(StatsType.Shield, amount);
    }
    public void ApplyFear(CharacterCTRL source, float duration)
    {
        if (IsFeared)
        {
            fearDuration += duration;
            return;
        }
        IsFeared = true;
        fearSource = source;
        fearDuration = duration;
        CustomLogger.Log(this, $"進入恐懼狀態，恐懼來源: {fearSource.name}");

        // 可以用協程來控制恐懼時間
        StartCoroutine(FearRoutine());
    }

    private IEnumerator FearRoutine()
    {
        // 恐懼狀態持續
        float timer = 0f;
        while (timer < fearDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 恐懼結束
        IsFeared = false;
        fearSource = null;
        CustomLogger.Log(this, "恐懼狀態結束");

        // 先把角色拉回當前 Hex 的中心（如果因為誤差造成偏移）
        RecenterOnHex();

        // 恢復正常目標搜尋
        FindTarget();
    }

    public void RecenterOnHex()
    {
        if (CurrentHex != null)
        {
            transform.position = CurrentHex.Position + offset;
        }
    }
    public void Stun(bool s)
    {
        CustomLogger.Log(this, $"{characterStats.name} getting stuuned : {s}");
        if (isObj) return;
        stunned = s;
        if (stunned)
        {
            customAnimator.ChangeState(CharacterState.Stunned);
            customAnimator.animator.SetBool("StunFinished", false);
        }
        else
        {
            SetAnimatorStateToAttack();
            customAnimator.animator.SetBool("StunFinished", true);
        }
    }
    public void SetWakamoMark(int ratio, CharacterCTRL wakamo)
    {
        WakamoMark = true;
        wakamoMarkRatio = ratio * 0.01f;
        WakamoMarkParent = wakamo;
    }
    public void OnWakamoMarkCountedDown()
    {
        WakamoMark = false;
        wakamoMarkRatio = 0;
        GetHit(dmgRecivedOnWakamoMarked, WakamoMarkParent, DamageSourceType.Skill.ToString(), false);
        WakamoMarkParent = null;
        dmgRecivedOnWakamoMarked = 0;

    }
    public void Clarity()
    {
        effectCTRL.ClearEffects(EffectType.Negative);
        isCCImmune = true;
    }
    public void SetCCImmune(bool immune)
    {
        isCCImmune = immune;
    }
    public void SetMarked(bool marked)
    {

        isMarked = marked;
        Debug.Log($"{name} is {(isMarked ? "marked" : "no longer marked")}.");
        if (marked)
        {
            foreach (var item in GetEnemies())
            {
                if ((item.transform.position - transform.position).magnitude <= item.GetStat(StatsType.Range) + 2 && !item.characterStats.logistics)
                {
                    item.UpdateTarget(gameObject, (item.transform.position - transform.position).magnitude);
                }
            }
        }
    }
    public void ModifyStats(StatsType statsType, float amount, string source = null)
    {
        AddStat(statsType, amount);
        if (source != null)
        {
            CustomLogger.Log(this, $"source = {source} , modifing {statsType} to {GetStat(statsType)} by added {amount}");
        }
    }
    public void ModifyMultipleStats(Dictionary<StatsType, float> statsModifiers, string source = null, bool isRevert = false)
    {
        foreach (var modifier in statsModifiers)
        {
            float valueToApply = isRevert ? -modifier.Value : modifier.Value;
            AddStat(modifier.Key, valueToApply);

            if (source != null)
            {
                CustomLogger.Log(this,
                    $"source = {source}, modifying {modifier.Key} to {GetStat(modifier.Key)} by {(isRevert ? "removing" : "adding")} {Math.Abs(modifier.Value)}");
            }
        }
    }

    private bool CheckDeath()
    {
        if (GetStat(StatsType.currHealth) <= 0.5f)
        {
            isAlive = false;
            ShutdownEverything();
            return true;
        }
        return false;
    }
    public void ShutdownEverything()
    {
        PathRequestManager.Instance.ReleaseCharacterReservations(this);
        FearManager.Instance.RemoveCharacterFromFear(this);

        foreach (var item in SpawnGrid.Instance.hexNodes.Values)
        {
            if (item.IsReservedBy(this))
            {
                item.HardRelease();
            }

        }
        StopAllCoroutines();


    }
    public void TriggerCharacterStart()
    {
        traitController.TriggerCharacterStart();
        equipmentManager.TriggerCharacterStart();
        foreach (var item in observers)
        {
            item.CharacterStart(this);
        }
    }
    public void TriggerManualUpdate()
    {
        ResourcePool.Instance.ally.TriggerManualUpdate();
        ResourcePool.Instance.enemy.TriggerManualUpdate();
    }
    public virtual void Die()
    {
        SetStat(StatsType.currHealth, 0);
        Debug.Log($"{gameObject.name} Die()");
        SpawnGrid.Instance.RemoveCenterPoint(CurrentHex);
        foreach (var item in observers)
        {
            item.OnDying(this);
        }
        IsDying = true;
        customAnimator.ForceDying();
        CurrentHex.OccupyingCharacter = null;
        isTargetable = false;
        CurrentHex.HardRelease();
        TriggerManualUpdate();
    }
    public void BattleOverTime()
    {
        Overtime = true;
        AntiHeal = true;
        AddStat(StatsType.PercentageResistence, -20);
        AntiHealRatio = 0.8f;
    }
    public void MarkedByWakamoStart()
    {

    }
    public void WakamoMarkEnd()
    {

    }
    public SkillContext GetSkillContext()
    {
        UpdateSkillContext();
        return SkillContext;
    }
    public void UpdateSkillContext()
    {
        CharacterCTRL c;
        if (Target != null)
        {
            c = Target.GetComponent<CharacterCTRL>();
        }
        else
        {
            c = null;
        }
        SkillContext = new SkillContext
        {
            Parent = this,
            Enemies = GetEnemies(),
            Allies = GetAllies(),
            hexMap = SpawnGrid.Instance.hexNodes.Values.ToList(),
            Range = 5,
            duration = 2f,
            currHex = CurrentHex,
            CurrentTarget = c,
            CharacterLevel = star
        };

    }
    #endregion

    #region Layer and Team Operations
    public LayerMask GetTargetLayer() => IsAlly ? enemyLayer : allyLayer;

    public LayerMask GetAllyLayer() => IsAlly ? allyLayer : enemyLayer;
    public List<CharacterCTRL> GetEnemies()
    {
        List<CharacterCTRL> enemies = new List<CharacterCTRL>();
        var characterPool = !IsAlly ? ResourcePool.Instance.ally.childCharacters : ResourcePool.Instance.enemy.childCharacters;

        foreach (var character in characterPool)
        {
            CharacterCTRL characterCtrl = character.GetComponent<CharacterCTRL>();

            if (characterCtrl.CurrentHex.IsBattlefield && characterCtrl.gameObject.activeInHierarchy && !characterCtrl.IsDying && !characterCtrl.characterStats.logistics && characterCtrl.isTargetable)
            {
                enemies.Add(characterCtrl);

            }
        }
        return enemies;
    }

    public List<CharacterCTRL> GetAllies()
    {
        List<CharacterCTRL> allies = new List<CharacterCTRL>();
        var characterPool = IsAlly ? ResourcePool.Instance.ally.childCharacters : ResourcePool.Instance.enemy.childCharacters;

        foreach (var character in characterPool)
        {
            CharacterCTRL characterCtrl = character.GetComponent<CharacterCTRL>();

            if (characterCtrl.CurrentHex.IsBattlefield && characterCtrl.gameObject.activeInHierarchy && !characterCtrl.characterStats.logistics)
            {
                allies.Add(characterCtrl);
            }
        }

        return allies;
    }
    public Vector3 GetCollidPos() => DetectedPos;
    #endregion

    #region Behavior and Skill Management
    public Dictionary<int, Func<CharacterObserverBase>> characterBehaviors = new()
    {
        {  2, () => new AyaneObserver()},
        {  9, () => new SerinaObserver()},
        { 10, () => new ShizukoObserver()},
        { 12, () => new AkoObserver()},
        { 15, () => new AyaneObserver()},
        { 16, () => new IzunaObserver()},
        { 22, () => new ShirokoObserver()},

        { 25, () => new HinaObserver()},
        { 26, () => new HoshinoObserver()},
        { 27,() => new MikaObserver()},
        { 29, () => new TsurugiObserver()},
        { 31, () => new Shiroko_Terror_Observer()},
        { 34,() => new MisakiObserver()},
        { 36,()=> new MiyuObserver()}
    };

    public Dictionary<int, Func<CharacterSkillBase>> characterSkills = new()
    {
        {  0, () => new NullSkill()},
        {  1, () => new ArisSkill()},
        {  2, () => new AyaneSkill()},
        {  3, () => new HarukaSkill()},
        {  4, () => new HarunaSkill()},
        {  5, () => new MichiruSkill()},
        {  6, () => new NatsuSkill()},
        {  7, () => new NoaSkill()},
        {  8, () => new SerikaSkill()},
        {  9, () => new SerinaSkill()},
        { 10, () => new ShizukoSkill()},
        { 11, () => new SumireSkill()},
        { 12, () => new AkoSkill()},
        { 13, () => new AzusaSkill()},
        { 14, () => new ChiseSkill()},
        { 15, () => new FuukaSkill()},
        { 16, () => new IzunaSkill()},
        { 17, () => new KayokoSkill()},
        { 18, () => new KazusaSkill()},
        { 19, () => new MineSkill()},
        { 20, () => new MomoiSkill()},
        { 21, () => new NonomiSkill()},
        { 22, () => new ShirokoSkill()},
        { 23, () => new TsubakiSkill()},
        { 24, () => new YuukaSkill()},
        { 25, () => new HinaSkill()},
        { 26, () => new HoshinoSkill()},
        { 27, () => new MikaSkill()},
        { 28, () => new NeruSkill()},
        { 29, () => new TsurugiSkill()},
        { 30, () => new WakamoSkill()},
        { 31, () => new Shiroko_TerrorSkill()},
        { 32, () => new Atsuko_Skill()},
        { 33, () => new Hiyori_Skill()},
        { 34, () => new Misaki_Skill()},
        { 35, () => new Miyako_Skill()},
        { 36, () => new Miyu_Skill()},
        { 37, () => new Moe_Skill()},
        { 38, () => new Saki_Skill()},
        { 39, () => new Saori_Skill()},
        { 40, () => new Toki_Skill()},
        {  504, () => new HarunaSkill()},
    };
    #endregion

}

