
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterCTRL : MonoBehaviour
{
    #region === Grid / Target ===
    public HexNode CurrentHex;
    public HexNode HexWhenBattleStart;

    public GameObject _target;
    public GameObject Target
    {
        get
        {
            return _target;
        }
        set
        {
            string oldName = _target != null ? _target.name : "null";
            string oldHex = _target != null ? _target.GetComponent<CharacterCTRL>()?.CurrentHex?.ToString() : "null";
            string newName = value != null ? value.name : "null";
            string newHex = value != null ? value.GetComponent<CharacterCTRL>()?.CurrentHex?.ToString() : "null";
            CustomLogger.Log(this, $"[Setter] {gameObject.name} 的Target 由 `{oldName}` at {oldHex} 改為 `{newName}` at {newHex}");
            _target = value;
        }
    }
    public GameObject PreTarget;

    public delegate void GridChanged(HexNode oldTile, HexNode newTile);
    public event GridChanged OnGridChanged;

    private bool isFindingPath = false;
    private readonly Vector3 offset = new Vector3(0, 0.14f, 0);
    public bool IsDragable = true;
    #endregion

    #region === Team / Layer ===
    public bool IsAlly;
    public LayerMask allyLayer;
    public LayerMask enemyLayer;
    public LayerMask GridLayer;
    #endregion

    #region === Core Character / Stats ===
    public Character characterStats;
    public int star = 1;

    public StatsContainer stats;
    public StatsContainer ExtraPernamentStats = new StatsContainer();
    public Dictionary<string, (StatsType stat, float amount)> ExtraPernamentStatDict = new();
    public StatsContainer PercentageStats = new StatsContainer();
    public Dictionary<string, (StatsType fromStat, StatsType toStat, float amount)> PercentageStatsDict = new();

    public int DealtDamageThisRound;
    public int TakeDamageThisRound;
    public int AkoAddedCrit;
    public int SpecialSkillStack;
    public int critTransferAmount;
    readonly int crtiTranferRatio = 70;
    public int StealManaCount;
    #endregion

    #region === Gameplay Components ===
    public CharacterBars characterBars;
    public ModifierCTRL modifierCTRL;
    public EffectCTRL effectCTRL;
    public CharacterEquipmentManager equipmentManager;
    public TraitController traitController;
    public CustomAnimatorController customAnimator;
    public CharacterAudioManager AudioManager;
    public List<CharacterObserverBase> observers = new();
    public CharacterObserverBase characterObserver;
    #endregion

    #region === Combat State ===
    public bool isAlive = true;
    public bool enterBattle = false;
    public bool isWalking = false;
    public bool IsDying = false;
    public bool IsCastingAbility = false;
    public bool IsHeroicEnhanced = false;
    public bool ManaLock = false;
    public bool stunned = false;
    public bool isMarked = false;
    public bool Taunted = false;
    public bool Invincible = false;
    public bool isCCImmune = false;
    public bool AntiHeal = false;
    public float AntiHealRatio = 0;
    public bool isTargetable = true;
    public bool FaceDirectionLock = false;
    private bool tempDirectionLock = false;
    public bool Undying = false;
    public bool CanWalk = true;
    public bool CanAttack = true;
    public bool InStasis = false;
    public bool isAugment125Reinforced = false;
    #endregion

    #region === Status Effects / Special Conditions ===
    public bool WakamoMark = false;
    private float wakamoMarkRatio;
    public int dmgRecivedOnWakamoMarked;
    private CharacterCTRL WakamoMarkParent;

    public bool IsFeared { get; set; }
    private CharacterCTRL fearSource = null;
    private float fearDuration = 0;
    public Coroutine fearCorutine;

    private List<Shield> shields = new();
    #endregion

    #region === Combat / Skill ===
    private float attackTimer = 0f;
    private float attackSpeed = 1.0f;

    public CharacterSkillBase ActiveSkill;
    public SkillContext SkillContext;
    public Vector3 DetectedPos = Vector3.zero;

    private int Amulet_WatchObserverManaRestoreAmount;
    #endregion

    #region === Animation / FX ===
    public Transform FirePoint;
    public Transform GetHitPoint;
    public GameObject Halo;
    #endregion

    #region === Ownership / Context ===
    public CharacterParent allyParent;
    public CharacterParent enemyParent;
    public bool isObj;
    public GameObject Logistic_dummy;
    public bool isShirokoTerror;
    public Shiroko_Terror_DroneCTRL droneCTRL;
    public int RengeSkillAmount;
    #endregion

    #region === Coroutine Handles ===
    private Coroutine moveCoroutine;
    private Coroutine movingCoroutine;
    #endregion

    #region === Material / Rendering ===
    public GameObject OGBody;
    public Material[] materials;
    public Material[] OGMat;
    #endregion
    #region Unity Lifecycle Methods
    public void ResetToBeforeBattle()
    {
        GetComponent<BoxCollider>().enabled = true;
        if (fearCorutine != null)
        {
            StopCoroutine(fearCorutine);
        }

        if (customAnimator != null)
        {
            customAnimator.ForceIdle();
        }

        enterBattle = false;
        FaceDirectionLock = false;
        tempDirectionLock = false;
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
        attackTimer = 0f;
        WakamoMark = false;
        stunned = false;
        isTargetable = true;
        fearSource = null;
        fearDuration = 0;
        effectCTRL.ClearAllEffect();
        SetStat(StatsType.Mana, 0);
        int health = (int)GetStat(StatsType.Health);
        SetStat(StatsType.currHealth, health);
    }
    public virtual void OnEnable()
    {
        if (star <= 1) star = 1;
        GetComponent<BoxCollider>().enabled = true;
        allyParent = ResourcePool.Instance.ally;
        enemyParent = ResourcePool.Instance.enemy;
        modifierCTRL = GetComponent<ModifierCTRL>();
        effectCTRL = GetComponent<EffectCTRL>();
        effectCTRL.characterCTRL = GetComponent<CharacterCTRL>();
        traitController = GetComponent<TraitController>();
        isTargetable = true;
        Invincible = false;
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
            if (AddObserver(observer))
            {
                characterObserver = observer;
            }
        }
        GlobalBaseObserver globalObserver = new GlobalBaseObserver();
        AddObserver(globalObserver);
        if (characterSkills.TryGetValue(characterId, out var characterSkillFunc))
        {
            var baseSkill = characterSkillFunc();
            if (GameController.Instance.CheckCharacterEnhance(characterStats.CharacterId, IsAlly))
            {
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
        if (GameController.Instance.CheckCharacterEnhance(19, true) && characterId == 19)
        {
            GetComponent<SkillAnimationReplacer>().SwitchToEnhancedSkill();
        }
        if (OGBody.TryGetComponent<SkinnedMeshRenderer>(out var t))
        {
            materials = t.materials;
            OGMat = t.materials;
        }


        OnCharaterEnabled();
        RecalculateStats();
    }
    public void OnCharaterEnabled()
    {
        foreach (var item in observers)
        {
            item.OncharacterEnabled(this);
        }
    }
    public void GetSkill()
    {
        int characterId = characterStats.CharacterId;
        if (characterSkills.TryGetValue(characterId, out var characterSkillFunc))
        {
            CustomLogger.Log(this, $"characterId {characterId} getting {characterSkillFunc} TestEnhanceSkill");
            var baseSkill = characterSkillFunc();
            if (GameController.Instance.CheckCharacterEnhance(characterId, IsAlly))
            {
                CustomLogger.Log(this, $"characterId {characterId} getting {characterSkillFunc} TestEnhanceSkill");
                ActiveSkill = baseSkill.GetHeroicEnhancedSkill();
            }
            else
            {
                ActiveSkill = baseSkill;
            }
        }
    }
    public void OnDisable()
    {
        if (OGBody.TryGetComponent<SkinnedMeshRenderer>(out var t))
        {
            t.materials = OGMat;
        }
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
    public void RemoveHealth(int amount)
    {
        CustomLogger.Log(this, $"Remove {amount} health");
        AddStat(StatsType.currHealth, -amount);
        if (CheckDeath() && !IsDying)
        {
            Die();
        }
    }
    private bool CanTakeDamage()
    {
        return isAlive
            && !IsDying
            && !(characterStats.TestEnhanceSkill)
            && !characterStats.TestBuildInvinvicble
            && !Invincible
            && !characterStats.logistics;
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
    private void Start()
    {
        allyLayer = 1 << LayerMask.NameToLayer("Ally");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        GridLayer = 1 << LayerMask.NameToLayer("Grid");
        customAnimator = GetComponent<CustomAnimatorController>();
        TriggerCharacterStart();
        TriggerManualUpdate();
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
            customAnimator.TryGetBool(customAnimator.animator, "CastSkill", out bool val);
            bool canAttack = !val && !isWalking;
            if (enterBattle)
            {
                if (CurrentHex.IsDesertified())
                {
                    CustomLogger.Log(this, "CurrentHex.isDesertified");
                    bool isAbydos = traitController.GetAcademy() == Traits.Abydos;
                    Effect effect = EffectFactory.CreateAbydosEffect(isAbydos, AbydosManager.Instance.level, IsAlly);
                    CustomLogger.Log(this, $"isabydos = {isAbydos}, effect = {effect.Source}");
                    effectCTRL.AddEffect(effect, this);
                    if (SelectedAugments.Instance.CheckAugmetExist(106, IsAlly) && !effectCTRL.HaveEffect("AbydosMark"))
                    {
                        Effect effect1 = EffectFactory.CreateStunEffect(5f, this);
                        Effect effect2 = EffectFactory.AbydosEnhancedMark();
                        effectCTRL.AddEffect(effect1, this);
                        effectCTRL.AddEffect(effect2, this);
                    }

                }
                if (CurrentHex.oasis)
                {
                    Effect effect = EffectFactory.CreateAbydosMillenniumEffect(AbydosManager.Instance.level);
                    effectCTRL.AddEffect(effect, this);
                }
            }
            if (enterBattle)
            {
                DetectedPos = CurrentHex.transform.position + new Vector3(0, 0.3f, 0);
                customAnimator.TryGetBool(customAnimator.animator, "CastSkill", out bool b);
                if (!(stunned || IsFeared || b || effectCTRL.HaveEffect("Stun") || IsCastingAbility))
                {
                    HandleTargetFinding();
                    HandleAttack(canAttack);
                }
                CheckEveryThing();
                foreach (var hitCollider in Physics.OverlapSphere(transform.position, 0.2f, GridLayer))
                {
                    if (isAlive)
                    {
                        CurrentHex = hitCollider.GetComponent<HexNode>();
                    }

                }
            }
        }


        if (Target != null && (Target.GetComponent<CharacterCTRL>().IsDying || !Target.GetComponent<CharacterCTRL>().isAlive || !Target.activeInHierarchy) || effectCTRL.HaveEffect("Stun"))
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
    public void OnBattleStart()
    {
        foreach (var item in observers)
        {
            item.OnBattleStart();
        }
        if (traitController != null)
        {
            traitController.OnBattleStart();
        }
        if (equipmentManager != null)
        {
            equipmentManager.OnBattleStart();
        }
    }
    public void CharacterUpdate()
    {
        foreach (var item in observers)
        {
            item.CharacterUpdate();
        }
        if (traitController != null)
        {
            traitController.CharacterUpdate();
        }
        if (equipmentManager != null)
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
        if (!canAttack || IsCastingAbility || Target == null || !Target.activeInHierarchy || !CanAttack)
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
        if (characterStats.CharacterId == 41) time = 1f / attackSpeed;
        if (attackTimer >= time)
        {
            attackTimer = 0f;
        }
    }
    public void SetAnimatorStateToAttack()
    {
        if (IsCastingAbility || isWalking || isFindingPath || GameStageManager.Instance.CurrGamePhase == GamePhase.Preparing)
        {
            return;
        }
        customAnimator.ChangeState(CharacterState.Attacking);
    }
    public int ExecuteActiveSkill()
    {
        GetSkill();
        CharacterCTRL target = GetTargetCTRL();
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
            TargetCTRLPosition = target?.transform.position ?? Vector3.zero,
            posRecorded = target == null ? false : true,
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
        IsCastingAbility = true;
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

        if (Utility.Iscrit(GetStat(StatsType.CritChance), this))
        {
            dmg = (int)(dmg * (1 + GetStat(StatsType.CritRatio) * 0.01f));
            CustomLogger.Log(this, $"character {name} crit");
            iscrit = true;
        }
        return (iscrit, dmg);
    }
    public void Attack()
    {
        CustomLogger.Log(this, $"character {characterStats.CharacterName} Attack()");
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
            List<HitEffect> hitEffect = new List<HitEffect> { };
            if (GameController.Instance.CheckCharacterEnhance(30, true) && characterStats.CharacterId == 30)
            {
                hitEffect.Add(new WakamoEnhancedSkillEffect());
            }
            var bulletComponent = bullet.GetComponent<NormalBullet>();
            // if (!Target) return;
            int damage = (int)(GetStat(StatsType.Attack) * (1 + modifierCTRL.GetTotalStatModifierValue(ModifierType.DamageDealt) * 0.01f));
            (bool, int) tuple = CalculateCrit(damage);
            bool iscrit = tuple.Item1;
            damage = tuple.Item2;
            bulletComponent.Initialize(damage, GetTargetLayer(), this, 20, Target, false, iscrit, hitEffect);

        }

        if (Target)
        {
            transform.LookAt(Target.transform);
        }

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


    public void HandleAttacking(bool ChangeSpeed = true)
    {
        AudioManager.PlayOnAttack();
        if (ChangeSpeed) customAnimator.animator.speed = GetStat(StatsType.AttackSpeed);
        if (!ManaLock && characterStats.CharacterId != 41)
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
    public int GetHealAmount(int amount, CharacterCTRL source)
    {
        amount = BeforeHealing(amount, source);
        if (AntiHeal)
        {
            amount = (int)(amount * (1 - AntiHealRatio));
        }
        if (amount <= 1)
        {
            return 0;
        }
        return amount;
    }
    public void HealToFull(CharacterCTRL source)
    {
        int amount = (int)(GetStat(StatsType.Health) - GetStat(StatsType.currHealth));
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.None, amount, screenPos, false, true);
        foreach (var item in source.observers)
        {
            item.OnHealing(source);
        }
        if (traitController != null)
        {
            source.traitController.OnHealing();
        }
        if (equipmentManager != null)
        {
            source.equipmentManager.OnHealing();
        }
        AudioManager.PlayHpRestoredSound();
        AddStat(StatsType.currHealth, amount);
    }
    public virtual void Heal(int amount, CharacterCTRL source)
    {
        amount = (int)(amount * (1 + HealerManager.instance.amount * 0.01f));
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
        foreach (var item in source.observers)
        {
            item.OnHealing(source);
        }
        if (traitController != null)
        {
            source.traitController.OnHealing();
        }
        if (equipmentManager != null)
        {
            source.equipmentManager.OnHealing();
        }
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
    public void OnEnterGrid()
    {
        foreach (var item in observers)
        {
            item.OnEnterBattleField(this);
        }
        if (characterObserver != null)
        {
            characterObserver.OnEnterBattleField(this);
        }

        traitController.TriggerOnEnterBattleField();
        equipmentManager.TriggerOnEnterBattleField();
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
    public void SetTaunt(bool b)
    {
        if (!Taunted && isCCImmune) return;
        Taunted = b;
    }
    public void AbydosMillenniumBuff(int percent, bool start)
    {
        if (start)
        {
            int health = (int)(GetStat(StatsType.Health) * percent * 0.01f);
            Heal(health, this);
            Addmana(2);
        }
    }
    public void AbydosBuff(bool isAbydos, int data2, int data3, bool start)
    {
        AddAttackSpeed(data2 * 0.01f);
        if (start)
        {
            int health = (int)(GetStat(StatsType.Health) * (isAbydos ? 1 : -1) * data3 * 0.01f);
            AddStat(StatsType.currHealth, health);
            if (SelectedAugments.Instance.CheckAugmetExist(104, IsAlly) && IsAlly)
            {
                AbydosManager.Instance.AddSRTCounter(Math.Abs(health));
            }
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
    public bool AddObserver(CharacterObserverBase observer)
    {
        if (!observers.Contains(observer)) observers.Add(observer);
        return observers.Contains(observer);
    }

    public void RemoveObserver(CharacterObserverBase observer) => observers.Remove(observer);

    public void CheckEveryThing()
    {
        if (CheckDeath() && !IsDying)
        {
            Die();
            return;
        }
        customAnimator.TryGetBool(customAnimator.animator, "CastSkill", out bool val);
        if (GetStat(StatsType.Mana) >= GetStat(StatsType.MaxMana) && !val && !isWalking && !isObj && !Taunted)
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
        int animationIndex = 7;
        if (isShirokoTerror)
        {
            animationIndex = i + 8;
            customAnimator.animator.SetInteger("SkillID", i);
        }
        float sec1 = customAnimator.GetAnimationClipInfo(animationIndex).Item2;
        CustomLogger.Log(this, $"character {characterStats.name}animation sec = {sec1}");
        if (characterStats.CharacterId == 16)
        {
            sec1 /= GetStat(StatsType.AttackSpeed);
        }
        var clipInfoList = customAnimator.GetAllClipInfos();

        for (int j = 0; j < clipInfoList.Count; j++)
        {
            var (name, length) = clipInfoList[j];
            CustomLogger.Log(this, $"index = {j}, name = {name}, length = {length}");
        }
        StartCoroutine(CastSkillWaitTime(sec1));
    }
    public IEnumerator CastSkillWaitTime(float sec)
    {
        CustomLogger.Log(this, $"character {characterStats.name} wait sec = {sec}, time = {Time.time}");
        yield return new WaitForSeconds(sec);
        if (enterBattle)
        {
            SetAnimatorStateToAttack();
            customAnimator.animator.SetBool("CastSkill", false);
        }
        CustomLogger.Log(this, $"character {characterStats.name} after wait {sec}, time = {Time.time}");
        AfterCastSkill();
    }
    public float GetHealthPercentage() => GetStat(StatsType.currHealth) / (float)GetStat(StatsType.Health);
    public float GetExtraStat(StatsType statsType) => ExtraPernamentStats.GetStat(statsType);
    public float GetStat(StatsType statsType, bool check = true)
    {
        return stats.GetStat(statsType);
    }
    public void DodgeCorrection()
    {
        if (GetStat(StatsType.DodgeChance) >= 90)
        {
            SetStat(StatsType.DodgeChance, 90);
        }
    }
    public void LifeStealCorrection()
    {
        if ((int)GetStat(StatsType.Lifesteal) >= 100)
        {
            int amount = (int)GetStat(StatsType.Lifesteal) - 100;
            AddStat(StatsType.DamageIncrease, amount, false);
            SetStat(StatsType.Lifesteal, 100);
        }
    }
    public void CritCorrection()
    {
        int currentCritChance = (int)GetStat(StatsType.CritChance);
        float ratio = crtiTranferRatio * 0.01f;
        if (currentCritChance > 100)
        {
            int overflow = currentCritChance - 100;
            float addedCritRatio = overflow * ratio;
            AddStat(StatsType.CritRatio, addedCritRatio, false);
            SetStat(StatsType.CritChance, 100);
            critTransferAmount = overflow;

            CustomLogger.Log(this, $"Applied overflow: {overflow} overflow converted to {addedCritRatio} extra CritRatio.");
        }
        else // currentCritChance <= 100
        {
            int deficit = 100 - currentCritChance;
            float neededCritRatio = deficit * ratio;
            if (critTransferAmount > 0)
            {
                if (critTransferAmount >= deficit)
                {
                    float removalCritRatio = deficit * ratio;
                    AddStat(StatsType.CritRatio, -removalCritRatio, false);
                    SetStat(StatsType.CritChance, 100);
                    critTransferAmount -= deficit;
                    CustomLogger.Log(this, $"Restored full deficit: restored {deficit} CritChance by removing {removalCritRatio} CritRatio.");
                }
                else
                {
                    // 如果不足，則部分還原：依據現有轉換量還原對應的爆擊率
                    int restoredCritChance = critTransferAmount; // 已有的全部轉換點數都能還原
                    float removalCritRatio = restoredCritChance * ratio;
                    AddStat(StatsType.CritRatio, -removalCritRatio, false);
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
    public void AddStat(StatsType statsType, float amount, bool refresh = true)
    {
        if (statsType == StatsType.PercentageResistence)
        {
            CustomLogger.Log(this, $"modify StatsType.PercentageResistence for {amount}");
        }
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
        if (refresh)
        {
            RecalculateStats();
        }

    }
    public StatsContainer GetRawStats()
    {
        if (star <= 1) star = 1;
        int currHealth = (int)GetStat(StatsType.currHealth);
        int currMana = (int)GetStat(StatsType.Mana);
        stats = BuildBaseStats();
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Battling)
        {
            stats.SetStat(StatsType.currHealth, currHealth);
            stats.SetStat(StatsType.Mana, currMana);
        }
        else
        {
            stats.SetStat(StatsType.currHealth, GetStat(StatsType.Health, false));
        }
        return stats;
    }
    public virtual void RecalculateStats()
    {
        if (star <= 1) star = 1;

        int currHealth = (int)GetStat(StatsType.currHealth);
        int currMana = (int)GetStat(StatsType.Mana);
        stats = BuildBaseStats();
        stats.AddFrom(equipmentManager.GetEqStats());
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Battling)
        {
            stats.SetStat(StatsType.currHealth, currHealth);
            stats.SetStat(StatsType.Mana, currMana);
        }
        else
        {
            stats.SetStat(StatsType.currHealth, GetStat(StatsType.Health, false));
        }
        if (effectCTRL.GetStatsEffects().Count > 0)
        {
            foreach (var item in effectCTRL.GetStatsEffects())
            {
                item.OnApply.Invoke(this);
            }
        }
        if (SelectedAugments.Instance.CheckAugmetExist(101, IsAlly))
        {
            int id = characterStats.CharacterId;
            int partnerId = id == 25 ? 26 : id == 26 ? 25 : -1;
            if (partnerId != -1)
            {
                CharacterCTRL partner = ResourcePool.Instance.ally.GetStrongestCharacterByID(partnerId);
                if (partner != null)
                {
                    StatsContainer s = partner.GetRawStats().MultiplyBy(0.3f);
                    stats.AddFrom(s);
                }
            }
        }

        stats.AddFrom(GetPercentageBonus());
        CritCorrection();
        LifeStealCorrection();
        DodgeCorrection();
        if (GetStat(StatsType.AttackSpeed) >= 5)
        {
            SetStat(StatsType.AttackSpeed, 5);
        }
        if (GetStat(StatsType.MaxMana) <= 30)
        {
            SetStat(StatsType.MaxMana, 30);
        }
        if (GameController.Instance.CheckSpecificCharacterEnhanced(this, 43, IsAlly))
        {
            AddStat(StatsType.Range, 3, false);
        }
    }

    private StatsContainer BuildBaseStats()
    {
        int maxHealth = characterStats.Health[star - 1];
        int attack = characterStats.Attack[star - 1];

        StatsContainer result = characterStats.Stats.Clone();
        result.SetStat(StatsType.Health, maxHealth);
        result.SetStat(StatsType.Attack, attack);

        equipmentManager.UpdateEquipmentStats();
        result.AddFrom(ExtraPernamentStats);
        result.AddFrom(GameController.Instance.TeamExtraStats);

        if (traitController.HasTrait(Traits.SRT))
            result.AddFrom(BattlingProperties.Instance.GetSRTStats(IsAlly));

        if (traitController.HasTrait(Traits.Abydos) && SelectedAugments.Instance.CheckAugmetExist(104, IsAlly))
            result.AddFrom(BattlingProperties.Instance.GetSRTStats(IsAlly).MultiplyBy(0.5f));
        if (traitController.HasTrait(Traits.Arius) && SelectedAugments.Instance.CheckAugmetExist(117, IsAlly))
            result.AddFrom(BattlingProperties.Instance.GetSRTStats(IsAlly));
        if (traitController.HasTrait(Traits.Arius) && !SelectedAugments.Instance.CheckIfConditionMatch(107, IsAlly))
            result.AddFrom(BattlingProperties.Instance.GetSRTStats(IsAlly));
        if (traitController.HasTrait(Traits.Arius) && SelectedAugments.Instance.CheckAugmetExist(125, IsAlly))
            result.AddFrom(BattlingProperties.Instance.GetSRTStats(IsAlly));
        if (characterStats.CharacterId == 41 && SelectedAugments.Instance.CheckAugmetExist(114, IsAlly))
            result.AddFrom(GetComponent<Panchan_AnimatorCTRL>().GetExtraStats());
        if (SelectedAugments.Instance.CheckAugmetExist(107, IsAlly))
        {
            CharacterParent c = ResourcePool.Instance.ally;
            List<CharacterCTRL> Arius = c.GetCharacterWithTraits(Traits.Arius);
            List<CharacterCTRL> SRT = c.GetCharacterWithTraits(Traits.SRT);
            if (Utility.CompareTwoGroups(SRT, Arius))
            {
                result.AddFrom(BattlingProperties.Instance.GetSRTStats(IsAlly));
            }
        }


        return result;
    }

    public void AddPercentageBonus(StatsType fromStat, StatsType toStat, int percent, string identifier)
    {
        if (PercentageStatsDict.TryGetValue(identifier, out var val))
        {
            if (val.fromStat == fromStat && val.toStat == toStat && val.amount <=percent)
            {
                return;
            }
        }
        PercentageStatsDict[identifier] = (fromStat, toStat, percent);
        RecalculateStats();
    }

    public void RemovePercentageBonus(string identifier)
    {
        if (PercentageStatsDict.ContainsKey(identifier))
            PercentageStatsDict.Remove(identifier);
        RecalculateStats();
    }

    public StatsContainer GetPercentageBonus()
    {
        PercentageStats.Clear();
        if (PercentageStatsDict.Count == 0) return PercentageStats;
        foreach (var kvp in PercentageStatsDict)
        {
            var (fromStat, toStat, percent) = kvp.Value;
            if (fromStat == StatsType.Null)
            {
                float baseValue = GetStat(toStat);
                float bonusValue = baseValue * (percent / 100f);
                PercentageStats.AddValue(toStat, bonusValue);
            }
            else
            {
                float val = GetStat(fromStat) * (percent / 100f);
                PercentageStats.AddValue(toStat, val);
            }
        }
        return PercentageStats;
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
    public void AddExtraStat(StatsType statsType, float amount, string identefier, bool stackable)
    {
        if (ExtraPernamentStatDict.ContainsKey(identefier))
        {
            if (stackable)
            {
                ExtraPernamentStatDict[identefier] = (statsType, ExtraPernamentStatDict[identefier].amount + amount);
            }
            else
            {
                ExtraPernamentStatDict[identefier] = (statsType, amount);
            }
        }
        else
        {
            ExtraPernamentStatDict.Add(identefier, (statsType, amount));
        }
        foreach (var item in ExtraPernamentStatDict.Values)
        {
            ExtraPernamentStats.SetStat(item.stat, item.amount);
        }
        RecalculateStats();
    }
    public void SetStat(StatsType statsType, float amount)
    {

        stats.SetStat(statsType, amount);
    }

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
            TargetCTRLPosition = c?.transform.position ?? Vector3.zero,
            posRecorded = c == null ? false : true,
            Range = 5,
            duration = 2f,
            currHex = CurrentHex,
            TargetHex = GetTargetCTRL().CurrentHex,
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
        RemovePercentageBonus("HimariActiveSkill");
        IsCastingAbility = false;
        StealManaCount = 0;
    }
    public bool EquipItem(IEquipment equipment)
    {
        CustomLogger.Log(this, $"EquipItem {equipment.EquipmentName}");
        if (equipment is SpecialEquipment specialEquipment)
        {
            foreach (var item in equipmentManager.GetEquippedItems())
            {
                if (item is SpecialEquipment)
                {
                    PopupManager.Instance.CreatePopup("已經有一個轉學證明了", 2);
                    return false;
                }
            }
        }
        if (equipment is ConsumableItem consumable)
        {
            CustomLogger.Log(this, $"{consumable} working");
            consumable.OnActivated();
            PopupManager.Instance.CreatePopup("觸發成功", 2);
            BugReportLogger.Instance.EquipItemToCharacter(name, consumable.EquipmentName);
            return true;
        }
        bool result = equipmentManager.EquipItem(equipment);

        if (result)
        {
            BugReportLogger.Instance.EquipItemToCharacter(name, equipment.EquipmentName);
            UpdateEquipmentUI();
            ResourcePool.Instance.ally.UpdateTraitEffects();
        }
        return result;
    }
    public void UpdateEquipmentUI()
    {
        characterBars.UpdateEquipmentDisplay(equipmentManager.GetEquippedItems());
        RecalculateStats();
    }
    #endregion

    #region Targeting and Pathfinding
    public bool FindTarget()
    {
        if (characterStats.CharacterId == 30 && GameController.Instance.CheckCharacterEnhance(30, true))
        {
            CharacterParent characterParent = IsAlly ? ResourcePool.Instance.enemy : ResourcePool.Instance.ally;
            foreach (var item in characterParent.GetBattleFieldCharacter())
            {
                if (item.effectCTRL.GetEffect("WakamoEnhancedMark") == null)
                {
                    PreTarget = null;
                    Target = item.gameObject;
                    return true;
                }
            }
        }

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
            CustomLogger.Log(this, "UpdatingTarget");
            if (closestDistance <= GetStat(StatsType.Range) + 0.1f)
            {
                PreTarget = closestTarget;
                Target = closestTarget;
                CustomLogger.Log(this, "Attacking");
            }
            else
            {
                Target = null;
                PreTarget = closestTarget;

                if (movingCoroutine == null && !isFindingPath && !IsCastingAbility && CanWalk)
                {
                    CustomLogger.Log(this, "TargetFinder");
                    TargetFinder();
                }
            }
            return true;
        }
        return false;
    }
    public void TargetFinder()
    {
        if (isFindingPath && characterStats.isObj)
        {
            return;
        }
        isFindingPath = true;
        HexNode targetNode = GetTarget().GetComponent<CharacterCTRL>().CurrentHex;
        HexNode startNode = CurrentHex;
        if (startNode == null)
        {
            CustomLogger.LogWarning(this, "startNode is null");
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
        if (Target == null && PreTarget != null && !isWalking && path.Count > 0)
        {
            PathRequestManager.Instance.ReleaseReserve(this, path);
            CustomLogger.Log(this, $"{name} on path found");
            if (moveCoroutine != null)
            {
                PathRequestManager.Instance.HardReleaseReservation(this);
                StopCoroutine(moveCoroutine);

            }
            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }
            moveCoroutine = StartCoroutine(MoveAlongPath(path));
        }
    }



    private IEnumerator MoveAlongPath(List<HexNode> path)
    {
        if (this is StaticObject @static) yield break;
        isWalking = true;
        customAnimator.ChangeState(CharacterState.Moving);
        var node = path[0];
        Vector3 targetPos = node.Position + offset;
        yield return movingCoroutine = StartCoroutine(MoveTowardsPosition(targetPos));
        PathRequestManager.Instance.ReleaseCharacterReservations(this);
        PathRequestManager.Instance.HardReleaseReservation(this);
        isWalking = false;
        FindTarget();
        moveCoroutine = null;
        if (movingCoroutine != null)
        {
            transform.position = targetPos;
            StopCoroutine(movingCoroutine);
            movingCoroutine = null;
        }
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
        CurrentHex.Release();
        CurrentHex = SpawnGrid.Instance.GetHexNodeByPosition(targetPos - offset);
        CurrentHex.SetOccupyingCharacter(this);
    }
    public bool CheckDist(CharacterCTRL enemy)
    {
        float dist = Vector3.Distance(transform.position, enemy.transform.position);
        return dist <= GetStat(StatsType.Range) + 0.1f;
    }
    public void ForceChangeTarget(CharacterCTRL newTarget)
    {
        if (Taunted || !CheckDist(newTarget)) return;
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

        sourceCharacter.traitController.NotifyOnKilledEnemy(detailedSource, this);
        Die();

    }
    public virtual void GetHitByTrueDamage(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {

        if (!CanTakeDamage()) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);

        if (WakamoMark)
        {
            dmgRecivedOnWakamoMarked += (int)(amount * wakamoMarkRatio);
        }
        CustomLogger.Log(this, $"{name} get hit by {sourceCharacter}'s {detailedSource} with {amount} iscrit:{isCrit} ,trueDmg");
        while (amount > 0 && shields.Count > 0)
        {
            Shield shield = shields[0];
            if (shield.amount >= amount)
            {
                if (isCrit)
                {
                    TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Weak, amount, screenPos, false);
                }
                else
                {
                    TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, amount, screenPos, false);
                }

                shield.amount -= amount;
                AddStat(StatsType.Shield, -amount);
                amount = 0;
            }
            else
            {
                if (isCrit)
                {
                    TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Weak, amount, screenPos, false);
                }
                else
                {
                    TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, amount, screenPos, false);
                }
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
            item.GetHit(this, sourceCharacter, amount, isCrit, detailedSource, true);
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
        return !(enterBattle || GameStageManager.Instance.CurrGamePhase == GamePhase.Battling || !IsAlly || !IsDragable);
    }
    public bool Dodge(CharacterCTRL sourceCharacter)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        int accuracy = (int)sourceCharacter.GetStat(StatsType.Accuracy);
        int rand = UnityEngine.Random.Range(1, 100);
        if (GetStat(StatsType.DodgeChance) - accuracy >= rand)
        {
            AudioManager.PlayDodgedSound();
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Miss, 0, screenPos, false);
            foreach (var item in observers)
            {
                item.OnDodged(this);
            }
            traitController.Dodged();
            equipmentManager.OnParentDodged();
            CustomLogger.Log(this, $"{sourceCharacter} attack got dodged");
            return true;
        }
        return false;
    }
    public void OnCrit()
    {
        foreach (var item in observers)
        {
            CustomLogger.Log(this, $"observer = {item.GetType()}");
            item.OnCrit(this);
        }
        traitController.OnCrit();
        equipmentManager.OnCrit();
    }
    public virtual void GetHit(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit, bool recursion = true)
    {
        int rawdmg = amount;
        if (!CanTakeDamage()) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        if (Dodge(sourceCharacter)) return;

        int finalAmount = ObserverDamageModifier(amount, sourceCharacter, detailedSource, isCrit);
        finalAmount = BeforeDealtDmg(finalAmount, sourceCharacter, detailedSource, isCrit);
        float r = GetStat(StatsType.Resistence);
        if (sourceCharacter.characterStats.CharacterId == 39)
        {
            r *= 0.75f;
        }
        float mother = 0;
        if (r > 0) { mother = r; }
        float ratio = r / (100 + mother);
        finalAmount = (int)(finalAmount * (1 - ratio));
        int PercentageResistence = (int)GetStat(StatsType.PercentageResistence);
        if (sourceCharacter.characterStats.CharacterId != 39 && PercentageResistence > 0)
        {
            finalAmount = (int)(finalAmount * (1 - PercentageResistence / 100f));
        }
        finalAmount = (int)MathF.Max(finalAmount, 1);
        finalAmount = traitController.ModifyDamageTaken(finalAmount, sourceCharacter, detailedSource, isCrit, recursion);
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
        CustomLogger.Log(this, $"{name} get hit by {sourceCharacter}'s {detailedSource} with {finalAmount} iscrit:{isCrit} as {detailedSource},raw dmg = {rawdmg}");
        if (recursion)
        {
            foreach (var item in observers)
            {
                CustomLogger.Log(this, $"observer = {item.GetType()}");
                item.GetHit(this, sourceCharacter, finalAmount, isCrit, detailedSource, true);
            }
            traitController.NotifyGetHit(this, sourceCharacter, finalAmount, isCrit, detailedSource);
            equipmentManager.OnParentGethit(this, sourceCharacter, finalAmount, isCrit, detailedSource);
        }

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

        if (CheckDeath() && !IsDying)
        {
            sourceCharacter.traitController.NotifyOnKilledEnemy(detailedSource, this);
            Die();

        }
    }
    public int ObserverDamageModifier(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {
        if (sourceCharacter.characterObserver != null)
        {
            CustomLogger.Log(this, $"observer {sourceCharacter.characterObserver} modifiying");
            amount = sourceCharacter.characterObserver.DamageModifier(sourceCharacter, this, amount, detailedSource, isCrit);
        }
        foreach (var item in sourceCharacter.observers)
        {
            CustomLogger.Log(this, $" {sourceCharacter.name} DamageModifierObserver = {item.GetType()}");
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
    public int GetShieldStats()
    {
        int amount = 0;
        foreach (var item in shields)
        {
            amount += item.amount;
        }
        return amount;
    }
    public void AddShield(int amount, float duration, CharacterCTRL source)
    {
        amount = (int)(amount * (1 + HealerManager.instance.amount * 0.01f));
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
        if (!stunned && isCCImmune) return;
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

        EffectCanvas.Instance.GetOrCreateWakamoImage(this, ResourcePool.Instance.WakamosSprite);
        EffectCanvas.Instance.FadeToRedOverFiveSeconds(this);
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
        EffectCanvas.Instance.ResetImage(this, true);
    }
    public void OnWakamoEnhancedMarkEnd()
    {
        GetHit(dmgRecivedOnWakamoMarked, WakamoMarkParent, DamageSourceType.Skill.ToString(), false);
        dmgRecivedOnWakamoMarked = 0;
        CustomLogger.Log(this, "OnWakamoEnhancedMarkEnd()");
        EffectCanvas.Instance.ResetImage(this, false);
        if (enterBattle)
        {
            Effect effect = EffectFactory.WakamoEnhancedMark(this, 20);
            effectCTRL.AddEffect(effect, this);
        }
    }
    public void Clarity()
    {
        int count = effectCTRL.ClearEffects(EffectType.Negative);
        if (GameController.Instance.CheckCharacterEnhance(6, IsAlly) && characterStats.CharacterId == 6)
        {
            int attack = ActiveSkill.GetCharacterLevel()[star].Data4;
            int health = ActiveSkill.GetCharacterLevel()[star].Data3;
            if (count == 0)
            {
                AddExtraStat(StatsType.Attack, attack, "NatsuAttack", true);
            }
            else
            {
                AddExtraStat(StatsType.Health, health * count, "NatsuHealth", true);
            }
        }
        isCCImmune = true;
    }
    public void SetCCImmune(bool immune)
    {
        isCCImmune = immune;
    }
    public void SetMarked(bool marked)
    {
        bool preMarked = isMarked;
        isMarked = marked;
        Debug.Log($"{name} is {(isMarked ? "marked" : "no longer marked")}.");
        if (GameController.Instance.CheckCharacterEnhance(7, !IsAlly))
        {
            CharacterParent characterParent = IsAlly ? ResourcePool.Instance.enemy : ResourcePool.Instance.ally;
            CharacterCTRL noa = Utility.GetSpecificCharacterByIndex(characterParent.GetBattleFieldCharacter(), 7);
            Effect Effect = EffectFactory.StatckableStatsEffct(0, "Noa Enhanced Skill", -20, StatsType.PercentageResistence, noa, true);
            Effect.SetActions(
                (character) => character.ModifyStats(StatsType.PercentageResistence, Effect.Value, Effect.Source),
                (character) => character.ModifyStats(StatsType.PercentageResistence, -Effect.Value, Effect.Source)
            );
            effectCTRL.AddEffect(Effect, this);
        }
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
        AddStat(statsType, amount, false);
    }
    public void ModifyMultipleStats(Dictionary<StatsType, float> statsModifiers, string source = null, bool isRevert = false)
    {
        foreach (var modifier in statsModifiers)
        {
            float valueToApply = isRevert ? -modifier.Value : modifier.Value;
            AddStat(modifier.Key, valueToApply, false);

            if (source != null)
            {
                CustomLogger.Log(this,
                    $"source = {source}, modifying {modifier.Key} to {GetStat(modifier.Key)} by {(isRevert ? "removing" : "adding")} {Math.Abs(modifier.Value)}");
            }
        }
        CritCorrection();
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
    public void SetStasis()
    {
        Undying = true;
        FaceDirectionLock = true;
        CanWalk = false;
        CanAttack = false;
        InStasis = true;
    }
    public void RemoveStasis()
    {
        Undying = false;
        FaceDirectionLock = false;
        CanWalk = true;
        CanAttack = true;
        InStasis = false;
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
        if (Undying)
        {
            SetStat(StatsType.currHealth, 1);
            isTargetable = false;
            return;
        }
        if (traitController.BeforeDying())
        {
            IsDying = false;
            return;
        }
        if (equipmentManager.BeforeDying())
        {
            IsDying = false;
            return;
        }
        foreach (var item in observers)
        {
            if (item.BeforeDying(this))
            {
                CustomLogger.Log(this, $"{item.GetType()} triggering beforedying");
                IsDying = false;
                return;
            }
        }
        foreach (var item in observers)
        {
            item.OnDying(this);
        }
        traitController.NotifyOnDying();
        GetComponent<BoxCollider>().enabled = false;
        IsDying = true;
        customAnimator.ForceDying();
        CurrentHex.OccupyingCharacter = null;

        isTargetable = false;
        CurrentHex.HardRelease();
        TriggerManualUpdate();
        SetStat(StatsType.currHealth, 0);
        Debug.Log($"{gameObject.name} Die()");
        if (TryGetComponent<Panchan_AnimatorCTRL>(out var p))
        {
            p.SwitchForm(true);
            p.gameObject.SetActive(false);
        }

    }
    public void BattleOverTime()
    {
        AntiHeal = true;
        AntiHealRatio = 0.8f;
    }
    public void HyakkiyakoDyingEffectStart()
    {


    }
    public void HyakkiyakoDyingEffectEnd()
    {
        int amount = (int)(GetStat(StatsType.Health) * 0.35f);
        RemoveHealth(amount);
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
        CharacterCTRL target = GetTargetCTRL();
        SkillContext = new SkillContext
        {
            Parent = this,
            Enemies = GetEnemies(),
            Allies = GetAllies(),
            TargetCTRLPosition = target?.transform.position ?? Vector3.zero,
            posRecorded = target == null ? false : true,
            hexMap = SpawnGrid.Instance.hexNodes.Values.ToList(),
            Range = 5,
            duration = 2f,
            currHex = CurrentHex,
            CurrentTarget = target,
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
    {  0, () => new NullObserver() },
    {  1, () => new ArisObserver() },
    {  2, () => new AyaneObserver() },
    {  3, () => new NullObserver() },
    {  4, () => new NullObserver() },
    {  5, () => new NullObserver() },
    {  6, () => new NullObserver() },
    {  7, () => new NullObserver() },
    {  8, () => new SerikaObserver() },
    {  9, () => new SerinaObserver() },
    { 10, () => new ShizukoObserver() },
    { 11, () => new SumireObserver() },
    { 12, () => new AkoObserver() },
    { 13, () => new AzusaObserver() },
    { 14, () => new NullObserver() },
    { 15, () => new FuukaObserver() },
    { 16, () => new IzunaObserver() },
    { 17, () => new KayokoObserver() },
    { 18, () => new NullObserver() },
    { 19, () => new NullObserver() },
    { 20, () => new NullObserver() },
    { 21, () => new NullObserver() },
    { 22, () => new ShirokoObserver() },
    { 23, () => new TsubakiObserver() },
    { 24, () => new YuukaObserver() },
    { 25, () => new HinaObserver() },
    { 26, () => new HoshinoObserver() },
    { 27, () => new MikaObserver() },
    { 28, () => new NeruObserver() },
    { 29, () => new TsurugiObserver() },
    { 30, () => new WakamoObserver() },
    { 31, () => new Shiroko_Terror_Observer() },
    { 32, () => new NullObserver() },
    { 33, () => new HiyoriObserver() },
    { 34, () => new MisakiObserver() },
    { 35, () => new NullObserver() },
    { 36, () => new MiyuObserver() },
    { 37, () => new MoeObserver() },
    { 38, () => new NullObserver() },
    { 39, () => new SaoriObserver() },
    { 40, () => new TokiObserver() },
    { 46, () => new HimariObserver()},
    { 47, () => new KarinObserver()},
    {504, () => new NullObserver() },
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
        { 41, () => new Panchan_Skill()},
        { 42,() => new KasumiSkill()},
        { 43,() => new MeguSkill()},
        { 44,() => new YukariSkill()},
        { 45,() => new RengeSkill()},
        { 46, ()=> new HimariSkill()},
        { 47, ()=> new KarinSkill()},
        { 48, ()=> new SeiyaSkill()},
        { 49, () => new SakurakoSkill()},
        {  504, () => new HarunaSkill()},
    };
    #endregion

}

