using UnityEngine;

public class ClientIdentifier : MonoBehaviour
{
    public static int ClientNumber { get; set; }
    public static readonly string ClientIdKeyPrefix = "ClientID_";
    private static string clientId;

    public static void InitializeClientId(int clientNumber)
    {
        string clientIdKey = ClientIdKeyPrefix + clientNumber;

        if (PlayerPrefs.HasKey(clientIdKey))
        {
            clientId = PlayerPrefs.GetString(clientIdKey);
        }
        else
        {
            clientId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(clientIdKey, clientId);
            PlayerPrefs.Save();
        }

        Debug.Log($"Client ID for Client {clientNumber}: {clientId}");
    }

    public static string GetClientId()
    {
        return clientId;
    }

    public static void SavePlayerPosition(Vector3 position)
    {
        string positionKey = clientId + "_Position";
        var saveStr = $"{position.x}:{position.y}:{position.z}";
        PlayerPrefs.SetString(positionKey, saveStr);
        PlayerPrefs.Save();
    }

    public static Vector3 LoadPlayerPosition()
    {
        string positionKey = clientId + "_Position";
        if (PlayerPrefs.HasKey(positionKey))
        {
            string[] positionValues = PlayerPrefs.GetString(positionKey).Split(':');
            float x = float.Parse(positionValues[0]);
            float y = float.Parse(positionValues[1]);
            float z = float.Parse(positionValues[2]);
            return new Vector3(x, y, z);
        }
        return Vector3.zero; // Default position if no saved data found
    }

    public static bool HasSavedPosition()
    {
        string positionKey = clientId + "_Position";
        return PlayerPrefs.HasKey(positionKey);
    }
}
