using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;  // �Ω����s��

    private AudioSource audioSource;
    public List<AudioClip> bgmClips;    // �N BGM ���֩즲�i��
    private bool isPlayingBGM = false;  // �ΨӰO���ثe�O�_���b����
    private int currentIndex = -1;      // �O���ثe���� BGM ����
    public Slider VolumeSlider;
    private void Awake()
    {
        // �T�O������b���������ɤ��Q�P���A�B�O����ҼҦ�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        VolumeSlider.value = 0.75f;
        audioSource.volume = VolumeSlider.value*0.1f;
    }
    private void Update()
    {
        // ���]���`������ (loop = false) �A��e���ֵ����� isPlaying �|�ܬ� false
        // �ˬd�G�p�G���b���񪬺A�лx isPlayingBGM �� true�A����ڤW�S�����֦b�� (isPlaying=false)
        if (isPlayingBGM && !audioSource.isPlaying)
        {
            // BGM ���񵲧��A��ܤU�@���H�� BGM ����
            PlayRandomBGM();
        }
    }
    public void VolumeSetting()
    {
        audioSource.volume = VolumeSlider.value * 0.1f;
    }
    public void PlayBGM(int index, bool loop = false)
    {
        if (index < 0 || index >= bgmClips.Count) return;

        currentIndex = index;
        audioSource.clip = bgmClips[index];
        audioSource.loop = loop;
        audioSource.Play();
        
        isPlayingBGM = true;
    }

    public void StopBGM()
    {
        audioSource.Stop();
        isPlayingBGM = false;
        currentIndex = -1;
    }

    public void ToggleBGM()
    {
        if (!isPlayingBGM)
        {
            // �p�G�ثe�S�����񭵼֡A���N����Ĥ@���]���H�����^
            if (bgmClips.Count > 0)
            {
                PlayBGM(0, false); // �w�]�����0���A���Q�T�w�N���� PlayRandomBGM();
            }
        }
        else
        {
            // �p�G�ثe���b���A���U��N����
            StopBGM();
        }
    }

    public void PlayRandomBGM()
    {
        if (bgmClips.Count == 0) return;

        int randomIndex = Random.Range(0, bgmClips.Count);

        // �Y���Q���Ƽ���P�@���A�i�ˬd�O�_�P currentIndex �ۦP�A�Y�ۦP�N�A�H���@���G
        while (randomIndex == currentIndex && bgmClips.Count > 1)
        {
             randomIndex = Random.Range(0, bgmClips.Count);
        }

        PlayBGM(randomIndex, false);
    }

    

}
