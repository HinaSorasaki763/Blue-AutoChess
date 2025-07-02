using System.Collections.Generic;
using UnityEngine;

public class Shiroko_Terror_AugmentCheck : MonoBehaviour
{
    public bool Detecting;
    public Sprite Nonomi, Serika, Hoshino, Ayane;

    private Dictionary<int, Sprite> idToSprite;
    public CharacterCTRL parent;
    private void Awake()
    {
        idToSprite = new Dictionary<int, Sprite>
        {
            { 2, Ayane },
            { 8, Serika },
            { 21, Nonomi },
            { 26, Hoshino }
        };
    }
    void Start()
    {

    }
    void Update()
    {

    }
    public void TriggerDetecting()
    {

        Detecting = true;
        Shiroko_Canva.Instance.parent.SetActive(true);
        Shiroko_Canva.Instance.Reserving(parent);
        CheckAllies();
    }
    public void StopDetecting()
    {
        Detecting = false;
        Shiroko_Canva.Instance.parent.SetActive(false);
        Shiroko_Canva.Instance.Release();
    }

    public void CheckAllies()
    {
        int count = 0;
        int diedcount = 0;
        foreach (var item in ResourcePool.Instance.ally.GetAllCharacter())
        {
            if (!item.CurrentHex.IsBattlefield)
                continue;

            if (idToSprite.TryGetValue(item.characterStats.CharacterId, out Sprite sprite))
            {
                if (count < Shiroko_Canva.Instance.images.Count)
                {
                    Shiroko_Canva.Instance.images[count].sprite = sprite;
                    Shiroko_Canva.Instance.images[count].color = Color.white;
                    if (!item.isAlive)
                    {
                        Shiroko_Canva.Instance.images[count].color = Color.grey;
                        diedcount++;
                    }
                    count++;
                }
            }
            if (item.TryGetComponent<StaticObject>(out StaticObject s))
            {
                if (idToSprite.TryGetValue(s.characterStats.CharacterId, out Sprite sp) && !item.isAlive)
                {
                    Shiroko_Canva.Instance.images[count].sprite = sprite;
                    Shiroko_Canva.Instance.images[count].color = Color.white;
                    if (!item.isAlive)
                    {
                        Shiroko_Canva.Instance.images[count].color = Color.grey;
                        diedcount++;
                    }
                }
                diedcount++;
                count++;
            }
        }

        for (int i = count; i < Shiroko_Canva.Instance.images.Count; i++)
        {
            Shiroko_Canva.Instance.images[i].sprite = null;
        }
        if (diedcount >= 3)
        {
            ResourcePool.Instance.ally.Shiroko_Terror_Postponed = true;
        }
    }
}
