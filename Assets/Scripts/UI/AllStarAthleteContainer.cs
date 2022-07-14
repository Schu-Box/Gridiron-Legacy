using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AllStarAthleteContainer : MonoBehaviour
{
    public PositionGroup positionGroup;
    public AttributeGroup displayedAttributeGroup;
    public AttributeGroup secondaryAttributeGroup;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI positionText;
    public AthleteRosterContainer athleteRosterContainer;
    public TextMeshProUGUI teamText;
    public Transform statBlockHolder;
    public GameObject statBlockPrefab;
    public Color statColor1;
    public Color statColor2;

    public GameObject leftCycleButton;
    public GameObject rightCycleButton;

    private Athlete assignedAthlete;
    private List<Athlete> listOfViewableAthletes;

    private bool alternator = true;

    private LeagueManager leagueManager;
    private RosterInterfaceManager rosterInterfaceManager;

	public void AssignPossibileAthletes(List<Athlete> possibleAthletes)
	{
        leagueManager = FindObjectOfType<LeagueManager>();
        rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();

        listOfViewableAthletes = possibleAthletes;

        SetForAthlete(possibleAthletes[0]);

        if(possibleAthletes.Count <= 1)
		{
            leftCycleButton.SetActive(false);
            rightCycleButton.SetActive(false);
		}
        else
		{
            leftCycleButton.SetActive(true);
            rightCycleButton.SetActive(true);
		}
	}

    public void SetForAthlete(Athlete athlete)
    {
        assignedAthlete = athlete;

        alternator = true;

        int currentSeason = leagueManager.currentSeason;
        
        if (athlete.mvpSeasons.Contains(currentSeason))
		{
            titleText.text = "MVP";
		}
        else if(athlete.allStarSeasons.Contains(currentSeason))
		{
            titleText.text = "All Star";
		}
        else
        {
            titleText.text = "";
        }

        positionText.text = positionGroup.ToString();

        athleteRosterContainer.SetAthlete(athlete, displayedAttributeGroup);
        //athleteRosterContainer.GetButton().onClick.AddListener(() => rosterInterfaceManager.OpenAthletePanel(athlete));
        teamText.text = "plays for " + athlete.GetTeam().GetTeamName();

        for (int i = statBlockHolder.childCount - 1; i >= 0; i--) //Clear children
            Destroy(statBlockHolder.GetChild(i).gameObject);

        //Could consider making statBlock it's own script
        GameObject totalDivineSpectacleStatBlock = Instantiate(statBlockPrefab, statBlockHolder);
        totalDivineSpectacleStatBlock.GetComponentInChildren<TextMeshProUGUI>().text = athlete.GetDivineSpectacleSum_CurrentSeason().ToString();
        totalDivineSpectacleStatBlock.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        for (int i = 0; i < athlete.GetStatList_SeasonTotal().Count; i++)
        {
            Stat stat = athlete.GetStatList_SeasonTotal()[i];
            if (stat.statType != StatType.Snaps && stat.statType != StatType.TotalYards)
			{
                if (stat.count != 0) //If the stat is non-zero, then display it
                {
                    GameObject newStatBlock = Instantiate(statBlockPrefab, statBlockHolder);
                    
                    if(stat.count == leagueManager.GetBestStatInLeague(stat.statType).count) //If this athlete is leading the league in this stat, bold it
                        newStatBlock.GetComponentInChildren<TextMeshProUGUI>().text = "<b>" + stat.count + " " + stat.statType.ToString() + "</b>";
                    else
                        newStatBlock.GetComponentInChildren<TextMeshProUGUI>().text = stat.count + " " + stat.statType.ToString();

                    if (!alternator)
                        newStatBlock.GetComponent<Image>().color = statColor1;
                    else
                        newStatBlock.GetComponent<Image>().color = statColor2;

                    alternator = !alternator; //Used to flip the colors on stat blocks
                }
                else
                {
                        //TODO: Still display the stat if it's negative divine spectacle but possible
                        //We want 0 interceptions to be highlighted, but not things like 0 PATs when PATs are not yet possible
                }
            }
        }
    }

    public void CycleAthlete(bool right)
	{
        int currentIndex = listOfViewableAthletes.IndexOf(assignedAthlete);

        int nextIndex = currentIndex;
        if (right)
        {
            nextIndex++;
            if (nextIndex >= listOfViewableAthletes.Count)
                nextIndex = 0;
        }
        else
        {
            nextIndex--;
            if (nextIndex < 0)
                nextIndex = listOfViewableAthletes.Count - 1;
        }

        SetForAthlete(listOfViewableAthletes[nextIndex]);
	}
}
