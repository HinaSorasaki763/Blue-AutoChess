using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Text;

public class NetworkUIManager : MonoBehaviour
{
    // �o�O InputField�A�Ψӿ�J IP �a�}
    public InputField ipInputField;

    // ����U���s�ɡA�եγo�Ӥ�k�ӹ��ճs��
    public void OnConnectButtonClicked()
    {
        // �q InputField �������J�� IP �a�}
        string ipAddress = ipInputField.text;

        // �]�w�Ȥ�ݪ� IP �a�}�A�o�̰��]�ϥ� UnityTransport
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.SetConnectionData(ipAddress, 7777); // 7777 �O�q�{�ݤf���A�i�H�ھڻݭn���
        }

        // �}�l�@���Ȥ�ݶi��s��
        NetworkManager.Singleton.StartClient();
    }

    // ��ХD�I���}�Ы��s�ɡA�Ұʦ��A��
    public void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }
}
