using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LawButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image image;

    private LeagueInterfaceManager leagueInterfaceManager;

	private void Awake()
	{
        leagueInterfaceManager = FindObjectOfType<LeagueInterfaceManager>();
    }

    public void SetLawChange(Amendment ammy)
	{
        button.onClick.AddListener(() => leagueInterfaceManager.DisplayLawChange(ammy));

        titleText.text = ammy.law.GetLawButtonString(ammy.amendmentType);
        descriptionText.text = ammy.law.descriptionString;
        image.sprite = ammy.law.image;
    }
}
