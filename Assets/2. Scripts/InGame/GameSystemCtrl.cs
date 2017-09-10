using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameSystemCtrl : MonoBehaviour { // This object tag must be "GameController"

	[Header("Game Settings")]
	public float AddTimeDelay = 1f;
	public int MaxEventNum = 100;
	public GameObject[] EventAreas;

	[Header("View Settings")]
	public float MaxViewSize = 1.5f;
	public float MinViewSize = 0.5f;
	public Vector2 MaxViewPosition;
	public float CameraMoveSpeed = 10f;
	public float CameraZoomSpeed = 10f;

	[Header("Prefab Settings")]
	public GameObject AlertForm;

	// [Header("Time Settings")]
	[HideInInspector]
	public int StartDay = 1;
	[HideInInspector]
	public int StartMonth = 1;
	[HideInInspector]
	public int StartYear = 2017;

	[Header("UI Settings")]
	public Canvas UiCanvas;
	public Text TimeText;
	public Color TimeRunColor;
	public Color TimeStopColor;
	public Image MultipleImage;
	public Sprite[] MultipleSprites;
	public Slider RatingSlider;
	public Text RatingText;
	public Animator LeftUI;
	public Animator GameExitUI;
	public Animator ForegroundAnim;
	public Animator UIAnim;
	public Image SelectedImage;
	public Text SelectedText;
	public Image ApprovalCircleGraph;
	public Text ApprovalAgreePercentText;
	public Text ApprovalDisagreePercentText;
	public Text AllApprovalPercentText;
	public Image ApprovalCheckApprovalCircleGraph;
	public Text ApprovalCheckApprovalPercentText;

	private int CurrentAnimPos = 0;

	private int TimeDay;
	private int TimeMonth;
	private int TimeYear;
	private int TimeMultiple = 1;
	private bool TimeRunning = true;

	private float StartTouchDistance = -1;
	private float StartViewSize;
	private float CurrentViewSize = 1;

	private List<EventCtrl> EventQueue;

	private float[,] ApprovalRatings = new float[3, 9];
	/* Set Approval Ratings Array values meaning ( Name of Sprite sources )
	 * 0: { Culture, Defense, Develop, Diplomacy, Food, Fuel, Renewable, Road, Science }
	 * 1: { Export, GDP, IMF, Import, Law, Religion, Safety, Tax, Welfare }
	 * 2: { Child, Education, Enterprise, Labor, Market, Medical, Nature, Pleasure, Social }
	 */
	private float this[int index1, int index2]{
		get {
			return ApprovalRatings[index1, index2];
		}
		set {
			if (value < 0)
				ApprovalRatings[index1, index2] = 0;
			if (value > 100f)
				ApprovalRatings[index1, index2] = 100f;
		}
	}

	/* Variables */

	void Start () {
		if (MaxEventNum <= 0)
			MaxEventNum = 1;
		EventQueue = new List<EventCtrl>(MaxEventNum);
		StartViewSize = Camera.main.orthographicSize;

		StartDay = System.DateTime.Now.Day;
		StartMonth = System.DateTime.Now.Month;
		StartYear = System.DateTime.Now.Year;

		for (int i = 0; i < ApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < ApprovalRatings.GetLength (1); j++)
				ApprovalRatings [i, j] = 50f;
		}

		GameStart ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (UIAnim.GetCurrentAnimatorStateInfo (0).IsName ("GameUIMoveAR"))
				MoveApprovalRating (false);
			else if (CurrentAnimPos != 0)
				MoveUI (-CurrentAnimPos);
			else if (GameObject.FindGameObjectsWithTag ("Alert").Length == 0)
				MoveQuitUI ();
		}
		if (TimeRunning)
			TouchView ();
	}

	void GameRun () { // Active this when game time running
		for (int i = 0; i < ApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < ApprovalRatings.GetLength (1); j++) {
				ApprovalRatings [i, j] = Random.Range(0, 100f);
			}
		}

		SetApprovalUI ();
	}

	/* Event Functions */

	public AlertFormCtrl Alert (string head, string info, int happy = 0, float appadd = 0f, float appsub = 0f) {
		GameObject form = Instantiate (AlertForm);
		form.transform.SetParent (UiCanvas.transform);
		form.transform.localPosition = AlertForm.transform.position;
		AlertFormCtrl afc = form.GetComponent<AlertFormCtrl> ();
		afc.HeadText.text = head;
		afc.InfoText.text = info;

		afc.Happiness = happy;
		afc.AppAdd = appadd;
		afc.AppSub = appsub;

		return afc;
	}

	public void AddMultiple() {
		if (!TimeRunning)
			return;
		TimeMultiple = TimeMultiple < 4 ? TimeMultiple * 2 : 1;
		SetMultipleImage (TimeMultiple);
	}

	public void AddEvent (EventCtrl e) {
		EventQueue.Add (e);
	}

	void SetApprovalUI () {
		float approval = 0;
		for (int i = 0; i < ApprovalRatings.GetLength(0); i++) {
			for (int j = 0; j < ApprovalRatings.GetLength (1); j++) {
				approval += ApprovalRatings [i, j];
			}
		}
		approval /= ApprovalRatings.GetLength (0) * ApprovalRatings.GetLength (1);
		RatingSlider.value = approval;
		RatingText.text = (int)RatingSlider.value + "%";
		ApprovalCheckApprovalCircleGraph.fillAmount = approval / 100f;
		ApprovalCheckApprovalPercentText.text = (int)approval + "%";
	}

	/* ↓ UI Functions ↓ */

	void SetMultipleImage(int multiple) {
		switch (multiple) {
		case 1:
			MultipleImage.sprite = MultipleSprites [1];
			break;
		case 2:
			MultipleImage.sprite = MultipleSprites [2];
			break;
		case 4:
			MultipleImage.sprite = MultipleSprites [3];
			break;
		default:
			MultipleImage.sprite = MultipleSprites [0];
			break;
		}
		MultipleImage.SetNativeSize ();
		if (MultipleImage.sprite == MultipleSprites [0]) {
			MultipleImage.rectTransform.sizeDelta = new Vector2 (48, 48);
		}
	}

	void TouchView () {
		Vector3 CamPos = Camera.main.transform.position;
		float mul = (MaxViewSize - CurrentViewSize) / MinViewSize;
		CurrentViewSize = Mathf.Clamp (CurrentViewSize, MinViewSize, MaxViewSize);
		Camera.main.orthographicSize = CurrentViewSize * StartViewSize;

		// View Move
		if (Input.touches.Length == 1) {
			if (CurrentViewSize < MaxViewSize) {
				CamPos.x = Mathf.Clamp (CamPos.x - Input.touches[0].deltaPosition.x * CameraMoveSpeed, MaxViewPosition.x * -mul, MaxViewPosition.x * mul);
				CamPos.y = Mathf.Clamp (CamPos.y - Input.touches[0].deltaPosition.y * CameraMoveSpeed, MaxViewPosition.y * -mul, MaxViewPosition.y * mul);
				CamPos.z = Camera.main.transform.position.z;
				Camera.main.transform.position = CamPos;
			} else 
				Camera.main.transform.position = Vector3.forward * Camera.main.transform.position.z;
		}

		// View Zoom
		if (Input.touches.Length == 2) {
			
			for (int i = 0; i < EventAreas.Length; i++)
				EventAreas [i].transform.localScale = Vector3.one * CurrentViewSize;
			
			/* Code for Test
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) 
				CurrentViewSize -= Input.GetAxis ("Mouse ScrollWheel") * CameraZoomSpeed;
			*/

			if (Input.touches [0].phase == TouchPhase.Moved && Input.touches [1].phase == TouchPhase.Moved) {
				if (StartTouchDistance < 0) {
					StartTouchDistance = Vector2.Distance (Input.touches [0].position, Input.touches [1].position);
					return;
				}
				float currentDistance = Vector2.Distance (Input.touches [0].position, Input.touches [1].position);
				CurrentViewSize -= (currentDistance / StartTouchDistance - 1) * CameraZoomSpeed;

				if (CurrentViewSize < MaxViewSize) {
					CamPos.x = Mathf.Clamp (CamPos.x, MaxViewPosition.x * -mul, MaxViewPosition.x * mul);
					CamPos.y = Mathf.Clamp (CamPos.y, MaxViewPosition.y * -mul, MaxViewPosition.y * mul);
					CamPos.z = Camera.main.transform.position.z;
					Camera.main.transform.position = CamPos;
				} else 
					Camera.main.transform.position = Vector3.forward * Camera.main.transform.position.z;
				
			} else
				StartTouchDistance = -1;
		}
	}

	public void SetSelected () {
		GameObject obj = EventSystem.current.currentSelectedGameObject;
		SelectedImage.sprite = obj.GetComponentsInChildren<Image> ()[1].sprite;
		SelectedText.text = obj.GetComponentInChildren<Text> ().text;
	}

	public void SetGraphSelected (int selected) {
		int i = UIAnim.GetInteger ("UINum") - 1;

		ApprovalCircleGraph.fillAmount = ApprovalRatings [i, selected] / 100f;
		ApprovalAgreePercentText.text = ((int)ApprovalRatings [i, selected]).ToString() + "%";
		ApprovalDisagreePercentText.text = ((int)(100f - ApprovalRatings [i, selected])).ToString() + "%";
		AllApprovalPercentText.text = ApprovalRatings [i, selected] / (ApprovalRatings.GetLength (0) * ApprovalRatings.GetLength (1)) + "%";
	}

	public void MoveLeftUI () {
		LeftUI.SetTrigger (LeftUI.GetCurrentAnimatorStateInfo (0).IsName ("LeftUIOn") ? "Off" : "On");
	}

	public void MoveQuitUI () {
		GameExitUI.SetTrigger (GameExitUI.GetCurrentAnimatorStateInfo (0).IsName ("GameExitOn") ? "Off" : "On");
		TimeStop ();
	}

	public void MoveApprovalRating(bool isOn){
		if (UIAnim.GetCurrentAnimatorStateInfo (0).IsName ("GameUIMoveAR") && isOn)
			return;
		UIAnim.SetBool ("MoveAR", isOn);
		UIAnim.SetTrigger ("Start");
		ForegroundAnim.ResetTrigger ("Show");
		ForegroundToggle (isOn);
		TimeStop (isOn);
		LeftUI.SetTrigger (isOn ? "Destroy" : "Create");
		if (isOn) {
			UIAnim.SetInteger ("UINum", 0);
			UIAnim.SetInteger ("Move", 0);
			CurrentAnimPos = 0;
		}
	}

	public void GotoMainMenu () {
		SceneManager.LoadScene("LobbyScene");
	}

	public void MoveUI (int num) {
		if (UIAnim.IsInTransition (0))
			return;

		if (num > 0)
			CurrentAnimPos = num;
		else 
			CurrentAnimPos = Mathf.Abs (num) - 1;

		switch (num) {
		case -1:
			LeftUI.SetTrigger ("Create");
			ForegroundToggle (false);
			TimeStop (false);
			break;
		case 1:
			LeftUI.SetTrigger ("Destroy");
			ForegroundToggle (true);
			TimeStop (true);
			break;
		}

		UIAnim.SetInteger ("Move", num);
		UIAnim.SetTrigger ("Start");
	}

	public void BackUI () {
		MoveUI (-CurrentAnimPos);
	}

	public void MoveUINumSet (int num) {
		UIAnim.SetInteger ("UINum", num);
	}

	void ForegroundToggle (bool isOn) {
		ForegroundAnim.ResetTrigger ("Show");
		ForegroundAnim.ResetTrigger ("Hide");
		ForegroundAnim.SetTrigger (isOn ? "Show" : "Hide");
	}

	/* ↓ Run Functions ↓ */

	void GameStart () {
		TimeDay = StartDay;
		TimeMonth = StartMonth;
		TimeYear = StartYear;
		StartCoroutine (RunTime());
	}

	IEnumerator RunTime () {
		string m, d;
		m = TimeMonth < 10 ? "0" + TimeMonth : TimeMonth.ToString();
		d = TimeDay < 10 ? "0" + TimeDay : TimeDay.ToString();
		TimeText.text = TimeYear + "." + m + "." + d;

		while (!TimeRunning) 
			yield return null;
		
		yield return new WaitForSeconds (AddTimeDelay / TimeMultiple);

		if (!TimeRunning) {
			StartCoroutine (RunTime ());
			yield break;
		}

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

		StartCoroutine (RunTime());
	}

	public void TimeStop () {
		TimeRunning = !TimeRunning;
		TimeText.color = TimeRunning ? TimeRunColor : TimeStopColor;
		SetMultipleImage (TimeRunning ? TimeMultiple : 0);
	}

	public void TimeStop (bool time) {
		TimeRunning = !time;
		TimeText.color = TimeRunning ? TimeRunColor : TimeStopColor;
		SetMultipleImage (TimeRunning ? TimeMultiple : 0);
	}

	public bool TimeCheck (int Day, int Month, int Year) {
		return Day == TimeDay && Month == TimeMonth && Year == TimeYear;
	}
}
