using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;
using RosMessageTypes.Std;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;

public class FaceExpressionManager : MonoBehaviour
{
    private bool wasButtonPressed = false;
    private ROSConnection ros;

    public GameObject LeftEmoticonsList;
    public GameObject RightEmoticonsList;
    public GameObject LeftEmoticons2Send;
    public GameObject RightEmoticons2Send;

    private bool isListActive = false; // Variabile per controllare se la lista è attiva

    private Transform[] leftEmoticons; // Array per contenere gli elementi della lista sinistra
    private Transform[] rightEmoticons; // Array per contenere gli elementi della lista destra

    public GameObject RightCircle;
    public GameObject LeftCircle;
    private int currentIndex = 1; // Indice corrente per la lista

    private bool isMovingUp = false; // Stato del movimento verso l'alto
    private bool isMovingDown = false; // Stato del movimento verso il basso
    private Coroutine currentCoroutine;

    private bool initialized = false;
    public string Topic = "";
    private string buttonYTopic;
    //public static bool enable_out = false;
    private string robot_namespace;

    void Start()
    {
        robot_namespace = SwitchRobotManager.robot_name;
        // Verifica se il robot_namespace è "robot_alterego3" o "robot_alterego5"
        if (robot_namespace == "robot_alterego3" || robot_namespace == "robot_alterego5")
        {
            // Inizializza gli array di emoticons
            leftEmoticons = LeftEmoticonsList.GetComponentsInChildren<Transform>();
            rightEmoticons = RightEmoticonsList.GetComponentsInChildren<Transform>();
        }
        else
        {
            // Disattiva il GameObject se il robot_namespace non corrisponde
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Setto il topic in base alla variabile booleana
        if (!initialized)
            SetTopic();


        if (isListActive)
        {
            // Gestisci il movimento del thumbstick
            float thumbstickVertical = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

            // Muove verso l'alto
            if (thumbstickVertical > 0.6f && !isMovingUp)
            {
                MoveSelection(ref currentIndex);
                isMovingUp = true; // Imposta lo stato di movimento verso l'alto
                isMovingDown = false; // Resetta lo stato di movimento verso il basso
            }
            // Muove verso il basso
            else if (thumbstickVertical < -0.6f && !isMovingDown)
            {
                MoveSelection(ref currentIndex, true);
                isMovingDown = true; // Imposta lo stato di movimento verso il basso
                isMovingUp = false; // Resetta lo stato di movimento verso l'alto
            }
            // Resetta gli stati quando il thumbstick è in posizione neutra
            else if (thumbstickVertical < 0.5f && thumbstickVertical > -0.5f)
            {
                isMovingUp = false;
                isMovingDown = false;
            }

            // Aggiorna le posizioni dei cerchi
            UpdateCirclePositions();

            bool buttonPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch);
            if (wasButtonPressed && !buttonPressed) // Quando si rilascia il pulsante
            {
                // Disattiva la lista e i cerchi solo se l'indice corrente non è 1
                LeftEmoticonsList.SetActive(false);
                RightEmoticonsList.SetActive(false);
                RightCircle.SetActive(false);
                LeftCircle.SetActive(false);
                isListActive = false;

                if (currentIndex != 1)
                {
                    LeftEmoticons2Send.SetActive(true);
                    RightEmoticons2Send.SetActive(true);
                    ActivateEmoticonInSend(LeftEmoticons2Send);
                    ActivateEmoticonInSend(RightEmoticons2Send);

                    // Interrompi la coroutine precedente se esiste
                    if (currentCoroutine != null)
                    {
                        StopCoroutine(currentCoroutine);
                    }
                    currentCoroutine = StartCoroutine(DeactivateEmoticonAfterDelay(LeftEmoticons2Send, RightEmoticons2Send, 5f));
                    PublishMessage();
                }
            }
            wasButtonPressed = buttonPressed; // Aggiorna lo stato del pulsante
        }
        else
        {
            bool buttonPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch);
            if (wasButtonPressed && !buttonPressed && PlayPausePublisher.enable_out) // Quando si rilascia il pulsante
            {
                LeftEmoticons2Send.SetActive(false);
                RightEmoticons2Send.SetActive(false);
                // Attiva le liste delle emoticons
                LeftEmoticonsList.SetActive(true);
                RightEmoticonsList.SetActive(true);
                RightCircle.SetActive(true);
                LeftCircle.SetActive(true);
                isListActive = true;
            }
            wasButtonPressed = buttonPressed; // Aggiorna lo stato del pulsante
        }
    }

    void PublishMessage()
    {
        Float64Msg message = new Float64Msg(currentIndex-1);
        ros.Publish(buttonYTopic, message);
    }
    void SetTopic()
    {
        buttonYTopic = "/" + robot_namespace + Topic;


        if (!string.IsNullOrEmpty(buttonYTopic))
        {

            // Ricordati di registrare nuovamente il publisher se cambi il topic
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<Float64Msg>(buttonYTopic);
        }
        initialized = true;
    }

    void MoveSelection(ref int currentIndex, bool down = false)
    {
        // Aggiorna l'indice parte da 0 e quando va verso l'alto decrementa, quando va verso il basso incrementa
        if (down)
        {
            currentIndex++;
            if (currentIndex >= leftEmoticons.Length)
            {
                currentIndex = 1; // Ricomincia dal primo
            }
        }
        else
        {
            currentIndex--;
            if (currentIndex < 1)
            {
                currentIndex = leftEmoticons.Length - 1; // Ricomincia dall'ultimo
            }
        }
    }

    void UpdateCirclePositions()
    {
        // Posiziona i cerchi sugli elementi correnti
        RightCircle.transform.position = rightEmoticons[currentIndex].position;
        LeftCircle.transform.position = leftEmoticons[currentIndex].position;
    }

    void ActivateEmoticonInSend(GameObject emoticons2Send)
    {
        // Disattiva tutti gli oggetti all'interno di emoticons2Send
        foreach (Transform emoticon in emoticons2Send.transform)
        {
            emoticon.gameObject.SetActive(false);
        }

        // Attiva l'oggetto corrispondente all'indice corrente
        if ((currentIndex - 2) < emoticons2Send.transform.childCount)
        {
            emoticons2Send.transform.GetChild((currentIndex - 2)).gameObject.SetActive(true);
            //Debug.Log("Attivato: " + emoticons2Send.transform.GetChild((currentIndex - 2)).name);
        }
    }

    // Coroutine per disattivare l'emoticon dopo un certo periodo
    private IEnumerator DeactivateEmoticonAfterDelay(GameObject leftemoticons2Send, GameObject rightemoticons2Send, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Disattiva tutti gli oggetti all'interno di emoticons2Send
        foreach (Transform emoticon in leftemoticons2Send.transform)
        {
            emoticon.gameObject.SetActive(false);
        }

        foreach (Transform emoticon in rightemoticons2Send.transform)
        {
            emoticon.gameObject.SetActive(false);
        }
    }
}
