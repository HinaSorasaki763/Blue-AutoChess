using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public class GoldRotate : MonoBehaviour, IPointerDownHandler
{
    private float rotationSpeed = 100f;
    public CollectionRewardType rewardType;
    public int CharacterIndex;
    public void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        switch (rewardType)
        {
            case CollectionRewardType.None:
                break;
            case CollectionRewardType.Gold:
                GameController.Instance.AddGold(1);
                break;
            case CollectionRewardType.RandComponent:
                Utility.GetMultipleRandomEquipRewards(1,0,6).Item1[0].Award();

                break;
            case CollectionRewardType.RandCompleteItem:
                Utility.GetMultipleRandomEquipRewards(1, 6, 26).Item1[0].Award();
                break;
            case CollectionRewardType.Character:
                if (BenchManager.Instance.IsBenchFull())
                {
                    PopupManager.Instance.CreatePopup("角色達到上限，無法領取，先清理盤面", 2);
                    return;
                }
                else
                {
                    List<Character> allCharacters = ResourcePool.Instance.GetAllCharacters();
                    foreach (var item in allCharacters)
                    {
                        CustomLogger.Log(this, $"try Get {item.CharacterName} for index {item.CharacterId},want {CharacterIndex}");
                        if (item.CharacterId == CharacterIndex)
                        {
                            BenchManager.Instance.AddToBench(item.Model);
                            CustomLogger.Log(this, $"Get {item.CharacterName}");
                        }
                    }
                }
                break;
            case CollectionRewardType.Others:
                break;
            default:
                break;
        }
        Destroy(gameObject);
    }
}
