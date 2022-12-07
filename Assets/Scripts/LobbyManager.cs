using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : NetworkBehaviour
{
    private List<LobbyPlayerPanel> playerPanels;

    public GameObject playerScrollContent;
    public TMPro.TMP_Text txtPlayerNumber;
    public Button btnStart;
    public Button btnReady;

    public Button btngroundCamo;
    public Button btnmountainCamo;
    public Button btnleafCamo;
    public LobbyPlayerPanel playerPanelPrefab;

    public NetworkVariable<int> ChooseMat = new NetworkVariable<int>(0); 
    public Material chosenMaterial;


    public void Awake() {
        playerPanels = new List<LobbyPlayerPanel>();
    }

    public void Start() {
        if (IsHost) {
            RefreshPlayerPanels();
            btnStart.onClick.AddListener(HostOnBtnStartClick);
            btnleafCamo.onClick.AddListener(leafPref);
            btngroundCamo.onClick.AddListener(groundPref);
            btnmountainCamo.onClick.AddListener(mountainPref);
        }

        if (IsClient) {
            btnReady.onClick.AddListener(ClientOnReadyClicked);
            btnleafCamo.onClick.AddListener(leafPref);
            btngroundCamo.onClick.AddListener(groundPref);
            btnmountainCamo.onClick.AddListener(mountainPref);
        }

        ApplyTexture();
    }

    private void leafPref()
    {
        btngroundCamo.gameObject.SetActive(false);
        btnmountainCamo.gameObject.SetActive(false);
    }

    private void groundPref()
    {
        btnmountainCamo.gameObject.SetActive(false);
        btnleafCamo.gameObject.SetActive(false);
    }

    private void mountainPref()
    {
        btngroundCamo.gameObject.SetActive(false);
        btnleafCamo.gameObject.SetActive(false);
    }


    public void ApplyTexture()
    {

        if (ChooseMat.Value == 1)
        {
            chosenMaterial = Resources.Load("Ground") as Material;
            transform.Find("RArm").transform.Find("ksvk").GetComponent<MeshRenderer>().material = chosenMaterial;
        }
        else if (ChooseMat.Value == 2)
        {
            chosenMaterial = Resources.Load("Mountain") as Material;
            transform.Find("RArm").transform.Find("ksvk").GetComponent<MeshRenderer>().material = chosenMaterial;
        }
        else
        {
            chosenMaterial = Resources.Load("Ground") as Material;
            transform.Find("RArm").transform.Find("ksvk").GetComponent<MeshRenderer>().material = chosenMaterial;
            chosenMaterial = Resources.Load("Leaf") as Material;
        }

        GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("LArm").GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("RArm").GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("REye").GetComponent<MeshRenderer>().material = chosenMaterial;
        transform.Find("LEye").GetComponent<MeshRenderer>().material = chosenMaterial;

    }


    public override void OnNetworkSpawn() {
        if (IsHost) {
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            btnReady.gameObject.SetActive(false);

            int myIndex = GameData.Instance.FindPlayerIndex(NetworkManager.LocalClientId);
            if(myIndex != -1) {
                PlayerInfo info = GameData.Instance.allPlayers[myIndex];
                info.isReady = true;
                GameData.Instance.allPlayers[myIndex] = info;
            }
        }

        if (IsClient && !IsHost) {
            btnStart.gameObject.SetActive(false);
        }

        txtPlayerNumber.text = $"Player #{NetworkManager.LocalClientId}";
        GameData.Instance.allPlayers.OnListChanged += ClientOnAllPlayersChanged;
        EnableStartIfAllReady();
    }

    public override void OnDestroy()
    {
        GameData.Instance.allPlayers.OnListChanged -= ClientOnAllPlayersChanged;
    }

    // -----------------------
    // Private
    // -----------------------
    private void AddPlayerPanel(PlayerInfo info) {
        LobbyPlayerPanel newPanel = Instantiate(playerPanelPrefab);
        newPanel.transform.SetParent(playerScrollContent.transform, false);
        newPanel.SetName($"Player {info.clientId.ToString()}");
        newPanel.SetColor(info.color);
        newPanel.SetReady(info.isReady);
        playerPanels.Add(newPanel);
    }

    private void RefreshPlayerPanels() {
        foreach (LobbyPlayerPanel panel in playerPanels) {
            Destroy(panel.gameObject);
        }
        playerPanels.Clear();

        foreach (PlayerInfo pi in GameData.Instance.allPlayers) {
            AddPlayerPanel(pi);
        }
    }

    private void EnableStartIfAllReady() {
        int readyCount = 0;
        foreach (PlayerInfo readyInfo in GameData.Instance.allPlayers) {
            if (readyInfo.isReady) {
                readyCount += 1;
            }
        }

        btnStart.enabled = readyCount == GameData.Instance.allPlayers.Count;
        if (btnStart.enabled) {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
        } else {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "<Waiting for Ready>";
        }
    }

    // -----------------------
    // Events
    // -----------------------
    private void ClientOnAllPlayersChanged(NetworkListEvent<PlayerInfo> changeEvent) {
        RefreshPlayerPanels();
    }

    private void HostOnBtnStartClick() {
        StartGame();        
    }

    private void HostOnClientConnected(ulong clientId) {
        EnableStartIfAllReady();
    }

    private void ClientOnReadyClicked() {
        ToggleReadyServerRpc();
    }


    // -----------------------
    // Public
    // -----------------------
    public void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("GameEnv", UnityEngine.SceneManagement.LoadSceneMode.Single);

        btnStart.enabled = false;
    }


    [ServerRpc(RequireOwnership = false)]
    public void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default) {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        int playerIndex = GameData.Instance.FindPlayerIndex(clientId);
        PlayerInfo info = GameData.Instance.allPlayers[playerIndex];

        info.isReady = !info.isReady;
        GameData.Instance.allPlayers[playerIndex] = info;

        EnableStartIfAllReady();
    }

}
