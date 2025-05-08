using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Robotics.ROSTCPConnector;
using System.Collections.Generic;

public class SwitchRobotManager : MonoBehaviour
{
    public static string robot_name;
    private OVRInput.Button primaryTriggerButton = OVRInput.Button.PrimaryIndexTrigger;
    private OVRInput.Button secondaryTriggerButton = OVRInput.Button.SecondaryIndexTrigger;
    public GameObject PausePainting;
    public GameObject VideoStreamReceiver;
    
    public GameObject CustomROSConnector;

    public Camera LeftEye;
    public Camera RightEye;
    public GameObject StereoCamera;



    public Camera ActiveCamera; // Assegna la tua camera qui
    public string ToolTipsPlayLayer; // Nome del layer che vuoi controllare
    public string ToolTipsSelectLayer; // Nome del layer che vuoi controllare
    private int ToolTipsPlayLayerMask;
    private int ToolTipsSelectLayerMask;

    void Start()
    {
        LeftEye.enabled = false;
        RightEye.enabled = false;
        StereoCamera.SetActive(true);


        PausePainting.SetActive(false);
        CustomROSConnector.SetActive(false);


        // Ottieni l'indice del layer
        int PlayLayer = LayerMask.NameToLayer(ToolTipsPlayLayer);
        int SelectLayer = LayerMask.NameToLayer(ToolTipsSelectLayer);


        ToolTipsPlayLayerMask = 1 << PlayLayer;
        ToolTipsSelectLayerMask = 1 << SelectLayer;
        UpdateLayerVisibility(false);


    }

    private void OnEnable()
    {
        //Attivo tutti gli oggetti con il tag Tooltips
        UpdateLayerVisibility(false);
    }
    public void UpdateLayerVisibility(bool isVisible)
    {
        if (ActiveCamera == null)
        {
            Debug.LogError("Camera non assegnata.");
            return;
        }

        if (isVisible)
        {
            ActiveCamera.cullingMask |= ToolTipsPlayLayerMask; // Aggiungi il layer alla mask
            ActiveCamera.cullingMask &= ~ToolTipsSelectLayerMask; // Rimuovi il layer dalla mask
        }
        else
        {
            ActiveCamera.cullingMask &= ~ToolTipsPlayLayerMask; // Rimuovi il layer dalla mask
            ActiveCamera.cullingMask |= ToolTipsSelectLayerMask; // Aggiungi il layer alla mask
        }

    }

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
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Collider collider = hit.collider;
            if (collider is BoxCollider)
            {

                TextMeshPro textMeshPro = collider.GetComponentInChildren<TextMeshPro>();
                if (textMeshPro != null)
                {
                    robot_name = "robot_" + textMeshPro.text;
                    PausePainting.SetActive(true);
                    VideoStreamReceiver.SetActive(true);
                    CustomROSConnector.SetActive(true);

                    UpdateLayerVisibility(true);

                }
                else
                {
                    Debug.LogWarning("No TextMeshPro component found in clicked object!");
                }
            }
        }
    }

}