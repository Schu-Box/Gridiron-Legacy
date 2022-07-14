using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchInterfaceManager : MonoBehaviour
{
	private MatchManager matchManager;
	private RosterInterfaceManager rosterInterfaceManager;

	public GameObject matchPanel;

	public GameObject homeTeamButton;
	public GameObject awayTeamButton;
	public TextMeshProUGUI timerText;
	public TextMeshProUGUI homeScoreText;
	public TextMeshProUGUI awayScoreText;

	public Image movementArrow;
	private RectTransform movementArrowRect;
	private float movementArrowOffset;
	public float movementArrowStartWidth = 900;
	public float movementArrowYardWidth = 150; //This should be changed to dynamically change w/ line of scrimmage - need better art asset

	public Image passingArrow;
	public Image puntingArrow;

	public TextMeshProUGUI downAndDistanceText;

    public GameObject gameLogHolder;
	public GameObject gameLogPrefab;
    private List<GameLog> gameLogs = new List<GameLog>();

	[Header("Field")]
	public GameObject baseFieldObject;
	public Image lineOfScrimmageImage;
	public Image firstDownLineImage;
	public Image homeEndzoneImage;
	public Image awayEndzoneImage;

	[Header("Post Game Panel")]
	public GameObject postGamePanel;
	public Transform postGameLogHolder;

	int gameLogInt = 0;
	//int gameLogMax;

	private float logOffset;
	private Vector2 logStart = new Vector2(0, 270);

	private bool rosterDisplayed = false;
	private bool homePossession = true;
	private Team homeTeam;
	private Team awayTeam;

	private string statStringFront = "<#FFFFFF>";
	private string statStringBack = "</color>";

	private void Awake()
	{
		matchManager = FindObjectOfType<MatchManager>();
		rosterInterfaceManager = FindObjectOfType<RosterInterfaceManager>();

		movementArrowOffset = movementArrow.transform.localPosition.x;
		movementArrowRect = movementArrow.GetComponent<RectTransform>();

		ResetMatchInterface();

		DisplayPanel(false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			ToggleGameSpeed(0);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			ToggleGameSpeed(1);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			ToggleGameSpeed(2);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			ToggleGameSpeed(3);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			ToggleGameSpeed(4);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			ToggleGameSpeed(5);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			ToggleGameSpeed(6);
		}
	}

	public void ToggleGameSpeed(int numSpeed)
	{
		switch(numSpeed)
		{
			case 0:
				Time.timeScale = 0f;
				break;
			case 1:
				Time.timeScale = 1f;
				break;
			case 2:
				Time.timeScale = 2f;
				break;
			case 3:
				Time.timeScale = 10f;
				break;
			case 4:
				Time.timeScale = 20f;
				break;
			case 5:
				Time.timeScale = 30f;
				break;
			case 6:
				Time.timeScale = 50f;
				break;
		}
	}

	//Tool
	public string GetOrdinalIndicator(int number)
	{
		string returnString = number.ToString();

		int num;
		if (number <= 13) //If the number is in the teens let it be
			num = number;
		else
			num = Mathf.Abs(number) % 10;

		if (num == 1)
			returnString += "st";
		else if (number == 2)
			returnString += "nd";
		else if (number == 3)
			returnString += "rd";
		else
			returnString += "th";

		return returnString;
	}

	public string GetStatString(string text)
	{
		return statStringFront + text + statStringBack;
	}

	public void DisplayPanel(bool enabled)
	{
		if (enabled)
			matchPanel.SetActive(true);
		else
			matchPanel.SetActive(false);
	}

	public void ResetMatchInterface()
	{
		for (int i = gameLogHolder.transform.childCount - 1; i > -1; i--)
		{
			Destroy(gameLogHolder.transform.GetChild(i).gameObject);
		}

		logOffset = gameLogPrefab.GetComponent<RectTransform>().sizeDelta.y;

		movementArrow.gameObject.SetActive(false);
		passingArrow.gameObject.SetActive(false);

		postGamePanel.SetActive(false);
	}

	public void SetupMatchInterface(Team h, Team a)
	{
		ResetMatchInterface();

		homeTeam = h;
		awayTeam = a;
		firstDownLineImage.color = homeTeam.colorPrimary;
		movementArrow.color = homeTeam.colorPrimary;
		passingArrow.color = homeTeam.colorPrimary;

		homeTeamButton.GetComponentInChildren<TextMeshProUGUI>().text = homeTeam.GetTeamName();
		awayTeamButton.GetComponentInChildren<TextMeshProUGUI>().text = awayTeam.GetTeamName();

		UpdateLineOfScrimmage();
		UpdateFirstDown();
	}

	public void OpenTeamPanel(bool home)
	{
		if (home)
			rosterInterfaceManager.OpenTeamPanel(homeTeam);
		else
			rosterInterfaceManager.OpenTeamPanel(awayTeam);
	}

	public void UpdateTime(int timeLeft)
	{
		//This should be animated

		timerText.text = timeLeft.ToString();
	}

	public void AddNewGameLog(Team possessingTeam, string log, int time)
	{
		GameLog newLog = Instantiate(gameLogPrefab, gameLogHolder.transform).GetComponent<GameLog>();

		newLog.SetGameLog(possessingTeam.colorPrimary, log, time);

		newLog.transform.localPosition = logStart;
		newLog.transform.localScale = Vector3.zero;
		LeanTween.scale(newLog.gameObject, Vector3.one, matchManager.newLogDuration * 0.1f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() => AnimateGameLogs());


		//AnimateGameLogs();
	}

	public void AnimateGameLogs() //Shift all game logs down
	{
		//LeanTween.moveLocal(gameLogHolder, newPosition, delay).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => FinishAnimation());	

		for(int i = 0; i < gameLogHolder.transform.childCount; i++)
		{
			Vector3 newPosition = gameLogHolder.transform.GetChild(i).localPosition;
			newPosition.y -= logOffset;
			//gameLogHolder.transform.GetChild(i).localPosition = newPosition;

			LeanTween.moveLocal(gameLogHolder.transform.GetChild(i).gameObject, newPosition, matchManager.newLogDuration * 0.9f).setEase(LeanTweenType.easeInExpo);
		}
	}

	//public void FinishAnimation()
	//{
	//	for(int i = gameLogs.Count - 2; i > 0; i--) //Bottom text (i = gameLogs.Count-1) is ignored, starting with bottom + 1, copy the previous log.
	//	{
	//		gameLogs[i].SetText(gameLogs[i - 1].GetText());
	//	}

	//	gameLogs[0].SetText("");

	//	gameLogHolder.transform.localPosition = Vector3.zero;
	//}

	public void FlipPossession(bool homeSide)
	{
		if (homeSide)
		{
			firstDownLineImage.color = homeTeam.colorPrimary;

			movementArrow.color = homeTeam.colorPrimary;
			movementArrow.transform.eulerAngles = new Vector3(0, 0, 0);
			movementArrow.transform.localPosition = new Vector3(movementArrowOffset, 0, 0);

			passingArrow.color = homeTeam.colorPrimary;
			passingArrow.transform.eulerAngles = new Vector3(0, 180, 0);
			movementArrow.transform.localPosition = new Vector3(-40, 0, 0);

			puntingArrow.color = homeTeam.colorPrimary;
			puntingArrow.transform.eulerAngles = new Vector3(0, 0, 0);
			puntingArrow.transform.localPosition = new Vector3(-60, 0, 0);
		}
		else
		{
			firstDownLineImage.color = awayTeam.colorPrimary;

			movementArrow.color = awayTeam.colorPrimary;
			movementArrow.transform.eulerAngles = new Vector3(0, 180, 0);
			movementArrow.transform.localPosition = new Vector3(-movementArrowOffset, 0, 0);

			passingArrow.color = awayTeam.colorPrimary;
			passingArrow.transform.eulerAngles = new Vector3(0, 0, 0);
			movementArrow.transform.localPosition = new Vector3(40, 0, 0);

			puntingArrow.color = awayTeam.colorPrimary;
			puntingArrow.transform.eulerAngles = new Vector3(0, 180, 0);
			puntingArrow.transform.localPosition = new Vector3(70, 0, 0);
		}

		homePossession = homeSide;
	}

	public void HideMovementArrow()
	{
		movementArrow.gameObject.SetActive(false);

		ShowPassingArrow(false);
		ShowPuntingArrow(false);
	}

	public void SetMovementArrowLane(int lane)
	{
		Vector3 newPosition = movementArrow.transform.localPosition;

		if (lane == 0)
			newPosition.y = -85;
		else if (lane == 1)
			newPosition.y = 0;
		else
			newPosition.y = 85;

		movementArrow.transform.localPosition = newPosition;

		Vector2 movementArrowSize = new Vector2(movementArrowStartWidth, movementArrowRect.sizeDelta.y);
		movementArrowRect.sizeDelta = movementArrowSize;
	}

	public void UpdateMovementArrow(int yardsGainedSoFar)
	{
		movementArrow.gameObject.SetActive(true);

		Vector2 newSize;
		newSize.y = movementArrowRect.sizeDelta.y;

		float newWidth = movementArrowStartWidth + yardsGainedSoFar * movementArrowYardWidth;

		LeanTween.value(movementArrowRect.sizeDelta.x, newWidth, matchManager.newLogDuration).setOnUpdate((value) =>
		{
			newSize.x = value;

			movementArrowRect.sizeDelta = newSize;
		});
	}

	public void UpdateMovementArrowToEndzone(int yardsGained, bool home)
	{
		movementArrow.gameObject.SetActive(true);

		Vector2 newSize;
		newSize.y = movementArrowRect.sizeDelta.y;

		float newWidth = movementArrowStartWidth + yardsGained * movementArrowYardWidth;

		LeanTween.value(movementArrowRect.sizeDelta.x, newWidth, matchManager.newLogDuration).setOnUpdate((value) =>
		{
			newSize.x = value;

			movementArrowRect.sizeDelta = newSize;
		}).setOnComplete(() =>
		{
			if (home)
				homeEndzoneImage.color = homeTeam.colorPrimary;
			else
				awayEndzoneImage.color = awayTeam.colorPrimary;
		});
	}

	public void ShowPassingArrow(bool show)
	{
		passingArrow.gameObject.SetActive(show);
	}

	public void ShowPuntingArrow(bool show)
	{
		puntingArrow.gameObject.SetActive(show);
	}

	public void UpdateLineOfScrimmage()
	{
		int down = matchManager.GetDown();
		int newYardage = matchManager.GetLineOfScrimmage();
		int yardsToGain = matchManager.GetYardsToGain();

		Vector3 newScrimmagePosition = lineOfScrimmageImage.transform.localPosition;
		
		float fieldWidth = baseFieldObject.GetComponent<RectTransform>().sizeDelta.x;
		float yardWidth = (fieldWidth / matchManager.fieldLength);

		newScrimmagePosition.x = (yardWidth * newYardage) - (fieldWidth / 2);
		
		if (!homePossession)
			newScrimmagePosition.x = -newScrimmagePosition.x;

		LeanTween.moveLocal(lineOfScrimmageImage.gameObject, newScrimmagePosition, matchManager.newLogDuration).setEase(LeanTweenType.easeSpring).setOnComplete(() =>
		{
			downAndDistanceText.text = GetOrdinalIndicator(down) + " and " + yardsToGain;
		});
	}

	public void UpdateFirstDown()
	{
		//Debug.Log("Updating first down");

		int down = matchManager.GetDown();
		int newYardage = matchManager.GetLineOfScrimmage();
		int yardsToGain = matchManager.GetYardsToGain();

		Vector3 newFirstDownPosition = firstDownLineImage.transform.localPosition;

		float fieldWidth = baseFieldObject.GetComponent<RectTransform>().sizeDelta.x;
		float yardWidth = (fieldWidth / matchManager.fieldLength);

		newFirstDownPosition.x = (yardWidth * (newYardage + yardsToGain) - (fieldWidth / 2));

		if (!homePossession)
			newFirstDownPosition.x = -newFirstDownPosition.x;

		LeanTween.moveLocal(firstDownLineImage.gameObject, newFirstDownPosition, matchManager.newLogDuration).setEase(LeanTweenType.easeSpring).setOnComplete(() =>
		{
			downAndDistanceText.text = GetOrdinalIndicator(down) + " and " + yardsToGain;
		});
	}

	public void ToggleFirstDownLine(bool enabled)
	{
		firstDownLineImage.gameObject.SetActive(enabled);
	}

	public void UpdateScore(bool home, int newScore)
	{
		if (home)
		{
			homeScoreText.text = newScore.ToString();
			AnimateGoal(true);
		}
		else
		{
			awayScoreText.text = newScore.ToString();
			AnimateGoal(false);
		}
	}

	public void AnimateGoal(bool home)
	{
		HideMovementArrow();

		downAndDistanceText.text = "";
		if (home)
		{
			LeanTween.value(homeEndzoneImage.gameObject, homeTeam.colorPrimary, Color.black, matchManager.newLogDuration).setOnUpdate((Color val) =>
			{
				homeEndzoneImage.color = val;
			});
		}
		else
		{
			LeanTween.value(awayEndzoneImage.gameObject, awayTeam.colorPrimary, Color.black, matchManager.newLogDuration).setOnUpdate((Color val) =>
			{
				awayEndzoneImage.color = val;
			});
		}
	}

	public void DisplayPostGame()
	{
		postGamePanel.SetActive(true);

		for(int i = postGameLogHolder.childCount - 1; i > -1; i--)
		{
			Destroy(postGameLogHolder.GetChild(i).gameObject);
		}
	}

	public void DisplayTopPerformers(List<Athlete> athletes, List<Stat> stats)
	{
		for(int i = 0; i < athletes.Count; i++)
		{
			GameLog newLog = Instantiate(gameLogPrefab, postGameLogHolder).GetComponent<GameLog>();

			newLog.SetText(athletes[i].GetName() + " had " + stats[i].count + " " + stats[i].statType + ". Their " + stats[i].attributeGroupImproved.ToString() + " improved!");
		}
	}

	public void DisplayExitMatch()
	{
		matchPanel.SetActive(false);
		postGamePanel.SetActive(false);

		matchManager.ExitMatch();
	}
}
