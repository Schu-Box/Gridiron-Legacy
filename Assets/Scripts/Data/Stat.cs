using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
	public StatType statType;
	public int count;

	public AttributeGroup attributeGroupImproved;
	public int divineSpectacle;

	public Stat(StatType type, int divineSpectacleValue, AttributeGroup agImproved)
	{
		statType = type;
		//stat string?
		divineSpectacle = divineSpectacleValue;
		attributeGroupImproved = agImproved;
	}

	public int GetDivineSpectacle()
	{
		return divineSpectacle * count;
	}
}
