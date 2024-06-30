using UnityEngine.SceneManagement;
using UnityEngine;

public class CheckMenuLoad : MonoBehaviour
{
    public static bool menuLoaded;

    private void Awake()
    {
        if (!menuLoaded)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                menuLoaded = true;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

    }
}
