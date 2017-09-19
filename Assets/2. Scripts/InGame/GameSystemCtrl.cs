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
	public GameObject EventActive;

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
	public Image[] PeopleRatingImages = new Image[9];
	public Image[] MajestyRatingImages = new Image[9];
	public Image[] DominionRatingImages = new Image[9];
	public Color GoodColor = new Color (81f / 255f, 181f / 255f, 81f / 255f, 1f);
	public Color BadColor = new Color (253f / 255f, 69f / 255f, 53f / 255f, 1f);
	public Color NormalColor = new Color (67f / 255f, 67f / 255f, 67f / 255f, 1f);
	public Vector2 NormalColorRange = new Vector2 (40f, 60f);

	private int CurrentAnimPos = 0;

	private static int TimeDay;
	private static int TimeMonth;
	private static int TimeYear;
	private int TimeMultiple = 1;
	private bool TimeRunning = true;

	private float StartTouchDistance = -1;
	private float StartViewSize;
	private float CurrentViewSize = 1;

	private List<EventCtrl> EventQueue;

	private float[,] ApprovalRatings = new float[3, 9];
	/* Set Approval Ratings Array values meaning ( Name of Sprite sources )
	 * 0: Dominion { Culture, Defense, Develop, Diplomacy, Food, Fuel, Renewable, Road, Science }
	 * 1: Majesty { Export, GDP, IMF, Import, Law, Religion, Safety, Tax, Welfare }
	 * 2: People { Child, Education, Enterprise, Labor, Market, Medical, Nature, Pleasure, Social }
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

		EventInitialize ();
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
		SetApprovalUI ();
	}

	/* Event Functions */

	void EventInitialize() {
		for (int i = 0; i < MaxEventNum; i++) {
			EventCtrl e = Instantiate (EventActive).GetComponent<EventCtrl> ();
			int year = StartYear;
			int month = StartMonth;
			int day = Random.Range(1, PlayerSettings.GameLength * 365) + StartDay;

			while (day > GetMaxDay (year, month)) {
				day -= GetMaxDay (year, month);
				if (month != 12)
					month++;
				else {
					month = 1;
					year++;
				}
			}
			e.ActiveYear = year;
			e.ActiveMonth = month;
			e.ActiveDay = day;
			e.EventKind = i;

			EventQueue.Add (e);
		}
		SortEventQueue ();

		EventQueue [0].ActiveDay = StartDay + 1;
		EventQueue [0].ActiveMonth = StartMonth;
		EventQueue [0].ActiveYear = StartYear;
	}

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

	public void RandomRating() {
		for (int i = 0; i < ApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < ApprovalRatings.GetLength (1); j++) {
				ApprovalRatings [i, j] = Random.Range(0, 100f);
			}
		}
	}

	public void AddRating(int type, int index, float value = 0) {
		ApprovalRatings [type, index] += value;
	}

	/* ↓ UI Functions ↓ */

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

		switch (i) { // AnimInteger to ArrayIndex Convert
		case 0:
			i = 2;
			break;
		case 1:
			i = 1;
			break;
		case 2:
			i = 0;
			break;
		}

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
		case 2:
			int j = UIAnim.GetInteger ("UINum") - 1;
			if (j == 0) {
				for (int i = 0; i < PeopleRatingImages.Length; i++) {
					Color color = NormalColor;
					if (ApprovalRatings [2, i] < NormalColorRange.x)
						color = BadColor;
					else if (ApprovalRatings [2, i] > NormalColorRange.y)
						color = GoodColor;
					PeopleRatingImages [i].color = color;
				}
			}
			if (j == 1) {
				for (int i = 0; i < MajestyRatingImages.Length; i++) {
					Color color = NormalColor;
					if (ApprovalRatings [1, i] < NormalColorRange.x)
						color = BadColor;
					else if (ApprovalRatings [1, i] > NormalColorRange.y)
						color = GoodColor;
					MajestyRatingImages [i].color = color;
				}
			}
			if (j == 1) {
				for (int i = 0; i < DominionRatingImages.Length; i++) {
					Color color = NormalColor;
					if (ApprovalRatings [0, i] < NormalColorRange.x)
						color = BadColor;
					else if (ApprovalRatings [0, i] > NormalColorRange.y)
						color = GoodColor;
					DominionRatingImages [i].color = color;
				}
			}
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
		m = TimeMonth < 10 ? "0" + TimeMonth : TimeMonth.ToString ();
		d = TimeDay < 10 ? "0" + TimeDay : TimeDay.ToString ();
		TimeText.text = TimeYear + "." + m + "." + d;

		while (!TimeRunning)
			yield return null;
		
		yield return new WaitForSeconds (AddTimeDelay / TimeMultiple);

		if (!TimeRunning) {
			StartCoroutine (RunTime ());
			yield break;
		}
		TimeDay++;

		if (TimeDay > GetMaxDay (TimeYear, TimeMonth)) {
			TimeDay = 1;
			if (TimeMonth == 12) {
				TimeMonth = 1;
				TimeYear++;
			} else
				TimeMonth++;
		}

		while (EventQueue.Count > 0 && TimeCheck (EventQueue [0].ActiveDay, EventQueue [0].ActiveMonth, EventQueue [0].ActiveYear, false)) {
			EventQueue [0].enabled = true;
			EventQueue.RemoveAt (0);
			SortEventQueue ();
		}

		if (StartYear + PlayerSettings.GameLength <= TimeYear && StartMonth <= TimeMonth && StartDay <= TimeDay) {
			//Game End
			Debug.Log ("Game End");
			yield break;
		}

		GameRun ();
		StartCoroutine (RunTime ());
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

	void SortEventQueue() {
		for (int loop = 0; loop < EventQueue.Count - 1; loop++) {
			for (int i = 0; i < EventQueue.Count - 1 - loop; i++) {
				if (EventQueue [i].ActiveYear > EventQueue [i + 1].ActiveYear) {
					EventCtrl temp = EventQueue [i];
					EventQueue [i] = EventQueue [i + 1];
					EventQueue [i + 1] = temp;
				}
			}
		}
		for (int loop = 0; loop < EventQueue.Count - 1; loop++) {
			for (int i = 0; i < EventQueue.Count - 1 - loop; i++) {
				if (EventQueue [i].ActiveYear == EventQueue [i + 1].ActiveYear && EventQueue [i].ActiveMonth > EventQueue [i + 1].ActiveMonth) {
					EventCtrl temp = EventQueue [i];
					EventQueue [i] = EventQueue [i + 1];
					EventQueue [i + 1] = temp;
				}
			}
		}
		for (int loop = 0; loop < EventQueue.Count - 1; loop++) {
			for (int i = 0; i < EventQueue.Count - 1 - loop; i++) {
				if (EventQueue [i].ActiveYear == EventQueue [i + 1].ActiveYear && EventQueue [i].ActiveMonth == EventQueue [i + 1].ActiveMonth && EventQueue [i].ActiveDay > EventQueue [i + 1].ActiveDay) {
					EventCtrl temp = EventQueue [i];
					EventQueue [i] = EventQueue [i + 1];
					EventQueue [i + 1] = temp;
				}
			}
		}
	}

	public static bool TimeCheck (int Day, int Month, int Year, bool isExact = true) {
		return isExact ? 
			Day == TimeDay && Month == TimeMonth && Year == TimeYear :
			Year < TimeYear || Year == TimeYear && Month < TimeMonth || Year == TimeYear && Month == TimeMonth && Day <= TimeDay;
	}

	public static int GetMaxDay (int Year, int Month){
		switch (Month) {
		case 1:
			return 31;
		case 2:
			return Year % 4 == 0 && Year % 100 != 0 || Year % 400 == 0 ? 29 : 28;
		case 3:
			return 31;
		case 4:
			return 30;
		case 5:
			return 31;
		case 6:
			return 30;
		case  7:
			return 31;
		case 8:
			return 31;
		case 9:
			return 30;
		case 10:
			return 31;
		case 11:
			return 30;
		case 12:
			return 31;
		default:
			Debug.LogError ("Month Number Error");
			return 31;
		}
	}
}
