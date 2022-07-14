using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchup
{
    public Team homeTeam;
    public Team awayTeam;

    public int homeScore = 0;
    public int awayScore = 0;

    public bool completed = false;

    private List<string> postGameLogs;

    public Matchup(Team home, Team away)
	{
        homeTeam = home;
        awayTeam = away;

        postGameLogs = new List<string>();
	}

    public void AddPostGameLog(string newLog)
	{
        postGameLogs.Add(newLog);
	}

    public List<string> GetPostGameLogs()
	{
        return postGameLogs;
	}
}
