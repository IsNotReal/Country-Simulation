using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemCtrl : MonoBehaviour { // This object tag must be "GameController"

	[Header("Game Settings")]
	public float AddTimeDelay = 1f;
	public int MaxEventNum = 100;

	[Header("Time Settings")]
	public int StartDay = 1;
	public int StartMonth = 1;
	public int StartYear = 2017;

	[Header("UI Settings")]
	public Canvas UiCanvas;
	public Text TimeText;
	public Image MultipleImage;
	public Sprite[] MultipleSprites;
	public Slider RatingSlider;
	public Text RatingText;
	public Animator LeftUI;

	private int TimeDay;
	private int TimeMonth;
	private int TimeYear;
	private int TimeMultiple = 1;

	private List<EventCtrl> EventQueue;

	/* Variables */

	void Start () {
		if (MaxEventNum <= 0)
			MaxEventNum = 1;
		EventQueue = new List<EventCtrl>(MaxEventNum);
		GameStart ();
	}

	void FixedUpdate () {
		RatingText.text = RatingSlider.value + "%";
	}

	void GameRun () { // Use this function like Update Event
		
	}

	/* Event Functions */

	public void AddMultiple() {
		TimeMultiple = TimeMultiple < 4 ? TimeMultiple * 2 : 1;
		switch (TimeMultiple) {
		case 1:
			MultipleImage.sprite = MultipleSprites [0];
			break;
		case 2:
			MultipleImage.sprite = MultipleSprites [1];
			break;
		case 4:
			MultipleImage.sprite = MultipleSprites [2];
			break;
		default:
			MultipleImage.sprite = null;
			break;
		}
		MultipleImage.SetNativeSize ();
	}

	public void AddEvent (EventCtrl e) {
		EventQueue.Add (e);
	}

	public void MoveLeftUi () {
		if (LeftUI.GetCurrentAnimatorStateInfo (0).IsName ("LeftUIOn"))
			LeftUI.SetTrigger ("Off");
		else
			LeftUI.SetTrigger ("On");
	}

	/* ↓ Run Functions ↓ */

	void GameStart () {
		TimeDay = StartDay;
		TimeMonth = StartMonth;
		TimeYear = StartYear;
		StartCoroutine (RunTime ());
	}

	IEnumerator RunTime () {
		string m, d;
		m = TimeMonth < 10 ? "0" + TimeMonth : TimeMonth.ToString();
		d = TimeDay < 10 ? "0" + TimeDay : TimeDay.ToString();
		TimeText.text = TimeYear + "." + m + "." + d;

		yield return new WaitForSeconds (AddTimeDelay / TimeMultiple);

		TimeDay++;

		int maxDay = 0;
		switch (TimeMonth) {
		case 1:
			maxDay = 31;
			break;
		case 2:
			maxDay = 28;
			break;
		case 3:
			maxDay = 31;
			break;
		case 4:
			maxDay = 30;
			break;
		case 5:
			maxDay = 31;
			break;
		case 6:
			maxDay = 30;
			break;
		case  7:
			maxDay = 31;
			break;
		case 8:
			maxDay = 31;
			break;
		case 9:
			maxDay = 30;
			break;
		case 10:
			maxDay = 31;
			break;
		case 11:
			maxDay = 30;
			break;
		case 12:
			maxDay = 31;
			break;
		}

		if (TimeDay > maxDay) {
			TimeDay = 1;
			if (TimeMonth == 12) {
				TimeMonth = 1;
				TimeYear++;
			} else
				TimeMonth++;
		}

		if (EventQueue.Count > 0 && TimeCheck (EventQueue [0].ActiveDay, EventQueue [0].ActiveMonth, EventQueue [0].ActiveYear)) {
			EventQueue [0].enabled = true;
			EventQueue.RemoveAt (0);
		}

		GameRun ();
		StartCoroutine (RunTime ());
	}

	public bool TimeCheck (int Day, int Month, int Year) {
		return Day == TimeDay && Month == TimeMonth && Year == TimeYear;
	}
}
