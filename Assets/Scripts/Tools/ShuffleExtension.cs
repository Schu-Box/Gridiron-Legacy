using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShuffleExtension
{
    public static void Shuffle<T> (this List<T> list)
	{
		for(int i = list.Count - 1; i > 0; i--)
		{
			int randomIndex = Random.Range(0, i);

			T temp = list[i];

			list[i] = list[randomIndex];
			list[randomIndex] = temp;
		}
	}

	public static void ShiftAsideFromFirst<T> (this List<T> list)
	{
		List<T> newOrder = new List<T>();
		newOrder.Add(list[0]);

		for (int i = 1; i < list.Count; i++)
		{
			newOrder.Add(list[(i + 1) % list.Count]);
		}

		list = newOrder;
	}
}
