using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarContainer : MonoBehaviour
{
	public GameObject starPrefab;

	public void SetStarValue(float value)
	{
		int halfStarsEarned = 0;

		halfStarsEarned = Mathf.RoundToInt(value / 0.1f);

		int fullStarsEarned = halfStarsEarned;
		if (fullStarsEarned % 2 == 1)
			fullStarsEarned--;

		fullStarsEarned = fullStarsEarned / 2;

		//Debug.Log(value + " is " + halfStarsEarned + " half stars --- " + fullStarsEarned);

		for(int i = transform.childCount - 1; i >=0; i--) //Destroy all old stars
		{
			Destroy(transform.GetChild(i).gameObject);
		}

		for(int i = 0; i < fullStarsEarned; i++)
		{
			GameObject newStar = Instantiate(starPrefab, transform);

			newStar.transform.GetChild(0).GetComponent<Image>().enabled = false;
		}

		if (halfStarsEarned > fullStarsEarned * 2) //If there's a half star, display it in the next starHolder
		{
			GameObject newStar = Instantiate(starPrefab, transform);

			newStar.transform.GetChild(0).GetComponent<Image>().enabled = true;
		}

		//for (int i = 0; i < fullStarsEarned; i++) //Show all earned stars
		//{
		//	transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;

		//	transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
		//}

		//for (int i = fullStarsEarned; i < transform.childCount; i++) //Hide all non-earned stars
		//{
		//	transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;

		//	transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
		//}

		//if (halfStarsEarned > fullStarsEarned * 2) //If there's a half star, display it in the next starHolder
		//{

		//	transform.GetChild(fullStarsEarned).GetChild(0).GetComponent<Image>().enabled = true;

		//	transform.GetChild(fullStarsEarned).GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
		//}
	}
}
