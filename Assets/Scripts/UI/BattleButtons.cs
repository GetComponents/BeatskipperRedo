using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleButtons : MonoBehaviour
{
    [SerializeField]
    PlayerUnitManager myManager;

    [SerializeField]
    Image[] weightButtons;

    [SerializeField]
    Image idleButton, attackButton, defendButton;

    [SerializeField]
    TextMeshProUGUI button1Text, button2Text, button3Text;

    [SerializeField]
    TextMeshProUGUI weight1Text, weight2Text, weight3Text;

    [SerializeField]
    TextMeshProUGUI Unit1Info, Unit2Info;

    [SerializeField]
    string unit1DefaultText, unit2DefaultText;

    private PlayerUnit.EAction lastAction;

    private void Start()
    {
        myManager.UnitSelected.AddListener(TurnOffWeightButtons);
        myManager.OnStartCombat.AddListener(ResetTexts);
    }

    /// <summary>
    /// Readies up the player
    /// </summary>
    public void Commit()
    {
        if (Unit1Info.text != unit1DefaultText && Unit2Info.text != unit2DefaultText)
        {
            myManager.ReadyUp();
        }
        else
        {
            PlayerErrorText.Instance.DisplayPlayerError(EPlayerError.NO_ACTION_SELECTED);
        }
    }

    /// <summary>
    /// Resets the units action info after a round
    /// </summary>
    public void ResetTexts()
    {
        Unit1Info.text = unit1DefaultText;
        if (myManager.MyUnits.Count == 2)
        {
            Unit2Info.text = unit2DefaultText;
        }
        else
        {
            Unit2Info.text = "";
        }
    }

    /// <summary>
    /// Logs in the action the player chose for a unit
    /// </summary>
    /// <param name="_action"></param>
    public void LoginAction(int _action)
    {

        lastAction = (PlayerUnit.EAction)_action;
        if (myManager.SelectedUnit == null || myManager.SelectedUnit.isEnemy)
        {
            return;
        }

        ChangeWeightButtons(_action);
        switch (_action)
        {
            case 0:
                ChangeButtonColor(idleButton.color);
                break;
            case 1:
                ChangeButtonColor(attackButton.color);
                break;
            case 2:
                ChangeButtonColor(defendButton.color);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Changes the colors of the weight buttons
    /// </summary>
    /// <param name="_color"></param>
    private void ChangeButtonColor(Color _color)
    {
        foreach (Image image in weightButtons)
        {
            image.color = _color;
        }
    }

    /// <summary>
    /// Turns the weight buttons off. Gets triggered when a unit is selected
    /// </summary>
    private void TurnOffWeightButtons()
    {
        if (myManager.SelectedUnit == null || myManager.SelectedUnit.isEnemy)
        {
            idleButton.gameObject.SetActive(false);
            attackButton.gameObject.SetActive(false);
            defendButton.gameObject.SetActive(false);
        }
        else
        {
            idleButton.gameObject.SetActive(true);
            attackButton.gameObject.SetActive(true);
            defendButton.gameObject.SetActive(true);
        }
        foreach (Image image in weightButtons)
        {
            image.gameObject.SetActive(false);
        }
        if (myManager.SelectedUnit.playerClass == PlayerClass.SUPPORT)
        {
            button1Text.text = "Attack & Debuff";
            button2Text.text = "Generate Energy";
            button3Text.text = "Idle";
        }
        else
        {
            button1Text.text = "Attack";
            button2Text.text = "Defend";
            button3Text.text = "Idle";
        }
    }

    /// <summary>
    /// Changes the text of the weight depending on what action was selected
    /// </summary>
    /// <param name="_action"></param>
    private void ChangeWeightButtons(int _action)
    {
        foreach (Image image in weightButtons)
        {
            image.gameObject.SetActive(true);
        }
        idleButton.gameObject.SetActive(true);
        attackButton.gameObject.SetActive(true);
        defendButton.gameObject.SetActive(true);
        switch (_action)
        {
            case 0:
                foreach (Image image in weightButtons)
                {
                    image.gameObject.SetActive(false);
                }
                LoginWeight(0);
                break;
            case 1:
                if (myManager.SelectedUnit.playerClass == PlayerClass.SUPPORT)
                {
                    weight1Text.text = $"{myManager.SelectedUnit.attack1Dmg} Dmg, {myManager.SelectedUnit.nerf1Strength} Weak | {myManager.SelectedUnit.attack1Cost} Energy";
                    weight2Text.text = $"{myManager.SelectedUnit.attack2Dmg} Dmg, {myManager.SelectedUnit.nerf2Strength} Weak | {myManager.SelectedUnit.attack2Cost} Energy";
                    weight3Text.text = $"{myManager.SelectedUnit.attack3Dmg} Dmg, {myManager.SelectedUnit.nerf3Strength} Weak | {myManager.SelectedUnit.attack3Cost} Energy";
                }
                else
                {
                    weight1Text.text = $"{myManager.SelectedUnit.attack1Dmg} Dmg | {myManager.SelectedUnit.attack1Cost} Energy";
                    weight2Text.text = $"{myManager.SelectedUnit.attack2Dmg} Dmg | {myManager.SelectedUnit.attack2Cost} Energy";
                    weight3Text.text = $"{myManager.SelectedUnit.attack3Dmg} Dmg | {myManager.SelectedUnit.attack3Cost} Energy";
                }
                break;
            case 2:
                if (myManager.SelectedUnit.playerClass == PlayerClass.SUPPORT)
                {
                    weight1Text.text = $"{myManager.SelectedUnit.buff1Strength} Energy for Allies | {myManager.SelectedUnit.defend1Cost} Energy";
                    weight2Text.text = $"{myManager.SelectedUnit.buff2Strength} Energy for Allies | {myManager.SelectedUnit.defend2Cost} Energy";
                    weight3Text.text = $"{myManager.SelectedUnit.buff3Strength} Energy for Allies | {myManager.SelectedUnit.defend3Cost} Energy";
                }
                else
                {
                    weight1Text.text = $"{myManager.SelectedUnit.defend1Armor} Armor | {myManager.SelectedUnit.defend1Cost} Energy";
                    weight2Text.text = $"{myManager.SelectedUnit.defend2Armor} Armor | {myManager.SelectedUnit.defend2Cost} Energy";
                    weight3Text.text = $"{myManager.SelectedUnit.defend3Armor} Armor | {myManager.SelectedUnit.defend3Cost} Energy";
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Logs in the weight the player selected for a unit and updates the Energy that is being used this turn
    /// </summary>
    /// <param name="_weight"></param>
    public void LoginWeight(int _weight)
    {
        if (!myManager.SelectedUnit.HaveEnoughEnergyToPerformAction(lastAction, _weight, myManager.MovementState[myManager.SelectedUnit]))
        {
            PlayerErrorText.Instance.DisplayPlayerError(EPlayerError.NOT_ENOUGH_ENERGY);
        }
        else
        {
            if (myManager.SelectedUnit == myManager.MyUnits[0])
            {
                ConstructUnitInfo(Unit1Info, lastAction, _weight);
            }
            else
            {
                ConstructUnitInfo(Unit2Info, lastAction, _weight);
            }
            //Updates the Energy used this turn
            int oldCost = myManager.SelectedUnit.CostOfPerformingAction((PlayerUnit.EAction)myManager.myUnitMoves[myManager.SelectedUnit][2], myManager.myUnitMoves[myManager.SelectedUnit][3]);
            int newCost = myManager.SelectedUnit.CostOfPerformingAction(lastAction, _weight);
            myManager.EnergyUsedThisTurn[myManager.SelectedUnit] += newCost - oldCost;
            myManager.UnitSelected?.Invoke();
            myManager.myUnitMoves[myManager.SelectedUnit][2] = (int)lastAction;
            myManager.myUnitMoves[myManager.SelectedUnit][3] = _weight;
        }
    }

    /// <summary>
    /// Displays the action and weight the player chose
    /// </summary>
    /// <param name="_unitInfo"></param>
    /// <param name="_action"></param>
    /// <param name="_weight"></param>
    private void ConstructUnitInfo(TextMeshProUGUI _unitInfo, PlayerUnit.EAction _action, int _weight)
    {
        _unitInfo.text = $"{myManager.SelectedUnit.name} is ";
        switch (_action)
        {
            case PlayerUnit.EAction.IDLE:
                _unitInfo.text = $"{myManager.SelectedUnit.name} is resting";
                break;
            case PlayerUnit.EAction.ATTACK:
                _unitInfo.text = $"{myManager.SelectedUnit.name} is attacking {ConstructWeightText(_weight)}";
                break;
            case PlayerUnit.EAction.DEFEND:
                _unitInfo.text = $"{myManager.SelectedUnit.name} is defending {ConstructWeightText(_weight)}";
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Constructs a string depending on what weight the player chose
    /// </summary>
    /// <param name="_weight"></param>
    /// <returns></returns>
    private string ConstructWeightText(int _weight)
    {
        switch (_weight)
        {
            case 0:
                return "lightly";
            case 1:
                return "medium";
            case 2:
                return "strongly";
            default:
                break;
        }
        return "";
    }
}
