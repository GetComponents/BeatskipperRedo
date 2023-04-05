using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Events;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public enum EDifficultyEvent
{
    NONE,
    UNIT_DEATH,
    HP_LOST,
    WIN,
    PLAYER_ATTACK,
    PLAYER_DEFEND,
}

public class DifficultyManager : Agent
{
    [SerializeField]
    AIUnitManager enemyManager;
    [SerializeField]
    float wantedWinPercentage;
    [SerializeField]
    Vector2 wantedMulitplierSpan;
    [SerializeField]
    int AITypeAmount;
    [SerializeField]
    float statDiversionWeight, extremeStatWeight;
    public bool MadeDecision;

    [SerializeField]
    int maxRounds;

    private int roundNumber;


    private float PlayerWinLoss, EnemyUnitsKilled, PlayerUnitsKilled, EnemyHPLost, PlayerHPLost, PlayerSuccessfulAttacks, PlayerSuccessfulDefends;
    private int finalPlayerWinLoss;
    //private PlayerClass playerUnit1, playerUnit2, enemyUnit1, enemyUnit2;

    private float attackMultiplier, defendMultiplier, healthMultiplier, aiType;

    [SerializeField]
    NNModel balancedAI, agressiveAI, reallyAgressiveAI, defensiveAI, unexperiencedAI;

    private void Awake()
    {
        attackMultiplier = 1;
        defendMultiplier = 1;
        healthMultiplier = 1;
        aiType = 0;
    }

    public void EndRound()
    {
        roundNumber++;
        if (roundNumber == maxRounds)
        {
            //TODO undo?
            //ChangePlayerAI();
            EvaluateGame();
            roundNumber = 0;
            EndEpisode();
        }
        else
        {
            RequestDecision();
        }
    }

    public override void OnEpisodeBegin()
    {
        RequestDecision();
    }

    /// <summary>
    /// Resets all observable stats
    /// </summary>
    private void ResetValues()
    {
        finalPlayerWinLoss += Mathf.RoundToInt(PlayerWinLoss);
        PlayerWinLoss = 0;
        EnemyUnitsKilled = 0;
        PlayerUnitsKilled = 0;
        EnemyHPLost = 0;
        PlayerHPLost = 0;
        PlayerSuccessfulAttacks = 0;
        PlayerSuccessfulDefends = 0;
    }

    /// <summary>
    /// Changes the ai of the player, so that the player ai plays differently
    /// </summary>
    private void ChangePlayerAI()
    {
        string debugInfo = "Player AI Changed to: ";
        if (!enemyManager.MyBattleManager.playerManager.isAI)
        {
            return;
        }
        int tmp = Random.Range(0, 5);
        switch (tmp)
        {
            case 0:
                enemyManager.MyBattleManager.playerManager.GetComponent<BehaviorParameters>().Model = unexperiencedAI;
                debugInfo += "unexperiencedAI";
                break;
            case 1:
                enemyManager.MyBattleManager.playerManager.GetComponent<BehaviorParameters>().Model = balancedAI;
                debugInfo += "balancedAI";
                break;
            case 2:
                enemyManager.MyBattleManager.playerManager.GetComponent<BehaviorParameters>().Model = agressiveAI;
                debugInfo += "agressiveAI";
                break;
            case 3:
                enemyManager.MyBattleManager.playerManager.GetComponent<BehaviorParameters>().Model = reallyAgressiveAI;
                debugInfo += "reallyAgressiveAI";
                break;
            case 4:
                enemyManager.MyBattleManager.playerManager.GetComponent<BehaviorParameters>().Model = defensiveAI;
                debugInfo += "defensiveAI";
                break;
            default:
                break;
        }
        //Debug.Log(debugInfo);
    }

