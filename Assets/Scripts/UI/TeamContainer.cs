using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamContainer : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI recordText;
    public Button button;

    private Team team;

    private LeagueInterfaceManager leagueInterfaceManager;
    private RosterInterfaceManager rosterInterfaceManager;

	private void Start()
	{
        leagueInterfaceManager = FindObjectOfType<LeagueInterfaceManager>();
        rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();
	}

	public void SetTeamContainer(Team assignedTeam)
	{
        team = assignedTeam;

        nameText.text = team.GetTeamName();

        recordText.text = team.GetRecordString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => rosterInterfaceManager.OpenTeamPanel(team));
    }
}
