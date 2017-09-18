using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCtrl : MonoBehaviour {
	
	[Header("Event Settings")]
	public string EventName;
	[TextArea(3, 8)]
	public string EventText;
	public int EventKind;
	public int EventActivePosition; // this value must lower than GameSystemCtrl.EventAreas.length
	public bool isPositive = true;

	[Header("Active Time Settings")]
	public int ActiveDay = 1;
	public int ActiveMonth = 1;
	public int ActiveYear = 2017;
	public float DestroyTime = 10f;

	[Header("Other Settings")]
	public Sprite PositiveImage;
	public Sprite NegativeImage;

	private GameSystemCtrl GameSystem;
	private Animator thisAnim;
	private UnityEngine.UI.Image thisImage;
	private bool Actived = false;

	void Awake() {
		GameSystem = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameSystemCtrl> ();
		EventActivePosition = (int) Random.Range (0, GameSystem.EventAreas.Length); // test code

		thisAnim = gameObject.GetComponent<Animator> ();
		thisImage = gameObject.GetComponentInChildren<UnityEngine.UI.Image> ();
	}

	void Start () {
		if (ActiveDay <= 0 || ActiveMonth <= 0) {
			Debug.LogError ("Active time can't set under 0");
			return;
		}

		gameObject.transform.SetParent (GameSystem.EventAreas[EventActivePosition].transform);
		gameObject.transform.localScale = Vector3.zero;
		gameObject.transform.localPosition = Vector3.zero;
		thisImage.sprite = isPositive ? PositiveImage : NegativeImage;
		thisImage.SetNativeSize ();
		thisAnim.SetTrigger ("On");

		StartCoroutine (AutoDestroy ());
	}

	public void ActiveEvent() {
		Debug.Log ("Event Actived, Event Kind: " + EventKind);
		switch(EventKind){
		default:
			GameSystem.RandomRating ();
			break;
		}
	}

	public void Alert () {
		Actived = true;
		if (GameSystem == null) {
			Debug.LogError ("GameSystem can't null, Check EventSystem in GameController tag");
			return;
		}
		AlertFormCtrl afc = GameSystem.Alert (EventName, EventText, 1, 3f, 5f);
		afc.ReturnEvent = this;

		gameObject.GetComponent<UnityEngine.UI.Button> ().enabled = false;
	}

	public void DestroyEvent () {
		thisAnim.SetTrigger ("Off");
		StartCoroutine (DeleteObject (1f));
	}

	IEnumerator AutoDestroy () {
		yield return new WaitForSeconds (DestroyTime);
		if (!Actived)
			DestroyEvent ();
	}

	IEnumerator DeleteObject (float time) {
		yield return new WaitForSeconds (time);
		gameObject.transform.SetParent (null);
		gameObject.transform.position = Vector3.zero;
		gameObject.SetActive (false);
		// Destroy (gameObject);
	}
}
