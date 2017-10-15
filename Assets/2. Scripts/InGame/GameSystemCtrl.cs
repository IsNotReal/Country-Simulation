﻿using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameSystemCtrl : MonoBehaviour { // This object tag must be "GameController"

	[Header("Game Settings")]
	public float AddTimeDelay = 1f;
	public GameObject[] EventAreas;

	[Header("View Settings")]
	public float MaxViewSize = 1.5f;
	public float MinViewSize = 0.5f;
	public Vector2 MaxViewPosition;
	public float CameraMoveSpeed = 10f;
	public float CameraZoomSpeed = 10f;

	[Header("Prefab Settings")]
	public TextAsset VotersXML;
	public GameObject AlertForm;
	public GameObject EventActive;

	// [Header("Time Settings")]
	[HideInInspector]
	public int StartDay = 1;
	[HideInInspector]
	public int StartMonth = 1;
	[HideInInspector]
	public int StartYear = 2017;
	public static bool TimeRunning = true;

	[Header("UI Settings")]
	public Canvas UiCanvas;
	public Text TimeText;
	public Color TimeRunColor;
	public Color TimeStopColor;
	public Image MultipleImage;
	public Sprite[] MultipleSprites;
	public Slider RatingSlider;
	public Text RatingText;
	public Text HappinessText;
	public Text UnhappinessText;
	public Animator LeftUI;
	public Animator GameExitUI;
	public Animator ForegroundAnim;
	public Animator UIAnim;
	public Animator ConfigureUI;
	public Image SelectedImage;
	public Text SelectedText;
	public Image ApprovalCircleGraph;
	public Text ApprovalAgreePercentText;
	public Text ApprovalDisagreePercentText;
	public Text AllApprovalPercentText;
	public Image ApprovalCheckApprovalCircleGraph;
	public Text ApprovalCheckApprovalPercentText;
	public Text ApprovalInvestNumText;
	public Image[] PeopleRatingImages = new Image[9];
	public Image[] MajestyRatingImages = new Image[9];
	public Image[] DominionRatingImages = new Image[9];
	public Color GoodColor = new Color (81f / 255f, 181f / 255f, 81f / 255f, 1f);
	public Color BadColor = new Color (253f / 255f, 69f / 255f, 53f / 255f, 1f);
	public Color NormalColor = new Color (67f / 255f, 67f / 255f, 67f / 255f, 1f);
	public Vector2 NormalColorRange = new Vector2 (40f, 60f);
	public Sprite MusicOnSprite;
	public Sprite MusicOffSprite;
	public Sprite AudioOnSprite;
	public Sprite AudioOffSprite;
	public Image MusicImage;
	public Image AudioImage;

	private float prevMusicSound = 1;
	private float prevAudioSound = 1;

	private int CurrentAnimPos = 0;

	private float StartTouchDistance = -1;
	private float StartViewSize;
	private float CurrentViewSize = 1;

	private static int TimeDay;
	private static int TimeMonth;
	private static int TimeYear;
	private int TimeMultiple = 1;

	private int SelectedApproval = 0;
	private int Happiness = 0;
	private int Unhappiness = 0;

	/* Set Approval Ratings Array values meaning ( Name of Sprite sources )
	 * 0: Dominion { Culture, Defense, Develop, Diplomacy, Food, Fuel, Renewable, Road, Science }
	 * 1: Majesty { Export, GDP, IMF, Import, Law, Religion, Safety, Tax, Welfare }
	 * 2: People { Child, Education, Enterprise, Labor, Market, Medical, Nature, Pleasure, Social }
	 */

	/* Set Approval Voters values meaning
	* 0: Voters.유권자 ID1
	* 1: Voters.체제 ID2
	* 2: Voters.소득층 ID3
	* 3: Voters.특수유권자 ID4
	*/

	private int[,] ApprovalPoints = new int[3, 9];
	private float[,] Investments = new float[3, 9];
	private float[,] AddedInvest = new float[3, 9];
	private float[,] GoodApprovalRatings = new float[3, 9];
	private float[,] BadApprovalRatings = new float[3, 9];

	/* Variables */


	void InitializeValues() {
		StartViewSize = Camera.main.orthographicSize;

		StartDay = System.DateTime.Now.Day;
		StartMonth = System.DateTime.Now.Month;
		StartYear = System.DateTime.Now.Year;

		for (int i = 0; i < ApprovalPoints.GetLength (0); i++) {
			for (int j = 0; j < ApprovalPoints.GetLength (1); j++)
				ApprovalPoints [i, j] = (int)Random.Range (0, 5);
		}
	}

	/* Initializes */


	void Awake () {
		InitializeValues ();
	}

	void Start () {
		GameStart ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (ConfigureUI.GetCurrentAnimatorStateInfo (0).IsName ("GameExitOn"))
				ConfigureUI.SetTrigger ("Off");
			else if (UIAnim.GetCurrentAnimatorStateInfo (0).IsName ("GameUIMoveAR"))
				MoveApprovalRating (false);
			else if (CurrentAnimPos != 0)
				MoveUI (-CurrentAnimPos);
			else if (GameObject.FindGameObjectsWithTag ("Alert").Length == 0)
				MoveQuitUI ();
		}
		if (TimeRunning)
			TouchView ();
	}

	void DayRun () {
		SetApprovalUI ();
	}

	void MonthRun () {

	}

	void YearRun () {

	}

	/* Event Functions */


	void ApplyInvest () {
		for (int i = 0; i < AddedInvest.GetLength (0); i++) {
			for (int j = 0; j < AddedInvest.GetLength (1); j++) {
				if (AddedInvest [i, j] == 0)
					continue;
				else if (AddedInvest [i, j] > 0)
					GoodApprovalRatings [i, j] += Mathf.Abs ((int)(AddedInvest [i, j] / GetVotersState (true, i, j)));
				else
					BadApprovalRatings [i, j] += Mathf.Abs ((int)(-AddedInvest [i, j] / GetVotersState (false, i, j)));
				AddedInvest [i, j] = 0;
				Debug.Log ("Good: " + GoodApprovalRatings [i, j] + ", Bad: " + BadApprovalRatings [i, j]);
			}
		}

	}

	public void AddInvest (bool isAdd){
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

		float addNum = 0;
		if (Investments [i, SelectedApproval] < 10f)
			addNum = 0.5f;
		else if (Investments [i, SelectedApproval] < 100f)
			addNum = 5f;
		else
			addNum = 50f;

		if (!isAdd && Investments [i, SelectedApproval] <= 0) {
			Investments [i, SelectedApproval] = 0;
			return;
		}

		addNum *= isAdd ? 1 : -1;
		Investments [i, SelectedApproval] = Investments [i, SelectedApproval] + addNum;
		AddedInvest [i, SelectedApproval] += addNum;

		SetGraphSelected (SelectedApproval);
	}

	public float GetVotersState(bool isGood, int array, int approval){
		string arrname = "";
		string apvname = "";
		switch (array) {
		case 0:
			arrname = "Ter";
			switch (approval) {
			case 0:
				apvname = "Cul";
				break;
			case 1:
				apvname = "Def";
				break;
			case 2:
				apvname = "Urb";
				break;
			case 3:
				apvname = "Dip";
				break;
			case 4:
				apvname = "Foo";
				break;
			case 5:
				apvname = "Fos";
				break;
			case 6:
				apvname = "Ren";
				break;
			case 7:
				apvname = "Roa";
				break;
			case 8:
				apvname = "Res";
				break;
			}
			break;
		case 1:
			arrname = "Sov";
			switch (approval) {
			case 0:
				apvname = "Exp";
				break;
			case 1:
				apvname = "Gdp";
				break;
			case 2:
				apvname = "Imf";
				break;
			case 3:
				apvname = "Imp";
				break;
			case 4:
				apvname = "Met";
				break;
			case 5:
				apvname = "Rel";
				break;
			case 6:
				apvname = "Saf";
				break;
			case 7:
				apvname = "Tax";
				break;
			case 8:
				apvname = "Wel";
				break;
			}
			break;
		case 2:
			arrname = "Civil";
			switch (approval) {
			case 0:
				apvname = "Chi";
				break;
			case 1:
				apvname = "Edu";
				break;
			case 2:
				apvname = "Ent";
				break;
			case 3:
				apvname = "Lab";
				break;
			case 4:
				apvname = "Mar";
				break;
			case 5:
				apvname = "Med";
				break;
			case 6:
				apvname = "Env";
				break;
			case 7:
				apvname = "Ple";
				break;
			case 8:
				apvname = "Soc";
				break;
			}
			break;
		}

		if (arrname == "" || apvname == "") {
			Debug.LogError ("Can't find XML Node");
			return 0;
		}

		XmlDocument xml = new XmlDocument ();
		xml.LoadXml (VotersXML.ToString());
		XmlNode root = xml.DocumentElement;

		float result = 0;
		int num = 0;
		for (int i = 0; i < root.ChildNodes.Count; i++) {
			XmlNode id = root.ChildNodes.Item (i);
			if (id.Name == "#comment")
				continue;
//			Debug.Log ("---- ["+id.Name + "] ----");
			for (int j = 0; j < root.ChildNodes.Item (i).ChildNodes.Count; j++) {
				XmlNode target = id.ChildNodes.Item (j);
				if (target.Name == "#comment")
					continue;
//				Debug.Log (target.Name + ": " + target.SelectSingleNode (arrname).SelectSingleNode (apvname).SelectSingleNode (isGood ? "Inc" : "Dec").InnerText);
				result += float.Parse (target.SelectSingleNode (arrname).SelectSingleNode (apvname).SelectSingleNode (isGood ? "Inc" : "Dec").InnerText);
				num++;
			}
		}
		result = result / num;
//		Debug.Log ("평균 반응: " + result);
		return result;
	}

	public void AddHappiness (bool isHappy) {
		if (isHappy)
			Happiness++;
		else
			Unhappiness++;
		HappinessText.text = Happiness.ToString();
		UnhappinessText.text = Unhappiness.ToString();
	}

	public void EventCreate (int kind = 0) {
		EventCtrl obj = Instantiate (EventActive).GetComponent<EventCtrl> ();
		obj.EventKind = kind;
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
		for (int i = 0; i < GoodApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < GoodApprovalRatings.GetLength (1); j++)
				GoodApprovalRatings [i, j] += (int)Random.Range (0, 100f);
		}
		for (int i = 0; i < BadApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < BadApprovalRatings.GetLength (1); j++)
				BadApprovalRatings [i, j] += (int)Random.Range (0, 100f);
		}
		SetApprovalUI ();
	}

	float AllApprovalPercent(float num = 1, bool isGood = true) {
		float ga = 0;
		float ba = 0;
		for (int i = 0; i < GoodApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < GoodApprovalRatings.GetLength (1); j++)
				ga += GoodApprovalRatings [i, j];
		}
		for (int i = 0; i < BadApprovalRatings.GetLength (0); i++) {
			for (int j = 0; j < BadApprovalRatings.GetLength (1); j++)
				ba += BadApprovalRatings [i, j];
		}
		if (ga + ba == 0)
			return 0.5f * num;
		return (isGood ? ga : ba) / (ga + ba) * num;
	}

	float AllApprovalPercent(int index1, int index2, float num = 1, bool isGood = true) {
		if (GoodApprovalRatings [index1, index2] + BadApprovalRatings [index1, index2] == 0)
			return 0.5f * num ;
		return (isGood ? GoodApprovalRatings [index1, index2] : BadApprovalRatings [index1, index2]) / (GoodApprovalRatings [index1, index2] + BadApprovalRatings [index1, index2]) * num;
	}

	/* ↓ UI Functions ↓ */


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

	public void MoveConfigureUI () {
		ConfigureUI.SetTrigger (ConfigureUI.GetCurrentAnimatorStateInfo (0).IsName ("GameExitOn") ? "Off" : "On");
	}

	void SetApprovalUI () {
		RatingSlider.value = AllApprovalPercent (100);
		RatingText.text = RatingSlider.value.ToString ("F0") + "%";
		ApprovalCheckApprovalCircleGraph.fillAmount = AllApprovalPercent();
		ApprovalCheckApprovalPercentText.text = AllApprovalPercent (100).ToString ("F0") + "%";
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
			
		ApprovalCircleGraph.fillAmount = AllApprovalPercent (i, selected);
		ApprovalAgreePercentText.text = AllApprovalPercent (i, selected, 100).ToString ("F0") + "%";
		ApprovalDisagreePercentText.text = AllApprovalPercent (i, selected, 100, false).ToString ("F0") + "%";
		AllApprovalPercentText.text = AllApprovalPercent (i, selected, 100).ToString ("F2") + "%";
		ApprovalInvestNumText.text = "$" + Investments [i, selected].ToString ("F2") + "G";
		SelectedApproval = selected;
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
		UIAnim.SetTrigger ("StartAR");
		ForegroundAnim.ResetTrigger ("Show");

		bool isTimeStop = isOn || !(UIAnim.GetInteger ("Move") == 0 || UIAnim.GetInteger ("Move") == -1);
		ForegroundToggle (isTimeStop);
		TimeStop (isTimeStop);
		LeftUI.SetTrigger (isTimeStop ? "Destroy" : "Create");
		if (!isOn && isTimeStop)
			StartCoroutine (MoveUIBack (UIAnim.GetCurrentAnimatorClipInfo (0) [0].clip.length));
	}

	IEnumerator MoveUIBack(float time) {
		yield return new WaitForSeconds (time);
		int move = UIAnim.GetInteger ("Move") < 0 ? Mathf.Abs (UIAnim.GetInteger ("Move")) - 1 : UIAnim.GetInteger ("Move");
		UIAnim.SetInteger ("Move", move);
		UIAnim.SetTrigger ("Start");
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
		case -3:
			if (num == -3)
				ApplyInvest ();
			
			int j = UIAnim.GetInteger ("UINum") - 1;
			if (j == 0) {
				for (int i = 0; i < PeopleRatingImages.Length; i++) {
					Color color = NormalColor;
					if (AllApprovalPercent (2, i, 100) < NormalColorRange.x)
						color = BadColor;
					else if (AllApprovalPercent (2, i, 100) > NormalColorRange.y)
						color = GoodColor;
					PeopleRatingImages [i].color = color;
				}
			}
			if (j == 1) {
				for (int i = 0; i < MajestyRatingImages.Length; i++) {
					Color color = NormalColor;
					if (AllApprovalPercent (1, i, 100) < NormalColorRange.x)
						color = BadColor;
					else if (AllApprovalPercent (1, i, 100) > NormalColorRange.y)
						color = GoodColor;
					MajestyRatingImages [i].color = color;
				}
			}
			if (j == 1) {
				for (int i = 0; i < DominionRatingImages.Length; i++) {
					Color color = NormalColor;
					if (AllApprovalPercent (0, i, 100) < NormalColorRange.x)
						color = BadColor;
					else if (AllApprovalPercent (0, i, 100) > NormalColorRange.y)
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

	public void MusicSlider (Slider slider) {
		if (slider.value <= 0f)
			MusicImage.sprite = MusicOffSprite;
		else
			MusicImage.sprite = MusicOnSprite;
		MusicImage.SetNativeSize ();
		MusicImage.rectTransform.sizeDelta /= 8;
		PlayerSettings.MusicSound = slider.value;
	}

	public void MusicButton (Slider slider) {
		if (slider.value > 0) {
			prevMusicSound = slider.value;
			slider.value = 0;
		} else {
			float value = slider.value;
			slider.value = prevMusicSound;
			prevMusicSound = value;
		}
		PlayerSettings.MusicSound = slider.value;
	}

	public void AudioSlider (Slider slider) {
		if (slider.value <= 0f)
			AudioImage.sprite = AudioOffSprite;
		else
			AudioImage.sprite = AudioOnSprite;
		AudioImage.SetNativeSize ();
		AudioImage.rectTransform.sizeDelta /= 8;
		PlayerSettings.AudioSound = slider.value;
	}

	public void AudioButton (Slider slider) {
		if (slider.value > 0) {
			prevAudioSound = slider.value;
			slider.value = 0;
		} else {
			float value = slider.value;
			slider.value = prevAudioSound;
			prevAudioSound = value;
		}
		PlayerSettings.AudioSound = slider.value;
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
				YearRun ();
			} else
				TimeMonth++;
			MonthRun ();
		}

		if (StartYear + PlayerSettings.GameLength <= TimeYear && StartMonth <= TimeMonth && StartDay <= TimeDay) {
			//Game End
			Debug.Log ("Game End");
			yield break;
		}

		DayRun ();
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
