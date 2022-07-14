using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HistoryContainer : MonoBehaviour
{
	public TextMeshProUGUI trophyIconText;
	public TextMeshProUGUI championTeamNameText;
	public AthleteRosterContainer mvpRosterContainer;
	public TextMeshProUGUI mvpTeamNameText;
	public TextMeshProUGUI lawChangedText;

	private SeasonHistory history;

	public void SetHistory(SeasonHistory assignedHistory)
	{
		history = assignedHistory;

		trophyIconText.text = (history.seasonInt + 1).ToString();
		championTeamNameText.text = history.championTeam.GetTeamName();
		mvpRosterContainer.SetAthlete(history.mostValuablePlayer, null);
		mvpTeamNameText.text = "plays for " + history.mostValuablePlayer.GetTeam().GetTeamName();

		string lawChangeString = history.lawChanged.descriptionString + " was ";
		if (history.lawChanged.value == 0)
		{
			if (history.lawChanged.enacted)
				lawChangeString += "<b>enacted.</b>";
			else
				lawChangeString += "<b>revoked.</b>";
		}
		else
		{
			lawChangeString += " changed to <b>" + history.lawChanged.value + ".</b>";
		}
		lawChangedText.text = lawChangeString;
	}
}
