using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    public Button connectButton1;
    public Button connectButton2;
    public Button hostButton;
    public string gameSceneName = "GameScene";

    private void Start()
    {
        connectButton1.onClick.AddListener(() => ConnectToServer(1));
        connectButton2.onClick.AddListener(() => ConnectToServer(2));
        hostButton.onClick.AddListener(StartHost);
    }

    private void ConnectToServer(int clientNumber)
    {
        ClientIdentifier.ClientNumber = clientNumber;
        ClientIdentifier.InitializeClientId(clientNumber);
        NetworkManager.Singleton.StartClient();
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
        {
            SceneManager.LoadScene(gameSceneName);
        }

        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.IsServer)
        {
            SceneManager.LoadScene(gameSceneName);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
