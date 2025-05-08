using System.Collections.Generic;
using TMPro; // Assicurati di avere questo using per accedere a TextMeshPro
using UnityEngine;
using UnityEngine.UI; // Per accedere ai componenti dell'immagine e del bottone

public class DiscoveryOnlineRobot: MonoBehaviour
{
    public TextMeshProUGUI textMeshProComponent; // Assegna questo dall'inspector
    public Image imageToChange; // Assegna anche questo dall'inspector
    public Button buttonToDisable; // Aggiungi un riferimento al bottone dall'inspector

    void Update()
    {
        // Assicurati che UDPListener sia inizializzato e che i componenti siano assegnati
        if (UDPListener.Instance != null && textMeshProComponent != null && imageToChange != null && buttonToDisable != null)
        {
            List<string> robotNames = UDPListener.Instance.GetRobotNames();
            string currentText = textMeshProComponent.text;

            // Controlla se il nome corrente è presente nella lista dei nomi dei robot
            if (robotNames.Contains(currentText))
            {
                // Cambia il colore dell'immagine in bianco
                imageToChange.color = Color.white;
                // Disabilita il bottone
                buttonToDisable.interactable = true;
            }
            else
            {
                // Opzionale: reimposta il colore se il nome non corrisponde
                imageToChange.color = new Color(100f / 255f, 100f / 255f, 100f / 255f); // Imposta il colore a grigio (100, 100, 100)

                // Riabilita il bottone
                buttonToDisable.interactable = false;
            }
        }
    }
}
