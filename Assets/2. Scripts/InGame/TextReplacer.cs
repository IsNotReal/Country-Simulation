using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextReplacer : MonoBehaviour {

	public Color GoodColor = new Color (81f / 255f, 181f / 255f, 81f / 255f, 1f);
	public Color BadColor = new Color (253f / 255f, 69f / 255f, 53f / 255f, 1f);

	private string[] targetString;
	private Text thisText;

	void Awake () {
		thisText = gameObject.GetComponent<Text> ();
		targetString = new string[] {
			"PlayerName",
			"TeamName",
			"PlayerNumber"
		};
	}

	void Start () {
		Replace ();
	}

	public void Replace () {
		bool target = true;
		while (target) {
			target = false;
			string isContains = "";
			int numContains = 0;

			for (int i = 0; i < targetString.Length; i++){
				if (thisText.text.Contains ("{{" + targetString [i] + "}}")) {
					isContains = "{{" + targetString [i] + "}}";
					numContains = i;
					target = true;
					break;
				}
			}
				
			if (!target)
				return;

			string innerText = " ";
			switch (numContains) {
			case 0:
				innerText = PlayerSettings.PlayerName;
				break;
			case 1:
				innerText = PlayerSettings.TeamName;
				break;
			case 2:
				innerText = PlayerSettings.PlayerNumber.ToString();
				break;
			default:
				break;
			}

			thisText.text = thisText.text.Replace (isContains, innerText);
		}
	}

}
