using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

// MonoBehaviourPunCallbacks���p�����āAPUN�̃R�[���o�b�N���󂯎���悤�ɂ���
public class PlayStart : MonoBehaviour
{
    public static PlayStart instance;
    public int sCardNo;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayStartButton()
    {
        PlayerPrefs.SetInt("SCardKey", sCardNo);
        PlayerPrefs.Save();

        SceneManager.LoadScene("StandardStandBy");
    }
}