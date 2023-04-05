using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;

public class StateEvent : UnityEvent<ActionBuffers> { }

/// <summary>
/// Moves the agent. Used for the proof of concept phase
/// </summary>
public class BSMovement : Agent
{

    private bool newCycle, usingHeuristics, won, lost, touchingGoal;

    [SerializeField]
    private float walkCooldown;
    private int distanceTraveled;

    [SerializeField]
    Transform goalTransform;

    [SerializeField]
    MeshRenderer ground;

    [SerializeField]
    Material winMat, loseMat, idleMat;

    private StateEvent currentState;

    public int CombatState
    {
        get => combatState;
        set
        {
            if (combatState != value)
            {
                currentState.RemoveAllListeners();
                switch (combatState)
                {
                    case 0:
                        IdleExit();
                        break;
                    case 1:
                        MovementExit();
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
                switch (value)
                {
                    case 0:
                        IdleStart();
                        currentState.AddListener(IdleUpdate);
                        break;
                    case 1:
                        MovementStart();
                        currentState.AddListener(MovementUpdate);
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
                combatState = value;
            }
        }
    }
    [SerializeField]
    private int combatState;

    [SerializeField]
    public int needsToMoveX, needsToMoveY;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        needsToMoveX = 0;
        needsToMoveY = 0;
        distanceTraveled = 0;
        if (currentState != null)
        {
            currentState.RemoveAllListeners();
        }
        else
        {
            currentState = new StateEvent();
        }
        goalTransform.localPosition = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        currentState.AddListener(IdleUpdate);

        if (won)
        {
            ground.material = winMat;
            won = false;
        }
        else if (lost)
        {
            ground.material = loseMat;
            lost = false;
        }
        else
        {
            ground.material = idleMat;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(goalTransform.localPosition.x);
        sensor.AddObservation(goalTransform.localPosition.z);
        //Debug.Log("Observationsize: "+ sensor.ObservationSize());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        currentState.Invoke(actions);
        AddReward(-1f / MaxStep);
    }

    private IEnumerator MoveCharacter()
    {

        needsToMoveX -= 5;
        needsToMoveY -= 5;
        while (needsToMoveX != 0 || needsToMoveY != 0)
        {
            if (needsToMoveX != 0)
            {
                transform.localPosition += new Vector3(needsToMoveX / Mathf.Abs(needsToMoveX), 0, 0);
                needsToMoveX -= needsToMoveX / Mathf.Abs(needsToMoveX);
                distanceTraveled++;
            }
            if (needsToMoveY != 0)
            {
                transform.localPosition += new Vector3(0, 0, needsToMoveY / Mathf.Abs(needsToMoveY));
                needsToMoveY -= needsToMoveY / Mathf.Abs(needsToMoveY);
                distanceTraveled++;
            }
            yield return new WaitForSeconds(0.2f);
        }
        if (touchingGoal)
        {
            won = true;
            AddReward(1);
            EndEpisode();
            touchingGoal = false;
        }
        else
        {
            yield return new WaitForSeconds(distanceTraveled * walkCooldown);
            distanceTraveled = 0;
            CombatState = 0;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        usingHeuristics = true;
        ActionSegment<int> discreteAction = actionsOut.DiscreteActions;
        if (newCycle)
        {
            discreteAction[0] = 0;
            newCycle = false;
        }
        discreteAction[1] = needsToMoveX;
        discreteAction[2] = needsToMoveY;
    }

    #region StateMethods

    private void IdleStart()
    {
        newCycle = true;
    }

    private void IdleUpdate(ActionBuffers actions)
    {
        if (!usingHeuristics)
        {
            needsToMoveX = actions.DiscreteActions[1];
            needsToMoveY = actions.DiscreteActions[2];
        }
        CombatState = actions.DiscreteActions[0];
    }

    private void IdleExit()
    {

    }

    private void MovementStart()
    {
        StartCoroutine(MoveCharacter());
    }
    private void MovementUpdate(ActionBuffers actions)
    {

    }
    private void MovementExit()
    {
        //needsToMoveX = 5;
        //needsToMoveY = 5;
    }

    #endregion StateMethods


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            touchingGoal = true;

        }
        else if (other.tag == "Wall")
        {
            lost = true;
            AddReward(-1);
            EndEpisode();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Goal")
        {
            touchingGoal = false;
        }
    }
}
