using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicInputReader : MonoBehaviour
{
    [SerializeField]
    BSMovement agent;
 
    [SerializeField]
    int AIModel;

    void Update()
    {
        switch (AIModel)
        {
            case 1:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    agent.CombatState = Mathf.Abs(agent.CombatState - 1);
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    agent.needsToMoveY++;
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    agent.needsToMoveX--;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    agent.needsToMoveY--;
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    agent.needsToMoveX++;
                }
                break;
            case 2:
                //if (Input.GetKeyDown(KeyCode.Tab))
                //{
                //    GetComponent<AIUnitManager>().PressedTab = true;
                //}
                //if (Input.GetKeyDown(KeyCode.Space))
                //{
                //    GetComponent<AIUnitManager>().PressedSpace = true;
                //}
                //if (Input.GetKeyDown(KeyCode.Mouse0))
                //{
                //    GetComponent<AIUnitManager>().PressedMouse1 = true;
                //}
                break;
            default:
                break;
        }   
    }
}
