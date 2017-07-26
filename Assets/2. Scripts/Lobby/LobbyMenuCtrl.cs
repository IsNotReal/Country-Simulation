using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMenuCtrl : MonoBehaviour {

	private int CurrentPage = 0;
	private Animator thisAnim;

	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape) && CurrentPage > 0) {
			MoveAnim (-CurrentPage);
			CurrentPage--;
		}
	}

	public void MoveAnim(int num){
		CurrentPage = num;
		switch (num) {
		case 1:
			thisAnim.SetTrigger ("MoveF1");
			break;
		case -1:
			thisAnim.SetTrigger ("BackF1");
			break;
		default:
			Debug.LogError ("Wrong page select \n\rCheck button event");
			break;
		}
	}
}
