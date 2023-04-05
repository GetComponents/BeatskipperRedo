using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


/// <summary>
/// Controller for an agent in the proof of concept phase
/// </summary>
public class MLController : Agent
{
    [SerializeField]
    Transform targetTransform;

    [SerializeField]
    MeshRenderer ground;

    [SerializeField]
    Material winMat, loseMat;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-10, 11), 0, Random.Range(-10, 11));        
        Vector3 randomPos;
        do
        {
            randomPos = new Vector3(Random.Range(-10, 11), 0, Random.Range(-10, 11));
        } while (randomPos == transform.localPosition);
        targetTransform.localPosition = randomPos;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(actions.DiscreteActions[0]);
        switch (actions.DiscreteActions[0])
        {
            case 0:
                break;
            case 1:
                transform.position += new Vector3(0, 0, 1);
                break;
            case 2:
                transform.position += new Vector3(-1, 0, 0);
                break;
            case 3:
                transform.position += new Vector3(0, 0, -1);
                break;
            case 4:
                transform.position += new Vector3(1, 0, 0);
                break;
            default:
                break;
        }

        AddReward(-1f / MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteAction = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteAction[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteAction[0] = 2;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteAction[0] = 3;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteAction[0] = 4;
        }
        else
        {
            discreteAction[0] = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            ground.material = winMat;
            AddReward(1);
            EndEpisode();
        }
        else if (other.tag == "Wall")
        {
            ground.material = loseMat;
            AddReward(-1);
            EndEpisode();
        }
    }
}
