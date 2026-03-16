using UnityEngine;
using UnityEngine.SceneManagement;

public static class BootstrapLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrap()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
        }
    }
}