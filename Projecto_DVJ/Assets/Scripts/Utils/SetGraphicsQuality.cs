using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class QualitySettingsManager : MonoBehaviour
{
    public RenderPipelineAsset lowQualityAsset;
    public RenderPipelineAsset mediumQualityAsset;
    public RenderPipelineAsset highQualityAsset;

    public void OnChangeQuality(InputAction.CallbackContext context)
    {
        string key = context.control.displayName;

        switch (key)
        {
            case "0":
                SetQuality(0);
                break;
            case "1":
                SetQuality(1);
                break;
            case "2":
                SetQuality(2);
                break;
            default:
                Debug.LogWarning("Tecla no asignada a ninguna calidad.");
                break;
        }
    }

    private void SetQuality(int qualityLevel)
    {
        switch (qualityLevel)
        {
            case 0:
                QualitySettings.renderPipeline = lowQualityAsset;
                Debug.Log("Calidad baja activada.");
                break;
            case 1:
                QualitySettings.renderPipeline  = mediumQualityAsset;
                Debug.Log("Calidad media activada.");
                break;
            case 2:
                QualitySettings.renderPipeline  = highQualityAsset;
                Debug.Log("Calidad alta activada.");
                break;
            default:
                Debug.LogWarning("Calidad no soportada.");
                break;
        }
    }
}
