using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HealthCanvas : MonoBehaviour
{
    [SerializeField]
    RectTransform canvasTransform, barTransform;

    [SerializeField]
    public Canvas healthCanvas;

    [Header("User Settings")]
    [SerializeField]
    public EHealthbarType healthbarType;

    [SerializeField]
    Vector2 Size = new Vector2(2, 0.5f);
    [SerializeField]
    Vector2 Position = new Vector2(0, 1);

    public bool LookTowardsCamera = true;

    [SerializeField]
    float sizeConversion = 100, referencePPU = 5;

    private void Update()
    {
        UpdateBarType();
        UpdateTransform();
    }

    /// <summary>
    /// Because of ExecuteInEditMode happens whenever the canvas gets changed
    /// </summary>
    private void UpdateTransform()
    {
        switch (healthbarType)
        {
            case EHealthbarType.NONE:
                break;
            case EHealthbarType.OnScreen:
                barTransform.position = new Vector2(Screen.width * Position.x, Screen.height * Position.y)/2 + new Vector2(Screen.width, Screen.height)/2;
                barTransform.sizeDelta = Size * sizeConversion;
                break;
            case EHealthbarType.OnEnemy:
                canvasTransform.position = new Vector3(transform.position.x + Position.x, transform.position.y + Position.y, transform.position.z);
                barTransform.position = new Vector3(transform.position.x + Position.x, transform.position.y + Position.y, transform.position.z);
                canvasTransform.sizeDelta = Size;
                barTransform.sizeDelta = Size;
                break;
            default:
                break;
        }
    }

    private void UpdateBarType()
    {
        switch (healthbarType)
        {
            case EHealthbarType.NONE:
                break;
            case EHealthbarType.OnScreen:
                healthCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                healthCanvas.referencePixelsPerUnit = sizeConversion * referencePPU;
                break;
            case EHealthbarType.OnEnemy:
                healthCanvas.renderMode = RenderMode.WorldSpace;
                healthCanvas.referencePixelsPerUnit = referencePPU;
                break;
            default:
                break;
        }
    }
}

public enum EHealthbarType
{
    NONE,
    OnScreen,
    OnEnemy
}
