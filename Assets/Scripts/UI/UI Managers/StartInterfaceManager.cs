using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartInterfaceManager : MonoBehaviour
{
	public GameObject startOptionsPanel;
	public GameObject createTeamPanel;

	public TMP_InputField cityNameInputField;
	public TMP_InputField mascotNameInputField;
	public Button startCampaignButton;

	private LeagueManager leagueManager;

	private string teamCity = "";
	private string teamMascot = "";

	private void Start()
	{
		leagueManager = FindObjectOfType<LeagueManager>();

		startCampaignButton.interactable = false;
	}

	public void SelectQuickPlay()
	{
		StartCampaign();
	}

	public void SelectCreateTeam()
	{
		startOptionsPanel.SetActive(false);
		createTeamPanel.SetActive(true);
	}

	public void EditTeamCity()
	{
		teamCity = cityNameInputField.text;

		CheckForFinishedTeamCustomization();
	}

	public void EditTeamMascot()
	{
		teamMascot = mascotNameInputField.text;

		CheckForFinishedTeamCustomization();
	}

	private void CheckForFinishedTeamCustomization()
	{
		if (cityNameInputField.text != "" && mascotNameInputField.text != "")
			startCampaignButton.interactable = true;
	}

	public void FinalizeTeamAndStart()
	{
		Debug.Log(teamCity + " and " + teamMascot);

		Team newTeam = new Team(teamCity, teamMascot);

		leagueManager.GenerateRoster(newTeam);
		leagueManager.teamList.Add(newTeam);

		leagueManager.SetUserTeam(newTeam);

		StartCampaign();
	}

	public void StartCampaign()
	{
		Debug.Log("Starting New Campaign");

		leagueManager.SetupLeague();

		gameObject.SetActive(false);
	}
}
