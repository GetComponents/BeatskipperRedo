using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitInfo : MonoBehaviour
{
    [SerializeField]
    PlayerUnitManager myManager;

    [SerializeField]
    TextMeshProUGUI unitName, hpText, energyText, logText;

    void Start()
    {
        myManager.UnitSelected.AddListener(ChangeUnitInfoUI);
    }

    /// <summary>
    /// Displays information of the currently selected unit
    /// </summary>
    private void ChangeUnitInfoUI()
    {
        unitName.text = myManager.SelectedUnit.gameObject.name;
        hpText.text = $"HP: {myManager.SelectedUnit.HealthPoints} / {myManager.SelectedUnit.MaxHealthPoints}";
        if (myManager.EnergyUsedThisTurn[myManager.SelectedUnit] != 0)
        {
            energyText.text = $"Energy: {myManager.SelectedUnit.Energy} (-{myManager.EnergyUsedThisTurn[myManager.SelectedUnit]})/ {myManager.SelectedUnit.MaxEnergy}";
        }
        else
        {
            energyText.text = $"Energy: {myManager.SelectedUnit.Energy} / {myManager.SelectedUnit.MaxEnergy}";
        }
    }
}
