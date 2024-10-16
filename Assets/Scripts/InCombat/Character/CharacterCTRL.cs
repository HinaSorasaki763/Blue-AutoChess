
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
    public bool enterBattle;
    public bool isAlive = true;
    public bool IsCastingAbility = false;
    public bool isWalking = false;
    public bool IsDying;
    public bool isMarked;
    public bool isCCImmune;
    public bool Invincible;
    private bool isFindingTarget = false;
    private bool isFindingPath = false;
    private bool ManaAdded;
    public float attackRate = 5.0f;
    private float attackTimer = 0f;
    private float attackSpeed = 1.0f;
    public bool stunned;
    public GameObject bulletPrefab;
    public Transform FirePoint;
    public Transform GetHitPoint;
    public CharacterSkillBase ActiveSkill;
    public Vector3 DetectedPos = Vector3.zero;
    public SkillContext SkillContext;
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


    #endregion
    #region Unity Lifecycle Methods
    private void OnEnable()
    {
        stats = characterStats.Stats.Clone();
        ResetStats();
        allyParent = ResourcePool.Instance.ally;
        enemyParent = ResourcePool.Instance.enemy;
        modifierCTRL = GetComponent<ModifierCTRL>();
        effectCTRL = GetComponent<EffectCTRL>();
        effectCTRL.characterCTRL = this;
        CustomLogger.Log(this, $"gifting effectCTRL {gameObject.name} ctrl");
        traitController = GetComponent<TraitController>();
        customAnimator = null;
        IsDying = false;
        if (characterBars!= null)
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
            ActiveSkill = characterSkillFunc();
        }

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

    public void Update()
    {
        if (isShirokoTerror)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        characterBars.UpdateText(customAnimator.GetState().Item1.ToString());
        if (Target!= null&& !Target.activeInHierarchy)
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
        if (!canAttack || IsCastingAbility ||  Target == null || !Target.activeInHierarchy)
        {
            if (Target == null && !isFindingTarget && !isWalking && !isFindingPath)
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
            Shiroko_Terror_SkillCTRL shiroko_Terror_SkillCTRL =  GetComponent<Shiroko_Terror_SkillCTRL>();
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
        if (IsDying) return;

        if (!customAnimator.GetState().Item2 || Target == null)
        {

            customAnimator.ChangeState(CharacterState.Idling);
            return;
        }

        if (characterStats.logistics)
        {
            logistics();
            return;
        }
        HandleAttacking();
        customAnimator.ChangeState(CharacterState.Attacking);
        var bullet = ResourcePool.Instance.GetBullet(FirePoint.position);
        var bulletComponent = bullet.GetComponent<Bullet>();
        var targetCtrl = Target.GetComponent<CharacterCTRL>();
        bulletComponent.SetTarget(targetCtrl.GetHitPoint.position + GetRandomDeviation(0.2f), targetCtrl);
        bulletComponent.SetParent(this);

        int damage = (int)(GetStat(StatsType.Attack) * (1 + modifierCTRL.GetTotalStatModifierValue(ModifierType.DamageDealt) * 0.01f));
        if (Utility.Iscrit(GetStat(StatsType.CritChance)))
        {
            damage = (int)(damage*(1 + GetStat(StatsType.CritRatio)*0.01f));
            CustomLogger.Log(this,$"character {name} crit");
        }
        bulletComponent.SetDmg(damage);
        transform.LookAt(Target.transform);
    }

    private void HandleTargetFinding()
    {
        if (Target == null && PreTarget == null && !FindTarget())
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
        foreach (var item in observers)
        {
            item.OnAttacking(this);
        }
        AddStat(StatsType.Mana, 10);
    }


    public void Heal(int amount) => AddStat(StatsType.currHealth, amount);

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

        if (GetStat(StatsType.Mana) >= GetStat(StatsType.MaxMana) && !IsCasting() && !isWalking)
        {
            SetStat(StatsType.Mana, 0);
            StartCoroutine(CastSkill());
        }
    }

    public float GetHealthPercentage() => GetStat(StatsType.currHealth) / (float)GetStat(StatsType.Health);

    public int GetStat(StatsType statsType) => stats.GetStat(statsType);

    public void AddStat(StatsType statsType, int amount) => SetStat(statsType, GetStat(statsType) + amount);

    public void SetStat(StatsType statsType, int amount) => stats.SetStat(statsType, amount);

    public IEnumerator CastSkill()
    {
        Debug.Log($"{characterStats.CharacterName}CastSkill()");
        IsCastingAbility = true;
        customAnimator.ChangeState(CharacterState.CastSkill);
        int i = ExecuteActiveSkill();
        int animationIndex = 7;
        if(isShirokoTerror)
        {
            animationIndex = i+8;
        }
        else
        {
            
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
        customAnimator.AfterCastSkill();
    }
    public bool IsCasting() => customAnimator.animator.GetBool("CastSkill");
    #endregion

    #region Targeting and Pathfinding
    public bool FindTarget()
    {
        if (isFindingTarget||IsCastingAbility)
        {
            return false;
        }
        isFindingTarget = true;

        var hitColliders = Physics.OverlapSphere(transform.position, 50, GetTargetLayer());
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.TryGetComponent(out CharacterCTRL C)|| !C.CurrentHex.IsBattlefield) continue;
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

            if (C.isTargetable && distance < closestDistance)
            {
                closestTarget = hitCollider.gameObject;
                closestDistance = distance;
            }
        }
        bool found = UpdateTarget(closestTarget, closestDistance);
        isFindingTarget = false;
        return found;
    }
    private bool UpdateTarget(GameObject closestTarget, float closestDistance)
    {
        if (closestTarget != null)
        {
            if (closestDistance <= GetStat(StatsType.Range))
            {
                PreTarget = null;
                Target = closestTarget;
                Debug.Log($"{characterStats.CharacterName} 更新目標: {Target.name}");
             //   Attack();
            }
            else
            {
                Target = null;
                PreTarget = closestTarget;
                if (!isWalking && !isFindingPath)
                {
                    CustomLogger.Log(this,$"{characterStats.CharacterName} 目標超出範圍，呼叫 TargetFinder");
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
        PathRequestManager.Instance.RequestPath(this, startNode, targetNode, OnPathFound,stats.GetStat(StatsType.Range));
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
            Vector3 targetPos = node.Position+ offset;
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

            HexNode newGrid = SpawnGrid.Instance.GetHexNodeByPosition(targetPos -offset);
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
        bool inRange = closestDistance <= GetStat(StatsType.Range) ||
                       (PreTarget != null && Vector3.Distance(transform.position, PreTarget.transform.position) <= GetStat(StatsType.Range));
        return inRange;
    }

    #endregion

    #region Damage and Death
    public void GetHit(int amount, CharacterCTRL sourceCharacter)
    {
        if (IsDying) return;

        int finalAmount = traitController?.ModifyDamageTaken(amount, sourceCharacter) ?? amount;
        while (finalAmount > 0 && shields.Count > 0)
        {
            Shield shield = shields[0]; // 取得剩餘時間最少的護盾

            if (shield.amount >= finalAmount)
            {
                // 護盾足夠承受傷害
                shield.amount -= finalAmount;
                AddStat(StatsType.Shield, -finalAmount);
                finalAmount = 0;
            }
            else
            {
                // 護盾不足，扣除護盾並移除
                finalAmount -= shield.amount;
                AddStat(StatsType.Shield, -shield.amount);
                shields.RemoveAt(0);
            }
        }

        // 如果護盾不足以承受所有傷害，扣血量
        if (finalAmount > 0)
        {
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
    public void AddShield(int amount, float duration)
    {
        Shield newShield = new Shield(amount, duration);
        shields.Add(newShield);
        shields.Sort((a, b) => a.remainingTime.CompareTo(b.remainingTime));
        AddStat(StatsType.Shield, amount);
    }

    public void Stun(bool s)
    {
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
            Debug.Log($"{name} is no longer stunned.");
        }
    }
    public void SetCCImmune(bool immune)
    {
        isCCImmune = immune;
        Debug.Log($"{name} is {(isCCImmune ? "immune to CC" : "no longer immune to CC")}.");
    }
    public void SetMarked(bool marked)
    {
        isMarked = marked;
        Debug.Log($"{name} is {(isMarked ? "marked" : "no longer marked")}.");
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

    private IEnumerator Die()
    {
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
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(customAnimator.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length-0.6f);
        gameObject.SetActive(false);
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

            if (characterCtrl.CurrentHex.IsBattlefield&&characterCtrl.gameObject.activeInHierarchy)
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

            if (characterCtrl.CurrentHex.IsBattlefield)
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
        { 25, () => new HinaObserver() },
        { 26, () => new HoshinoObserver()},
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
        { 31, () => new Shiroko_TerrorSkill()}
    };
    #endregion

}
public class Shield
{
    public int amount; // 護盾數值
    public float remainingTime; // 剩餘持續時間

    public Shield(int amount, float remainingTime)
    {
        this.amount = amount;
        this.remainingTime = remainingTime;
    }
}
