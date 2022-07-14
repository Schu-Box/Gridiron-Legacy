using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AthleteRosterContainer : MonoBehaviour
{
	public TextMeshProUGUI nameText;
	public List<StarContainer> starContainers;
	public Image athleteImage;
	public Image image;
	public Button button;

	public GameObject modifier_Hurt;
	public GameObject modifier_Injured;

	public Athlete athlete;

	private RosterInterfaceManager rosterInterfaceManager;

	public void SetAthlete(Athlete a, AttributeGroup? attributeGroupDisplayed)
	{
		athlete = a;

		rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();

		if (athlete != null)
		{
			gameObject.SetActive(true);

			nameText.text = athlete.firstName + " " + athlete.lastName;

			if (attributeGroupDisplayed != null)
				starContainers[0].SetStarValue(athlete.GetAttributeAggregate(attributeGroupDisplayed));
			else
				starContainers[0].SetStarValue(athlete.GetOverallAggregate());

			athleteImage.gameObject.SetActive(true);
			athleteImage.sprite = athlete.sprite;

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => rosterInterfaceManager.OpenAthletePanel(athlete));

			if (athlete.weeksInjured > 0)
			{
				modifier_Injured.SetActive(true);
				modifier_Hurt.SetActive(false);
			}
			else
			{
				modifier_Injured.SetActive(false);

				if (athlete.health <= athlete.harmThresholdForInjury)
					modifier_Hurt.SetActive(true);
				else
					modifier_Hurt.SetActive(false);
			}
		}
		else
		{
			gameObject.SetActive(false);

			nameText.text = "";

			starContainers[0].SetStarValue(0);

			athleteImage.gameObject.SetActive(false);

			modifier_Injured.SetActive(false);
			modifier_Hurt.SetActive(false);
		}
	}

	

	public void SetColor(Color color)
	{
		image.color = color;
	}

	public Button GetButton()
	{
		return button;
	}
}
