using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkUIManager : MonoBehaviour
{
    // �o�O InputField�A�Ψӿ�J IP �a�}
    public TMP_InputField ipInputField;

    // �o�O UI Panel �Ψ����ĵ�i�T��
    public GameObject warningPanel;  // Panel ����A�q Unity �s�边���j�w

    // �o�Oĵ�i�T���� Text
    public TextMeshProUGUI warningText;  // �p�G�ϥΪ��O TextMeshPro

    // ����U���s�ɡA�եγo�Ӥ�k�ӹ��ճs��
    public void OnConnectButtonClicked()
    {

       
        // �q InputField �������J�� IP �a�}
        string ipAddress = ipInputField.text;

        // �ˬd�O�_��J�F IP �a�}
        if (string.IsNullOrEmpty(ipAddress))
        {
            // ���ĵ�i Panel �ó]�mĵ�i�T��
            warningPanel.SetActive(true);
            warningText.text = "Please enter a valid IP address!";
            return;
        }

        // �p�G����J IP�A�h����ĵ�i Panel
        warningPanel.SetActive(false);

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

    public void OnCloseButton()
    {
        // �^��StartMenu
        SceneManager.LoadScene("StartMenu");
    }
    public void OnCloseButtonClicked()
    {
        // ����ĵ�i Panel
        warningPanel.SetActive(false);
    }
}
