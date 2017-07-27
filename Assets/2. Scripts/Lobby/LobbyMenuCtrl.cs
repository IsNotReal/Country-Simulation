using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMenuCtrl : MonoBehaviour {

	public Animator ForegroundAnim;

	private int CurrentPage = 0;
	private Animator thisAnim;

	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape) && CurrentPage > 0) {
			int page = CurrentPage;
			MoveAnim (-CurrentPage);
			CurrentPage = page - 1;
		}
	}

	public void MoveAnim(int num){
		CurrentPage = num;
		if (num != 0) {
			thisAnim.SetInteger ("Move", num);
			thisAnim.SetTrigger ("Start");
		}
		if (num == -1)
			ForegroundAnim.SetTrigger ("Hide");
		if (num == 1)
			ForegroundAnim.SetTrigger ("Show");
	}
}
