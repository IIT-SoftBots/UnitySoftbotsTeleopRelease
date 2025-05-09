using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RobotSpawner : MonoBehaviour
{
    public GameObject RobotTwinPrefab;
    public GameObject Alterego2Prefab;
    public GameObject Alterego3Prefab;
    public Transform desk;
    private Dictionary<string, GameObject> spawnedRobots = new Dictionary<string, GameObject>();
    //List<string> robotNames = new List<string> { "robot_twin" };
    //List<string> robotNames = new List<string> { "robot_goldenego", "robot_alterego3", "robot_twin" };
    void Update()
    {
        List<string> robotNames = UDPListener.Instance.GetRobotNames();

        for (int i = 0; i < robotNames.Count; i++)
        {
            string name = robotNames[i];
            if (!spawnedRobots.ContainsKey(name))
            {
                SpawnRobot(name, i);
            }
        }

        List<string> namesToRemove = new List<string>();
        foreach (var pair in spawnedRobots)
        {
            if (!robotNames.Contains(pair.Key))
            {
                DeactivateRobot(pair.Key);
                namesToRemove.Add(pair.Key);
            }
        }

        foreach (string name in namesToRemove)
        {
            spawnedRobots.Remove(name);
        }
    }

    void SpawnRobot(string name, int index)
    {
        Vector3 localPosition = new Vector3(CalculateXPosition(index), -3f, 0.2f);
        Vector3 worldPosition = desk.TransformPoint(localPosition);

        GameObject newRobot;

        if (name.Contains("alterego3") || name.Contains("alterego5"))
        {
            newRobot = Instantiate(Alterego3Prefab, worldPosition, Quaternion.identity);
        }
        else if (name.Contains("alterego2") || name.Contains("alterego4") || name.Contains("goldenego"))
        {
            newRobot = Instantiate(Alterego2Prefab, worldPosition, Quaternion.identity);
        }
        else if(name.Contains("twin"))
        {
            newRobot = Instantiate(RobotTwinPrefab, worldPosition, Quaternion.identity);
        }
        else
        {
            newRobot = Instantiate(Alterego2Prefab, worldPosition, Quaternion.identity);
        }
        spawnedRobots.Add(name, newRobot);

        TextMeshPro textMeshPro = newRobot.GetComponentInChildren<TextMeshPro>();
        if (textMeshPro != null)
        {
            string displayName = name.Replace("robot_", "");
            textMeshPro.text = displayName;
        }
        else
        {
            Debug.LogWarning("No TextMeshPro component found in prefab!");
        }
    }

    void DeactivateRobot(string name)
    {
        if (spawnedRobots.TryGetValue(name, out GameObject robot))
        {
            robot.SetActive(false); // Disattiva il GameObject
        }
    }

    public void DeactivateAllRobots()
    {
        foreach (var pair in spawnedRobots)
        {
            if (pair.Value != null)
            {
                pair.Value.SetActive(false);
            }
        }
    }

    public void ActivateAllRobots()
    {
        foreach (var pair in spawnedRobots)
        {
            if (pair.Value != null)
            {
                pair.Value.SetActive(true);
            }
        }
    }

    float CalculateXPosition(int index)
    {
        // Definisci il numero di passi necessari per raggiungere 5
        int stepsToReachFive = Mathf.CeilToInt(5f / 1f); // 5 passi

        // Se l'indice è inferiore a stepsToReachFive, calcola la posizione incrementale
        if (index < stepsToReachFive)
        {
            return index * 1f;
        }
        else
        {
            // Dopo aver raggiunto 5, alterna tra -1 e -2
            int relativeIndex = (index - stepsToReachFive) % 2;

            if (relativeIndex == 0)
            {
                return -1f;
            }
            else
            {
                return -2f;
            }
        }
    }

}
