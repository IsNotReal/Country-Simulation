using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyIconCtrl : MonoBehaviour {

	public Vector2 ShowTime;

	private Animator thisAnim;
	private bool isAnimRun = false;

	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
	}

	void Update () {
		if (thisAnim.GetCurrentAnimatorStateInfo (0).IsName ("IdleState") && !isAnimRun) {
			StartCoroutine (IconShow (Random.Range (ShowTime.x, ShowTime.y)));
			isAnimRun = true;
		}
	}

	IEnumerator IconShow(float time){
		yield return new WaitForSeconds (time);
		thisAnim.SetTrigger ("Start");
		isAnimRun = false;
	}
}
