using UnityEngine;

public class ShizukoActiveSkill : MonoBehaviour
{
    public GameObject TruckPrefab;
    public GameObject Reference;
    readonly float fallSpeed = 10.0f;
    private StaticObject ctrl;
    public CharacterCTRL Parent;
    public void Start()
    {

    }
    public void OnEnable()
    {

    }
    public void Update()
    {
        if (Reference != null)
        {
            if (Reference.transform.position.y > 0.25f)
            {
                Reference.transform.position += fallSpeed * Time.deltaTime * Vector3.down;
            }
            else
            {
                Vector3 v = Reference.transform.position;
                Reference.transform.position = new Vector3(v.x, 0.25f, v.z);
            }
        }
    }
    public void SpawnTruck(HexNode h, CharacterCTRL parent)
    {
        if (Reference != null)
        {
            Reference.GetComponent<StaticObject>().CurrentHex.HardRelease();
            Destroy(Reference);
            Reference = null;
        }
        Parent = parent;
        Reference = Instantiate(TruckPrefab, h.Position + new Vector3(0, 5, 0), Quaternion.Euler(-90, 0, 0)); 
        ctrl = Reference.GetComponent<StaticObject>();
        ctrl.IsAlly = Parent.IsAlly;
        ctrl.CurrentHex = h;
        h.OccupyingCharacter = ctrl;
    }
}
