using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCtrl : MonoBehaviour {
	
	[Header("Event Settings")]
	public string EventName;
	[TextArea(3, 8)]
	public string EventText;
	public int EventKind;

	[Header("Active Time Settings")]
	public int ActiveDay = 1;
	public int ActiveMonth = 1;
	public int ActiveYear = 1;

	private GameSystemCtrl GameSystem;

	void Awake() {
		GameSystem = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameSystemCtrl> ();
		StartCoroutine (AddEvent ());
	}

	void Start () {
		
		if (ActiveDay <= 0 || ActiveMonth <= 0) {
			Debug.LogError ("Active time can't set under 0");
			return;
		}

		switch(EventKind){
		default:
			Debug.Log ("Event Actived, Event Kind: " + EventKind);
			break;
		}

		Destroy (gameObject);
	}

	IEnumerator AddEvent(){
		yield return new WaitForEndOfFrame ();
		GameSystem.AddEvent (this);
	}

}
