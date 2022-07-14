using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RosterInterfaceManager : MonoBehaviour
{
	public GameObject teamPanel;
	public TextMeshProUGUI teamNameText;
	public TextMeshProUGUI teamRecordText;
	public Transform teamTrophyCase;
	public List<RosterPositionContainer> positionContainers;

	[Header("Athlete Panel")]
	public GameObject athletePanel;
	public TextMeshProUGUI athleteNameText;
	public Image athleteImage;
	public Transform athleteTrophyCase;
	public List<AthleteAttributeContainer> athleteAttributeContainers;
	public GameObject rosterInteractionButtons;
	public GameObject benchButton;
	public Image healthIndicatorFill;

	[Header("Trophy Prefabs")]
	public GameObject trophyPrefab_Championship;
	public GameObject trophyPrefab_MVP;
	public GameObject trophyPrefab_AllStar;

	[Header("Bench Panel")]
	public GameObject benchPanel;
	public TextMeshProUGUI benchTitleText;
	public TextMeshProUGUI attributeText;
	public Transform benchAthleteRosterContainerHolder;
	public GameObject athleteRosterContainerPrefab;

	[Header("Colors")]
	public Color grayColor1;
	public Color grayColor2;

	private Team team;
	private Athlete selectedAthlete = null;

	private LeagueManager leagueManager;

	private void Awake()
	{
		leagueManager = FindObjectOfType<LeagueManager>();
	}

	public void OpenTeamPanel(Team newTeam)
	{
		team = newTeam;

		teamPanel.gameObject.SetActive(true);

		teamNameText.text = team.GetTeamName();
		teamRecordText.text = team.GetCurrentSeasonStats().wins + " - " + team.GetCurrentSeasonStats().losses;

		for(int i = 0; i < positionContainers.Count; i++)
		{
			positionContainers[i].SetRosterContainer(team);
		}

		for(int i = teamTrophyCase.childCount - 1; i > -1; i--)
		{
			Destroy(teamTrophyCase.GetChild(i).gameObject);
		}

		for(int i = 0; i < team.championshipSeasons.Count; i++)
		{
			int season = team.championshipSeasons[i];
			season++; //To display 0th season as first

			GameObject newTrophy = Instantiate(trophyPrefab_Championship, teamTrophyCase);
			newTrophy.GetComponentInChildren<TextMeshProUGUI>().text = season.ToString();
		}
	}

	public void TogglePositionGroupVisibility(PositionGroup positionGroup, bool visible)
	{
		for (int i = 0; i < positionContainers.Count; i++)
		{
			if (positionContainers[i].positionGroup == positionGroup)
				positionContainers[i].gameObject.SetActive(visible);
		}
	}

	public void CycleTeam(bool right)
	{
		int currentIndex = leagueManager.teamList.IndexOf(team);

		int nextIndex = currentIndex;
		if (right)
		{
			nextIndex++;
			if (nextIndex >= leagueManager.teamList.Count)
				nextIndex = 0;
		}
		else
		{
			nextIndex--;
			if (nextIndex < 0)
				nextIndex = leagueManager.teamList.Count - 1;
		}

		OpenTeamPanel(leagueManager.teamList[nextIndex]);
	}

	public void HideTeamPanel()
	{
		teamPanel.gameObject.SetActive(false);
	}

	public void OpenAthletePanel(Athlete athlete)
	{
		selectedAthlete = athlete;

		athletePanel.SetActive(true);

		athleteNameText.text = athlete.GetName();
		athleteImage.sprite = athlete.sprite;

		float normalizedHealth = athlete.health / athlete.maxHealth;
		healthIndicatorFill.fillAmount = normalizedHealth;
		healthIndicatorFill.color = Color.Lerp(Color.red, Color.green, normalizedHealth);

		AttributeGroup[] attributeGroups = (AttributeGroup[])System.Enum.GetValues(typeof(AttributeGroup));
		for(int i = 0; i < attributeGroups.Length; i++) //Assumes attributeCategoryHolder.childCount == attributeGroups.Length
		{
			athleteAttributeContainers[i].SetAthleteAttributeContainer(athlete, attributeGroups[i]);
		}

		for (int i = athleteTrophyCase.childCount - 1; i > -1; i--)
		{
			Destroy(athleteTrophyCase.GetChild(i).gameObject);
		}

		for(int i = 0; i < leagueManager.currentSeason; i++)
		{
			int season = i + 1; //Display season as +1 (for non-programmer viewing)

			if (athlete.championshipSeasons.Contains(i))
			{
				GameObject newTrophy = Instantiate(trophyPrefab_Championship, athleteTrophyCase);
				newTrophy.GetComponentInChildren<TextMeshProUGUI>().text = season.ToString();
			}

			if(athlete.mvpSeasons.Contains(i))
			{
				GameObject newTrophy = Instantiate(trophyPrefab_MVP, athleteTrophyCase);
				newTrophy.GetComponentInChildren<TextMeshProUGUI>().text = season.ToString();
			}

			if(athlete.allStarSeasons.Contains(i))
			{
				GameObject newTrophy = Instantiate(trophyPrefab_AllStar, athleteTrophyCase);
				newTrophy.GetComponentInChildren<TextMeshProUGUI>().text = season.ToString();
			}
		}

		if (athlete.GetTeam().playerControlled && leagueManager.seasonChampion == null && athlete.weeksInjured <= 0) //Don't allow interactions w/ opposing teams or during the period after champions have been crowned (to prevent issues w/ All Star Lists)
			rosterInteractionButtons.SetActive(true);
		else
			rosterInteractionButtons.SetActive(false);

		if (athlete.positionGroup != null)
			benchButton.SetActive(true);
		else
			benchButton.SetActive(false);
	}

	public void SendAthleteToBench()
	{
		selectedAthlete.ChangePosition(null);

		OpenTeamPanel(team);
		OpenAthletePanel(selectedAthlete);
	}

	public void HideAthletePanel()
	{
		selectedAthlete = null;

		athletePanel.SetActive(false);
	}

	public void OpenBenchPanel() //For UI interaction, to view list of all bench athletes with no add functionality
	{
		OpenBenchPanel(null);
	}

	public void OpenBenchPanel(PositionGroup? positionGroupToAdd)
	{
		benchPanel.SetActive(true);

		if (positionGroupToAdd == null)
			benchTitleText.text = "Bench";
		else
			benchTitleText.text = "Add a " + positionGroupToAdd.ToString() + " from your bench.";

		AttributeGroup? displayedAttributeGroup = GetBestAttributeGroupForAthlete(positionGroupToAdd);

		if (displayedAttributeGroup == null)
			attributeText.text = "Overall";
		else
			attributeText.text = displayedAttributeGroup.ToString();

		for (int i = benchAthleteRosterContainerHolder.childCount - 1; i >= 0; i--)
		{
			Destroy(benchAthleteRosterContainerHolder.GetChild(i).gameObject);
		}
		
		List<Athlete> athletesOnBench = team.GetAthletesByPosition(null);
		for(int i = 0; i < athletesOnBench.Count; i ++)
		{
			if (athletesOnBench[i].weeksInjured <= 0 || positionGroupToAdd == null) //If the player is injured and the user is attempting to add a player, don't display the injured player (this is kinda scuffed but the logic works for now)
			{
				AthleteRosterContainer newRosterContainer = Instantiate(athleteRosterContainerPrefab, benchAthleteRosterContainerHolder).GetComponent<AthleteRosterContainer>();

				newRosterContainer.SetAthlete(athletesOnBench[i], displayedAttributeGroup);

				if (positionGroupToAdd != null)
				{
					newRosterContainer.button.onClick.RemoveAllListeners();
					newRosterContainer.button.onClick.AddListener(() => AddFromBench(newRosterContainer.athlete, positionGroupToAdd));
				}
			}
		}
	}

	public AttributeGroup? GetBestAttributeGroupForAthlete(PositionGroup? positionGroup = null)
	{
		AttributeGroup? determinedAttributeGroup = null;
		switch (positionGroup)
		{
			case PositionGroup.QuarterBack:
				determinedAttributeGroup = AttributeGroup.Passing;
				break;
			case PositionGroup.ReceivingBack:
				determinedAttributeGroup = AttributeGroup.Receiving;
				break;
			case PositionGroup.RunningBack:
				determinedAttributeGroup = AttributeGroup.Rushing;
				break;
			case PositionGroup.OffensiveLineman:
				determinedAttributeGroup = AttributeGroup.Blocking;
				break;
			case PositionGroup.DefensiveLineman:
				determinedAttributeGroup = AttributeGroup.Blitzing;
				break;
			case PositionGroup.DefensiveBack:
				determinedAttributeGroup = AttributeGroup.Coverage;
				break;
			case PositionGroup.Kicker:
				determinedAttributeGroup = AttributeGroup.Kicking;
				break;
			default: //No position (on bench)
				determinedAttributeGroup = null;
				break;
		}

		return determinedAttributeGroup;
	}

	public void AddFromBench(Athlete athlete, PositionGroup? newPosition)
	{
		Debug.Log("Adding athlete from bench");

		athlete.ChangePosition(newPosition);

		HideBenchPanel();
		OpenTeamPanel(team);
	}

	public void HideBenchPanel()
	{
		benchPanel.SetActive(false);
	}
}
