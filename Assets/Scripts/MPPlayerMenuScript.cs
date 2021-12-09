using MLAPI;
using MLAPI.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPPlayerMenuScript : NetworkBehaviour
{
    [SerializeField] private InputField playerName;
    public void OnHostButtonClicked()
    {
        PlayerPrefs.SetString("PName", playerName.text);
        NetworkManager.Singleton.StartHost();
        NetworkSceneManager.SwitchScene("LobbyScene");
        //startMenu.SetActive(false);
    }

    public void OnClientButtonClicked()
    {
        PlayerPrefs.SetString("PName", playerName.text);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client Started");
        //startMenu.SetActive(false);
    }
}
