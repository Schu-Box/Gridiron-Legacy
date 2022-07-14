using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchupContainer : MonoBehaviour
{
	public Button homeTeamButton;
	public Button awayTeamButton;
	public TextMeshProUGUI awayTeamNameText;
	public TextMeshProUGUI homeTeamNameText;
	public TextMeshProUGUI awayTeamRecordText;
	public TextMeshProUGUI homeTeamRecordText;
	public TextMeshProUGUI awayTeamScoreText;
	public TextMeshProUGUI homeTeamScoreText;

	public Transform postGameLogHolder;
	public GameObject postGameLogPrefab;

	public Button watchButton;
	public Button simulateButton;

	private LeagueInterfaceManager leagueInterfaceManager;
	private RosterInterfaceManager rosterInterfaceManager;

	public Matchup matchup;

	public void SetTeams(Matchup match)
	{
		leagueInterfaceManager = FindObjectOfType<LeagueInterfaceManager>();
		rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();

		matchup = match;

		awayTeamNameText.text = matchup.awayTeam.GetTeamName();
		homeTeamNameText.text = matchup.homeTeam.GetTeamName();

		UpdateRecordTexts();
		
		awayTeamScoreText.text = "0";
		homeTeamScoreText.text = "0";

		homeTeamButton.onClick.RemoveAllListeners();
		awayTeamButton.onClick.RemoveAllListeners();
		homeTeamButton.onClick.AddListener(() => rosterInterfaceManager.OpenTeamPanel(matchup.homeTeam));
		awayTeamButton.onClick.AddListener(() => rosterInterfaceManager.OpenTeamPanel(matchup.awayTeam));
	}

    public void WatchOrSimulateMatchup(bool watching)
	{
        if(watching)
		{
			leagueInterfaceManager.WatchMatchup(matchup);
		}
		else
		{
			leagueInterfaceManager.SimulateMatchup(matchup);
		}
	}

	public void ShowResults(Matchup match)
	{
		homeTeamScoreText.text = match.homeScore.ToString();
		awayTeamScoreText.text = match.awayScore.ToString();

		UpdateRecordTexts();

		watchButton.gameObject.SetActive(false);
		simulateButton.gameObject.SetActive(false);

		DisplayPostGameLogs(match.GetPostGameLogs());
	}

	public void UpdateRecordTexts()
	{
		homeTeamRecordText.text = matchup.homeTeam.GetRecordString();

		awayTeamRecordText.text = matchup.awayTeam.GetRecordString();
	}

	public void DisplayPostGameLogs(List<string> logs)
	{
		for(int i = postGameLogHolder.childCount - 1; i > -1; i--)
		{
			Destroy(postGameLogHolder.GetChild(i).gameObject);
		}

		for(int i = 0; i < logs.Count; i++)
		{
			GameObject newLog = Instantiate(postGameLogPrefab, postGameLogHolder);

			newLog.GetComponent<TextMeshProUGUI>().text = logs[i];
		}
	}
}
