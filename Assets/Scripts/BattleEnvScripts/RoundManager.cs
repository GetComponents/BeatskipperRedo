using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public UnityEvent OnRoundStart;

    [SerializeField]
    int roundsForWin;

    [SerializeField]
    BattleManager bm;
    Vector2Int Score = new Vector2Int();
    AIUnitManager.EWonLastEpisode whoWonLastEpisode;

    [SerializeField]
    CanvasGroup roundCanvas;

    [SerializeField]
    GameObject playerPoint1, playerPoint2, playerPoint3, enemyPoint1, enemyPoint2, enemyPoint3, matchPointText, nextGameButton;

    [SerializeField]
    Color neutralColor, winColor, loseColor;

    [SerializeField]
    Image background;

    [SerializeField]
    TextMeshProUGUI roundWonText, victoryText;

    [SerializeField]
    int cyclesForPointFadeIn;
    [SerializeField]
    float timeForEachCycle;
    public int CurrentRound
    {
        get => m_currentRound;
        set
        {
            m_currentRound = value;
            StartCoroutine(StartRoundUI());
        }
    }
    private int m_currentRound;

    private void Awake()
    {
        roundCanvas.alpha = 0;
    }
    private void Update()
    {
        //Use if you want to debug win/loss
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    Debug.Log("player wins");
        //    Score.x++;
        //    whoWonLastEpisode = AIUnitManager.EWonLastEpisode.PLAYER_WON;
        //    CurrentRound++;
        //}
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    Score.y++;
        //    whoWonLastEpisode = AIUnitManager.EWonLastEpisode.ENEMY_WON;
        //    CurrentRound++;
        //}
    }

    public void EndRound(AIUnitManager.EWonLastEpisode _whoWon)
    {
        whoWonLastEpisode = _whoWon;
        switch (_whoWon)
        {
            case AIUnitManager.EWonLastEpisode.NONE:
                matchPointText.SetActive(false);
                victoryText.text = "";
                return;
            case AIUnitManager.EWonLastEpisode.PLAYER_WON:
                Score.x++;
                break;
            case AIUnitManager.EWonLastEpisode.TIE:
                break;
            case AIUnitManager.EWonLastEpisode.ENEMY_WON:
                Score.y++;
                break;
            default:
                break;
        }
        CurrentRound++;
    }

    /// <summary>
    /// Increases the opacity of the round canvas and resets the battlefield after doing so
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartRoundUI()
    {
        SettleScore();
        while (true)
        {
            roundCanvas.alpha += 1f / cyclesForPointFadeIn;
            yield return new WaitForSeconds(timeForEachCycle);
            if (roundCanvas.alpha == 1)
            {
                break;
            }
        }
        roundCanvas.blocksRaycasts = true;
        bm.ResetBattlefield();
        //if (Score.x == roundsForWin)
        //{
        //    ResetRoundStats(true);
        //}
        //else if (Score.y == roundsForWin)
        //{
        //    ResetRoundStats(false);
        //}
    }

    /// <summary>
    /// Button method to start the round
    /// </summary>
    public void EndRoundUI()
    {
        roundCanvas.alpha = 0;
        roundCanvas.blocksRaycasts = false;       
    }

    /// <summary>
    /// Used to display who won this episode and what the score looks like
    /// </summary>
    private void SettleScore()
    {
        RoundText();
        if (whoWonLastEpisode == AIUnitManager.EWonLastEpisode.PLAYER_WON)
        {
            background.color = winColor;
            switch (Score.x)
            {
                case 1:
                    EnablePoint(playerPoint1, true);
                    break;
                case 2:
                    EnablePoint(playerPoint2, true);
                    matchPointText.SetActive(true);
                    break;
                case 3:
                    EnablePoint(playerPoint3, true);
                    matchPointText.SetActive(false);
                    roundWonText.text = "";
                    victoryText.gameObject.SetActive(true);
                    nextGameButton.SetActive(true);
                    victoryText.text = "VICTORY";
                    break;
                default:
                    break;
            }
        }
        else if (whoWonLastEpisode == AIUnitManager.EWonLastEpisode.ENEMY_WON)
        {
            background.color = loseColor;
            switch (Score.y)
            {
                case 1:
                    EnablePoint(enemyPoint1, false);
                    break;
                case 2:
                    EnablePoint(enemyPoint2, false);
                    matchPointText.SetActive(true);
                    break;
                case 3:
                    EnablePoint(enemyPoint3, false);
                    matchPointText.SetActive(false);
                    roundWonText.text = "";
                    victoryText.gameObject.SetActive(true);
                    nextGameButton.SetActive(true);
                    victoryText.text = "DEFEAT";
                    break;
                default:
                    break;
            }
        }
        else
        {
            background.color = neutralColor;
        }
    }

    /// <summary>
    /// Displays the text on how the last episode went
    /// </summary>
    private void RoundText()
    {
        switch (whoWonLastEpisode)
        {
            case AIUnitManager.EWonLastEpisode.NONE:
                roundWonText.text = $"Round {CurrentRound}";
                break;
            case AIUnitManager.EWonLastEpisode.PLAYER_WON:
                roundWonText.text = $"You won Round {CurrentRound}";
                break;
            case AIUnitManager.EWonLastEpisode.TIE:
                roundWonText.text = $"Round {CurrentRound} Tie!";
                break;
            case AIUnitManager.EWonLastEpisode.ENEMY_WON:
                roundWonText.text = $"You lost Round {CurrentRound}";
                break;
            default:
                break;
        }
    }

    private void EnablePoint(GameObject _pointToEnable, bool isPlayerPoint)
    {
        _pointToEnable.SetActive(true);
        StartCoroutine(FadePointIn(_pointToEnable.GetComponent<Image>(), isPlayerPoint));
    }

    /// <summary>
    /// Fades in a scorepoint
    /// </summary>
    /// <param name="_pointImage"></param>
    /// <param name="_isPlayerPoint"></param>
    /// <returns></returns>
    private IEnumerator FadePointIn(Image _pointImage, bool _isPlayerPoint)
    {
        _pointImage.color = new Color32(255, 255, 255, 0);
        byte otherColors = 255;
        byte imageA = 0;
        while (true)
        {
            imageA += (byte)Mathf.RoundToInt(255 / cyclesForPointFadeIn);
            otherColors -= (byte)Mathf.RoundToInt(255 / cyclesForPointFadeIn);
            if (_isPlayerPoint)
            {
                _pointImage.color = new Color32(otherColors, otherColors, 255, imageA);
            }
            else
            {
                _pointImage.color = new Color32(255, otherColors, otherColors, imageA);
            }
            if (imageA >= 255 - Mathf.RoundToInt(255 / cyclesForPointFadeIn))
            {
                break;
            }
            yield return new WaitForSeconds(timeForEachCycle);
        }
        if (_isPlayerPoint)
        {
            _pointImage.color = new Color32(0, 0, 255, 255);
        }
        else
        {
            _pointImage.color = new Color32(255, 0, 0, 255);
        }
    }

    /// <summary>
    /// Button method to reset the games info
    /// </summary>
    public void ResetRoundStats()
    {
        roundWonText.text = "";
        m_currentRound = 0;
        Score = new Vector2Int();
        playerPoint1.SetActive(false);
        playerPoint2.SetActive(false);
        playerPoint3.SetActive(false);
        enemyPoint1.SetActive(false);
        enemyPoint2.SetActive(false);
        enemyPoint3.SetActive(false);
        victoryText.gameObject.SetActive(false);
        nextGameButton.SetActive(false);
    }
}
