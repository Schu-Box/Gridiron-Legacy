using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RosterPositionContainer : MonoBehaviour
{
	public PositionGroup positionGroup;
	public AttributeGroup displayedAttributeGroup;

    public TextMeshProUGUI labelText;
    public Transform statNameHolder;
	public TextMeshProUGUI statLabelText;
    public Transform rosterContainer;

	public List<Image> athleteShowcaseImages;

	private Button button;

    public GameObject athleteRosterContainerPrefab;

	public Button addAthleteButton;

    private RosterInterfaceManager rosterInterfaceManager;

	private Team team;

	private void Awake()
	{
        rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();
	}

	public void SetRosterContainer(Team newTeam)
    {
		team = newTeam;

		labelText.text = positionGroup.ToString();
		statLabelText.text = displayedAttributeGroup.ToString();

		List<Athlete> athletes = team.GetAthletesByPosition(positionGroup);

		for (int i = 0; i < rosterContainer.childCount - 1; i++) //-1 because of + button
		{
			AthleteRosterContainer container = rosterContainer.GetChild(i).GetComponent<AthleteRosterContainer>();

			int index = i;
			if (athletes.Count > index)
			{
				Athlete athlete = athletes[i];

				container.SetAthlete(athlete, displayedAttributeGroup);

				if (index % 2 == 0)
					container.SetColor(rosterInterfaceManager.grayColor1);
				else
					container.SetColor(rosterInterfaceManager.grayColor2);

				//container.GetButton().onClick.AddListener(() => rosterInterfaceManager.OpenAthletePanel(athlete));
			}
			else
			{
				container.SetAthlete(null, AttributeGroup.Rushing);

				//container.SetColor(FindObjectOfType<RosterInterfaceManager>().grayColor1);
			}
		}

		for(int i = 0; i < athleteShowcaseImages.Count; i++)
		{
			if (athletes.Count > i)
			{
				athleteShowcaseImages[i].gameObject.SetActive(true);
				athleteShowcaseImages[i].sprite = athletes[i].sprite;
			}
			else
			{
				athleteShowcaseImages[i].gameObject.SetActive(false);
				athleteShowcaseImages[i].sprite = null;
			}
		}

		if (team.GetAthletesByPosition(null).Count > 0) //If there are athletes on the bench, allow them to be added to this position group
			addAthleteButton.interactable = true;
		else
			addAthleteButton.interactable = false;
	}

	public void OpenAddAthleteMenu()
	{
		rosterInterfaceManager.OpenBenchPanel(positionGroup);
	}

	//Should be able to remove this
	public void AddAthleteToPositionGroup(Athlete chosenAthlete)
	{
		chosenAthlete.ChangePosition(positionGroup);

		//SetRosterContainer(team);
		rosterInterfaceManager.OpenTeamPanel(team);
	}
}
