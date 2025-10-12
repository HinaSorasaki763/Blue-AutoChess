using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class KarinCostumSkill : MonoBehaviour
{
    private CharacterCTRL character;
    void Start()
    {
        character = GetComponent<CharacterCTRL>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public  Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,225)},
            {2, new StarLevelStats(225,300)},
            {3, new StarLevelStats(400,350)}
        };
        return statsByStarLevel;
    }
    public int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        return stats.Data1 + (int)(stats.Data2 * 0.01f * skillContext.Parent.GetAttack());
    }
    public void SkillShot1()
    {
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)(GetAttackCoefficient(skillContext) * 0.9f);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        CharacterCTRL target = Utility.GetNearestEnemy(character);
        character.transform.LookAt(target.transform.position);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, target.gameObject, true, iscrit);
    }
    public void SkillShot2()
    {
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)(GetAttackCoefficient(skillContext) * 1.1f);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        CharacterCTRL target = Utility.GetNearestEnemy(character);
        character.transform.LookAt(target.transform.position);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, target.gameObject, true, iscrit);
    }
    public void SkillShot3()
    {
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)(GetAttackCoefficient(skillContext) * 1.5f);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        CharacterCTRL target = Utility.GetNearestEnemy(character);
        character.transform.LookAt(target.transform.position);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, target.gameObject, true, iscrit);
    }
}
