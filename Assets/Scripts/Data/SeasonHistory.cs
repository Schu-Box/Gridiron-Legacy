using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonHistory
{
	public int seasonInt;
	public Team championTeam;
	public Athlete mostValuablePlayer;
	//public List<Athlete> allStars;
	public Law lawChanged;

	public SeasonHistory(int season, Team champion, Athlete mvp, Law law)
	{
		seasonInt = season;
		championTeam = champion;
		mostValuablePlayer = mvp;
		lawChanged = law;
	}
}
