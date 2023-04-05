using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ActionEvent : UnityEvent<PlayerUnit.EAction, int> { }

public class StateSignifier : MonoBehaviour
{
    [SerializeField]
    PlayerUnit myUnit;

    [SerializeField]
    MeshRenderer mr;

    [SerializeField]
    Material idleMat, attackMat, defendMat;

    [SerializeField]
    TextMeshProUGUI healthText;


    void Start()
    {
        myUnit.MyActionEvent.AddListener(ChangeActionSignifier);
        myUnit.OnHealthChanged.AddListener(HealthChangeSignifier);

        ChangeActionSignifier(0, 0);
        HealthChangeSignifier();
    }

    /// <summary>
    /// Changes the color of the image that displays what action was chosen
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_weight"></param>
    private void ChangeActionSignifier(PlayerUnit.EAction _action, int _weight)
    {
        switch (_action)
        {
            case PlayerUnit.EAction.IDLE:
                mr.material.color = idleMat.color;
                break;
            case PlayerUnit.EAction.ATTACK:
                mr.material.color = attackMat.color;
                break;
            case PlayerUnit.EAction.DEFEND:
                mr.material.color = defendMat.color;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Changes the displayed Health of the unit
    /// </summary>
    private void HealthChangeSignifier()
    {
        if (healthText != null)
        {
            healthText.text = "" + myUnit.HealthPoints;
        }
    }
}
