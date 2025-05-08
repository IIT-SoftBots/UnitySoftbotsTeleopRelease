using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RobotNamespaceSaver : MonoBehaviour
{
    public TextMeshProUGUI myTextMeshPro;
    public static string robot_name;

    void Start()
    {
    }

    public void OnButtonClicked()
    {
        robot_name = myTextMeshPro.text;
    }
}
