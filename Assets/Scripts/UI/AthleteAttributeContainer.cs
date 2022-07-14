using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AthleteAttributeContainer : MonoBehaviour
{
    private Athlete athlete;
    private AttributeGroup attributeGroup;

    public TextMeshProUGUI labelText;
    public StarContainer starContainer;
    public GameObject mysteryCoverup;

    private LeagueManager leagueManager;

    public void SetAthleteAttributeContainer(Athlete a, AttributeGroup ag)
	{
        leagueManager = FindObjectOfType<LeagueManager>();

        athlete = a;
        attributeGroup = ag;

        if (leagueManager.CheckAttributeIneligibility(attributeGroup))
        {
            labelText.text = "";
            starContainer.SetStarValue(0f);
        }
        else
		{
            labelText.text = attributeGroup.ToString();
            starContainer.SetStarValue(athlete.GetAttributeAggregate(attributeGroup));
        }
    }

    //TODO: Move this function somewhere that makes more sense and can be integrated with AI - probably LeagueManager or Team + LeagueInterfaceManager/RosterInterfaceManager
    //This assumes player is always the one paying to reveal
    public void PayToReveal()
	{
        athlete.GetTeam().ChangeGold(-1);
        ToggleStatsMysterized(false);

        FindObjectOfType<LeagueInterfaceManager>().UpdatePlayerBanner(); //Placeholder because this function needs a new home
	}

    public void ToggleStatsMysterized(bool mysterized)
	{
        if(mysterized)
		{
            starContainer.gameObject.SetActive(false);
            if (leagueManager.CheckAttributeIneligibility(attributeGroup))
                mysteryCoverup.SetActive(false);
            else
                mysteryCoverup.SetActive(true);
		}
        else
		{
            starContainer.gameObject.SetActive(true);
            mysteryCoverup.SetActive(false);
		}
	}
}
