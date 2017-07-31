using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyIconCtrl : MonoBehaviour {

	public Vector2 ShowTime;
	public float AnimationTime;

	private Animator thisAnim;

	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
		StartCoroutine (IconShow (Random.Range (ShowTime.x, ShowTime.y)));
	}

	IEnumerator IconShow(float time){
		yield return new WaitForSeconds (time);
		thisAnim.SetTrigger ("Start");
		StartCoroutine (IconShow (Random.Range (ShowTime.x, ShowTime.y) + AnimationTime));
	}

}
