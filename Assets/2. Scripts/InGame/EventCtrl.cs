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
	public Vector2 ActiveTimeRange = new Vector2 (10f, 15f);
	public float DestroyTime = 10f;

	[Header("Other Settings")]
	public Sprite PositiveImage;
	public Sprite NegativeImage;

	private float ActiveTime;
	private GameSystemCtrl GameSystem;
	private Animator thisAnim;
	private UnityEngine.UI.Image thisImage;
	private bool Actived = false;

	void Awake() {
		ActiveTime = Random.Range (ActiveTimeRange.x, ActiveTimeRange.y);
		GameSystem = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameSystemCtrl> ();
		EventActivePosition = (int) Random.Range (0, GameSystem.EventAreas.Length); // test code

		thisAnim = gameObject.GetComponent<Animator> ();
		thisImage = gameObject.GetComponentInChildren<UnityEngine.UI.Image> ();
		StartCoroutine (AutoActive ());
	}

	void Start () {
		gameObject.transform.SetParent (GameSystem.EventAreas[EventActivePosition].transform);
		gameObject.transform.localScale = Vector3.zero;
		gameObject.transform.localPosition = Vector3.zero;
		thisImage.sprite = isPositive ? PositiveImage : NegativeImage;
		thisImage.SetNativeSize ();
		thisAnim.SetTrigger ("On");

		StartCoroutine (AutoDestroy ());
	}

	public void ActiveEvent() {
		GameSystem.AddHappiness (isPositive);
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

	IEnumerator AutoActive () {
		while (!GameSystemCtrl.TimeRunning)
			yield return null;
		yield return new WaitForSeconds (ActiveTime);
		if (!GameSystemCtrl.TimeRunning) {
			StartCoroutine (AutoActive ());
			yield break;
		}
		this.enabled = true;
	}

	IEnumerator AutoDestroy () {
		while (!GameSystemCtrl.TimeRunning)
			yield return null;
		yield return new WaitForSeconds (DestroyTime);
		if (!GameSystemCtrl.TimeRunning) {
			StartCoroutine (AutoDestroy ());
			yield break;
		}
		if (!Actived)
			DestroyEvent ();
	}

	IEnumerator DeleteObject (float time) {
		yield return new WaitForSeconds (time);
		GameSystem.EventCreate ();
		gameObject.transform.SetParent (null);
		gameObject.transform.position = Vector3.zero;
		gameObject.SetActive (false);
		Destroy (gameObject);
	}
}
