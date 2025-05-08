using Oculus.Interaction.Body.Input;
using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

// Definizione della struct SideBodyTracking
struct SideBodyTracking
{
    public Transform HandObject;
    public Transform ShoulderChestObject;
    public Transform UpperArmObject;
    public float ArmLength;
    public string Topic;

    public SideBodyTracking(Transform handObject, Transform shoulderChestObject, Transform upperArmObject, float armLength, string topic)
    {
        HandObject = handObject;
        ShoulderChestObject = shoulderChestObject;
        UpperArmObject = upperArmObject;
        ArmLength = armLength;
        Topic = topic;
    }
}

public class BodyTrackingGameObjectManager : MonoBehaviour
{

    ROSConnection ros;
    //public Animator animator; // Riferimento all'Animator dell'avatar
    public Transform leftHandObject; 
    public Transform rightHandObject; 
    public Transform leftShoulderChestObject; 
    public Transform rightShoulderChestObject; 
    public Transform rightArmUpperObject;
    public Transform leftArmUpperObject;
    public Transform Chest;
    public Transform Hips;


    private string LeftWristTopic = "/hand_L_ref";
    private string RightWristTopic = "/hand_R_ref";
    float leftArmLength;
    float rightArmLength;
    private string robot_namespace;
    // Inizializzazione delle struct per il lato sinistro e destro
    SideBodyTracking leftSide;
    SideBodyTracking rightSide;


    private bool initialized = false;
    private void OnEnable()
    {
        robot_namespace = SwitchRobotManager.robot_name;
    }


    void Update()
    {
        if (PlayPausePublisher.enable_out)
        {
            // Setto il topic in base alla variabile booleana
            if (!initialized)
                SetTopic_and_ArmLenght();

            PosePublisher("right");
            PosePublisher("left");

        }
    }

    void SetTopic_and_ArmLenght()
    {
        LeftWristTopic = "/" + robot_namespace + LeftWristTopic;
        RightWristTopic = "/" + robot_namespace + RightWristTopic;

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>(LeftWristTopic);
        ros.RegisterPublisher<PoseMsg>(RightWristTopic);

        rightArmLength = CalculateArmLength(rightHandObject, Chest);
        leftArmLength = CalculateArmLength(leftHandObject, Chest);
        Debug.LogWarning("Right arm length: " + rightArmLength);
        Debug.LogWarning("Left arm length: " + leftArmLength);

        leftSide = new SideBodyTracking(leftHandObject, leftShoulderChestObject, leftArmUpperObject, leftArmLength, LeftWristTopic);
        rightSide = new SideBodyTracking(rightHandObject, rightShoulderChestObject, rightArmUpperObject, rightArmLength, RightWristTopic);

        initialized = true;
    }


    float CalculateArmLength(Transform wrist, Transform shoulder)
    {
        // Calcola la distanza tra il polso e la spalla
        return Vector3.Distance(wrist.position, shoulder.position);
    }


