using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeagueInterfaceManager : MonoBehaviour
{
	[Header("Player Banner")]
	public Image playerBannerBorder;
	public TextMeshProUGUI seasonStageText;
	public TextMeshProUGUI playerBannerTitleText;
	public TextMeshProUGUI goldText;
	public TextMeshProUGUI fanApprovalText;

	[Header("League Panel")]
	public Image advanceButtonBorderImage;
	public TextMeshProUGUI advanceText;

	private GameObject activeLeaguePanel;

	[Header("Matchup Panel")]
	public GameObject matchupPanel;
    public Transform matchupHolder;
	public GameObject matchupPrefab;

	[Header("Standings Panel")]
	public GameObject standingsPanel;
	public Transform standingsTeamHolder;
	public GameObject teamContainerPrefab;

	[Header("Season Summary Panel")]
	public GameObject allStarPanel;
	public TextMeshProUGUI seasonSummaryTitleText;
	public List<AllStarAthleteContainer> allStarContainerList;

	[Header("History Panel")]
	public GameObject historyPanel;
	public Transform historyHolder;
	public GameObject historyPrefab;

	[Header("Championship Panel")]
	public GameObject championshipPanel;
	public TextMeshProUGUI championshipText;
	public TextMeshProUGUI championshipTrophyText;
	public List<AllStarAthleteContainer> championshipTeamAllStarContainerList;

	public GameObject championshipHeaderTab;

	[Header("Event Panel")]
	public GameObject eventPanel;
	public Image eventImageBorder;
	public TextMeshProUGUI eventTitleText;
	public TextMeshProUGUI eventDescriptionText;
	public Transform eventOptionButonHolder;
	public GameObject eventOptionButtonPrefab;

	public GameObject eventResultPanel;
	public TextMeshProUGUI eventResultPanelText;

	[Header("Election Panel")]
	public GameObject electionPanel;
	public Transform lawChangeButtonHolder;
	public GameObject lawChangeButtonPrefab;

	[Header("Draft Panel")]
	public GameObject draftPanel;
	public Transform draftButtonHolder;
	public GameObject draftButtonPrefab;

	private RosterInterfaceManager rosterInterfaceManager;
	private LeagueManager leagueManager;
	private MatchManager matchManager;

	private void Awake()
	{
		leagueManager = FindObjectOfType<LeagueManager>();
		matchManager = FindObjectOfType<MatchManager>();
		rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();
		rosterInterfaceManager.HideTeamPanel();
		rosterInterfaceManager.HideAthletePanel();
		rosterInterfaceManager.HideBenchPanel();

		allStarPanel.SetActive(false);
		historyPanel.SetActive(false);
		championshipPanel.SetActive(false);
		electionPanel.SetActive(false);
		draftPanel.SetActive(false);

		ShowLeaguePanel(matchupPanel);
	}

	public void UpdatePlayerBanner()
	{
		Team userTeam = leagueManager.userTeam;
		playerBannerTitleText.text = userTeam.GetTeamName();
		goldText.text = userTeam.gold.ToString();
		fanApprovalText.text = (userTeam.fanApproval * 100f).ToString("F0") + "%";

		playerBannerBorder.color = userTeam.colorPrimary;

		eventImageBorder.color = userTeam.colorPrimary;

		advanceButtonBorderImage.color = userTeam.colorPrimary;

		if (!leagueManager.postseason)
		{
			seasonStageText.text = "Season " + (leagueManager.currentSeason + 1) + " - Week " + (leagueManager.currentWeek + 1);
		}
		else
		{
			seasonStageText.text = "Postseason " + (leagueManager.currentSeason + 1) + " - Week " + (leagueManager.currentWeek + 1);
		}
	}

	public void DisplaySeasonStart()
	{
		ShowLeaguePanel(matchupPanel);

		championshipHeaderTab.SetActive(false);

		UpdateHistoryPanel();
	}

	public void SetupMatchupPanel(List<Matchup> matchups) //Also updates Standings UI
	{
		for (int i = matchupHolder.childCount; i > 0; i--)
		{
			Destroy(matchupHolder.GetChild(i - 1).gameObject);
		}

		for(int i = 0; i < matchups.Count; i++)
		{
			if(matchups[i].homeTeam.playerControlled || matchups[i].awayTeam.playerControlled)
			{
				Matchup m = matchups[i];
				matchups.Remove(m);
				matchups.Insert(0, m); //Put the playerController matchup in the first slot
			}
		}

		for(int i = 0; i < matchups.Count; i++)
		{
			MatchupContainer matchupContainer = Instantiate(matchupPrefab, matchupHolder).GetComponent<MatchupContainer>();
			matchupContainer.SetTeams(matchups[i]);
		}

		advanceText.text = "Simulate Remaining";
	}

	public void UpdateStandingsPanel()
	{
		for (int i = standingsTeamHolder.childCount; i > 0; i--)
		{
			Destroy(standingsTeamHolder.GetChild(i - 1).gameObject);
		}

		List<Team> teamsByStandings = new List<Team>();
		teamsByStandings.AddRange(leagueManager.teamList);
		teamsByStandings.Sort((a, b) => b.GetCurrentSeasonStats().wins.CompareTo(a.GetCurrentSeasonStats().wins));
		for (int i = 0; i < teamsByStandings.Count; i++)
		{
			TeamContainer teamContainer = Instantiate(teamContainerPrefab, standingsTeamHolder).GetComponent<TeamContainer>();
			teamContainer.SetTeamContainer(teamsByStandings[i]);
		}
	}

	public void UpdateSeasonSummaryPanel()
	{
		if(leagueManager.postseason)
			seasonSummaryTitleText.text = "Season " + (leagueManager.currentSeason + 1) + " All Star Team";
		else
			seasonSummaryTitleText.text = "Season " + (leagueManager.currentSeason + 1) + " Stats Summary";

		for (int i = 0; i < allStarContainerList.Count; i++)
		{
			AllStarAthleteContainer allStarContainer = allStarContainerList[i];

			List<Athlete> eligibleAthletes = new List<Athlete>();

			for (int t = 0; t < leagueManager.teamList.Count; t++)
			{
				eligibleAthletes.AddRange(leagueManager.teamList[t].GetAthletesByPosition(allStarContainerList[i].positionGroup));
			}

			if (eligibleAthletes.Count > 0)
			{
				eligibleAthletes.Sort((a, b) => b.GetDivineSpectacleSum_CurrentSeason().CompareTo(a.GetDivineSpectacleSum_CurrentSeason()));

				allStarContainer.gameObject.SetActive(true);
				allStarContainer.AssignPossibileAthletes(eligibleAthletes);
			}
			else
			{
				allStarContainer.gameObject.SetActive(false);
			}
		}
	}

	public void UpdateHistoryPanel()
	{
		for (int i = historyHolder.childCount; i > 0; i--)
		{
			Destroy(historyHolder.GetChild(i - 1).gameObject);
		}

		for (int i = 0; i < leagueManager.historyList.Count; i++)
		{
			SeasonHistory history = leagueManager.historyList[i];
			HistoryContainer historyContainer = Instantiate(historyPrefab, historyHolder).GetComponent<HistoryContainer>();
			historyContainer.SetHistory(history);
		}
	}

	public void WatchMatchup(Matchup matchup)
	{
		matchManager.SetupMatch(matchup, false);
	}

	public void SimulateMatchup(Matchup matchup)
	{
		matchManager.SetupMatch(matchup, true);
	}

	public void DisplayCompleteMatchup(Matchup match)
	{
		for (int i = 0; i < matchupHolder.childCount; i++)
		{
			MatchupContainer matchupContainer = matchupHolder.GetChild(i).GetComponent<MatchupContainer>();
			if (matchupContainer.matchup == match)
			{
				matchupContainer.ShowResults(match);
			}
		}

		if (leagueManager.weekList[leagueManager.currentWeek].GetAllGamesComplete())
			advanceText.text = "Advance";
		else
			advanceText.text = "Simulate Remaining";


		UpdateStandingsPanel();
		UpdateSeasonSummaryPanel();
	}

	public void ShowLeaguePanel(GameObject leaguePanel)
	{
		rosterInterfaceManager.HideTeamPanel();
		rosterInterfaceManager.HideAthletePanel(); //Should be unneccessary but good safety net
		if(activeLeaguePanel)
			activeLeaguePanel.SetActive(false);

		leaguePanel.SetActive(true);
		activeLeaguePanel = leaguePanel;
	}

	public void UserAdvanceWeek()
	{
		if(leagueManager.seasonChampion == null)
		{
			ShowLeaguePanel(matchupPanel);

			if (leagueManager.weekList[leagueManager.currentWeek].GetAllGamesComplete())
			{
				leagueManager.AdvanceToNextWeek();
			}
			else
			{
				leagueManager.SimulateRemainingMatchupsForWeek();
			}
		}
		else //Champions have been crowned, season ends
		{
			TriggerEndSeason();
		}
	}

	public void DisplayEvent(Team displayedTeam, WeeklyEvent displayedEvent)
	{
		eventPanel.gameObject.SetActive(true);
		eventTitleText.text = displayedEvent.title;
		eventDescriptionText.text = displayedEvent.GetDescription();

		for (int i = eventOptionButonHolder.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(eventOptionButonHolder.transform.GetChild(i).gameObject);
		}

		for (int i = 0; i < displayedEvent.options.Count; i++)
		{
			GameObject newEventOptionButton = Instantiate(eventOptionButtonPrefab, eventOptionButonHolder);

			WeeklyEventOption option = displayedEvent.options[i];

			newEventOptionButton.GetComponent<Button>().onClick.AddListener(() => DisplayOptionChoice(displayedTeam, displayedEvent, option));

			newEventOptionButton.GetComponentInChildren<TextMeshProUGUI>().text = option.description;
		}
	}
	public void DisplayOptionChoice(Team team, WeeklyEvent eventChosen, WeeklyEventOption optionChosen)
	{
		if(optionChosen.optionResults.Count == 0) //No event
		{
			eventPanel.SetActive(false); //Just disable the eventPanel, nothing else needs to be done
		}
		else
		{
			Debug.Log("This is an event. Need to determine whether to display a result or to simply ignore.");

			eventPanel.SetActive(false);
		}

		leagueManager.ResolveWeeklyEvent(team, eventChosen, optionChosen);
		UpdatePlayerBanner();
	}

	public void DisplayChampion(Team champion)
	{
		Debug.Log("Displaying Champs");

		ShowLeaguePanel(championshipPanel);

		championshipHeaderTab.SetActive(true);
		championshipText.text = champion.GetTeamName() + " are League Champions!";
		championshipTrophyText.text = (leagueManager.currentSeason + 1).ToString();

		for(int i = 0; i < championshipTeamAllStarContainerList.Count; i++)
		{
			AllStarAthleteContainer allStarContainer = championshipTeamAllStarContainerList[i];

			List<Athlete> orderedListOfAthletes = champion.GetAthletesByPosition(allStarContainer.positionGroup);

			if (orderedListOfAthletes.Count > 0)
			{
				orderedListOfAthletes.Sort((a, b) => b.GetDivineSpectacleSum_CurrentSeason().CompareTo(a.GetDivineSpectacleSum_CurrentSeason()));

				allStarContainer.gameObject.SetActive(true);
				allStarContainer.AssignPossibileAthletes(orderedListOfAthletes);
			}
			else
			{
				allStarContainer.gameObject.SetActive(false);
			}
		}

		advanceText.text = "Conclude Season";
	}

	public void TriggerEndSeason()
	{
		Debug.Log("Triggering end season");

		championshipPanel.SetActive(false);

		leagueManager.ConcludeSeason();
	}

	public void DisplayElectionPanel(List<Amendment> amendmentList)
	{
		electionPanel.SetActive(true);

		for(int i = lawChangeButtonHolder.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(lawChangeButtonHolder.transform.GetChild(i).gameObject);
		}

		for(int i = 0; i < amendmentList.Count; i++)
		{
			LawButton newLawChangeButton = Instantiate(lawChangeButtonPrefab, lawChangeButtonHolder).GetComponent<LawButton>();

			Amendment ammy = amendmentList[i];

			newLawChangeButton.SetLawChange(ammy);
		}
	}

	public void DisplayLawChange(Amendment chosenAmendment)
	{
		Debug.Log(chosenAmendment.law.id + " is being changed now");

		electionPanel.SetActive(false);

		leagueManager.ConcludeElection(chosenAmendment);
	}

	public void UpdatePositionGroupVisibility()
	{
		if (leagueManager.quarterbacksAndReceiversAllowed)
		{
			rosterInterfaceManager.TogglePositionGroupVisibility(PositionGroup.QuarterBack, true);
			rosterInterfaceManager.TogglePositionGroupVisibility(PositionGroup.ReceivingBack, true);
		}
		else
		{
			rosterInterfaceManager.TogglePositionGroupVisibility(PositionGroup.QuarterBack, false);
			rosterInterfaceManager.TogglePositionGroupVisibility(PositionGroup.ReceivingBack, false);
		}

		if (leagueManager.kickersAllowed)
		{
			rosterInterfaceManager.TogglePositionGroupVisibility(PositionGroup.Kicker, true);
		}
		else
		{
			rosterInterfaceManager.TogglePositionGroupVisibility(PositionGroup.Kicker, false);
		}
	}

	public void DisplayDraftPanel(Team team, List<Athlete> draftChoices)
	{
		Debug.Log("Displaying draft");

		draftPanel.SetActive(true);

		for (int i = draftButtonHolder.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(draftButtonHolder.transform.GetChild(i).gameObject);
		}

		for (int i = 0; i < draftChoices.Count; i++)
		{
			DraftCard newDraftCard = Instantiate(draftButtonPrefab, draftButtonHolder).GetComponent<DraftCard>();

			Athlete draftChoice = draftChoices[i];

			Sprite newSprite = draftChoice.sprite;
			newDraftCard.logo.sprite = newSprite;
			newDraftCard.buttonText.text = draftChoice.GetName(false);

			newDraftCard.draftButton.onClick.AddListener(() => DisplayDraftChoice(team, draftChoice));
			newDraftCard.draftButton.GetComponentInChildren<TextMeshProUGUI>().text = "Draft " + draftChoice.GetName(false);

			AttributeGroup[] attributeGroups = (AttributeGroup[])System.Enum.GetValues(typeof(AttributeGroup));
			int randoReveal1 = Random.Range(0, attributeGroups.Length);
			int randoReveal2 = Random.Range(0, attributeGroups.Length);
			int randoReveal3 = Random.Range(0, attributeGroups.Length);
			for (int j = 0; j < attributeGroups.Length; j++)
			{
				AthleteAttributeContainer athleteAttributeContainer = newDraftCard.athleteAttributeContainers[j];
				athleteAttributeContainer.SetAthleteAttributeContainer(draftChoice, attributeGroups[j]);

				if (j == randoReveal1 || j == randoReveal2 || j == randoReveal3)
					athleteAttributeContainer.ToggleStatsMysterized(false);
				else
					athleteAttributeContainer.ToggleStatsMysterized(true);
			}
		}
	}

	public void DisplayDraftChoice(Team team, Athlete newAthlete)
	{
		draftPanel.SetActive(false);

		leagueManager.DraftPlayer(team, newAthlete);
		
		leagueManager.ConcludeRosterAdjustmentPeriod();
	}
}
