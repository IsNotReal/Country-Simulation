using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CostAddCtrl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public GameSystemCtrl GameSystemComponent;

	public float[] AddCostNumber;
	public float[] AddCostTime;
	public int RepeatLevel = 3;

	private bool isClicked;
	private int currentRepeat;
	private int currentCostLevel;
	private float currentCostTime;

	public void OnPointerDown (PointerEventData eventData) {
		currentRepeat = 0;
		currentCostLevel = 0;
		currentCostTime = 0;
		isClicked = true;
	}

	public void OnPointerUp (PointerEventData eventData) {
		isClicked = false;
	}

	void Update () {
		if (!isClicked)
			return;
		if (currentCostTime > 0)
			currentCostTime -= Time.deltaTime;
		else {
			currentCostTime = AddCostTime [currentCostLevel];
			GameSystemComponent.AddInvest (AddCostNumber [currentCostLevel]);
			if (currentRepeat == RepeatLevel - 1 && currentCostLevel < AddCostNumber.Length - 1 && currentCostLevel < AddCostTime.Length - 1) {
				currentCostLevel++;
				currentRepeat = 0;
			} else
				currentRepeat++;
		}
	}

}
