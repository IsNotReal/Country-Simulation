using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleScroll : MonoBehaviour {

	public RectTransform Content;
	public Toggle[] ContentItems;
	public bool Horizontal = true;
	public bool Vertical = true;
	public float MoveSpeed = 0.5f;
	public Vector2 MaxPos;
	public Vector2 MinPos;
	public Scrollbar HorizontalScrollbar;
	public Scrollbar VerticalScrollbar;

	private Vector3 target;

	void Awake () {
		if (Content == null)
			Debug.LogError ("Content Can't be null");

		if (HorizontalScrollbar != null) {
			HorizontalScrollbar.gameObject.SetActive (Horizontal);
			HorizontalScrollbar.size = gameObject.GetComponent<RectTransform> ().sizeDelta.x / Content.sizeDelta.x;
		}
		if (VerticalScrollbar != null) {
			VerticalScrollbar.gameObject.SetActive (Vertical);
			VerticalScrollbar.size = gameObject.GetComponent<RectTransform> ().sizeDelta.y / Content.sizeDelta.y;
		}

		for (int i = 0; i < ContentItems.Length; i++) {
			Toggle item = ContentItems [i];
			item.onValueChanged.AddListener(delegate {
				MoveContent (item.GetComponent<RectTransform> ());
				ContentAnim ();
			});
		}
		ContentAnim ();
	}

	void Update () {
		Vector3 vel = Vector3.zero;
		Content.localPosition = Vector3.SmoothDamp (Content.localPosition, target, ref vel, MoveSpeed * Time.deltaTime);
		if (Horizontal && HorizontalScrollbar != null)
			HorizontalScrollbar.value = (Content.localPosition.x / (MaxPos.x - MinPos.x)) + 0.5f;
		if (Vertical && VerticalScrollbar != null)
			VerticalScrollbar.value = (Content.localPosition.y / (MaxPos.y - MinPos.y)) + 0.5f;
	}

	public void MoveContent (RectTransform contentItem) {
		float x = Content.localPosition.x;
		float y = Content.localPosition.y;

		if (Horizontal)
			x = contentItem.localPosition.x;
		if (Vertical)
			y = contentItem.localPosition.y;

		target = new Vector2 (-x, -y);
	}

	public void ContentAnim () {
		for (int i = 0; i < ContentItems.Length; i++) {
			if (ContentItems [i].isOn != ContentItems [i].GetComponent<Animator> ().GetBool ("isOn"))
				ContentItems [i].GetComponent<Animator> ().SetBool ("isOn", ContentItems [i].isOn);
		}
	}
}
