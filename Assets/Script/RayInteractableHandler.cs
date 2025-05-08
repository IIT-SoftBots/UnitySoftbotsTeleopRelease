using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RayInteractableHandler : MonoBehaviour
{
    public static string robot_name;
    public string sceneName = "VideoStreaming";
    private OVRInput.Button primaryTriggerButton = OVRInput.Button.PrimaryIndexTrigger;
    private OVRInput.Button secondaryTriggerButton = OVRInput.Button.SecondaryIndexTrigger;

    private void Update()
    {
        if (OVRInput.GetDown(primaryTriggerButton))
        {
            HandleRaycast(OVRInput.Controller.LTouch);
        }

        if (OVRInput.GetDown(secondaryTriggerButton))
        {
            HandleRaycast(OVRInput.Controller.RTouch);
        }
    }

    private void HandleRaycast(OVRInput.Controller controller)
    {
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(controller),
                          OVRInput.GetLocalControllerRotation(controller) * Vector3.forward);
        Debug.Log("clicked");
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Collider collider = hit.collider;
            if (collider is BoxCollider)
            {
                Debug.Log("Object clicked: " + collider.gameObject.name);

                TextMeshPro textMeshPro = collider.GetComponentInChildren<TextMeshPro>();
                if (textMeshPro != null)
                {
                    robot_name = "robot_" + textMeshPro.text;
                    Debug.Log("Robot name: " + robot_name);
                    // Libera memoria non utilizzata
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();

                    // Avvia il caricamento asincrono della scena
                    StartCoroutine(LoadSceneAsync(sceneName));
                }
                else
                {
                    Debug.LogWarning("No TextMeshPro component found in clicked object!");
                }
            }
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