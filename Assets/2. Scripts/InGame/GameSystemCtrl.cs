﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameSystemCtrl : MonoBehaviour { // This object tag must be "GameController"

	[Header("Game Settings")]
	public float AddTimeDelay = 1f;
	public int MaxEventNum = 100;

	[Header("View Settings")]
	public float MaxViewSize = 1.5f;
	public float MinViewSize = 0.5f;
	public Vector2 MaxViewPosition;
	public float CameraMoveSpeed = 10f;

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
	public Animator GameExitUI;

	private int TimeDay;
	private int TimeMonth;
	private int TimeYear;
	private int TimeMultiple = 1;
	private bool TimeRunning = true;

	private float StartTouchDistance = -1;
	private float StartViewSize;
	private float CurrentViewSize = 1;

	private List<EventCtrl> EventQueue;

	/* Variables */

	void Start () {
		if (MaxEventNum <= 0)
			MaxEventNum = 1;
		EventQueue = new List<EventCtrl>(MaxEventNum);
		StartViewSize = Camera.main.orthographicSize;
		GameStart ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			MoveQuitUI ();
		}
		TouchView ();
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

	void TouchView () {
		CurrentViewSize = Mathf.Clamp (CurrentViewSize, MinViewSize, MaxViewSize);
		Camera.main.orthographicSize = CurrentViewSize * StartViewSize;

		// View Move
		if (Input.touches.Length == 1) {
			if (CurrentViewSize < 1) {
				float mul = (MaxViewSize - CurrentViewSize) / MinViewSize;
				Vector3 CamPos = (Vector2)Camera.main.transform.position + Input.touches[0].deltaPosition * CameraMoveSpeed;
				CamPos.x = Mathf.Clamp (CamPos.x, MaxViewPosition.x * -mul, MaxViewPosition.x * mul);
				CamPos.y = Mathf.Clamp (CamPos.x, MaxViewPosition.y * -mul, MaxViewPosition.y * mul);
				CamPos.z = Camera.main.transform.position.z;
				Camera.main.transform.position = CamPos;
			} else 
				Camera.main.transform.position = Vector3.forward * Camera.main.transform.position.z;
		}

		// View Zoom
		if (Input.touches.Length == 2) {
			if (Input.touches [0].phase == TouchPhase.Moved && Input.touches [1].phase == TouchPhase.Moved) {
				if (StartTouchDistance < 0) {
					StartTouchDistance = Vector2.Distance (Input.touches [0].position, Input.touches [1].position);
					return;
				}
				float currentDistance = Vector2.Distance (Input.touches [0].position, Input.touches [1].position);
				CurrentViewSize -= currentDistance / StartTouchDistance - 1;
			} else
				StartTouchDistance = -1;
		}
	}

	public void MoveLeftUi () {
		LeftUI.SetTrigger (LeftUI.GetCurrentAnimatorStateInfo (0).IsName ("LeftUIOn") ? "Off" : "On");
	}

	public void MoveQuitUI () {
		GameExitUI.SetTrigger (GameExitUI.GetCurrentAnimatorStateInfo (0).IsName ("GameExitOn") ? "Off" : "On");
		TimeStop ();
	}

	public void GotoMainMenu() {
		SceneManager.LoadScene("LobbyScene");
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

		while (!TimeRunning) 
			yield return null;

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

	public void TimeStop () {
		TimeRunning = !TimeRunning;
	}

	public bool TimeCheck (int Day, int Month, int Year) {
		return Day == TimeDay && Month == TimeMonth && Year == TimeYear;
	}
}