    /// <summary>
    /// Changes an observable stat
    /// </summary>
    /// <param name="_event"></param>
    /// <param name="_isPlayer"></param>
    /// <param name="_amount"></param>
    public void DifficultyEventTrigger(EDifficultyEvent _event, bool _isPlayer, int _amount)
    {
        switch (_event)
        {
            case EDifficultyEvent.NONE:
                break;
            case EDifficultyEvent.UNIT_DEATH:
                if (_isPlayer)
                {
                    PlayerUnitsKilled++;
                }
                else
                {
                    EnemyUnitsKilled++;

                }
                break;
            case EDifficultyEvent.HP_LOST:
                if (_isPlayer)
                {
                    PlayerHPLost += _amount;
                }
                else
                {
                    EnemyHPLost += _amount;
                }
                break;
            case EDifficultyEvent.WIN:
                if (_isPlayer)
                {
                    PlayerWinLoss++;
                }
                break;
            case EDifficultyEvent.PLAYER_ATTACK:
                PlayerSuccessfulAttacks += _amount;
                break;
            case EDifficultyEvent.PLAYER_DEFEND:
                PlayerSuccessfulDefends += _amount;
                break;
            default:
                break;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(attackMultiplier);
        sensor.AddObservation(defendMultiplier);
        sensor.AddObservation(healthMultiplier);
        sensor.AddObservation(aiType);

        sensor.AddObservation(EnemyUnitsKilled);
        sensor.AddObservation(PlayerUnitsKilled);
        sensor.AddObservation(EnemyHPLost);
        sensor.AddObservation(PlayerHPLost);
        sensor.AddObservation(PlayerSuccessfulAttacks);
        sensor.AddObservation(PlayerSuccessfulDefends);

        sensor.AddObservation(wantedWinPercentage);
        ResetValues();
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float action0, action1, action2, action3;
        //Translate the -1~1 span of the vector actors into the wanted span. in this case 0.01 ~ 2
        action0 = ((wantedMulitplierSpan.x + wantedMulitplierSpan.y) / 2f) + (((wantedMulitplierSpan.y - wantedMulitplierSpan.x) / 2f) * actions.ContinuousActions[0]);
        action1 = ((wantedMulitplierSpan.x + wantedMulitplierSpan.y) / 2f) + (((wantedMulitplierSpan.y - wantedMulitplierSpan.x) / 2f) * actions.ContinuousActions[1]);
        action2 = ((wantedMulitplierSpan.x + wantedMulitplierSpan.y) / 2f) + (((wantedMulitplierSpan.y - wantedMulitplierSpan.x) / 2f) * actions.ContinuousActions[2]);
        action3 = ((float)AITypeAmount / 2f) + (((float)AITypeAmount / 2f) * actions.ContinuousActions[3]);

        PunishAIForStatChanges(action0, attackMultiplier);
        attackMultiplier = action0;

        PunishAIForStatChanges(action1, defendMultiplier);
        defendMultiplier = action1;

        PunishAIForStatChanges(action2, healthMultiplier);
        healthMultiplier = action2;

        if (action3 < 0)
        {
            aiType = 0;
        }
        else if (action3 > 4)
        {
            aiType = 4;
        }
        else
        {
            aiType = Mathf.RoundToInt(action3);
        }
        ChangeDifficulty();
    }

    private void PunishAIForStatChanges(float _action, float _previousAction)
    {
        //Punish...
        //Extreme stats
        //float tmp = -Mathf.Abs((_action - ((wantedMulitplierSpan.y - wantedMulitplierSpan.x) / 2f)) / (wantedMulitplierSpan.y - wantedMulitplierSpan.x)) * (1f / (float)maxRounds);
        //Big stat change
        //AddReward(-(Mathf.Abs(_action - _previousAction) / (wantedMulitplierSpan.y - wantedMulitplierSpan.x)) * statDiversionPunishmentWeight * (1f / (float)maxRounds));

        //Reward...
        //Middle of the line stats
        float tmp = (1f / (1 + Mathf.Abs((_action - ((wantedMulitplierSpan.y - wantedMulitplierSpan.x) / 2f)) / (wantedMulitplierSpan.y - wantedMulitplierSpan.x)))) * (1f / (float)maxRounds);
        //Small stat changes
        AddReward((1f / ((1 + Mathf.Abs(_action - _previousAction) / (wantedMulitplierSpan.y - wantedMulitplierSpan.x)))) * statDiversionWeight * (1f / (float)maxRounds));
        AddReward(tmp * extremeStatWeight);
    }

    /// <summary>
    /// Changes the difficulty by changing the enemies stat multipliers and their behaviour
    /// </summary>
    public void ChangeDifficulty()
    {
        enemyManager.AttackMultiplier = attackMultiplier;
        enemyManager.DefenceMultiplier = defendMultiplier;
        enemyManager.HealthMultiplier = healthMultiplier;
        //My ranking predicting how good they will perform: 0 worst, 4 best
        switch (Mathf.RoundToInt(aiType))
        {
            case 0:
                //enemyManager.AIBrain.Model = tryingToLoseAI;
                enemyManager.AIBrain.Model = unexperiencedAI;
                break;
            case 1:
                enemyManager.AIBrain.Model = reallyAgressiveAI;
                break;
            case 2:
                enemyManager.AIBrain.Model = defensiveAI;
                break;
            case 3:
                enemyManager.AIBrain.Model = agressiveAI;
                break;
            case 4:
                enemyManager.AIBrain.Model = balancedAI;
                break;
            default:
                break;
        }
        MadeDecision = true;
        enemyManager.MyBattleManager.ResetBattlefield();
    }

    /// <summary>
    /// Reward/Punish ai depending on the wanted win/loss ratio
    /// </summary>
    private void EvaluateGame()
    {
        float tmp = (1 - wantedWinPercentage) * maxRounds;

        //Punish the AI for deviating from the wanted win/loss ratio
        //AddReward(-Mathf.Abs(tmp - finalPlayerWinLoss));

        //Reward the AI for getting the wanted win/loss ratio
        AddReward(1f / (1 + Mathf.Abs(tmp - finalPlayerWinLoss)));
        finalPlayerWinLoss = 0;
    }
}
