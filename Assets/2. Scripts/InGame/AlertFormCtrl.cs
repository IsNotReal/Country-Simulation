using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertFormCtrl : MonoBehaviour {

	public EventCtrl ReturnEvent;

	public int Happiness = 0;
	public float AppAdd = 0f;
	public float AppSub = 0f;

	public Text HeadText;
	public Text InfoText;
	public Text EventText;
	public Image HappinessImage;
	public Button AlertButton;

	public Color AddColor;
	public Color SubColor;
	public Sprite HapGood;
	public Sprite HapBad;

	private Animator thisAnim;
	private GameSystemCtrl GameSystem;

	// Use this for initialization
	void Start () {
		thisAnim = gameObject.GetComponent<Animator> ();
		GameSystem = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameSystemCtrl> ();

		thisAnim.SetTrigger ("On");
		GameSystem.TimeStop (true);

		string aColor = GetHexColor (AddColor);
		string sColor = GetHexColor (SubColor);

		string color = Happiness < 0 ? sColor : aColor;
		EventText.text = EventText.text.Replace ("{{Happiness}}", "<color='#" + color + "'>" + Happiness.ToString () + "</color>");
		color = AppAdd < 0 ? sColor : aColor;
		EventText.text = EventText.text.Replace ("{{AppAdd}}", "<color='#" + color + "'>" + AppAdd.ToString () + "%</color>");
		color = AppSub > 0 ? sColor : aColor;
		EventText.text = EventText.text.Replace ("{{AppSub}}", "<color='#" + color + "'>" + AppSub.ToString () + "%</color>");
		HappinessImage.sprite = Happiness < 0 ? HapBad : HapGood;
	}

	string GetHexColor(Color c){
		string r = ((int)(c.r * 255)).ToString ("X");
		string g = ((int)(c.g * 255)).ToString ("X");
		string b = ((int)(c.b * 255)).ToString ("X");
		return r + g + b;
	}

	public void Accept (bool isAccept) {
		if (isAccept)
			ReturnEvent.ActiveEvent ();
		thisAnim.SetTrigger ("Off");
		GameSystem.TimeStop (false);
		StartCoroutine (DestroyThis (0.5f));
	}

	IEnumerator DestroyThis (float t) {
		yield return new WaitForSeconds (t);
		ReturnEvent.DestroyEvent ();
		Destroy (gameObject);
	}
}
