using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public bool autoStart = true;

    public void LoadScene(string sceneName)
    {
        if (!PlayPausePublisher.enable_out || autoStart)
        {
            // Libera memoria non utilizzata
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            // Avvia il caricamento asincrono della scena
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Attendi fino a quando il caricamento è completato
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
