using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NetworkLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Елементи інтерфейсу Лобі")]
    public TextMeshProUGUI textRoomCodeDisplay;
    public TMP_InputField inputFieldRoomCode;
    public Button buttonJoinRoom;

    private string savedRoomCode = "";

    void Start()
    {
        // НАДВАЖЛИВО ДЛЯ ОНЛАЙНУ: синхронне завантаження сцени
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Підключення до серверу Photon...");
        }

        if (buttonJoinRoom != null)
            buttonJoinRoom.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Успішно підключено до серверу Photon Master!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Успішно зайшли в Головне Лобі.");
        // Якщо був збережений код, який не ввівся через збій сервера — заходимо зараз
        if (!string.IsNullOrEmpty(savedRoomCode))
        {
            PhotonNetwork.JoinRoom(savedRoomCode);
            savedRoomCode = "";
        }
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Фотон ще не готовий, зачекай секунду!");
            return;
        }

        string randomRoomCode = Random.Range(1000, 9999).ToString();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.CreateRoom(randomRoomCode, roomOptions, TypedLobby.Default);

        if (textRoomCodeDisplay != null)
        {
            textRoomCodeDisplay.text = "Ваш код: " + randomRoomCode;
        }
        Debug.Log("Кімнату створено на сервері! Код: " + randomRoomCode);
    }

    public void OnInputFieldChanged()
    {
        if (inputFieldRoomCode != null && buttonJoinRoom != null)
        {
            buttonJoinRoom.interactable = (inputFieldRoomCode.text.Length == 4);
        }
    }

    public void JoinRoom()
    {
        if (inputFieldRoomCode == null) return;

        string codeToJoin = inputFieldRoomCode.text.Trim();
        if (string.IsNullOrEmpty(codeToJoin) || codeToJoin.Length != 4) return;

        // Захист: якщо ми не на Майстер-сервері, скидаємо і перепідключаємося
        if (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby && PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            savedRoomCode = codeToJoin;
            PhotonNetwork.Disconnect();
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        PhotonNetwork.JoinRoom(codeToJoin);
        Debug.Log("Спроба підключення до кімнати за кодом: " + codeToJoin);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Успішно зайшли в кімнату: " + PhotonNetwork.CurrentRoom.Name);
        CheckPlayersCountAndStart();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Інший гравець підключився!");
        CheckPlayersCountAndStart();
    }

    private void CheckPlayersCountAndStart()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        Debug.Log($"Гравців у кімнаті: {PhotonNetwork.CurrentRoom.PlayerCount}/2");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("У кімнаті 2 гравці! Завантажуємо спільну сцену через Photon...");
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Не вдалося приєднатися! Помилка: " + message);
        if (textRoomCodeDisplay != null)
        {
            textRoomCodeDisplay.text = "Код не знайдено!";
        }
    }
}