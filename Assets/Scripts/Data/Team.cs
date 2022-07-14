using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
	public bool playerControlled = false;

	public bool the;
	public string cityName;
	public string mascotName;

	public int gold = 0;
	public float fanApproval = 1.0f; //Between 1 and 0

	public Color colorPrimary;

	public List<TeamSeasonStat> teamSeasonStats = new List<TeamSeasonStat>();

	public List<int> championshipSeasons = new List<int>();

	private List<Athlete> roster = new List<Athlete>();

	public Team(string city, string mascot)
	{
		cityName = city;
		mascotName = mascot;

		Color randomColor = new Color(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f));

		colorPrimary = randomColor;
	}

	public string GetTeamName(bool withColor = true)
	{
		if (withColor)
			return "<#" + ColorUtility.ToHtmlStringRGBA(colorPrimary) + ">" + cityName + " " + mascotName + "</color>";
		else
			return cityName + " " + mascotName;
	}

	public void SetTeamCity(string city)
	{
		cityName = city;
	}

	public void SetTeamMascot(string mascot)
	{
		mascotName = mascot;
	}

	public void ChangeFanApproval(float change)
	{
		fanApproval += change;

		fanApproval = Mathf.Clamp(fanApproval, 0f, 1f);
	}

	public void ChangeGold(int change)
	{
		gold += change;
	}

	public List<Athlete> GetRoster()
	{
		return roster;
	}

	public void SetRoster(List<Athlete> newRoster) //Overwrites roster
	{
		roster = newRoster;
	}

	public void AddToRoster(Athlete newbie)
	{
		roster.Add(newbie);

		newbie.CreateNewSeasonData(); //Assumes the new athlete didn't have a previous SeasonData (which could be the case during trades)
	}

	public List<Athlete> GetOffense()
	{
		List<Athlete> offenseList = new List<Athlete>();
		for(int i = 0; i < roster.Count; i++)
		{
			switch(roster[i].positionGroup)
			{
				case PositionGroup.OffensiveLineman:
				case PositionGroup.RunningBack:
				case PositionGroup.QuarterBack:
				case PositionGroup.ReceivingBack:
					offenseList.Add(roster[i]);
					break;
			}
		}

		return offenseList;
	}

	public List<Athlete> GetDefense()
	{
		List<Athlete> defenseList = new List<Athlete>();
		for (int i = 0; i < roster.Count; i++)
		{
			switch (roster[i].positionGroup)
			{
				case PositionGroup.DefensiveLineman:
				case PositionGroup.DefensiveBack:
					defenseList.Add(roster[i]);
					break;
			}
		}
		return defenseList;
	}

	public List<Athlete> GetAthletesByPosition(PositionGroup? pg)
	{
		List<Athlete> athletes = new List<Athlete>();

		for(int i = 0; i < roster.Count; i++)
		{
			if(roster[i].positionGroup == pg)
			{
				athletes.Add(roster[i]);
			}
		}

		return athletes;
	}

	public Athlete GetRandomAthleteByPosition(PositionGroup? pg)
	{
		List<Athlete> athletes = GetAthletesByPosition(pg);

		return athletes[Random.Range(0, athletes.Count)];
	}

	public void StartNewSeason()
	{
		teamSeasonStats.Add(new TeamSeasonStat());

		for(int i = 0; i < roster.Count; i++)
		{
			roster[i].CreateNewSeasonData();
		}
	}

	public TeamSeasonStat GetCurrentSeasonStats()
	{
		return teamSeasonStats[teamSeasonStats.Count - 1];
	}

	public string GetRecordString()
	{
		string recordString = GetCurrentSeasonStats().wins + " - " + GetCurrentSeasonStats().losses;
		if (GetCurrentSeasonStats().ties > 0)
			recordString += " - " + GetCurrentSeasonStats().ties;
		return recordString;
	}
}

public class TeamSeasonStat {
	public int wins = 0;
	public int losses = 0;
	public int ties = 0;
}