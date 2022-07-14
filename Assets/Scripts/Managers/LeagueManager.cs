using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeagueManager : MonoBehaviour
{
	public int teamCount = 8;
	//These are all starting counts
	public int offensiveLinemanCount = 3;
	public int runningBackCount = 3;
	public int quarterBackCount = 0;
	public int receivingBackCount = 0;
	public int defensiveLinemanCount = 3;
	public int defensiveBackCount = 3;
	public int kickerCount = 0;

	private int positionGroupLimit = 6;
	private int numDraftChoices = 3;

	public int regularSeasonLength = 7;
	public int postseasonTeamCount = 8;

	public bool quarterbacksAndReceiversAllowed = false;
	public bool kickersAllowed = false;

	//[Header("Name String Data Locations")]
	private string athleteNameListPath;
	private string[] athleteNameList;

	private string cityNameListPath;
	private string[] cityNameList;

	private string mascotNameListPath;
	private string[] mascotNameList;

	private string athleteSpritePath;
	private List<Sprite> athleteSpriteList;

	//private string lawObjectPath;
	//public List<Law> lawList;
	public Law[] lawList; //Should be generated from resources folder
	public WeeklyEvent[] weeklyEventList; //Should be generated from resources folder

	[Header("League")]
	public List<Team> teamList;
	private List<Team> postseasonTeamList;

	public List<SeasonHistory> historyList = new List<SeasonHistory>();

	public int currentSeason = -1;
	public int currentWeek = -1;
	public List<Week> weekList = new List<Week>();
	public bool postseason = false;
	public Team seasonChampion =  null;
	public Athlete seasonMVP = null;
	//private List<Week> postseasonWeekList = new List<Week>();


	//User Info
	[HideInInspector] public Team userTeam = null;

	private StartInterfaceManager startInterfaceManager;
	private MatchManager matchManager;
	private LeagueInterfaceManager interfaceManager;

	public void Start()
	{
		startInterfaceManager = FindObjectOfType<StartInterfaceManager>();
		interfaceManager = FindObjectOfType<LeagueInterfaceManager>();
		matchManager = GetComponent<MatchManager>();

		athleteNameListPath = Application.streamingAssetsPath + "/AthleteNames.txt";
		cityNameListPath = Application.streamingAssetsPath + "/CityNames.txt";
		mascotNameListPath = Application.streamingAssetsPath + "/MascotNames.txt";

		athleteSpritePath = "AthleteSprites";
		//lawObjectPath = "Laws";

		athleteNameList = System.IO.File.ReadAllLines(athleteNameListPath); //Generating the name arrays
		cityNameList = System.IO.File.ReadAllLines(cityNameListPath);
		mascotNameList = System.IO.File.ReadAllLines(mascotNameListPath);
		athleteSpriteList = new List<Sprite>(Resources.LoadAll<Sprite>(athleteSpritePath));

		userTeam = null;

		if(!startInterfaceManager.startOptionsPanel.activeSelf) //If the start screen is disabled, just start a rando
		{
			SetupLeague();
		}
	}

	public void SetUserTeam(Team user)
	{
		userTeam = user;
		user.playerControlled = true;

		Debug.Log("User team is " + userTeam.GetTeamName());
	}

	public void SetupLeague() //Called once
	{
		Debug.Log("Setting up League");

		EstablishLaws();

		interfaceManager.UpdatePositionGroupVisibility();

		GenerateTeams();

		if (userTeam == null) //If the user doesn't have a team, assign them one
			SetUserTeam(teamList[0]);

		SetNewSeason();

		BeginWeek();
	}

	public void EstablishLaws()
	{
		for(int i = 0; i < lawList.Length; i++)
		{
			lawList[i].enacted = lawList[i].startsEnacted;

			lawList[i].value = lawList[i].startValue;
		}
	}

	public void GenerateTeams()
	{
		for (int i = teamList.Count; i < teamCount; i++)
		{
			string cityName = cityNameList[UnityEngine.Random.Range(0, cityNameList.Length)];
			string mascotName = mascotNameList[UnityEngine.Random.Range(0, mascotNameList.Length)];

			Team newTeam = new Team(cityName, mascotName);
			teamList.Add(newTeam);

			GenerateRoster(newTeam);
		}
	}

	public void GenerateRoster(Team team)
	{
		List<Athlete> newRoster = new List<Athlete>();

		for (int j = 0; j < offensiveLinemanCount; j++)
		{
			Athlete newAthlete = GenerateNewAthlete(team);
			newAthlete.positionGroup = PositionGroup.OffensiveLineman;
			newRoster.Add(newAthlete);
		}
		for (int j = 0; j < runningBackCount; j++)
		{
			Athlete newAthlete = GenerateNewAthlete(team);
			newAthlete.positionGroup = PositionGroup.RunningBack;
			newRoster.Add(newAthlete);
		}

		for (int j = 0; j < defensiveLinemanCount; j++)
		{
			Athlete newAthlete = GenerateNewAthlete(team);
			newAthlete.positionGroup = PositionGroup.DefensiveLineman;
			newRoster.Add(newAthlete);
		}
		for (int j = 0; j < defensiveBackCount; j++)
		{
			Athlete newAthlete = GenerateNewAthlete(team);
			newAthlete.positionGroup = PositionGroup.DefensiveBack;
			newRoster.Add(newAthlete);
		}

		team.SetRoster(newRoster);//New Roster is Set

		for(int i = 0; i < team.GetRoster().Count; i++)
		{
			team.GetRoster()[i].SetLeagueManager(this); //Kind of a gross way of doing this but it should work for now
		}
	}

	public Athlete GenerateNewAthlete(Team team)
	{
		return new Athlete(team, athleteNameList[UnityEngine.Random.Range(0, athleteNameList.Length)], athleteNameList[UnityEngine.Random.Range(0, athleteNameList.Length)], athleteSpriteList[UnityEngine.Random.Range(0, athleteSpriteList.Count)]);
	}

	public void SetNewSeason()
	{
		currentSeason++;

		postseason = false;
		seasonChampion = null;
		seasonMVP = null;
		currentWeek = 0;

		for (int i = 0; i < teamList.Count; i++)
		{
			teamList[i].StartNewSeason();
		}

		GenerateSchedule();

		interfaceManager.DisplaySeasonStart();
	}

	//TODO: Fix schedule generation so that each team is given the same number of home/away games
	public void GenerateSchedule()
	{
		weekList = new List<Week>();

		List<Team> unassignedTeams = new List<Team>();
		for (int i = 0; i < teamList.Count; i++)
		{
			unassignedTeams.Add(teamList[i]);

		} //add null team if list is odd, don't worry about that for now

		unassignedTeams.Shuffle();

		for (int w = 0; w < regularSeasonLength; w++) //each week
		{
			List<Matchup> matchesThisWeek = new List<Matchup>();

			Team[] orderedTeams = new Team[unassignedTeams.Count];
			orderedTeams[0] = unassignedTeams[0];
			for(int t = 1; t < orderedTeams.Length; t++) //order each team
			{
				int orderedInt = (t + w) % (orderedTeams.Length - 1); //each team is ordered by the team int + week int, mod to keep it within teamSlot.Length
				orderedTeams[t] = unassignedTeams[orderedInt + 1];

				//orderedTeams[t].cityName = t.ToString();
			}

			for(int m = 0; m < orderedTeams.Length / 2; m++) //each matchup (half the number of teams)
			{
				Team team1;
				Team team2;

				team1 = orderedTeams[m];
				team2 = orderedTeams[orderedTeams.Length - 1 - m];

				Matchup newMatchup;
				if (w % 2 == 0) //Alternates home and away
					newMatchup = new Matchup(team1, team2);
				else
					newMatchup = new Matchup(team2, team1);

				matchesThisWeek.Add(newMatchup);
			}

			weekList.Add(new Week(matchesThisWeek));
		}
	}

	public void BeginWeek()
	{
		interfaceManager.UpdatePlayerBanner();

		interfaceManager.SetupMatchupPanel(weekList[currentWeek].matchups);
		interfaceManager.UpdateStandingsPanel();
		interfaceManager.UpdateSeasonSummaryPanel();

		for(int i = 0; i < teamList.Count; i++)
		{
			Team team = teamList[i];

			if (team.playerControlled) //Currently only players receive events - will later allow AI to receive them as well
			{
				WeeklyEvent weeklyEvent = GetWeeklyEvent(team);
				if (weeklyEvent)
				{
					interfaceManager.DisplayEvent(team, weeklyEvent);
				}
			}
			else
			{
				DetermineRosterAdjustments(team);
			}
		}
	}

	public void DetermineRosterAdjustments(Team team)
	{
		//Send hurt athletes to bench

		//Determine which positions need (or want) additional athletes

		//Add hurt athletes to positions of need
		//If postseason, add hurt athletes to positions of want

		if (quarterbacksAndReceiversAllowed && team.GetAthletesByPosition(PositionGroup.QuarterBack).Count == 0) //If the team has zero quarterbacks, move a player to this position
			MoveOrAddAthleteToPosition(team, PositionGroup.QuarterBack);

		if (quarterbacksAndReceiversAllowed && team.GetAthletesByPosition(PositionGroup.ReceivingBack).Count == 0)
			MoveOrAddAthleteToPosition(team, PositionGroup.ReceivingBack);

		if (kickersAllowed && team.GetAthletesByPosition(PositionGroup.Kicker).Count == 0)
			MoveOrAddAthleteToPosition(team, PositionGroup.Kicker);

		if (team.GetAthletesByPosition(PositionGroup.RunningBack).Count == 0)
			MoveOrAddAthleteToPosition(team, PositionGroup.RunningBack);

		if (team.GetAthletesByPosition(PositionGroup.OffensiveLineman).Count == 0)
			MoveOrAddAthleteToPosition(team, PositionGroup.OffensiveLineman);

		if (team.GetAthletesByPosition(PositionGroup.DefensiveLineman).Count == 0)
			MoveOrAddAthleteToPosition(team, PositionGroup.DefensiveLineman);

		if (team.GetAthletesByPosition(PositionGroup.DefensiveBack).Count == 0)
			MoveOrAddAthleteToPosition(team, PositionGroup.DefensiveBack);
	}

	public void MoveOrAddAthleteToPosition(Team team, PositionGroup position) //Tool to add an additional athlete to the selected PositionGroup
	{
		Athlete newAthlete;
		if (team.GetAthletesByPosition(null).Count > 0) //If there is a player on the bench, assign them to this position
		{
			newAthlete = team.GetAthletesByPosition(null)[0];
		}
		else
		{
			//Placeholder Hire Free Agent - TODO: Turn into function shared w/ player
			newAthlete = GenerateNewAthlete(team);
			team.AddToRoster(newAthlete);
			team.gold -= 10;
		}

		newAthlete.ChangePosition(position);
	}

	public WeeklyEvent GetWeeklyEvent(Team team)
	{
		WeeklyEvent newEvent = null;
		List<WeeklyEvent> possibleEvents = new List<WeeklyEvent>();

		for (int i = 0; i < weeklyEventList.Length; i++)
		{
			WeeklyEvent e = weeklyEventList[i];

			e.impactedTeam = team;

			bool canBeTriggered = true;

			if (e.weeksUntilRepeat > 0) //Subtract one week
				e.weeksUntilRepeat--;

			if (e.weeksUntilRepeat <= 0)
			{
				for (int t = 0; t < e.eventTriggers.Count; t++)
				{
					EventTrigger trigger = e.eventTriggers[t];
					float value = e.eventTriggerValue[t];

					switch (trigger)
					{
						case EventTrigger.StartSeason:
							if (currentWeek != 0) //If it's not the first week
								canBeTriggered = false;

							if (value != currentSeason) //If it's not the corresponding season
								canBeTriggered = false;
							break;
						case EventTrigger.FanApprovalBelow:
							if (team.fanApproval > value) //If fan approval is above the value threshold
								canBeTriggered = false;
							break;
					}
				}
			}
			else //This weeklyEvent is on cooldown
			{
				canBeTriggered = false;
			}

			if (canBeTriggered)
				possibleEvents.Add(e);
		}

		if (possibleEvents.Count > 0)
			newEvent = possibleEvents[UnityEngine.Random.Range(0, 1)];

		return newEvent;
	}

	public void ResolveWeeklyEvent(Team team, WeeklyEvent resolvedEvent, WeeklyEventOption chosenOption)
	{
		for(int i = 0; i < chosenOption.optionResults.Count; i++)
		{
			WeeklyOptionResult result = chosenOption.optionResults[i];
			float value = chosenOption.optionResultValues[i];

			switch(result)
			{
				case WeeklyOptionResult.FanApproval:
					team.fanApproval += value;
					break;
				case WeeklyOptionResult.Gold:
					team.gold += (int)value;
					break;
			}
		}

		resolvedEvent.weeksUntilRepeat = resolvedEvent.weeksBetweenRepeat;
	}

	public void FinishMatchup(Matchup matchup)
	{
		interfaceManager.DisplayCompleteMatchup(matchup);

		//if(weekList[currentWeek].GetAllGamesComplete())
			//SetBestStatsForSeason();
	}

	//TODO: Rework Postseason Week to be more integrated w/ Advance Week (maybe continue with current week - simply add to matchup list each week)
	public void AdvanceToNextWeek()
	{
		currentWeek++;

		for(int i = 0; i < teamList.Count; i++)
		{
			for(int a = 0; a < teamList[i].GetRoster().Count; a++)
			{
				teamList[i].GetRoster()[a].RecoverHealthForWeek();
			}
		}

		if (currentWeek == regularSeasonLength) //End of regular season
			GeneratePostseason();
		else if (currentWeek > regularSeasonLength) //Postseason week
			AdvancePostseasonWeek();
		else //Regular season week
			BeginWeek();
	}

	public void SimulateRemainingMatchupsForWeek()
	{
		for (int i = 0; i < weekList[currentWeek].matchups.Count; i++)
		{
			if (!weekList[currentWeek].matchups[i].completed) //If match is not completed
			{
				matchManager.SetupMatch(weekList[currentWeek].matchups[i], true); //Simulate the match
			}
		}
	}

	public void AdvancePostseasonWeek()
	{
		//Remove all the losers from the postseasonList
		for (int i = 0; i < weekList[currentWeek - 1].matchups.Count; i++)
		{
			Matchup match = weekList[currentWeek - 1].matchups[i];
			if (match.homeScore < match.awayScore) //If the home team lost
				postseasonTeamList.Remove(match.homeTeam);
			else //If the home team won or tied
				postseasonTeamList.Remove(match.awayTeam);
		}

		//Move this to BeginWeek possibly (maybe)
		if (postseasonTeamList.Count == 1)
		{
			Debug.Log("Postseason complete - champions crowned!");

			Team champion = postseasonTeamList[0];
			seasonChampion = champion;

			champion.championshipSeasons.Add(currentSeason);
			for (int i = 0; i < champion.GetRoster().Count; i++)
				champion.GetRoster()[i].championshipSeasons.Add(currentSeason);

			interfaceManager.DisplayChampion(champion);
		}
		else
		{
			GenerateNextPostseasonWeek();

			BeginWeek();
		}
	}


	public void GeneratePostseason()
	{
		Debug.Log("Generating Postseason");

		postseason = true;

		postseasonTeamList = new List<Team>();
		List<Team> remainingTeams = new List<Team>();

		remainingTeams.AddRange(teamList);

		for(int t = 0; t < postseasonTeamCount; t++)
		{
			Team highestTeam = null;
			int highestWins = -1;

			for (int i = 0; i < remainingTeams.Count; i++)
			{
				if(remainingTeams[i].GetCurrentSeasonStats().wins > highestWins)
				{
					highestTeam = remainingTeams[i];
					highestWins = highestTeam.GetCurrentSeasonStats().wins;
				}
			}

			postseasonTeamList.Add(highestTeam);
			remainingTeams.Remove(highestTeam);
		}

		GenerateNextPostseasonWeek();

		DetermineAllStars();

		BeginWeek();

		interfaceManager.ShowLeaguePanel(interfaceManager.allStarPanel);
	}

	public void GenerateNextPostseasonWeek()
	{
		//Debug.Log("Generating next postseason week");

		List<Matchup> matchups = new List<Matchup>();

		for (int i = 0; i < postseasonTeamList.Count; i += 2)
		{
			Team homeTeam = postseasonTeamList[i];
			Team awayTeam = postseasonTeamList[postseasonTeamList.Count - 1 - i];
			matchups.Add(new Matchup(homeTeam, awayTeam));
		}

		weekList.Add(new Week(matchups));
	}

	public void DetermineAllStars()
	{
		List<Athlete> allStars = new List<Athlete>();
		foreach(PositionGroup position in System.Enum.GetValues(typeof(PositionGroup)))
		{
			List<Athlete> eligibleAthletes = new List<Athlete>();

			for (int t = 0; t < teamList.Count; t++)
			{
				eligibleAthletes.AddRange(teamList[t].GetAthletesByPosition(position));
			}

			if (eligibleAthletes.Count > 0)
			{
				eligibleAthletes.Sort((a, b) => b.GetDivineSpectacleSum_CurrentSeason().CompareTo(a.GetDivineSpectacleSum_CurrentSeason()));

				Athlete allStarAtPosition = eligibleAthletes[0];
				allStarAtPosition.allStarSeasons.Add(currentSeason);

				allStars.Add(allStarAtPosition);
			}
		}

		Athlete mvp; //Determine overall regular season MVP

		allStars.Sort((a, b) => b.GetDivineSpectacleSum_CurrentSeason().CompareTo(a.GetDivineSpectacleSum_CurrentSeason()));
		mvp = allStars[0];
		seasonMVP = mvp;

		mvp.mvpSeasons.Add(currentSeason);
	}

	public void ConcludeSeason()
	{
		Debug.Log("Concluding Season");

		BeginOffseason();
	}

	public void BeginOffseason()
	{
		DisplayPostSeasonNews();
	}

	public void DisplayPostSeasonNews()
	{
		//Determine top season performers

		//TODO: Stats should be compiled based on categories - determine best athlete by position
		//DAN DO THIS ONE NEXT (maybe)

		//

		//

		BeginElection();
	}

	public void BeginElection()
	{
		//TODO: Clean this up with a list

		Amendment firstAmendment = lawList[UnityEngine.Random.Range(0, lawList.Length)].GeneratePotentialAmendment();
		Amendment secondAmendment = firstAmendment;
		while(secondAmendment.law.id == firstAmendment.law.id)
		{
			secondAmendment = lawList[UnityEngine.Random.Range(0, lawList.Length)].GeneratePotentialAmendment();
		}
		Amendment thirdAmendment = firstAmendment;
		while (thirdAmendment.law.id == firstAmendment.law.id || thirdAmendment.law.id == secondAmendment.law.id)
		{
			thirdAmendment = lawList[UnityEngine.Random.Range(0, lawList.Length)].GeneratePotentialAmendment();
		}

		List<Amendment> amendmentList = new List<Amendment>();
		amendmentList.Add(firstAmendment);
		amendmentList.Add(secondAmendment);
		amendmentList.Add(thirdAmendment);

		interfaceManager.DisplayElectionPanel(amendmentList);
	}

	public void ConcludeElection(Amendment amendmentPassed)
	{
		Law lawChanged = null;
		for(int i = 0; i < lawList.Length; i++)
		{
			if(amendmentPassed.law == lawList[i])
			{
				lawChanged = lawList[i];
				break;
			}
		}

		lawChanged.ChangeLaw(amendmentPassed.amendmentType);

		switch (lawChanged.id)
		{
			case LawType.ForwardPass:
				if (lawChanged.enacted) //If forward pass was enacted
				{
					quarterbacksAndReceiversAllowed = true;
				}
				else //If forward pass was revoked
				{
					quarterbacksAndReceiversAllowed = false;

					for(int i = 0; i < teamList.Count; i++) //For each team, send all ineligible players to the bench
					{
						Team team = teamList[i];

						List<Athlete> currentQuarterbacks = team.GetAthletesByPosition(PositionGroup.QuarterBack); //Send all Quarterbacks to the bench
						for (int a = 0; a < currentQuarterbacks.Count; a++)
						{
							currentQuarterbacks[a].ChangePosition(null);
						}

						List<Athlete> currentReceivers = team.GetAthletesByPosition(PositionGroup.ReceivingBack); //Send all Receivers to the bench
						for (int a = 0; a < currentReceivers.Count; a++)
						{
							currentReceivers[a].ChangePosition(null);
						}
					}
				}
				break;
			case LawType.Punt:
			case LawType.FieldGoal:
			case LawType.PointAfterTouchdown:
				if (lawChanged.enacted)
				{
					kickersAllowed = true;
				}
				else
				{
					if(!GetLaw(LawType.Punt).enacted && !GetLaw(LawType.FieldGoal).enacted && !GetLaw(LawType.PointAfterTouchdown).enacted)
					{
						kickersAllowed = false;

						for (int i = 0; i < teamList.Count; i++) //For each team, send all ineligible players to the bench
						{
							Team team = teamList[i];

							List<Athlete> currentKickers = team.GetAthletesByPosition(PositionGroup.Kicker); //Send all Kickers to the bench
							for (int a = 0; a < currentKickers.Count; a++)
							{
								currentKickers[a].ChangePosition(null);
							}
						}
					}
				}
				break;
		}

		interfaceManager.UpdatePositionGroupVisibility();

		GenerateSeasonHistory(lawChanged);

		BeginRosterAdjustmentPeriod(lawChanged);
	}

	public void GenerateSeasonHistory(Law lawChanged)
	{
		SeasonHistory newHistory = new SeasonHistory(currentSeason, seasonChampion, seasonMVP, lawChanged);
		historyList.Add(newHistory);
		//interfaceManager.UpdateHistoryPanel();
	}

	public void BeginRosterAdjustmentPeriod(Law lawChanged) //Each team will get their own set of decisions based on the law that was changed
	{
		for (int i = 0; i < teamList.Count; i++)
		{
			Team team = teamList[i];

			List<Athlete> draftChoices = new List<Athlete>();
			for (int p = 0; p < numDraftChoices; p++)
				draftChoices.Add(GenerateNewAthlete(team));

			if (team.playerControlled) //If the team is player controlled
			{
				interfaceManager.DisplayDraftPanel(team, draftChoices);
			}
			else
			{
				Athlete draftee = draftChoices[UnityEngine.Random.Range(0, draftChoices.Count)];
				DraftPlayer(team, draftee);
			}
		}
	}

	public void DraftPlayer(Team newTeam, Athlete athlete)
	{
		//Debug.Log(athlete.GetName(false) + " drafted by " + newTeam.GetTeamName(false));

		newTeam.AddToRoster(athlete);
	}

	/*
	public Decision GetRandomDecision(Team team)
	{
		List<Decision> validDecisions = new List<Decision>();

		//TODO: Could tailor these based on team's current roster (max limit per positiongroup)

		PositionGroup[] positions = (PositionGroup[])System.Enum.GetValues(typeof(PositionGroup));

		for (int i = 0; i < positions.Length; i++)
		{
			if(team.GetAthletesByPosition(positions[i]).Count > 0) //if there's already at least one athlete in this position group
			{
				validDecisions.Add(new Decision(DecisionType.Training, positions[i]));

				if(team.GetAthletesByPosition(positions[i]).Count < positionGroupLimit)
				{
					validDecisions.Add(new Decision(DecisionType.DraftAthlete, positions[i]));
				}
			}

			//TODO: Allow athletes to be moved/swapped to other positions based on the team's roster imbalances
		}

		return validDecisions[Random.Range(0, validDecisions.Count)];
	}

	public void MakeDecision(Team team, Decision decision)
	{
		switch(decision.decisionType)
		{
			case DecisionType.PositionSwap:
				team.GetRandomAthleteByPosition(decision.firstPositionGroup).ChangePosition(decision.secondPositionGroup);
				team.GetRandomAthleteByPosition(decision.secondPositionGroup).ChangePosition(decision.firstPositionGroup);
				break;
			case DecisionType.PositionMove:
				team.GetRandomAthleteByPosition(decision.firstPositionGroup).ChangePosition(decision.secondPositionGroup);
				break;
			case DecisionType.DraftAthlete:
				Athlete newAthlete = GenerateNewAthlete(team);
				newAthlete.ChangePosition(decision.firstPositionGroup);
				team.AddToRoster(newAthlete);
				break;
			case DecisionType.Training:

				//TODO: Determine which attribute to increase (probably based on position)
				AttributeGroup ag = AttributeGroup.Rushing;
				switch(decision.firstPositionGroup)
				{
					case PositionGroup.OffensiveLineman:
						ag = AttributeGroup.Blocking;
						break;
					case PositionGroup.RunningBack:
						ag = AttributeGroup.Rushing;
						break;
					case PositionGroup.QuarterBack:
						ag = AttributeGroup.Passing;
						break;
					case PositionGroup.ReceivingBack:
						ag = AttributeGroup.Receiving;
						break;
					case PositionGroup.DefensiveLineman:
						ag = AttributeGroup.Blitzing;
						//tackling?
						break;
					case PositionGroup.DefensiveBack:
						ag = AttributeGroup.Coverage;
						//tackling?
						break;
					case PositionGroup.Kicker:
						ag = AttributeGroup.Kicking;
						break;
				}

				team.GetRandomAthleteByPosition(decision.firstPositionGroup).IncreaseRandomAttribute(ag, 0.3f);
				break;
		}
	}
	*/

	public void ConcludeRosterAdjustmentPeriod()
	{
		SetNewSeason();

		BeginWeek();
	}

	public Law GetLaw(LawType lawType)
	{
		Law law = null;

		for(int i = 0; i < lawList.Length; i++)
		{
			if(lawList[i].id == lawType)
			{
				law = lawList[i];
				break;
			}
		}

		return law;
	}

	public bool CheckAttributeIneligibility(AttributeGroup attributeGroup)
	{
		if (!quarterbacksAndReceiversAllowed && (attributeGroup == AttributeGroup.Passing || attributeGroup == AttributeGroup.Receiving) || (!kickersAllowed && attributeGroup == AttributeGroup.Kicking))
			return true;
		else
			return false;
	}

	//I don't think this is actually necessary based on the new way I'm doing thangs
	public void SetBestStatsForSeason()
	{
		List<Stat> bestStats = new List<Stat>(); //Used purely for comparing athlete stat's with the best stats to show if they should be displayed.
		foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
		{
			bestStats.Add(GetBestStatInLeague(statType));
		}
	}

	public Stat GetBestStatInLeague(StatType statType)
	{
		Athlete athleteWithHighestStat = null;
		Stat highestStat = null;
		int highestValue = -1;
		for(int i = 0; i < teamList.Count; i++)
		{
			for(int a = 0; a < teamList[i].GetRoster().Count; a++)
			{
				Stat checkStat = teamList[i].GetRoster()[a].GetStat_CurrentSeason(statType);
				if(checkStat.count > highestValue)
				{
					highestValue = checkStat.count;
					highestStat = checkStat;
					athleteWithHighestStat = teamList[i].GetRoster()[a];
				}
			}
		}

		//Debug.Log("Best " + statType + " stater is " + athleteWithHighestStat.GetName(false));

		return highestStat;
	}

	public void InjuryOccured(Athlete athlete)
	{
		matchManager.InjuryOccured(athlete);

		//Matchup affectedMatchup;
		//for (int i = 0; i < weekList[currentWeek].matchups.Count; i++)
		//{
		//	if (weekList[currentWeek].matchups[i].homeTeam == athlete.GetTeam() || weekList[currentWeek].matchups[i].awayTeam == athlete.GetTeam()) //If athlete belongs to one of the teams participating in this matchup
		//	{
		//		matchManager.InjuryOccured(athlete);
		//	    break;
		//	}
		//}
	}
}
