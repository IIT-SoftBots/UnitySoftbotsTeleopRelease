using UnityEngine;

public class MaterialMirror : MonoBehaviour
{
    public Material sourceMaterial;   // Materiale sorgente da cui copiare la texture
    public GameObject mirrorPlane;    // Piano specchio

    private Renderer mirrorRenderer;

    void Start()
    {
        if (sourceMaterial == null)
        {
            Debug.LogError("Materiale sorgente non assegnato!");
            return;
        }

        if (mirrorPlane == null)
        {
            Debug.LogError("Piano specchio non assegnato!");
            return;
        }

        mirrorRenderer = mirrorPlane.GetComponent<Renderer>();
        if (mirrorRenderer == null)
        {
            Debug.LogError("Renderer non trovato sul piano specchio!");
            return;
        }

        // Imposta la texture iniziale
        UpdatePlaneMaterial();
    }

    void Update()
    {
        // Aggiorna la texture se necessario
        UpdatePlaneMaterial();
    }

    void UpdatePlaneMaterial()
    {
        if (sourceMaterial.mainTexture != null)
        {
            // Copia la texture dal materiale sorgente al materiale del piano specchio
            mirrorRenderer.material.mainTexture = sourceMaterial.mainTexture;
        }
    }
}
