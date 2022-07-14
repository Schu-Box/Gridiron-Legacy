using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LawType
{
	YardsForFirstDown,
	Downs,
	MatchTime,
	ForwardPass,
	Punt,
	FieldGoal,
	PointAfterTouchdown
}

[CreateAssetMenu(fileName = "Laws", menuName = "LawObject", order = 1)]
public class Law : ScriptableObject
{
	public LawType id;
	public Sprite image;
	public string buttonString;
	[TextArea(2, 5)]
	public string descriptionString;
	public bool enacted, alwaysEnacted;
	public int value, minValue, maxValue, changeIncrement;

	[Header("Starting Values")]
	public bool startsEnacted;
	public int startValue;

	public Amendment GeneratePotentialAmendment()
	{
		Amendment newAmendment = null;

		if (!enacted)
		{
			newAmendment = new Amendment(this, AmendmentType.Enact);
		}
		else
		{
			//TODO: Enable chance for amendment to be Revoked even if there are elible changes

			if (CanLawChange())
			{
				AmendmentType chosenType;

				if (Random.value > 0.5) //Do a random roll to determine to allow an increase or decrease first
				{
					if (value + changeIncrement <= maxValue)
						chosenType = AmendmentType.Increase;
					else
						chosenType = AmendmentType.Decrease;
				}
				else
				{
					if (value - changeIncrement >= minValue)
						chosenType = AmendmentType.Decrease;
					else
						chosenType = AmendmentType.Increase;
				}

				newAmendment = new Amendment(this, chosenType);
			}
			else
			{
				if(alwaysEnacted)
				{
					Debug.Log("This law can never be changed. What a stupid law.");
				}
				else
				{
					newAmendment = new Amendment(this, AmendmentType.Revoke);
				}
			}
		}

		return newAmendment;
	}

	public bool CanLawChange() //I think this could be improved to combine chosenType
	{
		bool canChange = false;

		if(changeIncrement != 0)
		{
			if (value + changeIncrement <= maxValue)
				canChange = true;

			if (value - changeIncrement >= minValue)
				canChange = true;
		}

		return canChange;
	}

	public void ChangeLaw(AmendmentType amendmentType)
	{
		Debug.Log(id + " has been " + amendmentType);

		switch(amendmentType)
		{
			case AmendmentType.Increase:
				value += changeIncrement;
				break;
			case AmendmentType.Decrease:
				value -= changeIncrement;
				break;
			case AmendmentType.Enact:
				enacted = true;
				break;
			case AmendmentType.Revoke:
				enacted = false;
				break;
		}
	}

	public string GetLawButtonString(AmendmentType at)
	{
		string s;

		switch(at)
		{
			case AmendmentType.Increase:
				s = "Increase " + buttonString + " to " + (value + changeIncrement);
				break;
			case AmendmentType.Decrease:
				s = "Decrease " + buttonString + " to " + (value - changeIncrement);
				break;
			case AmendmentType.Enact:
				s = "Enact " + buttonString;
				break;
			case AmendmentType.Revoke:
				s = "Revoke " + buttonString;
				break;
			default:
				s = "null";
				break;
		}

		return s;
	}
}

public class Amendment
{
	public Law law;
	public AmendmentType amendmentType;

	public Amendment(Law l, AmendmentType a)
	{
		law = l;
		amendmentType = a;
	}
}

public enum AmendmentType
{
	Increase,
	Decrease,
	Enact,
	Revoke
}
