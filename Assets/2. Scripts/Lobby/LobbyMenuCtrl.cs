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

	private int CurrentPage = 0;
	private Animator thisAnim;

	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (CurrentPage > 0) {
				int page = CurrentPage;
				MoveAnim (-CurrentPage);
				CurrentPage = page - 1;
			} else 
				GameExitAnim ();
		}
	}

	void FixedUpdate () {
		DifficultyPercentText.text = DifficultySlider.value * 100 / DifficultySlider.maxValue + "%";
	}

	public void MoveAnim(int num){
		
		if (num > 0 && CurrentPage == 3) {
			MoveScene ();
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

	void MoveScene() {
		SceneManager.LoadScene (GameSceneName);
	}

	public void GameExitAnim() {
		GameExitPanel.SetTrigger (GameExitPanel.GetCurrentAnimatorStateInfo(0).IsName("GameExitOn") ? "Off" : "On");
	}

	public void AppQuit() {
		Application.Quit ();
	}
}
