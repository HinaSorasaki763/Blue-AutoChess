using UnityEngine;

public class Shiroko_Terror_DroneCTRL : MonoBehaviour
{
    public Animator Animator;
    private string attacking = "Attacking";
    private CharacterCTRL Parent;
    public int stack;
    public void Update()
    {
        if (Parent != null && Parent.Target != null)
        {
            transform.LookAt(Parent.Target.transform);
        }
    }
    public void AssistAttack(CharacterCTRL target,CharacterCTRL parent)
    {
        Parent = parent;
        Animator.SetBool(attacking, true);

        Animator.speed = parent.GetStat(GameEnum.StatsType.AttackSpeed);
        Vector3 v = target.transform.position;
        transform.LookAt(v);
        int dmg = (int)parent.GetStat(GameEnum.StatsType.Attack)*stack;
        (bool, int) tuple = parent.CalculateCrit(dmg);
        target.GetHit(tuple.Item2,parent,"Shiroko_Terror_Drone",tuple.Item1);
        CustomLogger.Log(this,$"Dealing {dmg}");
    }
    public void EndAttack()
    {
        Animator.SetBool(attacking, false);
        CustomLogger.Log(this, $" end attack");
    }
}
