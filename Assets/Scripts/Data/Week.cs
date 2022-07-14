using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Week
{
    public List<Matchup> matchups;

    public Week(List<Matchup> matchupsForWeek)
	{
		matchups = matchupsForWeek;
	}

	public bool GetAllGamesComplete()
	{
		for(int i = 0; i < matchups.Count; i++)
		{
			if (!matchups[i].completed)
				return false;
		}

		return true;
	}
}
