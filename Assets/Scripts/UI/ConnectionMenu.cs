using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionMenu : MonoBehaviour
{
    public Button connectButton;
    public Button hostButton;
    public string gameSceneName = "GameScene";

    private void Start()
    {
        connectButton.onClick.AddListener(ConnectToServer);
        hostButton.onClick.AddListener(StartHost);
    }

    private void ConnectToServer()
    { 
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
    }
}