    QuaternionMsg ConvertUnityQuaternionToRos(string side, Quaternion Quaternion )
    {
        // Ruota di 90 gradi intorno all'asse X
        //Quaternion conversionQuaternion = Quaternion.Euler(90, 0, 0);
        //Quaternion = conversionQuaternion * Quaternion;

        if (side == "left")
        {

            // Assumendo che `originalQuaternion` sia il Quaternion originale da cui vuoi ottenere gli angoli di Eulero
            Vector3 originalEulerAngles = Quaternion.eulerAngles;

            // Inverti gli angoli y e z moltiplicandoli per -1
            float invertedX = (originalEulerAngles.x * -1) + 360; // Aggiungi 360 per gestire valori negativi
            float invertedY = (originalEulerAngles.y * -1) + 360;

            // Assicurati che gli angoli siano normalizzati tra 0 e 360
            invertedX %= 360;
            invertedY %= 360;

            // Crea un nuovo Vector3 con gli angoli di Eulero corretti
            Vector3 correctedEulerAngles = new Vector3(invertedX, invertedY, originalEulerAngles.z);

            // Crea un nuovo Quaternion dai nuovi angoli di Eulero
            Quaternion correctedQuaternion = Quaternion.Euler(correctedEulerAngles);

            return new QuaternionMsg
            {
                x = correctedQuaternion.x,
                y = correctedQuaternion.y,
                z = correctedQuaternion.z,
                w = correctedQuaternion.w
            };

        }
        else if (side == "right")
        {

            // Assumendo che `originalQuaternion` sia il Quaternion originale da cui vuoi ottenere gli angoli di Eulero
            Vector3 originalEulerAngles = Quaternion.eulerAngles;

            // Inverti gli angoli y e z moltiplicandoli per -1
            float invertedX = (originalEulerAngles.x * -1) + 360; // Aggiungi 360 per gestire valori negativi
            float invertedZ = (originalEulerAngles.z * -1) + 360;

            // Assicurati che gli angoli siano normalizzati tra 0 e 360
            invertedX %= 360;
            invertedZ %= 360;

            // Crea un nuovo Vector3 con gli angoli di Eulero corretti
            Vector3 correctedEulerAngles = new Vector3(invertedX, originalEulerAngles.y, invertedZ);

            // Crea un nuovo Quaternion dai nuovi angoli di Eulero
            Quaternion correctedQuaternion = Quaternion.Euler(correctedEulerAngles);

            return new QuaternionMsg
            {
                x = correctedQuaternion.x,
                y = correctedQuaternion.y,
                z = correctedQuaternion.z,
                w = correctedQuaternion.w
            };
        }

        return new QuaternionMsg
        {
            x = Quaternion.x,
            y = Quaternion.y,
            z = Quaternion.z,
            w = Quaternion.w
        };






    }


    PointMsg ConvertUnityPointToRos(string side, Vector3 WristNormalized)
    {
        Vector3 OutputWristNormalized = new Vector3(0, 0, 0);
        // Definisci positionMsg con posizione 0
        if (side == "left")
        {
            OutputWristNormalized.x = WristNormalized.y;
            OutputWristNormalized.y = WristNormalized.z;
            OutputWristNormalized.z = -WristNormalized.x;
        }
        else if (side == "right")
        {
            OutputWristNormalized.x = WristNormalized.y;
            OutputWristNormalized.y = WristNormalized.z;
            OutputWristNormalized.z = -WristNormalized.x;

        }
        //Debug.Log(side + ": " + OutputWristNormalized);

        PointMsg WristPositionMsg = new PointMsg
        {
            x = OutputWristNormalized.x,
            y = OutputWristNormalized.y,
            z = OutputWristNormalized.z
        };

        return WristPositionMsg;
    }




    void PosePublisher(string side)
    {
        SideBodyTracking sideTracking = side == "left" ? leftSide : rightSide;



        Vector3 WristPositionRelativeToChest = Chest.InverseTransformPoint(sideTracking.HandObject.position);
        Vector3 ArmUpperPositionRelativeToChest = Chest.InverseTransformPoint(sideTracking.UpperArmObject.position);
        Vector3 WristPositionRelativeToUpperArm = new Vector3(WristPositionRelativeToChest.x - ArmUpperPositionRelativeToChest.x, WristPositionRelativeToChest.y - ArmUpperPositionRelativeToChest.y, WristPositionRelativeToChest.z - ArmUpperPositionRelativeToChest.z);

        //Debug.Log(side + "WristPositionRelativeToChest: " + WristPositionRelativeToChest);
        //Debug.Log(side + "ArmUpperPositionRelativeToChest: " + ArmUpperPositionRelativeToChest);
        //Debug.Log(side + "WristPositionRelativeToUpperArm: " + WristPositionRelativeToUpperArm);

        //Vector3 WristNormalized = HandPositionRelativeToUpperArm / sideTracking.ArmLength;
        Vector3 WristNormalized = WristPositionRelativeToUpperArm / sideTracking.ArmLength;

        Quaternion WristRotationRelativeToShoulder = Quaternion.Inverse(Hips.rotation) * sideTracking.HandObject.rotation;

        PoseMsg wristPoseMsg = new PoseMsg(
            ConvertUnityPointToRos(side, WristNormalized), 
            ConvertUnityQuaternionToRos(side, WristRotationRelativeToShoulder)
            );

        ros.Publish(sideTracking.Topic, wristPoseMsg);

    }
}

