using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EPlayerError
{
    NONE,
    NO_ACTION_SELECTED,
    NOT_ENOUGH_ENERGY
}

public class PlayerErrorText : MonoBehaviour
{
    public static PlayerErrorText Instance;
    [SerializeField]
    private TextMeshProUGUI errorMessage;
    [SerializeField]
    private float fadeTime, fadeReduction;

    Coroutine currentFade;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Displays error text
    /// </summary>
    /// <param name="_errorType"></param>
    public void DisplayPlayerError(EPlayerError _errorType)
    {
        switch (_errorType)
        {
            case EPlayerError.NONE:
                break;
            case EPlayerError.NO_ACTION_SELECTED:
                errorMessage.text = "A Unit has not selected its action yet!";
                break;
            case EPlayerError.NOT_ENOUGH_ENERGY:
                errorMessage.text = "That unit does not have enough Energy for that!";
                break;
            default:
                break;
        }
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        currentFade = StartCoroutine(FadeOutMessage());
    }

    /// <summary>
    /// Fades error text message out
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutMessage()
    {
        float messageAlpha = 255;
        errorMessage.color = new Color32((byte)Mathf.RoundToInt(errorMessage.color.r * 255),
                (byte)Mathf.RoundToInt(errorMessage.color.g * 255), (byte)Mathf.RoundToInt(errorMessage.color.b * 255), (byte)Mathf.RoundToInt(messageAlpha));
        while (errorMessage.color.a > 0)
        {
            messageAlpha -= fadeReduction;
            if (messageAlpha < 0)
            {
                messageAlpha = 0;
            }
            errorMessage.color = new Color32((byte)Mathf.RoundToInt(errorMessage.color.r * 255),
                (byte)Mathf.RoundToInt(errorMessage.color.g * 255), (byte)Mathf.RoundToInt(errorMessage.color.b * 255), (byte)Mathf.RoundToInt(messageAlpha));
            yield return new WaitForSeconds(fadeTime * (1 / (255 / fadeReduction)));
        }
        currentFade = null;
    }
}
