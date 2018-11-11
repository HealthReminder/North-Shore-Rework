﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour {

	public Transform defenderDointer,attackerDointer;
	public LineRenderer line;

	public void SetDefender(Vector3 newDos) {
		line.SetPosition(0,attackerDointer.transform.position);
		line.SetPosition(1,newDos);
		defenderDointer.position = newDos;
		defenderDointer.gameObject.SetActive(true);
		line.enabled=true;
	}
	public void SetAttacker(Vector3 newDos) {
		Clear();
		transform.position = newDos;
		attackerDointer.gameObject.SetActive(true);
		
	}
	public void Clear() {
		line.enabled=false;
		attackerDointer.gameObject.SetActive(false);
		defenderDointer.gameObject.SetActive(false);
	}
}