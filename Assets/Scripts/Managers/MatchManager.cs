using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MatchManager : MonoBehaviour
{
	private LeagueManager leagueManager;
	private MatchInterfaceManager interfaceManager;

	[Header("Rules")]
	[HideInInspector] public int fieldLength = 100;
	private int ballStart = 20;
	private int yardsForFirstDown = 10;
	private int downsAllowed = 4;

	private int pointsFromTouchdown = 6;
	private int pointsFromFieldGoal = 3;
	private int pointsFromExtraPoint = 1;

	private int timeLeft;
	private bool overtime;

	private bool simulated = false;

	//Match Objects
	private Matchup matchup;
	private Team home;
	private Team away;

	private Team offense;
	private Team defense;

	private int lineOfScrimmage;
	private int firstDownYardage;
	private int down;

	//Reset each play
	//private int playLane;
	private int yardsGained = 0;
	private List<Athlete> whiffers = new List<Athlete>();

	//Timing
	public float newLogDuration = 1.3f;

	private void Start()
	{
		leagueManager = FindObjectOfType<LeagueManager>();

		interfaceManager = FindObjectOfType<MatchInterfaceManager>();
	}

	#region-Tools
	public void QueueNextAction(UnityAction nextAction)
	{
		if (simulated) //If this is simulated then no delay
		{
			nextAction();
		}
		else
		{
			StartCoroutine(QueueNextActionCoroutine(nextAction));
		}

	}
	public IEnumerator QueueNextActionCoroutine(UnityAction nextAction)
	{
		yield return new WaitForSeconds(newLogDuration);

		nextAction();
	}

	public int GetLineOfScrimmage()
	{
		return lineOfScrimmage;
	}
	public int GetDown()
	{
		return down;
	}
	public int GetYardsToGain()
	{
		return firstDownYardage - lineOfScrimmage;
	}
	#endregion

	public void SetupMatch(Matchup match, bool sim)
	{
		//Set Laws Function?
		yardsForFirstDown = leagueManager.GetLaw(LawType.YardsForFirstDown).value;
		downsAllowed = leagueManager.GetLaw(LawType.Downs).value;

		simulated = sim;

		matchup = match;
		home = matchup.homeTeam;
		away = matchup.awayTeam;

		offense = home;
		defense = away;

		timeLeft = leagueManager.GetLaw(LawType.MatchTime).value;
		down = 1;
		lineOfScrimmage = ballStart;
		firstDownYardage = lineOfScrimmage + yardsForFirstDown;

		for(int i = 0; i < home.GetRoster().Count; i++)
		{
			home.GetRoster()[i].CreateNewGameData();
		}
		for (int i = 0; i < away.GetRoster().Count; i++)
		{
			away.GetRoster()[i].CreateNewGameData();
		}

		if (!simulated)
		{
			interfaceManager.DisplayPanel(true);
			interfaceManager.SetupMatchInterface(home, away);
			interfaceManager.UpdateLineOfScrimmage();
			interfaceManager.UpdateScore(true, matchup.homeScore);
			interfaceManager.UpdateScore(false, matchup.awayScore);
			interfaceManager.UpdateTime(timeLeft);
		}

		QueueNextAction(() => StartMatch());
	}

	public void StartMatch()
	{
		QueueNextAction(() => NextPlay());
	}

	public void TickTime(int timeTicked)
	{
		timeLeft -= timeTicked;

		if (timeLeft >= 0)
		{
			if (!simulated)
				interfaceManager?.UpdateTime(timeLeft);
		}
	}

	public void CreateGameLog(string newLog)
	{
		if (!simulated)
			interfaceManager?.AddNewGameLog(offense, newLog, timeLeft);
	}

	public void NextPlay()
	{
		//playLane = 1; //This shouldn't matter. Default to center
		yardsGained = 0;
		whiffers = new List<Athlete>();

		bool matchEnding = false;
		if (timeLeft <= 0)
		{
			if(!overtime) //If it's not overtime, then the game can end. (overtime is unchecked on scoring plays?)
			{
				matchEnding = true;
			}
		}

		if (!matchEnding)
		{
			DeterminePlay();
		}
		else
		{
			EndMatch();
		}
	}

	Dictionary<Athlete, List<Athlete>> coverageMatchupDictionary;
	Dictionary<Athlete, List<Athlete>> blitzMatchupDictionary;

	public void DeterminePlay()
	{
		//Debug.Log("Determining Play");
		coverageMatchupDictionary = new Dictionary<Athlete, List<Athlete>>();
		blitzMatchupDictionary = new Dictionary<Athlete, List<Athlete>>();

		if (offense.GetAthletesByPosition(PositionGroup.ReceivingBack).Count > 0)
			coverageMatchupDictionary = DetermineMatchupsForPositionGroups(PositionGroup.ReceivingBack, true, PositionGroup.DefensiveBack);
		if (defense.GetAthletesByPosition(PositionGroup.DefensiveLineman).Count > 0)
			blitzMatchupDictionary = DetermineMatchupsForPositionGroups(PositionGroup.DefensiveLineman, false, PositionGroup.OffensiveLineman);

		bool punting = false;
		bool kicking = false;
		bool passing = false;

		if (down == downsAllowed)
		{
			if (firstDownYardage - lineOfScrimmage <= 1) //If it's 4th and 1, the team elects to go for it
			{
				kicking = false;
				punting = false;
			}
			else
			{
				int maxFieldGoalDistance = 40;
				int distanceFromFieldGoal = fieldLength - lineOfScrimmage;

				if (distanceFromFieldGoal > maxFieldGoalDistance)
				{
					if (leagueManager.GetLaw(LawType.Punt).enacted)
					{
						punting = true;
					}
				}
				else //Within field goal range
				{
					if (leagueManager.GetLaw(LawType.FieldGoal).enacted)
					{
						kicking = true;
					} //Else no reason to punt so run the play
				}
			}
		}

		if (leagueManager.GetLaw(LawType.ForwardPass).enacted && offense.GetAthletesByPosition(PositionGroup.QuarterBack).Count > 0)
		{
			float passAttemptChance = 0.5f;
			float passRoll = Random.value;
			
			if(passAttemptChance >= passRoll)
				passing = true;
		}

		Athlete chosenAthlete;

		if (kicking)
		{
			chosenAthlete = offense.GetAthletesByPosition(PositionGroup.Kicker)[Random.Range(0, offense.GetAthletesByPosition(PositionGroup.Kicker).Count)];

			if (!simulated)
			{
				CreateGameLog(offense.GetTeamName() + " have decided to attempt a field goal. " + chosenAthlete.GetName() + " takes their position");

				interfaceManager.ShowPuntingArrow(true);
			}

			QueueNextAction(() => AttemptFieldGoal(chosenAthlete));
		}
		else if (punting)
		{
			chosenAthlete = offense.GetAthletesByPosition(PositionGroup.Kicker)[Random.Range(0, offense.GetAthletesByPosition(PositionGroup.Kicker).Count)];

			if (!simulated)
			{
				CreateGameLog(offense.GetTeamName() + " have decided to punt. " + chosenAthlete.GetName() + " receives the long snap.");

				interfaceManager.ShowPuntingArrow(true);
			}
			QueueNextAction(() => PuntBall(chosenAthlete));
		}
		else if(passing)
		{
			chosenAthlete = offense.GetRandomAthleteByPosition(PositionGroup.QuarterBack);
			chosenAthlete.IncreaseStat(StatType.Snaps, 1);

			if (!simulated)
				CreateGameLog("Ball is snapped to " + chosenAthlete.GetName() + ".");

			QueueNextAction(() => DropBackToPass(chosenAthlete));
		}
		else //Default to running
		{
			//TODO: What happens if there's no runningbacks?

			chosenAthlete = offense.GetRandomAthleteByPosition(PositionGroup.RunningBack);

			if (!simulated)
				CreateGameLog("Ball is snapped to " + chosenAthlete.GetName() + ".");

			QueueNextAction(() => AttemptRush(chosenAthlete));
		}
	}

	public void GetBlitzResult(out List<Athlete> successfulBlitzers, out List<Athlete> failedBlockers)
	{
		successfulBlitzers = new List<Athlete>();
		failedBlockers = new List<Athlete>();
		
		foreach (Athlete blitzer in defense.GetAthletesByPosition(PositionGroup.DefensiveLineman))
		{
			List<Athlete> blockers = blitzMatchupDictionary[blitzer];

			float blitzRoll = Random.value * blitzer.GetAttribute(AttributeType.BlitzPenetration);
			float guardBlockRoll = 0;
			foreach(Athlete blocker in blockers)
				guardBlockRoll += Random.value * blocker.GetAttribute(AttributeType.PassBlocking);
			float blockDifferential = guardBlockRoll - blitzRoll;

			if (blockDifferential > 0)
			{
				successfulBlitzers.Add(blitzer);
				foreach (Athlete blocker in blockers)
				{ 
					failedBlockers.Add(blocker);
					blocker.IncreaseStat(StatType.MissedBlocks, 1);
				}
			}
			else
			{
				foreach (Athlete blocker in blockers)
					blocker.IncreaseStat(StatType.Blocks, 1);
			}
		}
	}

	public void DropBackToPass(Athlete passer)
	{
		TickTime(1);

		passer.IncreaseStat(StatType.DropBacks, 1);

		if (!simulated)
		{
			CreateGameLog(passer.GetName() + " is dropping back to pass."); //to the left right or center

			interfaceManager?.SetMovementArrowLane(1);
			interfaceManager.ShowPassingArrow(true);
		}

		QueueNextAction(() => ReleaseReceivers(passer));
	}

	public Dictionary<Athlete, List<Athlete>> DetermineMatchupsForPositionGroups(PositionGroup initiator, bool offenseInitiator, PositionGroup reactors)
	{
		Dictionary<Athlete, List<Athlete>> newCoverageDictionary = new Dictionary<Athlete, List<Athlete>>();

		List<Athlete> availableInitiators = new List<Athlete>();
		if (offenseInitiator)
			availableInitiators.AddRange(offense.GetAthletesByPosition(initiator));
		else
			availableInitiators.AddRange(defense.GetAthletesByPosition(initiator));

		List<Athlete> availableReactors = new List<Athlete>();
		if (offenseInitiator)
			availableReactors.AddRange(defense.GetAthletesByPosition(reactors));
		else
			availableReactors.AddRange(offense.GetAthletesByPosition(reactors));

		for (int i = 0; i < availableInitiators.Count; i++)
		{
			List<Athlete> chosenReactors = new List<Athlete>();
			if (availableInitiators.Count > i)
				chosenReactors.Add(availableInitiators[i]);

			newCoverageDictionary.Add(availableInitiators[i], chosenReactors);
		}

		//Debug.Log(availableInitiators.Count + " count");

		if (availableReactors.Count > availableInitiators.Count)
		{
			//Debug.Log("Extra Reactor (" + availableReactors.Count + ") vs (" + availableInitiators.Count + ")");

			for (int i = availableInitiators.Count; i < availableReactors.Count; i++)
			{
				Athlete initiatorChosenForAdditionalReactor = availableInitiators[Random.Range(0, availableInitiators.Count)];
				List<Athlete> listOfReactors = newCoverageDictionary[initiatorChosenForAdditionalReactor];
				listOfReactors.Add(availableReactors[i]);

				//Debug.Log(listOfReactors.Count + " new count for " + initiatorChosenForAdditionalReactor.GetName(false));

				newCoverageDictionary.Remove(initiatorChosenForAdditionalReactor); //Removes old dictionary item
				newCoverageDictionary.Add(initiatorChosenForAdditionalReactor, listOfReactors); //Replaces with new list
			}
		}

		return newCoverageDictionary;
	}

	public void ReleaseReceivers(Athlete passer)
	{
		bool allReceiversJammed = true;
		List<Athlete> unjammedReceivers = new List<Athlete>();

		foreach (Athlete receiver in offense.GetAthletesByPosition(PositionGroup.ReceivingBack))
		{
			List<Athlete> defenders = coverageMatchupDictionary[receiver];

			float receiverRoll = Random.value * receiver.GetAttribute(AttributeType.ReceivingStrength);
			float jamRoll = 0;

			foreach (Athlete defender in defenders)
				jamRoll += Random.value * defender.GetAttribute(AttributeType.CoverageJamming);
			float strengthDifferential = receiverRoll - jamRoll;

			if (strengthDifferential > 0) //Receiver wasn't jammed - has an opportunity to catch downfield
			{
				allReceiversJammed = false;

				unjammedReceivers.Add(receiver);
			}
		}

		if(allReceiversJammed)
		{
			//if (!simulated)
			//{
			//	CreateGameLog(passer.GetName() + " can't find an open receiver yet.");
			//}

			QueueNextAction(() => ScanForPass(passer));
		}
		else
		{
			Athlete receiver = unjammedReceivers[Random.Range(0, unjammedReceivers.Count)];

			int yardsThrown = (int)(Random.value * 9f + 1f); //0 to 10 yards for quick throw

			//if (!simulated)
			//{
			//	//CreateGameLog(passer.GetName() + " throws a quick " + interfaceManager.GetStatString(yardsThrown + " yard ") + "pass towards " + receiver.GetName() + ".");

			//	//passing arrow??
			//	//interfaceManager.UpdatePassingArrow(yardsThrown);
			//}

			QueueNextAction(() => AttemptPass(passer, receiver, coverageMatchupDictionary[receiver], yardsThrown));
		}
	}

	public void ScanForPass(Athlete passer)
	{
		TickTime(1);

		List<Athlete> successfulBlitzers;
		List<Athlete> failedBlockers;
		GetBlitzResult(out successfulBlitzers, out failedBlockers);

		if(successfulBlitzers.Count > 0)
		{
			//Chance to sack
			if (!simulated)
			{
				string blitzString = successfulBlitzers[0].GetName();
				for (int i = 1; i < successfulBlitzers.Count; i++)
					blitzString += " and " + successfulBlitzers[i].GetName();

				string blockString = failedBlockers[0].GetName();
				for (int i = 1; i < failedBlockers.Count; i++)
					blockString += " and " + failedBlockers[i].GetName();

				CreateGameLog(blitzString + " blitzed past " + blockString + " and has a chance to sack the passer!");
			}

			QueueNextAction(() => AttemptSack(passer, successfulBlitzers));
		}
		else
		{
			if(!simulated)
			{
				CreateGameLog(passer.GetName() + " is looking for an open receiver. The linemen are holding steady.");
			}

			QueueNextAction(() => DetermineReceiverForPass(passer));
		}
	}

	public void AttemptSack(Athlete passer, List<Athlete> sackers)
	{
		float sackRoll = 0;
		foreach (Athlete sacker in sackers)
			sackRoll += Random.value * sacker.GetAttribute(AttributeType.BlitzPursuit);
		float dodgeRoll = Random.value * passer.GetAttribute(AttributeType.PassingElusiveness);
		float differential = dodgeRoll - sackRoll;

		if (differential > 0) //Sack fails
		{
			foreach(Athlete sacker in sackers)
				sacker.IncreaseStat(StatType.MissedSacks, 1);

			if (!simulated)
			{
				string sackString = sackers[0].GetName();
				for (int i = 1; i < sackers.Count; i++)
					sackString += " and " + sackers[i].GetName();

				CreateGameLog(passer.GetName() + " evades " + sackString + " and is looking to pass.");
			}

			QueueNextAction(() => DetermineReceiverForPass(passer));
		}
		else //Sack succeeds
		{
			foreach(Athlete sacker in sackers)
				sacker.IncreaseStat(StatType.Sacks, 1);

			float passingHustleRoll = Random.value * passer.GetAttribute(AttributeType.PassingHustle);
			float blitzHustleRoll = 0;
			foreach(Athlete sacker in sackers)
				blitzHustleRoll += Random.value * sacker.GetAttribute(AttributeType.BlitzHustle);
			float hustleDifferential = passingHustleRoll - blitzHustleRoll;

			foreach(Athlete sacker in sackers)
				passer.ApplyHarm(sacker.GetAttribute(AttributeType.BlitzHarmfulness), passer.GetAttribute(AttributeType.PassingDurability)); //Apply harm by sacker to passer

			yardsGained += (int)(hustleDifferential / 2 - 0.5f); //Between 0 and 1

			Debug.Log("Sack - review if yards gained make sense below : " + yardsGained);

			if (!simulated)
			{
				string sackString = sackers[0].GetName();
				for (int i = 1; i < sackers.Count; i++)
					sackString += " and " + sackers[i].GetName();

				CreateGameLog(sackString + " sack " + passer.GetName() + " for a loss of " + interfaceManager.GetStatString(yardsGained + " yards") + "!");

				//interfaceManager?.UpdateMovementArrowToEndzone(yardsGained, offense == home);
			}

			QueueNextAction(() => EndPlay(passer));

			Debug.Log("sacked for " + yardsGained + " yards");
		}
	}

	public void DetermineReceiverForPass(Athlete passer)
	{
		List<Athlete> availableReceivers = new List<Athlete>();
		availableReceivers.AddRange(offense.GetAthletesByPosition(PositionGroup.ReceivingBack));

		Athlete chosenReceiverTarget;
		List<Athlete> defenders;
		if(availableReceivers.Count > 0)
		{
			//TODO: Have better selection of target, better QBs will target open receivers or advantageous coverages more often

			chosenReceiverTarget = availableReceivers[Random.Range(0, availableReceivers.Count)];

			defenders = coverageMatchupDictionary[chosenReceiverTarget];
		} 
		else
		{
			Debug.Log("QUARTERBACK HAS NO RECEIVERS - they should attempt a rush instead");

			QueueNextAction(() => AttemptRush(passer));
			return;
		}

		int yardsThrown = (int)(passer.GetAttribute(AttributeType.PassingStrength) * 100f); //This number is the athlete's max throwing distance
		
		QueueNextAction(() => AttemptPass(passer, chosenReceiverTarget, defenders, yardsThrown));
	}

	public void AttemptPass(Athlete passer, Athlete receiver, List<Athlete> defenders, int maxThrow)
	{
		TickTime(1);

		passer.IncreaseStat(StatType.PassAttempts, 1);

		//COULDO: A roll here to determine separation?

		int yardsThrown = maxThrow;

		if (!simulated)
		{
			CreateGameLog(passer.GetName() + " throws the ball " + interfaceManager.GetStatString(yardsThrown + " yards") + " downfield towards " + receiver.GetName() + ".");

			//passing arrow??
			//interfaceManager.UpdatePassingArrow(yardsThrown);
		}

		QueueNextAction(() => AttemptCatch(passer, receiver, defenders, yardsThrown));
	}

	public void AttemptCatch(Athlete passer, Athlete receiver, List<Athlete> defenders, int yardDepth)
	{
		TickTime(1);
		if (!simulated)
		{
			if(defenders.Count > 0)
			{
				string defenderString = defenders[0].GetName();
				for (int i = 1; i < defenders.Count; i++)
					defenderString += " and " + defenders[i].GetName();
				CreateGameLog(receiver.GetName() + " is covered by " + defenderString + ".");
			}
			else
			{
				CreateGameLog(receiver.GetName() + " is wide open!");
			}
		}

		bool intercepted = false;
		bool brokenUp = false;
		Athlete primaryDefender = null;

		foreach (Athlete defender in defenders)
		{
			float receiverSeparationRoll = Random.value * receiver.GetAttribute(AttributeType.RouteRunning);
			float coverageRoll = Random.value * defender.GetAttribute(AttributeType.CoverageBlanketness);
			float separation = receiverSeparationRoll - coverageRoll;

			if (separation < 0) //If the receiver doesn't have separation
			{
				float interceptionOpportunityRoll = Random.value * defender.GetAttribute(AttributeType.CoverageInterceptablity);

				if (interceptionOpportunityRoll > separation) //Interception Opportunity
				{
					receiver.ApplyHarm(defender.GetAttribute(AttributeType.CoverageHarmfulness), receiver.GetAttribute(AttributeType.ReceivingDurability)); //Harm applied by defender to receiver

					float interceptionRoll = Random.value * defender.GetAttribute(AttributeType.ReceivingHands);
					float interceptionPreventionRoll = Random.value * receiver.GetAttribute(AttributeType.CoverageBreakupability);
					if (interceptionRoll > interceptionPreventionRoll) //Ball is intercepted
					{
						intercepted = true;
						primaryDefender = defender;

						break; //Prevents it from looping through to next defender, thus splitting reality into multiple parallel universes
					}

					float breakupRoll = Random.value * defender.GetAttribute(AttributeType.CoverageBreakupability);
					float receptionRoll = Random.value * receiver.GetAttribute(AttributeType.ReceivingBoxoutability);
					if (breakupRoll > receptionRoll) //Ball is broken up
					{
						brokenUp = true;
						primaryDefender = defender;

						break;
					}
				}
			}
		}

		if(intercepted)
		{
			passer.IncreaseStat(StatType.InterceptionsThrown, 1);
			primaryDefender.IncreaseStat(StatType.ForcedInterceptions, 1);

			if (!simulated)
			{
				CreateGameLog("INTERCEPTION! " + primaryDefender.GetName() + " picks off the ball!");

				interfaceManager?.UpdateMovementArrow(yardsGained); //??
			}

			QueueNextAction(() => FlipPossessionLiveBall(primaryDefender, receiver));
		}
		else if(brokenUp)
		{
			primaryDefender.IncreaseStat(StatType.ForcedIncompletions, 1);
			passer.IncreaseStat(StatType.PassIncompletions, 1);

			if (!simulated)
			{
				CreateGameLog("Incomplete pass. Ball is batted down by " + primaryDefender.GetName() + ".");
			}

			QueueNextAction(() => EndPlay(passer));
		}
		else
		{
			float passerRoll = Random.value * passer.GetAttribute(AttributeType.PassingAccuracy);
			float receiverRoll = Random.value * receiver.GetAttribute(AttributeType.ReceivingHands);
			//float defenderRoll = Random.value * defender.GetAttribute(AttributeType.CoverageBreakupability);
			float dropRoll = Random.value * 0.5f; //50% max chance of dropping

			if (passerRoll + receiverRoll > dropRoll) //Pass is caught
			{
				passer.IncreaseStat(StatType.PassCompletions, 1);
				receiver.IncreaseStat(StatType.Catches, 1);
				passer.IncreaseStat(StatType.PassingYards, yardDepth);
				receiver.IncreaseStat(StatType.ReceivingYards, yardDepth);

				if (!simulated)
				{
					if (defenders.Count > 0)
					{
						string defenderString = defenders[0].GetName();
						for (int i = 1; i < defenders.Count; i++)
							defenderString += " and " + defenders[i].GetName();
						CreateGameLog(receiver.GetName() + " makes the " + interfaceManager.GetStatString(yardsGained + " yard") + " catch over " + defenderString + "!");

					}
					else
					{
						CreateGameLog(receiver.GetName() + " makes the wide open " + interfaceManager.GetStatString(yardsGained + " yard") + " catch!");
					}

					interfaceManager?.UpdateMovementArrow(yardsGained);
					//Hide Passing arrow?
				}

				yardsGained += yardDepth;

				if (defenders.Count > 0)
				{
					//TODO: Actually represent player positions to determine who is closest?
					Athlete closestDefender = defenders[Random.Range(0, defenders.Count)];
					QueueNextAction(() => AttemptTackle(receiver, closestDefender));
				}
			} //Pass is dropped
			else
			{
				passer.IncreaseStat(StatType.PassIncompletions, 1);
				receiver.IncreaseStat(StatType.MissedCatches, 1);

				if (!simulated)
				{
					CreateGameLog(receiver.GetName() + " dropped the pass!");
				}

				QueueNextAction(() => EndPlay(passer));
			}
		}
	}

	public void AttemptRush(Athlete rusher)
	{
		TickTime(1);

		rusher.IncreaseStat(StatType.Rushes, 1);

		List<Athlete> availableBlitzers = defense.GetAthletesByPosition(PositionGroup.DefensiveLineman);

		if (availableBlitzers.Count > 0)
		{
			Athlete blitzer = availableBlitzers[Random.Range(0, availableBlitzers.Count)]; //A random blitzer is chosen to be rushed into
			List<Athlete> guards = blitzMatchupDictionary[blitzer];

			if (!simulated)
			{
				int laneChosen = Random.Range(0, 3); //Running left, center, or right

				string laneName;
				if (laneChosen == 0)
					laneName = "left";
				else if (laneChosen == 1)
					laneName = "center";
				else
					laneName = "right";

				CreateGameLog(rusher.GetName() + " is rushing " + laneName + "."); //to the left right or center

				interfaceManager?.SetMovementArrowLane(laneChosen);
				interfaceManager?.UpdateMovementArrow(0);
			}

			QueueNextAction(() => AttemptBlock(rusher, guards, blitzer));
		}
		else
		{
			QueueNextAction(() => FreeRush(rusher));
		}
	}

	//Could replace this with DetermineAdditionalTackleAttempts()
	public void FreeRush(Athlete rusher)
	{
		Debug.Log("Free rush!");

		TickTime(1);

		yardsGained += (int)(rusher.GetAttribute(AttributeType.RushingSpeed) * 10f);

		if (!simulated)
		{
			CreateGameLog(rusher.GetName() + " found an opening and rushes " + interfaceManager.GetStatString(yardsGained + " yards" + " uncontested."));

			interfaceManager?.UpdateMovementArrow(yardsGained);
		}

		QueueNextAction(() => DetermineAdditionalTackleAttempts(rusher));
	}

	public void AttemptBlock(Athlete rusher, List<Athlete> blockers, Athlete blitzer)
	{
		float guardBlockRoll = 0;
		foreach (Athlete blocker in blockers)
			guardBlockRoll += Random.value * blocker.GetAttribute(AttributeType.RushBlocking);
		float blitzContainRoll = Random.value * blitzer.GetAttribute(AttributeType.BlitzContain);
		float blockDifferential = guardBlockRoll - blitzContainRoll;

		if (blockers.Count > 0 && blockDifferential > 0)
		{
			foreach (Athlete blocker in blockers)
			{
				blocker.IncreaseStat(StatType.Blocks, 1);
				blitzer.ApplyHarm(blocker.GetAttribute(AttributeType.BlockingHarmfulness), blitzer.GetAttribute(AttributeType.BlitzDurability)); //Harm is applied by the blocker to the blitzer
				//blitzer.IncreaseStat(StatType? - could add blitz contain fail stat
			}

			if (!simulated)
			{
				string blockerString = blockers[0].GetName();
				for (int i = 1; i < blockers.Count; i++)
					blockerString += " and " + blockers[i].GetName();
				CreateGameLog(blitzer.GetName() + " is blocked by " + blockerString + "!");
			}

			WhiffTackle(rusher, blitzer);
		}
		else
		{
			foreach (Athlete blocker in blockers)
			{
				blocker.IncreaseStat(StatType.MissedBlocks, 1);

				blocker.ApplyHarm(blitzer.GetAttribute(AttributeType.BlitzHarmfulness), blocker.GetAttribute(AttributeType.BlockingDurability)); //Harm is applied by the blitzer to the blocker
			}
			
			if (!simulated)
				CreateGameLog(blitzer.GetName() + " attempts the tackle.");

			QueueNextAction(() => AttemptTackle(rusher, blitzer));
		}
	}

	public void AttemptTackle(Athlete rusher, Athlete tackler)
	{
		TickTime(1);

		float elusiveRoll = Random.value * rusher.GetAttribute(AttributeType.RushingElusiveness);
		float pursuitRoll = Random.value * tackler.GetAttribute(AttributeType.TacklingPursuit) * 3f;

		float ballSecurityRoll = Random.value * rusher.GetAttribute(AttributeType.BallSecurity);
		float forceFumbleRoll = Random.value * tackler.GetAttribute(AttributeType.PeanutPunchability) / 15f;

		if (elusiveRoll > pursuitRoll)
		{
			rusher.IncreaseStat(StatType.Jukes, 1);
			tackler.IncreaseStat(StatType.MissedTackles, 1);

			CreateGameLog(tackler.GetName() + " missed the tackle!");

			WhiffTackle(rusher, tackler); //This QuenesNextAction()
		}
		else if(forceFumbleRoll > ballSecurityRoll)
		{
			rusher.IncreaseStat(StatType.Fumbles, 1);
			tackler.IncreaseStat(StatType.ForcedFumbles, 1);

			tackler.ApplyHarm(tackler.GetAttribute(AttributeType.TacklingHarmfulness), rusher.GetAttribute(AttributeType.RushingDurability)); //Harm is applied by the tackler to the rusher

			if (!simulated)
				CreateGameLog("FUMBLE! " + tackler.GetName() + " knocked the ball out of " + rusher.GetName() + "'s hands!");

			QueueNextAction(() => BallFumbled(rusher, tackler));
		}
		else
		{
			float rushRoll = Random.value * rusher.GetAttribute(AttributeType.RushingStrength);
			float tackleRoll = Random.value * tackler.GetAttribute(AttributeType.TacklingStrength) * 3f;

			if (tackleRoll > rushRoll) //Tackle is made - determine yards
			{
				float rushHustleRoll = Random.value * rusher.GetAttribute(AttributeType.RushingHustle);
				float tackleHustleRoll = Random.value * tackler.GetAttribute(AttributeType.TacklingHustle);
				float hustleDifferential = rushHustleRoll - tackleHustleRoll;

				yardsGained += (int)(hustleDifferential * 10f);

				if (yardsGained + lineOfScrimmage >= fieldLength) //Rusher gains enough yards for touchdown
				{
					yardsGained = fieldLength - lineOfScrimmage;

					rusher.IncreaseStat(StatType.BrokenTackles, 1); //Not really a broken tackle but they got a tuddy so we'll count it as one
					tackler.IncreaseStat(StatType.MissedTackles, 1);

					//Could apply harm from rusher to tackler, but they get the touchdown so that's prolly good enough

					if (!simulated)
					{ 
						CreateGameLog(rusher.GetName() + " overpowers " + tackler.GetName() + " and crosses into the endzone for a " + interfaceManager.GetStatString(yardsGained + " yard ") + " TOUCHDOWN!");

						interfaceManager?.UpdateMovementArrowToEndzone(yardsGained, offense == home);
					}

					QueueNextAction(() => TouchDown(rusher));
				}
				else //Tackle is made
				{
					//rusher.IncreaseStat(StatType.Tackled, 1);
					tackler.IncreaseStat(StatType.Tackles, 1);

					tackler.ApplyHarm(tackler.GetAttribute(AttributeType.TacklingHarmfulness), rusher.GetAttribute(AttributeType.RushingDurability)); //Harm is applied by the tackler to the rusher

					if (!simulated)
					{
						CreateGameLog(tackler.GetName() + " tackles " + rusher.GetName() + " for a gain of " + interfaceManager.GetStatString(yardsGained + " yards") + ".");

						interfaceManager?.UpdateMovementArrow(yardsGained);
					}

					QueueNextAction(() => EndPlay(rusher));
				}
			}
			else
			{
				rusher.IncreaseStat(StatType.BrokenTackles, 1);
				tackler.IncreaseStat(StatType.MissedTackles, 1);

				tackler.ApplyHarm(rusher.GetAttribute(AttributeType.RushingHarmfulness), tackler.GetAttribute(AttributeType.TacklingDurability)); //Harm is applied by the rusher to the tackler

				if (!simulated)
					CreateGameLog(rusher.GetName() + " broke the tackle attempt!");

				WhiffTackle(rusher, tackler);
			}
		}
	}

	public void WhiffTackle(Athlete rusher, Athlete tackler)
	{
		whiffers.Add(tackler);

		QueueNextAction(() => DetermineAdditionalTackleAttempts(rusher));
	}

	public void DetermineAdditionalTackleAttempts(Athlete rusher)
	{
		List<Athlete> availableTacklers = defense.GetAthletesByPosition(PositionGroup.DefensiveBack);
		availableTacklers.Shuffle();
		Athlete nextTackler = null;
		for (int i = 0; i < availableTacklers.Count; i++)
		{
			if (!whiffers.Contains(availableTacklers[i])) //If the next DB hasn't whiffed a tackle
			{
				nextTackler = availableTacklers[i];
				break;
			}
		}

		if (nextTackler == null)
		{
			yardsGained = fieldLength - lineOfScrimmage;

			if (!simulated)
			{
				CreateGameLog("Nobody can catch " + rusher.GetName() + "! " + interfaceManager.GetStatString(yardsGained + " yard") + " TOUCHDOWN for " + offense.GetTeamName() + "!");

				interfaceManager?.UpdateMovementArrowToEndzone(yardsGained, offense == home);
			}

			QueueNextAction(() => TouchDown(rusher));
		}
		else
		{
			DetermineExtraYardsBeforeNextTackleAttempt(rusher, nextTackler);
		}
	}

	public void DetermineExtraYardsBeforeNextTackleAttempt(Athlete rusher, Athlete nextTackler)
	{
		TickTime(1);

		float rushingSpeedRoll = Random.value * rusher.GetAttribute(AttributeType.RushingSpeed);
		float defendingSpeedRoll = Random.value * nextTackler.GetAttribute(AttributeType.RecoverySpeed);
		float speedDifferential = rushingSpeedRoll - defendingSpeedRoll;

		int extraYardsGained = (int)((1f + speedDifferential) * (10f * Random.value)); //>1 value multiplied by a random value between 0 and 10 (so between 0 and 20 yards gained before attempt)
		//That random value could be determined by rusher's "burst" or speed or something
		yardsGained += extraYardsGained;

		if (yardsGained + lineOfScrimmage >= fieldLength)
		{
			yardsGained = fieldLength - lineOfScrimmage;

			//TODO: This needs to award the yards here so that they can't be given to somebody else later (passes/laterals)

			if (!simulated)
			{
				CreateGameLog(rusher.GetName() + " runs into the endzone for a " + interfaceManager.GetStatString(yardsGained + " yard") + " TOUCHDOWN!");

				interfaceManager?.UpdateMovementArrowToEndzone(yardsGained, offense == home);
			}

			QueueNextAction(() => TouchDown(rusher));
		}
		else
		{
			if (!simulated)
			{
				CreateGameLog(rusher.GetName() + " picks up " + interfaceManager.GetStatString(extraYardsGained + " yards") + " before " + nextTackler.GetName() + " attempts the tackle.");

				interfaceManager?.UpdateMovementArrow(yardsGained);
			}

			QueueNextAction(() => AttemptTackle(rusher, nextTackler));
		}
	}

	public void PuntBall(Athlete punter)
	{
		//Athlete returner = defense.GetDefense()[Random.Range(0, defense.GetDefense().Count)];
		TickTime(1);

		Debug.Log("Punting");

		punter.IncreaseStat(StatType.Punts, 1);

		int puntingFromYardLine = lineOfScrimmage;
		float puntMax = punter.GetAttribute(AttributeType.PuntingStrength);
		int yardTarget = (int)(puntMax * 100f);
		int yardsPunted;

		int touchbackOffset = 5;
		
		float inaccuracyRoll = Random.value - punter.GetAttribute(AttributeType.PuntingAccuracy);

		if (yardTarget > fieldLength - puntingFromYardLine) //If the yardTarget is greater than the remaining field length
		{
			float precisionRoll = Random.value * (1 - punter.GetAttribute(AttributeType.PuntingPrecision)); //Used to determine how short of the endzone they'll target
			yardTarget = fieldLength - puntingFromYardLine - touchbackOffset - (int)(precisionRoll * 20f);

			yardsPunted = yardTarget + (int)(inaccuracyRoll * 30); //Max 30 yards over
		}
		else //Max punt
		{
			yardsPunted = yardTarget - (int)(inaccuracyRoll * 30); //Max 30 yards short
		}

		//Chance to block the punt


		Debug.Log("Targetted " + yardTarget + " but punted " + yardsPunted + " with inaccuracy of " + inaccuracyRoll);

		if(yardsPunted + lineOfScrimmage > fieldLength)
		{
			Debug.Log("Touchback for " + (fieldLength - lineOfScrimmage) + " yards");
			punter.IncreaseStat(StatType.PuntYards, fieldLength - lineOfScrimmage);

			lineOfScrimmage = fieldLength - ballStart;

			if (!simulated)
			{
				CreateGameLog(punter.GetName() + " punted the ball " + interfaceManager.GetStatString(yardsPunted + " yards") + " into the endzone for a touchback.");

				//TODO: Update punt arrow

				//interfaceManager?.UpdateMovementArrow(yardsPunted);
			}

			QueueNextAction(() => SwitchPossession());
		}
		else
		{
			punter.IncreaseStat(StatType.PuntYards, yardsPunted);

			lineOfScrimmage += yardsPunted; //Kind of a hack - really it's more like yardsGained
			if(fieldLength - lineOfScrimmage <= 10)
			{
				punter.IncreaseStat(StatType.PuntsInside10, 1);

				if (!simulated)
					CreateGameLog(punter.GetName() + " punted the ball precisely" + interfaceManager.GetStatString(yardsPunted + " yards") + ". It landed close to the endzone.");
			}
			else if (fieldLength - lineOfScrimmage <= 20)
			{
				punter.IncreaseStat(StatType.PuntsInside20, 1);

				if (!simulated)
					CreateGameLog(punter.GetName() + " punted the ball an effecient " + interfaceManager.GetStatString(yardsPunted + " yards") + ".");
			}
			else
			{
				punter.IncreaseStat(StatType.TouchBacks, 1);

				if (!simulated)
					CreateGameLog(punter.GetName() + " punted the ball " + interfaceManager.GetStatString(yardsPunted + " yards") + ".");
			}

			//TODO: Update punt arrow

			//interfaceManager?.UpdateMovementArrow(yardsPunted);

			QueueNextAction(() => SwitchPossession());
		}
	}

	public void AttemptFieldGoal(Athlete kicker)
	{
		TickTime(1);

		Debug.Log("Kicking field goal");

		kicker.IncreaseStat(StatType.FieldGoalsAttempted, 1);

		int attemptLength = fieldLength - lineOfScrimmage;
		int kickingMax = (int)(kicker.GetAttribute(AttributeType.KickingStrength) * 100f);

		float strengthRoll = Random.value * kicker.GetAttribute(AttributeType.KickingStrength) + Random.value;
		float accuracyRoll = Random.value * kicker.GetAttribute(AttributeType.KickingAccuracy);

		int yardsKicked = (int)(strengthRoll * kickingMax);
		float accuracyNeeded = (float)attemptLength / (float)kickingMax * Random.value;

		if(yardsKicked > attemptLength) //Kick is strong enough to succeed
		{
			if(accuracyRoll >= accuracyNeeded) //Kick is good
			{

				Debug.Log("MADE field goal from " + attemptLength);

				kicker.IncreaseStat(StatType.FieldGoalsMade, 1);
				kicker.IncreaseStat(StatType.FieldGoalKickYards, attemptLength);

				if (!simulated)
				{
					CreateGameLog("The kick is up and good! " + kicker.GetName() + " hits it from " + interfaceManager.GetStatString(attemptLength + " yards") + " out!");

					//interfaceManager?.UpdateMovementArrow(yardsPunted);
				}

				QueueNextAction(() => FieldGoal(kicker));
			}
			else //Kick misses
			{
				Debug.Log("INACC Missed field goal from " + attemptLength);

				kicker.IncreaseStat(StatType.FieldGoalsMissed, 1);

				if (!simulated)
				{
					CreateGameLog("The kick is no good! " + kicker.GetName() + " misses from " + interfaceManager.GetStatString(attemptLength + " yards") + " out!");

					//interfaceManager?.UpdateMovementArrow(yardsPunted);
				}

				QueueNextAction(() => SwitchPossession());
			}
		}
		else //Kick is short
		{
			Debug.Log("SHRT Missed field goal from " + attemptLength);

			kicker.IncreaseStat(StatType.FieldGoalsMissed, 1);

			if (!simulated)
			{
				CreateGameLog("The kick is short! " + kicker.GetName() + " misses from " + interfaceManager.GetStatString(attemptLength + " yards") + " out!");

				//interfaceManager?.UpdateMovementArrow(yardsPunted);
			}

			QueueNextAction(() => SwitchPossession());
		}
	}

	public void EndPlay(Athlete possessor)
	{
		possessor.IncreaseStat(StatType.TotalYards, yardsGained);
		//TODO: Need a way to determine if the possessor was a receiver or rusher (or fumble recoverer) - maybe track that on SnapSelection/Reception/Turnover points
		possessor.IncreaseStat(StatType.RushingYards, yardsGained);

		lineOfScrimmage += yardsGained;

		if(!possessionJustFlipped) //If offense still has the ball
		{
			if (lineOfScrimmage >= fieldLength) //Touch Down
			{
				Debug.Log("A FLUKE TOUCHDOWN HAS OCCURED");

				yardsGained = fieldLength - lineOfScrimmage;

				if (!simulated)
					CreateGameLog("This should never get called. The TOUCHDOWN call should occur when the touchdown occurs, please.");

				//Touch Down!
				QueueNextAction(() => TouchDown(possessor));
			}
			else if (lineOfScrimmage >= firstDownYardage) //First Down
			{
				possessor.IncreaseStat(StatType.FirstDowns, 1);

				FirstDown();
			}
			else
			{
				down++;

				if (down > downsAllowed)
				{
					if (!simulated)
						CreateGameLog("TURNOVER on Downs.");

					QueueNextAction(() => TurnOverOnDowns());
				}
				else
				{
					if (!simulated)
					{
						//Debug.Log(yardsGained + " yards gained.  Ball at the " + ballOnYard + GetOrdinalIndicator(ballOnYard) + " yard line. " + down + downOrdinal + " and " + yardsToGain);
						CreateGameLog(interfaceManager.GetStatString(yardsGained + " yards") + " gained.  Ball at the " + interfaceManager.GetStatString(lineOfScrimmage.ToString()) + ". "
							+ interfaceManager.GetStatString(interfaceManager.GetOrdinalIndicator(down)) + " and " + interfaceManager.GetStatString(GetYardsToGain().ToString()) + ".");

						interfaceManager?.UpdateLineOfScrimmage();
						interfaceManager?.HideMovementArrow();
					}

					QueueNextAction(() => NextPlay());
				}
			}
		}
		else //If there was a turnover on the play.
		{
			StartNewPossession(offense, defense);
		}
	}

	public void FirstDown()
	{
		down = 1;
		firstDownYardage = lineOfScrimmage + yardsForFirstDown;

		if(!simulated)
		{
			CreateGameLog("FIRST DOWN at the " + interfaceManager.GetStatString(lineOfScrimmage.ToString()) + "!");

			interfaceManager?.UpdateLineOfScrimmage();
			interfaceManager?.HideMovementArrow();

			if (firstDownYardage >= fieldLength)
				interfaceManager?.ToggleFirstDownLine(false);
			else
				interfaceManager?.UpdateFirstDown();
		}

		QueueNextAction(() => NextPlay());
	}

	public void TurnOverOnDowns()
	{
		SwitchPossession();
	}

	//Chance of injury during fumble recovery?
	public void BallFumbled(Athlete fumbler, Athlete forcer)
	{
		float ballSlipperiness = Random.value / 2;

		Athlete recoverer = null;
		float forcerRecoveryRoll = Random.value * forcer.GetAttribute(AttributeType.FumbleRecovery);
		float fumblerRecoveryRoll = Random.value * fumbler.GetAttribute(AttributeType.FumbleRecovery);

		if (forcerRecoveryRoll > ballSlipperiness)
		{
			recoverer = forcer;

			recoverer.IncreaseStat(StatType.FumbleRecoveries, 1);

			if (!simulated)
				CreateGameLog("TURNOVER! After forcing the fumble, " + recoverer.GetName() + " manages to recover the ball!");

			QueueNextAction(() => SwitchPossession());
		}
		else if (fumblerRecoveryRoll > ballSlipperiness)
		{ 
			recoverer = fumbler;

			recoverer.IncreaseStat(StatType.FumbleRecoveries, 1);

			if (!simulated)
				CreateGameLog(recoverer.GetName() + " manages to recover the ball. " + offense.GetTeamName() + " will retain possession.");

			QueueNextAction(() => EndPlay(recoverer));
		}
		else
		{
			DetermineFumbleRecoverer();
		}
	}

	public void DetermineFumbleRecoverer()
	{
		Athlete recoverer = null;
		float highestRecoveryRoll = -1f;
		bool offenseRetains = true;

		for (int i = 0; i < defense.GetDefense().Count; i++)
		{
			float roll = Random.value * defense.GetDefense()[i].GetAttribute(AttributeType.FumbleRecovery);

			if (roll > highestRecoveryRoll)
			{
				highestRecoveryRoll = roll;

				recoverer = defense.GetDefense()[i];
				offenseRetains = false;
			}
		}

		for (int i = 0; i < offense.GetOffense().Count; i++)
		{
			float roll = Random.value * offense.GetOffense()[i].GetAttribute(AttributeType.FumbleRecovery);

			if (roll > highestRecoveryRoll)
			{
				highestRecoveryRoll = roll;

				recoverer = offense.GetOffense()[i];
				offenseRetains = true;
			}
		}

		recoverer.IncreaseStat(StatType.FumbleRecoveries, 1);

		if (!simulated)
			CreateGameLog("It's a dogpile!! Somehow " + recoverer.GetName() + " comes up with the loose ball!");

		if (offenseRetains)
			QueueNextAction(() => EndPlay(recoverer));
		else
			QueueNextAction(() => SwitchPossession());
	}

	public void TouchDown(Athlete scorer)
	{
		scorer.IncreaseStat(StatType.TouchDowns, 1);

		if (offense == home)
		{
			matchup.homeScore += pointsFromTouchdown;

			if(!simulated)
				interfaceManager?.UpdateScore(true, matchup.homeScore);
		}
		else
		{
			matchup.awayScore += pointsFromTouchdown;

			if (!simulated)
				interfaceManager?.UpdateScore(false, matchup.awayScore);
		}

		if(leagueManager.GetLaw(LawType.PointAfterTouchdown).enacted)
		{
			Athlete kicker = offense.GetRandomAthleteByPosition(PositionGroup.Kicker);

			AttemptPointAfterTouchdown(kicker);
		}
		else
		{
			ScoringPlay();
		}
	}

	public void AttemptPointAfterTouchdown(Athlete kicker)
	{
		kicker.IncreaseStat(StatType.PATsAttempted, 1);

		lineOfScrimmage = fieldLength - 10;

		//TODO: Combine kicking functionality for PAT and FG - there be some duplicate code here

		int attemptLength = fieldLength - lineOfScrimmage;
		int kickingMax = (int)(kicker.GetAttribute(AttributeType.KickingStrength) * 100f);

		float strengthRoll = Random.value * kicker.GetAttribute(AttributeType.KickingStrength) + Random.value;
		float accuracyRoll = Random.value * kicker.GetAttribute(AttributeType.KickingAccuracy);

		int yardsKicked = (int)(strengthRoll * kickingMax);
		float accuracyNeeded = (float)attemptLength / (float)kickingMax * Random.value;

		if (yardsKicked > attemptLength) //Kick is strong enough to succeed
		{
			if (accuracyRoll >= accuracyNeeded) //Kick is good
			{
				kicker.IncreaseStat(StatType.PATsMade, 1);
				//kicker.IncreaseStat(StatType.FieldGoalKickYards, attemptLength);

				if (!simulated)
				{
					CreateGameLog(kicker.GetName() + " lines up for the point after touchdown... the kick is good!");

					//interfaceManager?.UpdateMovementArrow(yardsPunted);
				}

				if (offense == home)
				{
					matchup.homeScore += pointsFromExtraPoint;
				}
				else
				{
					matchup.awayScore += pointsFromExtraPoint;
				}

				QueueNextAction(() => ScoringPlay());
			}
			else //Kick misses
			{
				kicker.IncreaseStat(StatType.PATsMissed, 1);

				if (!simulated)
				{
					CreateGameLog(kicker.GetName() + " lines up for the point after touchdown... and they miss!");

					//interfaceManager?.UpdateMovementArrow(yardsPunted)
				}

				QueueNextAction(() => ScoringPlay());
			}
		}
		else //Kick is short
		{
			Debug.Log("SHRT Missed extra point from " + attemptLength);

			kicker.IncreaseStat(StatType.PATsMissed, 1);

			if (!simulated)
			{
				CreateGameLog("The kick is short! " + kicker.GetName() + " misses from " + interfaceManager.GetStatString(attemptLength + " yards") + " out!");

				//interfaceManager?.UpdateMovementArrow(yardsPunted);
			}

			QueueNextAction(() => ScoringPlay());
		}
	}

	public void FieldGoal(Athlete scorer)
	{
		if (offense == home)
		{
			matchup.homeScore += pointsFromFieldGoal;
		}
		else
		{
			matchup.awayScore += pointsFromFieldGoal;
		}

		ScoringPlay();
	}

	public void ScoringPlay()
	{
		overtime = false; //HACK - It cannot be overtime if a team scores. Overtime is declared at the start of the next play if time is less than zero and the teams are tied. This signals the breaking of a tie.

		if (!simulated)
		{
			string comparatorString;
			if (matchup.homeScore > matchup.awayScore)
				comparatorString = " leads ";
			else if (matchup.homeScore == matchup.awayScore)
				comparatorString = " are tied with ";
			else
				comparatorString = " trails ";

			CreateGameLog(home.GetTeamName() + comparatorString + away.GetTeamName() + " " + interfaceManager.GetStatString(matchup.homeScore.ToString()) + " to " + interfaceManager.GetStatString(matchup.awayScore.ToString()) + ".");
		}

		if (!simulated)
		{
			interfaceManager?.UpdateScore(true, matchup.homeScore);
			interfaceManager?.UpdateScore(false, matchup.awayScore);
		}

		lineOfScrimmage = fieldLength - ballStart; //Hack to flip ballStart before it gets flipped

		QueueNextAction(() => StartNewPossession(defense, offense));
	}

	public void SwitchPossession()
	{
		Team oldOffense = offense;
		Team oldDefense = defense;

		offense = oldDefense;
		defense = oldOffense;

		down = 1;
		lineOfScrimmage = fieldLength - lineOfScrimmage; //Flip the Field
		firstDownYardage = lineOfScrimmage + yardsForFirstDown;

		if (!simulated)
		{
			CreateGameLog(offense.GetTeamName() + " take over at the " + interfaceManager.GetStatString(lineOfScrimmage.ToString()) + ".");

			bool sidePossessing;
			if (home == offense)
				sidePossessing = true;
			else
				sidePossessing = false;

			if (!simulated)
			{
				interfaceManager?.FlipPossession(sidePossessing);
				interfaceManager?.UpdateLineOfScrimmage();
				interfaceManager?.HideMovementArrow();
				interfaceManager?.UpdateFirstDown();
			}
		}

		QueueNextAction(() => NextPlay());
	}

	private bool possessionJustFlipped = false;
	public void FlipPossessionLiveBall(Athlete possessor, Athlete closestDefender)
	{
		possessionJustFlipped = !possessionJustFlipped;

		Team oldOffense = offense;
		Team oldDefense = defense;

		offense = oldDefense;
		defense = oldOffense;

		if (!simulated)
		{
			CreateGameLog(possessor.GetName() + " is taking the ball the other way! " + closestDefender.GetName() + " has has chance to bring them down.");

			bool sidePossessing;
			if (home == offense)
				sidePossessing = true;
			else
				sidePossessing = false;

			interfaceManager?.FlipPossession(sidePossessing);
			interfaceManager.ShowPassingArrow(false); //Replace with hideMovementArrow?
		}

		//TODO: This will lead to issues eventually. This assumes that offensive backs are also defensive backs.
		//Example: Interception --> Former Offense is considering defense so they'll defend with defensive backs even if they aren't on the field

		QueueNextAction(() => AttemptTackle(possessor, closestDefender));
	}

	public void StartNewPossession(Team newOff, Team newDef)
	{
		offense = newOff;
		defense = newDef;

		down = 1;
		lineOfScrimmage = ballStart;
		firstDownYardage = lineOfScrimmage + yardsForFirstDown;

		if (!simulated)
		{
			CreateGameLog(offense.GetTeamName() + " start with the ball at their own " + interfaceManager.GetStatString(lineOfScrimmage.ToString()) + ".");

			bool sidePossessing;
			if (home == offense)
				sidePossessing = true;
			else
				sidePossessing = false;

			if (!simulated)
			{
				interfaceManager?.FlipPossession(sidePossessing);
				interfaceManager?.UpdateLineOfScrimmage();
				interfaceManager?.HideMovementArrow();
				interfaceManager?.UpdateFirstDown();
				interfaceManager?.ToggleFirstDownLine(true);
			}
		}

		QueueNextAction(() => NextPlay());
	}

	public void EndMatch()
	{
		if(!overtime && matchup.homeScore == matchup.awayScore) //If overtime conditions are met
		{
			overtime = true;

			if(!simulated)
			{
				CreateGameLog("No ties shall be allowed. A winner must be declared. The game resumes!");
			}

			matchup.AddPostGameLog("This game went to overtime!");

			QueueNextAction(() => NextPlay());
		}
		else
		{
			if (matchup.homeScore == matchup.awayScore)
			{
				Debug.Log("They tied - are we really allowing that?");

				home.GetCurrentSeasonStats().ties++;
				away.GetCurrentSeasonStats().ties++;
			}
			else if (matchup.homeScore > matchup.awayScore)
			{
				RecordWinnerAndLoser(home, away);
			}
			else
			{
				RecordWinnerAndLoser(away, home);
			}

			matchup.completed = true;

			if (!simulated)
			{
				CreateGameLog("End of match. Final score " + home.GetTeamName() + " " + interfaceManager.GetStatString(matchup.homeScore.ToString()) + ", " + away.GetTeamName() + " " + interfaceManager.GetStatString(matchup.awayScore.ToString()));

				interfaceManager.DisplayPostGame();
			}

			//Calculate best three statlines
			DetermineTopPerformers();

			if (simulated)
				ExitMatch();
		}
	}

	public void RecordWinnerAndLoser(Team winner, Team loser)
	{
		winner.GetCurrentSeasonStats().wins++;
		loser.GetCurrentSeasonStats().losses++;

		winner.ChangeGold(5);
		loser.ChangeGold(1);

		winner.ChangeFanApproval(Random.value / 20f); //Maximum 20% increase
		loser.ChangeFanApproval(-Random.value / 10f); //Maximum 10% decrease
	}

	public void ExitMatch()
	{
		simulated = true; //This should finish the game quickly

		leagueManager.FinishMatchup(matchup);
	}

	public void DetermineTopPerformers()
	{
		List<Athlete> unorderedAthletes = new List<Athlete>();
		List<Stat> unorderedStats = new List<Stat>();

		for (int a = 0; a < home.GetRoster().Count; a++)
		{
			Athlete athlete = home.GetRoster()[a];

			int bestDivineSpectacle = -1;
			Stat bestStat = null;
			for(int s = 0; s < athlete.GetStatList_CurrentGame().Count; s++)
			{
				Stat testStat = athlete.GetStatList_CurrentGame()[s];
				int testValue = testStat.GetDivineSpectacle();
				if(testValue > bestDivineSpectacle)
				{
					bestStat = testStat;
					bestDivineSpectacle = testValue;
				}
			}

			unorderedAthletes.Add(athlete);
			unorderedStats.Add(bestStat);
		}

		for (int a = 0; a < away.GetRoster().Count; a++)
		{
			Athlete athlete = away.GetRoster()[a];

			int bestDivineSpectacle = -1;
			Stat bestStat = null;
			for (int s = 0; s < athlete.GetStatList_CurrentGame().Count; s++)
			{
				Stat testStat = athlete.GetStatList_CurrentGame()[s];
				int testValue = testStat.GetDivineSpectacle();
				if (testValue > bestDivineSpectacle)
				{
					bestStat = testStat;
					bestDivineSpectacle = testValue;
				}
			}

			unorderedAthletes.Add(athlete);
			unorderedStats.Add(bestStat);
		}

		List<Athlete> topPerformers = new List<Athlete>();
		List<Stat> topStats = new List<Stat>();

		for(int c = 0; c < 4; c++) //Determines the number of top performers
		{
			int highestDivineSpectacle = -1;
			Athlete highestAthlete = null;
			Stat highestStat = null;
			for (int i = 0; i < unorderedAthletes.Count; i++)
			{
				Athlete testAthlete = unorderedAthletes[i];
				Stat testStat = unorderedStats[i]; //Assumes stat count always equals athlete count (which it should - no reason to have athletes with multiple "best stats")
				if (testStat.GetDivineSpectacle() > highestDivineSpectacle)
				{
					highestAthlete = testAthlete;
					highestDivineSpectacle = testStat.GetDivineSpectacle();
					highestStat = testStat;
				}
			}

			topPerformers.Add(highestAthlete);
			unorderedAthletes.Remove(highestAthlete);
			topStats.Add(highestStat);
			unorderedStats.Remove(highestStat);
		}

		float performanceImprovement = 0.05f + Random.value / 10; //Improves attribute by 0.05 to 0.15
		for (int i = 0; i < topPerformers.Count; i++)
		{
			topPerformers[i].IncreaseRandomAttribute(topStats[i].attributeGroupImproved, performanceImprovement);

			matchup.AddPostGameLog(topPerformers[i].GetName() + " had " + topStats[i].count + " " + topStats[i].statType + ".");
		}

		if(!simulated)
		{
			interfaceManager.DisplayTopPerformers(topPerformers, topStats);
		}
	}

	//public Athlete DetermineTopPerformerForStat(StatType st)
	//{
	//	Athlete topPerformer = null;
	//	int highestCount = -1;
	//	for(int i = 0; i < home.GetRoster().Count; i++)
	//	{
	//		Athlete athlete = home.GetRoster()[i];
	//		if(athlete.GetStat(st).count > highestCount)
	//		{
	//			topPerformer = athlete;
	//			highestCount = athlete.GetStat(st).count;
	//		}
	//	}
	//	for (int i = 0; i < away.GetRoster().Count; i++)
	//	{
	//		Athlete athlete = away.GetRoster()[i];
	//		if (athlete.GetStat(st).count > highestCount)
	//		{
	//			topPerformer = athlete;
	//			highestCount = athlete.GetStat(st).count;
	//		}
	//	}

	//	return topPerformer;
	//}

	public void InjuryOccured(Athlete athlete)
	{
		//Debug.Log(athlete.GetName(false) + " was injured");
		CreateGameLog(athlete.GetName() + " was injured on the play! They'll be out for " + athlete.weeksInjured + " weeks.");
		matchup.AddPostGameLog("Injured: " + athlete.GetName());
	}
}
