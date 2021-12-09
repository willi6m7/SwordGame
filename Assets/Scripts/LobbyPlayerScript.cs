using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerScript : MonoBehaviour
{
    [SerializeField] public Text playerName;
    [SerializeField] public Toggle readyIcon;
    [SerializeField] private Text waitingText;
    internal void UpdatePlayerName(Text playerNameIn)
    {
        playerName = playerNameIn;

    }
}
