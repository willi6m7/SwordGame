using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.NetworkVariable.Collections;
using MLAPI.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPLobbyScript : NetworkBehaviour
{
    [SerializeField] private LobbyPlayerScript[] lobbyPlayers;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Button startButton;

    private NetworkList<MPPlayerInfo> nwPlayers = new NetworkList<MPPlayerInfo>();

    [SerializeField] private GameObject chatPrefab;

    void Start()
    {
        if (IsOwner)
        {
            UpdateConnListServerRPC(NetworkManager.LocalClientId);
        }
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    public override void NetworkStart()
    {
        Debug.Log("Server Starting...");
        nwPlayers.OnListChanged += PlayersInfoChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectedHandle;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedHandle;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ClientConnectedHandle(client.ClientId);
            }
        }
    }

    private void OnDestroy()
    {
        nwPlayers.OnListChanged -= PlayersInfoChanged;
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnectedHandle;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnectedHandle;
        }
    }

    private void PlayersInfoChanged(NetworkListEvent<MPPlayerInfo> changeEvent)
    {
        int index = 0;
        foreach (MPPlayerInfo connectedPlayer in nwPlayers)
        {
            lobbyPlayers[index].playerName.text = connectedPlayer.networkPlayerName;
            lobbyPlayers[index].readyIcon.SetIsOnWithoutNotify(connectedPlayer.networkPlayerReady);
            index++;
        }
        for (; index < 2; index++)
        {
            lobbyPlayers[index].playerName.text = "Player Name";
            lobbyPlayers[index].readyIcon.SetIsOnWithoutNotify(false);
            index++;
        }
        if (IsHost)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = CheckIfEveryoneReady();
        }
    }

    public void StartGame()
    {
        if (IsServer)
        {
            NetworkSceneManager.OnSceneSwitched += SceneSwitched;
            NetworkSceneManager.SwitchScene("SampleScene");
        }
        else Debug.Log("You are not the host!");
    }

    private void HandleClientConnected(ulong clientId)
    {
        UpdateConnListServerRPC(clientId);
        Debug.Log("A player has connected to ID: " + clientId);
    }

    [ServerRpc]
    private void UpdateConnListServerRPC(ulong clientId)
    {
        nwPlayers.Add(new MPPlayerInfo(clientId, PlayerPrefs.GetString("PName"), false));
    }

    private void ClientDisconnectedHandle(ulong clientId)
    {
        for (int indx = 0; indx < nwPlayers.Count; indx++)
        {
            if (clientId == nwPlayers[indx].networkClientId)
            {
                nwPlayers.RemoveAt(indx);
                Debug.Log(clientId + " Disconnected");
                break;
            }
        }

    }

    private void ClientConnectedHandle(ulong clientId)
    {
        Debug.Log("Connected!");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReadyUpServerRPC(ServerRpcParams serverRpcParams = default)
    {
        for (int indx = 0; indx < nwPlayers.Count; indx++)
        {
            if (nwPlayers[indx].networkClientId == serverRpcParams.Receive.SenderClientId)
            {
                Debug.Log("Updated with new");
                nwPlayers[indx] = new MPPlayerInfo(nwPlayers[indx].networkClientId, nwPlayers[indx].networkPlayerName, !nwPlayers[indx].networkPlayerReady);
            }
        }
    }

    public void ReadyButtonPressed()
    {
        ReadyUpServerRPC();
    }

    private void SceneSwitched()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        GameObject[] spawnPointsTwo = GameObject.FindGameObjectsWithTag("SpawnPoint2");

        foreach (MPPlayerInfo tmpClient in nwPlayers)
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int index = UnityEngine.Random.Range(0, spawnPoints.Length);
            //int indexTwo = UnityEngine.Random.Range(0, spawnPointsTwo.Length);
            GameObject currentPoint = spawnPoints[index];
            //GameObject currentPointTwo = spawnPointsTwo[indexTwo];

            //Player spawn
            GameObject playerSpawn = Instantiate(playerPrefab, currentPoint.transform.position, Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientId);
            Debug.Log("Player Spawned For: " + tmpClient.networkPlayerName);

            //Player spawn 2
            //Unfortunately does not work

            /*
            GameObject playerSpawnTwo = Instantiate(playerPrefab, currentPointTwo.transform.position, Quaternion.identity);
            playerSpawnTwo.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientId);
            Debug.Log("Player Spawned For: " + tmpClient.networkPlayerName);
            */



            //spawn chat
            GameObject chatUISpawn = Instantiate(chatPrefab);
            chatUISpawn.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientId);
            chatUISpawn.GetComponent<MPChatUIScript>().chatPlayers = nwPlayers;
            Debug.Log("Chat Spawned For: " + tmpClient.networkPlayerName);
        }
    }

    private bool CheckIfEveryoneReady()
    {
        foreach (MPPlayerInfo player in nwPlayers)
        {
            if (!player.networkPlayerReady)
            {
                return false;
            }
        }
        return true;
    }
}
