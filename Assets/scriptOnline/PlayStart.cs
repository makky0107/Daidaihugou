using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

// MonoBehaviourPunCallbacks���p�����āAPUN�̃R�[���o�b�N���󂯎���悤�ɂ���
public class PlayStart : MonoBehaviour
{
    public int sCardNo;

    public void PlayStartButton()
    {
        sCardNo = 0;

        PlayerPrefs.SetInt("SCardKey", sCardNo);
        PlayerPrefs.Save();

        SceneManager.LoadScene("StandardStandBy");
    }
}