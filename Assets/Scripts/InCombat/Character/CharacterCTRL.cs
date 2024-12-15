
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CharacterCTRL : MonoBehaviour
{
    #region 屬性變數宣告
    // Grid and Target-related Fields
    public HexNode CurrentHex;
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
    public bool ManaLock = false;
    public bool enterBattle;
    public bool isAlive = true;
    public bool IsCastingAbility = false;
    public bool isWalking = false;
    public bool IsDying;
    public bool isMarked;
    public bool isCCImmune;
    public bool Invincible;
    public bool Taunted;
    private bool isFindingPath = false;
    private bool ManaAdded;
    public float attackRate = 5.0f;
    private float attackTimer = 0f;
    private float attackSpeed = 1.0f;
    public bool WakamoMark;
    private float wakamoMarkRatio;
    private int dmgRecivedOnWakamoMarked;
    private CharacterCTRL WakamoMarkParent;
    public bool stunned;
    public GameObject bulletPrefab;
    public Transform FirePoint;
    public Transform GetHitPoint;
    public CharacterSkillBase ActiveSkill;
    public Vector3 DetectedPos = Vector3.zero;
    public SkillContext SkillContext;
    public CharacterAudioManager AudioManager;
    // Traits and Effects-related Fields
    public TraitController traitController;
    private TraitEffectApplier traitEffectApplier = new TraitEffectApplier();
    private List<CharacterObserverBase> observers = new List<CharacterObserverBase>();

    // Animator-related Fields
    public CustomAnimatorController customAnimator;

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
    #endregion
    #region Unity Lifecycle Methods
    public virtual void OnEnable()
    {
        stats = characterStats.Stats.Clone();
        ResetStats();
        allyParent = ResourcePool.Instance.ally;
        enemyParent = ResourcePool.Instance.enemy;
        modifierCTRL = GetComponent<ModifierCTRL>();
        effectCTRL = GetComponent<EffectCTRL>();
        effectCTRL.characterCTRL = GetComponent<CharacterCTRL>();
        CustomLogger.Log(this, $"gifting effectCTRL {gameObject.name} ctrl");
        traitController = GetComponent<TraitController>();
        customAnimator = null;
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
        foreach (var item in observers)
        {
            item.CharacterStart(this);
        }
        if (characterSkills.TryGetValue(characterId, out var characterSkillFunc))
        {
            var baseSkill = characterSkillFunc();
            if (IsHeroicEnhanced)
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
    }
    public void ResetStats()
    {
        SetStat(StatsType.currHealth, GetStat(StatsType.Health));
        SetStat(StatsType.Mana, 0);
    }
    private void Start()
    {
        allyLayer = 1 << LayerMask.NameToLayer("Ally");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        GridLayer = 1 << LayerMask.NameToLayer("Grid");
        customAnimator = GetComponent<CustomAnimatorController>();
    }

    void Awake()
    {

    }

    public virtual void Update()
    {
        characterBars.UpdateText(customAnimator.GetState().Item1.ToString());
        if (Target != null && Target.GetComponent<CharacterCTRL>().IsDying)
        {
            Target = null;
        }
        bool canAttack = !customAnimator.animator.GetBool("CastSkill") && !isWalking;

        if (enterBattle)
        {
            DetectedPos = CurrentHex.transform.position + new Vector3(0, 0.3f, 0);

            if (!stunned)
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
        for (int i = shields.Count - 1; i >= 0; i--)
        {
            shields[i].remainingTime -= Time.deltaTime;
            if (shields[i].remainingTime <= 0)
            {
                RemoveShield(shields[i]);
                shields.RemoveAt(i);
            }
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

        customAnimator.ChangeState(CharacterState.Attacking);
        transform.LookAt(Target.transform);

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

    public int ExecuteActiveSkill()
    {
        var skillContext = new SkillContext
        {
            Parent = this,
            Enemies = GetEnemies(),
            Allies = GetAllies(),
            hexMap = SpawnGrid.Instance.hexNodes.Values.ToList(),
            Range = 5,
            duration = 2f,
            currHex = CurrentHex,
            CurrentTarget = Target.GetComponent<CharacterCTRL>(),
            CharacterLevel = 1
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
        foreach (var item in observers)
        {
            item.OnCastedSkill(this);
        }
        return i;

    }

    public void Attack()
    {
        if (Target!= null && Vector3.Distance(Target.transform.position,transform.position) > GetStat(StatsType.Range))
        {
            FindTarget();
            return;
        }
        if (IsDying) return;
        if (characterStats.logistics)
        {
            logistics();
            HandleAttacking();
            return;
        }
        HandleAttacking();
        customAnimator.ChangeState(CharacterState.Attacking);
        var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet,FirePoint.position, Quaternion.identity);
        var bulletComponent = bullet.GetComponent<NormalBullet>();
        if (!Target) return;
        var targetCtrl = Target.GetComponent<CharacterCTRL>();
        if (bulletComponent == null) 
        {
            CustomLogger.LogWarning(this,$"Target {Target} dont have ctrl");
        }
        int damage = (int)(GetStat(StatsType.Attack) * (1 + modifierCTRL.GetTotalStatModifierValue(ModifierType.DamageDealt) * 0.01f));
        bool iscrit = false;
        if (Utility.Iscrit(GetStat(StatsType.CritChance)))
        {
            damage = (int)(damage * (1 + GetStat(StatsType.CritRatio) * 0.01f));
            CustomLogger.Log(this, $"character {name} crit");
            iscrit = true;
        }
        bulletComponent.Initialize(targetCtrl.GetHitPoint.position, damage, GetTargetLayer(), this, 20,targetCtrl.gameObject,iscrit);

        transform.LookAt(Target.transform);
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
        if (!ManaLock)
        {
            AddStat(StatsType.Mana, 10);
        }
        foreach (var item in observers)
        {
            item.OnAttacking(this);
        }
        traitController.Attacking();
        AugmentEventHandler.Instance.Attacking(this);
    }


    public void Heal(int amount,CharacterCTRL source)
    {
        CustomLogger.Log(this,$"{source} heal {name} of {amount}");
        if (GetStat(StatsType.currHealth) + amount >= GetStat(StatsType.Health))
        {
            SetStat(StatsType.currHealth, GetStat(StatsType.Health));
            return;
        }
        AudioManager.PlayHpRestoredSound();
        AddStat(StatsType.currHealth, amount);
    }


    public void OnKillEnemy(CharacterCTRL enemy) => traitController.NotifyOnKilledEnemy();

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
        Taunted = b;
    }
    public void EmptyEffectFunction()
    {


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
            StartCoroutine(Die());
            return;
        }
        if (GetStat(StatsType.Mana) >= GetStat(StatsType.MaxMana) && !IsCasting() && !isWalking&&!isObj&&!Taunted)
        {
            AddStat(StatsType.Mana, -GetStat(StatsType.MaxMana));
            CustomLogger.Log(this,$"{gameObject.name} casting");
            StartCoroutine(CastSkill());
        }
    }

    public float GetHealthPercentage() => GetStat(StatsType.currHealth) / (float)GetStat(StatsType.Health);

    public float GetStat(StatsType statsType) => stats.GetStat(statsType);

    public void AddStat(StatsType statsType, float amount) => SetStat(statsType, GetStat(statsType) + amount);

    public void SetStat(StatsType statsType, float amount) => stats.SetStat(statsType, amount);

    public IEnumerator CastSkill()
    {
        AudioManager.PlayCastExSkillSound();
        CustomLogger.Log(this,$"{characterStats.CharacterName}CastSkill()");
        IsCastingAbility = true;
        customAnimator.ChangeState(CharacterState.CastSkill);
        int i = ExecuteActiveSkill();
        int animationIndex = 7;
        if (isShirokoTerror)
        {
            animationIndex = i + 8;
        }
        float sec = customAnimator.GetAnimationClipInfo(animationIndex).Item2 / 3f;
        Debug.Log($"[CharacterCTRL] index = {animationIndex} name = {customAnimator.GetAnimationClipInfo(animationIndex).Item1}  length = {customAnimator.GetAnimationClipInfo(animationIndex).Item2}");
        yield return new WaitForSeconds(sec);
        if (enterBattle)
        {
            customAnimator.ChangeState(CharacterState.Attacking);
            customAnimator.animator.SetBool("CastSkill", false);
        }
        yield return new WaitForSeconds(sec * 2 - 0.2f);
        AfterCastSkill();
        IsCastingAbility = false;
        yield return new WaitForSeconds(0.2f);
    }
    public void AfterCastSkill()
    {
        foreach (var item in observers)
        {
            item.OnSkillFinished(this);
        }
        ManaLock = false;
        customAnimator.AfterCastSkill();
    }
    public bool IsCasting() => customAnimator.animator.GetBool("CastSkill");
    public bool EquipItem(IEquipment equipment)
    {
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

            if (C.isTargetable && distance < closestDistance)
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
            if (closestDistance <= GetStat(StatsType.Range)+0.1f)
            {
                PreTarget = null;
                Target = closestTarget;
            }
            else
            {
                Target = null;
                PreTarget = closestTarget;
                if (!isWalking && !isFindingPath)
                {
                    CustomLogger.Log(this, $"{characterStats.CharacterName} 目標超出範圍，呼叫 TargetFinder");
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

        HexNode startNode = CurrentHex; // 确保起始节点是角色当前所在的格子

        if (startNode == null)
        {
            Debug.LogError("StartNode is null");
            isFindingPath = false;
            return;
        }

        if (targetNode == null)
        {
            Debug.LogError("TargetNode is null");
            isFindingPath = false;
            return;
        }

        Debug.Log($"StartNode Position: {startNode.Position}, Cube Coordinates: {startNode.X},{startNode.Y},{startNode.Z}");
        Debug.Log($"TargetNode Position: {targetNode.Position}, Cube Coordinates: {targetNode.X},{targetNode.Y},{targetNode.Z}");
        PathRequestManager.Instance.RequestPath(this, startNode, targetNode, OnPathFound, (int)stats.GetStat(StatsType.Range));
    }

    private void OnPathFound(List<HexNode> path)
    {
        isFindingPath = false;

        if (Target == null && PreTarget != null && !isWalking)
        {
            Debug.Log($"{characterStats.CharacterName} 找到路徑，開始移動");
            StartCoroutine(MoveAlongPath(path));
        }

        StringBuilder sb = new StringBuilder();
        int i = 0;
        sb.AppendLine($"角色 {characterStats.CharacterName} 移動路徑：");
        foreach (var item in path)
        {
            i++;
            sb.AppendLine($"節點 {i} : {item.Position}");
        }
        Debug.Log(sb.ToString());
    }


    private IEnumerator MoveAlongPath(List<HexNode> path)
    {
        isWalking = true;
        customAnimator.ChangeState(CharacterState.Moving);
        Debug.Log($"{characterStats.CharacterName} 開始沿路徑移動");

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
                Debug.Log($"{characterStats.CharacterName} 釋放節點位置: {previousNode.Position}");
            }

            previousNode = CurrentHex;
            CurrentHex = node;
            CurrentHex.SetOccupyingCharacter(this);

            currentNodeIndex = i;

            if (IsTargetInRange())
            {
                Debug.Log($"{characterStats.CharacterName} 發現目標在範圍內，中止移動");
                currentNodeIndex = i;
                break;
            }
        }
        PathRequestManager.Instance.ReleaseRemainingReservations(this, currentNodeIndex + 1);
        isWalking = false;
        FindTarget();
        Debug.Log($"{characterStats.CharacterName} 完成沿路徑移動");
    }





    private IEnumerator MoveTowardsPosition(Vector3 targetPos)
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
        float closestDistance = Mathf.Min(GetEnemies().Min(e => Vector3.Distance(transform.position, e.transform.position)), int.MaxValue);
        bool inRange = closestDistance <= GetStat(StatsType.Range)+0.1f ||
                       (PreTarget != null && Vector3.Distance(transform.position, PreTarget.transform.position) <= GetStat(StatsType.Range) + 0.1f);
        return inRange;
    }

    #endregion

    #region Damage and Death
    public virtual void GetHit(int amount, CharacterCTRL sourceCharacter,bool isCrit = false)
    {
        if (IsDying) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        int rand = UnityEngine.Random.Range(0, 100);
        if (GetStat(StatsType.DodgeChance) >= rand)
        {
            AudioManager.PlayDodgedSound();
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Miss, 0, screenPos,false);
            return;
        }
        int finalAmount = traitController?.ModifyDamageTaken(amount, sourceCharacter) ?? amount;
        float r = GetStat(StatsType.Resistence);
        float ratio = r / (100 + r);
        finalAmount = (int)(finalAmount*ratio);
        if (isCrit)
        {
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Weak, finalAmount, screenPos,false);
        }
        dmgRecivedOnWakamoMarked += (int)(finalAmount * wakamoMarkRatio); 
        while (finalAmount > 0 && shields.Count > 0)
        {
            Shield shield = shields[0];
            if (shield.amount >= finalAmount)
            {
                TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, finalAmount, screenPos, false);
                shield.amount -= finalAmount;
                AddStat(StatsType.Shield, -finalAmount);
                finalAmount = 0;
            }
            else
            {
                TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.Resist, shield.amount, screenPos, false);
                finalAmount -= shield.amount;
                AddStat(StatsType.Shield, -shield.amount);
                shields.RemoveAt(0);
            }
        }

        if (finalAmount > 0)
        {
            TextEffectPool.Instance.ShowTextEffect(BattleDisplayEffect.None, finalAmount, screenPos,true);
            AddStat(StatsType.currHealth, -finalAmount);
        }

        if (CheckDeath())
        {
            StartCoroutine(Die());
            if (sourceCharacter.traitController != null)
            {
                sourceCharacter.traitController.NotifyOnKilledEnemy();
            }
            else
            {
                Debug.LogWarning("sourceCharacter or its traitController is null, unable to notify observer.");
            }
        }
    }


    public void AddShield(int amount, float duration, CharacterCTRL source)
    {
        Shield newShield = new Shield(amount, duration);
        shields.Add(newShield);
        shields.Sort((a, b) => a.remainingTime.CompareTo(b.remainingTime));
        CustomLogger.Log(this, $"{source} Shield {name} of {amount}");
        AddStat(StatsType.Shield, amount);
    }
    public void Stun(bool s)
    {
        if (isObj) return;
        stunned = s;
        if (stunned)
        {
            customAnimator.ChangeState(CharacterState.Stunned);
            customAnimator.animator.SetBool("StunFinished", false);
        }
        else
        {
            customAnimator.ChangeState(CharacterState.Attacking);
            customAnimator.animator.SetBool("StunFinished", true);
        }
    }
    public void SetWakamoMark(float ratio,CharacterCTRL wakamo)
    {
        WakamoMark = true;
        wakamoMarkRatio = ratio;
        WakamoMarkParent = wakamo;
    }
    public void OnWakamoMarkCountedDown()
    {
        WakamoMark = false;
        wakamoMarkRatio = 0;
        GetHit(dmgRecivedOnWakamoMarked, WakamoMarkParent,false);
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
                if ((item.transform.position-transform.position).magnitude <= item.GetStat(StatsType.Range) && item.characterStats.logistics)
                {
                    item.PreTarget = null;
                    item.Target = gameObject;
                }
            }
        }

    }
    public void ModifyStats(StatsType statsType, float amount,string source = null)
    {
        AddStat(statsType, amount);
        if (source!= null)
        {
            CustomLogger.Log(this,$"source = {source} , modifing {statsType} to {GetStat(statsType)} by added {amount}");
        }
    }
    private bool CheckDeath()
    {
        if (GetStat(StatsType.currHealth) <= 0.5f)
        {
            isAlive = false;
            return true;
        }
        return false;
    }

    public virtual IEnumerator Die()
    {
        float soundLength = AudioManager.PlayOnDefeatedSound();
        float animationLength = customAnimator.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        Debug.Log($"{gameObject.name} Die()");
        SpawnGrid.Instance.RemoveCenterPoint(CurrentHex);
        foreach (var item in observers)
        {
            item.OnDying(this);
        }
        IsDying = true;
        customAnimator.ChangeState(CharacterState.Dying);
        CurrentHex.OccupyingCharacter = null;
        isTargetable = false;
        CurrentHex.HardRelease();
        if (soundLength >= animationLength)
        {
            yield return new WaitForSeconds(soundLength);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(animationLength - 0.6f);
        }
        gameObject.SetActive(false);
    }
    public void MarkedByWakamoStart()
    {

    }
    public void WakamoMarkEnd()
    {

    }
    #endregion

    #region Layer and Team Operations
    public LayerMask GetTargetLayer() => (IsAlly ^ characterStats.logistics) ? enemyLayer : allyLayer;

    public LayerMask GetAllyLayer() => IsAlly ? allyLayer : enemyLayer;
    public List<CharacterCTRL> GetEnemies()
    {
        List<CharacterCTRL> enemies = new List<CharacterCTRL>();
        var characterPool = !IsAlly ? ResourcePool.Instance.ally.childCharacters : ResourcePool.Instance.enemy.childCharacters;

        foreach (var character in characterPool)
        {
            CharacterCTRL characterCtrl = character.GetComponent<CharacterCTRL>();

            if (characterCtrl.CurrentHex.IsBattlefield && characterCtrl.gameObject.activeInHierarchy && !characterCtrl.IsDying)
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

            if (characterCtrl.CurrentHex.IsBattlefield && characterCtrl.gameObject.activeInHierarchy&&!characterCtrl.characterStats.logistics)
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
        { 22, () => new ShirokoObserver()},

        { 25, () => new HinaObserver()},
        { 26, () => new HoshinoObserver()},
        { 29, () => new TsurugiObserver()},
        { 31, () => new Shiroko_Terror_Observer()}
    };

    public Dictionary<int, Func<CharacterSkillBase>> characterSkills = new()
    {
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
        //40 is toki
    };
    #endregion

}

