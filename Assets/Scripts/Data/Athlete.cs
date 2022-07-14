using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class Athlete
{
    public string firstName;
    public string lastName;

    private string name;

    public Sprite sprite;

    private Team team;

    private List<Attribute> attributeList = new List<Attribute>();

    public PositionGroup? positionGroup;

    private List<List<List<Stat>>> careerStatList;
    private List<List<Stat>> seasonStatList;
    private List<Stat> gameStatList;
    private List<Stat> totalSeasonStatList;
    private List<Stat> totalCareerStatList;

    public List<int> championshipSeasons = new List<int>();
    public List<int> allStarSeasons = new List<int>();
    public List<int> mvpSeasons = new List<int>();

    public float maxHealth = 10f;
    public float health = 10f;
    public float harmThresholdForInjury = 5f;
    public int weeksInjured = 0;

    public bool restrictedToBench = false;

    public LeagueManager leagueManager;

    public Athlete(Team t, string first, string last, Sprite athleteSprite)
	{
        team = t;

        firstName = first;
        lastName = last;
        sprite = athleteSprite;

        name = firstName + " " + lastName;

        attributeList.Add(new Attribute(AttributeType.PassTendency, AttributeGroup.Passing, Random.value)); //Uncapped, completely random pass/rush split

        attributeList.Add(new Attribute(AttributeType.PassingStrength, AttributeGroup.Passing, (0.2f + (Random.value * 0.8f)))); //Guaranteed can throw 20 yards
        attributeList.Add(new Attribute(AttributeType.PassingAccuracy, AttributeGroup.Passing));
        attributeList.Add(new Attribute(AttributeType.PassingElusiveness, AttributeGroup.Passing));
        attributeList.Add(new Attribute(AttributeType.PassingHustle, AttributeGroup.Passing));
        attributeList.Add(new Attribute(AttributeType.PassingDurability, AttributeGroup.Passing));


        attributeList.Add(new Attribute(AttributeType.RushingElusiveness, AttributeGroup.Rushing));
        attributeList.Add(new Attribute(AttributeType.RushingStrength, AttributeGroup.Rushing));
        attributeList.Add(new Attribute(AttributeType.RushingSpeed, AttributeGroup.Rushing));
        attributeList.Add(new Attribute(AttributeType.RushingHustle, AttributeGroup.Rushing));
        attributeList.Add(new Attribute(AttributeType.BallSecurity, AttributeGroup.Rushing));
        attributeList.Add(new Attribute(AttributeType.RushingDurability, AttributeGroup.Rushing));
        attributeList.Add(new Attribute(AttributeType.RushingHarmfulness, AttributeGroup.Rushing));


        attributeList.Add(new Attribute(AttributeType.RouteRunning, AttributeGroup.Receiving));
        attributeList.Add(new Attribute(AttributeType.ReceivingStrength, AttributeGroup.Receiving));
        attributeList.Add(new Attribute(AttributeType.ReceivingSpeed, AttributeGroup.Receiving));
        attributeList.Add(new Attribute(AttributeType.ReceivingHands, AttributeGroup.Receiving));
        attributeList.Add(new Attribute(AttributeType.ReceivingBoxoutability, AttributeGroup.Receiving));
        attributeList.Add(new Attribute(AttributeType.ReceivingDurability, AttributeGroup.Receiving));

        attributeList.Add(new Attribute(AttributeType.FumbleRecovery, AttributeGroup.Receiving)); //This is considered receiving cuz WRs are good at recovering fumbles maybe I dunno they got hands

        attributeList.Add(new Attribute(AttributeType.PassBlocking, AttributeGroup.Blocking));
        attributeList.Add(new Attribute(AttributeType.RushBlocking, AttributeGroup.Blocking));
        attributeList.Add(new Attribute(AttributeType.RushAssistance, AttributeGroup.Blocking));
        attributeList.Add(new Attribute(AttributeType.BlockingDurability, AttributeGroup.Blocking));
        attributeList.Add(new Attribute(AttributeType.BlockingHarmfulness, AttributeGroup.Blocking));

        attributeList.Add(new Attribute(AttributeType.TacklingPursuit, AttributeGroup.Tackling));
        attributeList.Add(new Attribute(AttributeType.TacklingStrength, AttributeGroup.Tackling));
        attributeList.Add(new Attribute(AttributeType.TacklingHustle, AttributeGroup.Tackling));
        attributeList.Add(new Attribute(AttributeType.PeanutPunchability, AttributeGroup.Tackling));
		attributeList.Add(new Attribute(AttributeType.TacklingDurability, AttributeGroup.Tackling));
		attributeList.Add(new Attribute(AttributeType.TacklingHarmfulness, AttributeGroup.Tackling));

		attributeList.Add(new Attribute(AttributeType.BlitzPenetration, AttributeGroup.Blitzing));
        attributeList.Add(new Attribute(AttributeType.BlitzContain, AttributeGroup.Blitzing));
        attributeList.Add(new Attribute(AttributeType.BlitzPursuit, AttributeGroup.Blitzing));
        attributeList.Add(new Attribute(AttributeType.BlitzHustle, AttributeGroup.Blitzing));
		attributeList.Add(new Attribute(AttributeType.BlitzDurability, AttributeGroup.Blitzing));
		attributeList.Add(new Attribute(AttributeType.BlitzHarmfulness, AttributeGroup.Blitzing));

		attributeList.Add(new Attribute(AttributeType.CoverageJamming, AttributeGroup.Coverage));
        attributeList.Add(new Attribute(AttributeType.CoverageBlanketness, AttributeGroup.Coverage));
        attributeList.Add(new Attribute(AttributeType.CoverageBreakupability, AttributeGroup.Coverage));
        attributeList.Add(new Attribute(AttributeType.CoverageInterceptablity, AttributeGroup.Coverage));
        attributeList.Add(new Attribute(AttributeType.RecoverySpeed, AttributeGroup.Coverage));
        attributeList.Add(new Attribute(AttributeType.CoverageDurability, AttributeGroup.Coverage));
        attributeList.Add(new Attribute(AttributeType.CoverageHarmfulness, AttributeGroup.Coverage));

        attributeList.Add(new Attribute(AttributeType.PuntingStrength, AttributeGroup.Kicking, (0.1f + (Random.value * 0.5f)))); //Can punt between 10 and 60 yards
        attributeList.Add(new Attribute(AttributeType.PuntingAccuracy, AttributeGroup.Kicking));
        attributeList.Add(new Attribute(AttributeType.PuntingPrecision, AttributeGroup.Kicking));

        attributeList.Add(new Attribute(AttributeType.KickingStrength, AttributeGroup.Kicking, (0.1f + (Random.value * 0.5f)))); //Can kick between 10 and 60 yards
        attributeList.Add(new Attribute(AttributeType.KickingAccuracy, AttributeGroup.Kicking));

        attributeList.Add(new Attribute(AttributeType.KickingDurability, AttributeGroup.Kicking));

        attributeList.Add(new Attribute(AttributeType.RecoveryRate, AttributeGroup.Blocking)); //Counting this as Blocking cuz there's not a catch all category sorry future Dan

        //Stats
        careerStatList = new List<List<List<Stat>>>(); //New career season stat list
        totalCareerStatList = GenerateNewStatList();
        //CreateNewSeasonData();
    }

    public void SetLeagueManager(LeagueManager lm)
	{
        leagueManager = lm;
	}

    public void CreateNewSeasonData()
	{
        Debug.Log("Creating new Season Data");

        if(seasonStatList != null)
		{
            careerStatList.Add(seasonStatList);
		}

        seasonStatList = new List<List<Stat>>();
        totalSeasonStatList = GenerateNewStatList();
    }

    public void CreateNewGameData()
	{
		Debug.Log("Creating new game data");

		if (seasonStatList == null)
		{
            Debug.Log("This is the error here wtf");
		}

        List<Stat> newStatList = GenerateNewStatList();

        gameStatList = newStatList;

        seasonStatList.Add(gameStatList);
    }

    public List<Stat> GenerateNewStatList()
	{
        List<Stat> statList = new List<Stat>();

        statList.Add(new Stat(StatType.Snaps, 0, AttributeGroup.Rushing)); //Hidden Stat
        statList.Add(new Stat(StatType.TotalYards, 0, AttributeGroup.Rushing)); //Hidden Stat

		statList.Add(new Stat(StatType.FirstDowns, 25, AttributeGroup.Rushing)); //
        statList.Add(new Stat(StatType.TouchDowns, 100, AttributeGroup.Rushing)); //

        statList.Add(new Stat(StatType.RushingYards, 1, AttributeGroup.Rushing));
        statList.Add(new Stat(StatType.Rushes, 1, AttributeGroup.Rushing));
        statList.Add(new Stat(StatType.Jukes, 20, AttributeGroup.Rushing));
        statList.Add(new Stat(StatType.BrokenTackles, 20, AttributeGroup.Rushing));

        statList.Add(new Stat(StatType.ReceivingYards, 2, AttributeGroup.Receiving));
        statList.Add(new Stat(StatType.Catches, 20, AttributeGroup.Receiving));
        statList.Add(new Stat(StatType.MissedCatches, -20, AttributeGroup.Receiving));
        statList.Add(new Stat(StatType.FumbleRecoveries, 50, AttributeGroup.Receiving)); //

        statList.Add(new Stat(StatType.PassingYards, 2, AttributeGroup.Passing));
        statList.Add(new Stat(StatType.DropBacks, 5, AttributeGroup.Passing));
        statList.Add(new Stat(StatType.PassAttempts, 5, AttributeGroup.Passing));
        statList.Add(new Stat(StatType.PassCompletions, 15, AttributeGroup.Passing));
        statList.Add(new Stat(StatType.PassIncompletions, -10, AttributeGroup.Passing));

        statList.Add(new Stat(StatType.Fumbles, -50, AttributeGroup.Rushing));
        statList.Add(new Stat(StatType.InterceptionsThrown, -40, AttributeGroup.Passing));

        statList.Add(new Stat(StatType.Blocks, 10, AttributeGroup.Blocking));
        statList.Add(new Stat(StatType.MissedBlocks, -5, AttributeGroup.Blocking));

        statList.Add(new Stat(StatType.Tackles, 20, AttributeGroup.Tackling));
        statList.Add(new Stat(StatType.MissedTackles, -10, AttributeGroup.Tackling));
        statList.Add(new Stat(StatType.ForcedFumbles, 50, AttributeGroup.Tackling));

        statList.Add(new Stat(StatType.Sacks, 35, AttributeGroup.Blitzing));
        statList.Add(new Stat(StatType.MissedSacks, 5, AttributeGroup.Blitzing));

        statList.Add(new Stat(StatType.ForcedInterceptions, 60, AttributeGroup.Coverage));
        statList.Add(new Stat(StatType.ForcedIncompletions, 30, AttributeGroup.Coverage));

        statList.Add(new Stat(StatType.Punts, 30, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.PuntYards, 5, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.PuntsInside10, 80, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.PuntsInside20, 60, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.TouchBacks, 20, AttributeGroup.Kicking));

        statList.Add(new Stat(StatType.FieldGoalsAttempted, 30, AttributeGroup.Kicking)); ;
        statList.Add(new Stat(StatType.FieldGoalsMade, 60, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.FieldGoalsMissed, -40, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.FieldGoalKickYards, 5, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.PATsAttempted, 20, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.PATsMade, 40, AttributeGroup.Kicking));
        statList.Add(new Stat(StatType.PATsMissed, -60, AttributeGroup.Kicking));

        return statList;
    }

    public void ChangePosition(PositionGroup? newPositionGroup)
	{
        positionGroup = newPositionGroup;
	}

    public string GetName(bool withColor = true)
    {
        if (withColor)
            return "<#" + ColorUtility.ToHtmlStringRGBA(team.colorPrimary) + ">" + name + "</color>";
        else
            return name;
    }

    public Team GetTeam()
	{
        return team;
	}

    public float GetAttribute(AttributeType type)
	{
        for(int i = 0; i < attributeList.Count; i++)
		{
            if(attributeList[i].type == type)
			{
                return attributeList[i].value;
			}
		}

        Debug.Log("This attribute does not exist: " + type);
        return 0f;
	}

    public float GetAttributeAggregate(AttributeGroup? attributeGroup)
	{
        int numStats = 0;
        float aggregate = 0f;
        for(int i = 0; i < attributeList.Count; i++)
		{
            if(attributeList[i].attributeGroup == attributeGroup)
			{
                numStats++;
                aggregate += attributeList[i].value;
			}
		}

        return aggregate / numStats;
	}

    //Could combine the two functions into one, but I like the clarity provided by two (rather than having == null get overall)
    public float GetOverallAggregate()
	{
        int numStats = 0;
        float aggregate = 0f;
        for(int i = 0; i < attributeList.Count; i++)
		{
            numStats++;
            aggregate += attributeList[i].value;
		}

        return aggregate / numStats;
	}

    public void IncreaseRandomAttribute(AttributeGroup group, float improvement)
	{
        List<Attribute> availableAttributes = new List<Attribute>();
        for(int i = 0; i < attributeList.Count; i++)
		{
            if(attributeList[i].attributeGroup == group)
			{
                availableAttributes.Add(attributeList[i]);
			}
		}

        Attribute chosenAttribute = availableAttributes[Random.Range(0, availableAttributes.Count)];

        chosenAttribute.value += improvement;
	}

    public void IncreaseStat(StatType st, int increase)
	{
        GetStat_CurrentGame(st).count += increase;
        GetStat_CurrentSeason(st).count += increase;
        GetStat_Career(st).count += increase;
	}

    public List<Stat> GetStatList_CurrentGame()
	{
        return gameStatList;
	}

    public List<Stat> GetStatList_SeasonTotal()
	{
        return totalSeasonStatList;
	}

    public Stat GetStat_CurrentGame(StatType st)
	{
        for(int i = 0; i < gameStatList.Count; i++)
		{
            if(gameStatList[i].statType == st)
			{
                return gameStatList[i];
			}
		}

        Debug.Log("Stat Type does not exist.");
        return null;
	}

    public Stat GetStat_CurrentSeason(StatType st)
    {
        for (int i = 0; i < totalSeasonStatList.Count; i++)
        {
            if (totalSeasonStatList[i].statType == st)
            {
                return totalSeasonStatList[i];
            }
        }

        Debug.Log("Stat Type does not exist.");
        return null;
    }

    public Stat GetStat_Career(StatType st)
    {
        for (int i = 0; i < totalCareerStatList.Count; i++)
        {
            if (totalCareerStatList[i].statType == st)
            {
                return totalCareerStatList[i];
            }
        }

        Debug.Log("Stat Type does not exist.");
        return null;
    }

    public int GetDivineSpectacleSum_CurrentSeason()
	{
        int sum = 0;
        for(int i = 0; i < totalSeasonStatList.Count; i++)
		{
            sum += totalSeasonStatList[i].GetDivineSpectacle();
		}

        return sum;
	}

    public void ApplyHarm(float harmValue, float durabilityValue)
	{
        float harmRoll = Random.value * harmValue;
        float durabilityRoll = Random.value * durabilityValue;
        float harmApplied = harmRoll - durabilityRoll; ;

        if (harmApplied > 0f)
		{
            health -= harmApplied;

            if (health <= harmThresholdForInjury)
            {
                //Display hurtedness

                float injuryProbability = health / maxHealth; //normalize
                float rando = Random.value;

                if (rando > injuryProbability)
				{
                    ApplyInjury();
                }
            }
        }
	}

    public void ApplyInjury()
	{
        int numWeeksInjured = (int)harmThresholdForInjury - (int)health + 1;
        weeksInjured = numWeeksInjured;

        //Debug.Log(GetName(false) + ", " + positionGroup + ", was INJURED at " + health + " health for " + numWeeksInjured + " weeks!");

        if (GetTeam().playerControlled)
            Debug.Log("PLAYER ATHLETE INJURED!");

        Debug.Log(GetName(false) + " was injured");

        leagueManager.InjuryOccured(this); //Display the injury

        ChangePosition(null); //Bench the player since they can no longer play
        restrictedToBench = true;
    }

    public void RecoverHealthForWeek()
	{
        float healthToRecover = GetAttribute(AttributeType.RecoveryRate) + Random.value / 10f;

        health += healthToRecover;

        //check if no longer hurt

        if (weeksInjured > 0)
        {
            weeksInjured--;

            if(weeksInjured <= 0)
			{
                RecoverFromInjury();
			}
        }
	}

    public void RecoverFromInjury()
	{
        //Debug.Log(GetName(false) + ", " + positionGroup + ", RECOVERED from injury, now at " + health + " health");

        restrictedToBench = false;
    }
}
