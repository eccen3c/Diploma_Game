using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NetworkLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject panelChoice;
    public GameObject panelOnline;
    public GameObject panelLobby;
    public GameObject panelJoin;

    [Header("Lobby UI")]
    public TextMeshProUGUI textRoomCode;
    public TextMeshProUGUI textPlayer1Name;
    public TextMeshProUGUI textPlayer2Name;
    public Button buttonStartGame;

    [Header("Join UI")]
    public TMP_InputField inputRoomCode;
    public TextMeshProUGUI textJoinError;

    private string myNickname;

    void Start()
    {
        myNickname = GenerateNickname();
        PhotonNetwork.NickName = myNickname;
        PhotonNetwork.AutomaticallySyncScene = true;

        ShowPanel(panelChoice);

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    // --- Генерація нікнейму ---

    private string GenerateNickname()
    {
        string saved = PlayerPrefs.GetString("PlayerNickname", "");
        if (!string.IsNullOrEmpty(saved)) return saved;

        string[] animals = { "Fox", "Wolf", "Bear", "Eagle", "Tiger", "Hawk", "Lion", "Lynx" };
        string nick = animals[Random.Range(0, animals.Length)] + "#" + Random.Range(1000, 9999);
        PlayerPrefs.SetString("PlayerNickname", nick);
        return nick;
    }

    // --- Навігація між панелями ---

    public void ShowPanel(GameObject panel)
    {
        if (panelChoice) panelChoice.SetActive(panel == panelChoice);
        if (panelOnline) panelOnline.SetActive(panel == panelOnline);
        if (panelLobby) panelLobby.SetActive(panel == panelLobby);
        if (panelJoin) panelJoin.SetActive(panel == panelJoin);
    }

    public void OnClickOnline() => ShowPanel(panelOnline);
    public void OnClickCreateOpen() { ShowPanel(panelLobby); CreateRoom(); }
    public void OnClickJoinOpen() => ShowPanel(panelJoin);
    public void OnClickBack() => ShowPanel(panelOnline);
    public void OnClickBackToChoice() => ShowPanel(panelChoice);

    public void OnClickLeaveLobby()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            ShowPanel(panelOnline);
    }

    public override void OnLeftRoom()
    {
        ShowPanel(panelOnline);
    }

    // --- Створити кімнату ---

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;

        string code = Random.Range(1000, 9999).ToString();
        RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = true, IsOpen = true };
        PhotonNetwork.CreateRoom(code, options, TypedLobby.Default);
    }

    // --- Приєднатись до кімнати ---

    public void JoinRoom()
    {
        if (inputRoomCode == null) return;
        string code = inputRoomCode.text.Trim();
        if (code.Length != 4) return;

        if (textJoinError) textJoinError.gameObject.SetActive(false);
        PhotonNetwork.JoinRoom(code);
    }

    // --- Photon callbacks ---

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        ShowPanel(panelLobby);

        if (textRoomCode) textRoomCode.text = PhotonNetwork.CurrentRoom.Name;
        if (textPlayer1Name) textPlayer1Name.text = myNickname;
        if (textPlayer2Name) textPlayer2Name.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.Get("waiting") : "Waiting...";
        if (buttonStartGame) buttonStartGame.interactable = false;
    }

    public override void OnJoinedRoom()
    {
        ShowPanel(panelLobby);

        if (textRoomCode) textRoomCode.text = PhotonNetwork.CurrentRoom.Name;

        // Оновлюємо слоти гравців
        UpdatePlayerSlots();

        // Клієнт не може стартувати гру
        if (buttonStartGame) buttonStartGame.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerSlots();

        if (buttonStartGame && PhotonNetwork.CurrentRoom.PlayerCount == 2)
            buttonStartGame.interactable = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerSlots();

        if (buttonStartGame)
            buttonStartGame.interactable = PhotonNetwork.CurrentRoom.PlayerCount == 2;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (textJoinError)
        {
            textJoinError.gameObject.SetActive(true);
            textJoinError.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.Get("room_not_found") : "Room not found!";
        }
    }

    // --- Старт гри ---

    public void StartOnlineGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        GameSession.mode = GameMode.OnlineMulti;
        PhotonNetwork.LoadLevel("GameScene");
    }

    // --- Хелпери ---

    private void UpdatePlayerSlots()
    {
        var players = PhotonNetwork.PlayerList;

        if (textPlayer1Name)
            textPlayer1Name.text = players.Length > 0 ? players[0].NickName : "—";

        if (textPlayer2Name)
            textPlayer2Name.text = players.Length > 1 ? players[1].NickName : (LocalizationManager.Instance != null ? LocalizationManager.Instance.Get("waiting") : "Waiting...");
    }
}
