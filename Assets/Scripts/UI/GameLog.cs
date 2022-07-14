using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameLog : MonoBehaviour
{
	public Image borderImage;
	public Image fillImage;
    public TextMeshProUGUI logText;
	public TextMeshProUGUI timeText;

	private string log;

	public void SetGameLog(Color teamColor, string gameLogString, int timeLeft)
	{
		borderImage.color = teamColor;

		log = gameLogString;
		logText.text = gameLogString;

		timeText.text = timeLeft.ToString();
	}

	public void SetText(string txt)
	{
		logText.text = txt;
		timeText.text = "";
	}

	public string GetText()
	{
		return log;
	}
}
