using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyMenuCtrl : MonoBehaviour {

	public string GameSceneName = "GameScene";
	public int MaxLobby;

	[Header("Object Settings")]
	public Animator ForegroundAnim;
	public Animator GameExitPanel;
	public Text HeadText;
	public Slider DifficultySlider;
	public Text DifficultyPercentText;
	public Text VersionText;
	public Text PlayerNameText;
	public Text TeamNameText;
	public Toggle[] TeamNumberToggle;
	public Toggle[] GameLengthToggle;
	public Slider GameDifficultSlider;
	public Image MusicImage;
	public Sprite MusicOn;
	public Sprite MusicOff;
	public Image AudioImage;
	public Sprite AudioOn;
	public Sprite AudioOff;
	public Animator PlayerNameAlert;
	public Animator TeamNameAlert;

	private int CurrentPage = 0;
	private Animator thisAnim;

	private float prevMusicSound = 1f;
	private float prevAudioSound = 1f;

	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
		VersionText.text += UnityEngine.Application.version;
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			MoveAnim ();
		}
	}

	void FixedUpdate () {
		DifficultyPercentText.text = DifficultySlider.value * 100 / DifficultySlider.maxValue + "%";
	}

	public void MoveConfigure(bool conf){
		if (conf)
			ForegroundAnim.SetTrigger ("Show");
		else
			ForegroundAnim.SetTrigger ("Hide");
		thisAnim.SetBool ("Configure", conf);
	}

	public void MoveAnim(int num = 0){
		if (num == 3 && CurrentPage == 3) {
			MoveScene ();
			return;
		}
		if (num == 0) {
			if (thisAnim.GetBool ("Configure")) {
				MoveConfigure (false);
				return;
			}
			if (CurrentPage > 0) {
				int page = CurrentPage;
				MoveAnim (-CurrentPage);
				CurrentPage = page - 1;
			} else
				GameExitAnim ();
			return;
		}

		if (CurrentPage == num)
			return;
		
		CurrentPage = num;

		if (num != 0) {
			thisAnim.SetInteger ("Move", num);
			thisAnim.SetTrigger ("Start");
		}
		if (num == -1)
			ForegroundAnim.SetTrigger ("Hide");
		if (num == 1)
			ForegroundAnim.SetTrigger ("Show");
		if (num == 3)
			HeadText.text = "국가설정";
		if (num == -3)
			HeadText.text = "국가선택";
		
	}

	public void MoveSelectCountry(){
		if (CurrentPage != 3)
			return;
		int page = CurrentPage;
		MoveAnim (-3);
		CurrentPage = page - 1;
	}

	public void InputText (Animator thisAnim) {
		thisAnim.SetTrigger ("Off");
	}

	void MoveScene() {
		if (PlayerNameText.text == "" || TeamNameText.text == "") {
			PlayerNameAlert.ResetTrigger ("Off");
			TeamNameAlert.ResetTrigger ("Off");
			if (PlayerNameText.text == "")
				PlayerNameAlert.SetTrigger ("On");
			if (TeamNameText.text == "")
				TeamNameAlert.SetTrigger ("On");
			return;
		}
		PlayerSettings.PlayerName = PlayerNameText.text;
		PlayerSettings.TeamName = TeamNameText.text;
		for (int i = 0; i < TeamNumberToggle.Length; i++)
			PlayerSettings.TeamNumber = TeamNumberToggle [i].isOn ? i + 2 : PlayerSettings.TeamNumber;
		for (int i = 0; i < GameLengthToggle.Length; i++)
			PlayerSettings.GameLength = GameLengthToggle [i].isOn ? i + 2 : PlayerSettings.GameLength;
		PlayerSettings.GameDifficulty = (int)GameDifficultSlider.value;

		SceneManager.LoadScene (GameSceneName);
	}

	public void GameExitAnim() {
		GameExitPanel.SetTrigger (GameExitPanel.GetCurrentAnimatorStateInfo(0).IsName("GameExitOn") ? "Off" : "On");
	}

	public void AppQuit() {
		Application.Quit ();
	}

	public void MusicSlider (Slider slider) {
		if (slider.value <= 0f)
			MusicImage.sprite = MusicOff;
		else
			MusicImage.sprite = MusicOn;
		MusicImage.SetNativeSize ();
		MusicImage.rectTransform.sizeDelta /= 10;
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
			AudioImage.sprite = AudioOff;
		else
			AudioImage.sprite = AudioOn;
		AudioImage.SetNativeSize ();
		AudioImage.rectTransform.sizeDelta /= 10;
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
}
