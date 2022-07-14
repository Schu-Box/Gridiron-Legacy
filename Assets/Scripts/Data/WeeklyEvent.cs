using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EventTrigger
{
    StartSeason,
    FanApprovalBelow
}

[CreateAssetMenu(fileName = "WeeklyEvent", menuName = "EventObject", order = 2)]
public class WeeklyEvent : ScriptableObject
{
    //public string id;
    public List<EventTrigger> eventTriggers;
    [Tooltip("Must be the same length as eventTriggers")]
    public List<float> eventTriggerValue;
    public string title;
    [SerializeField] private string description;
    public int weeksBetweenRepeat;
    [HideInInspector] public int weeksUntilRepeat = 0;

    //need a way to get team/athlete name and insert into description + interactions where necessary

    public List<WeeklyEventOption> options;

    public Team impactedTeam;

    public string GetDescription()
	{
        string descriptionString = description;
        descriptionString = description.Replace("{team}", impactedTeam.GetTeamName());

        return descriptionString;
	}
}

public enum WeeklyOptionResult
{
    FanApproval,
    Gold
}

[System.Serializable]
public class WeeklyEventOption
{
    public string description;

    public List<WeeklyOptionResult> optionResults;
    [Tooltip("Must be the same length as eventTriggers")]
    public List<float> optionResultValues;
}
