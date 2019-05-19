using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour {

	Vector3 initialPosition;
	bool isPlayerHovering;
	public GameObject descriptionObject;
	void Start () {
		initialPosition = transform.position;
		isPlayerHovering = false;
	}
	public void ToggleHover (bool isOn) {
		if(isOn){
			if(descriptionObject!=null)
				descriptionObject.SetActive(true);
			if(!isPlayerHovering) {
				isPlayerHovering = true;
				StartCoroutine(WorkHover(isOn));
			}
		} else {
			if(descriptionObject!=null)
				descriptionObject.SetActive(false);
			if(isPlayerHovering) {
				isPlayerHovering = false;
				StartCoroutine(WorkHover(isOn));
			}
		}
	}

	IEnumerator WorkHover(bool isOn)
	{
		if(isOn)	
		while(isOn) {
			if(!isPlayerHovering)
				break;
			if(transform.position.x > initialPosition.x - 5){
				transform.position += new Vector3(-1,0,0);
			}
			yield return null;
		}
		else 
		while(transform.position.x < initialPosition.x){
			if(isPlayerHovering)
				break;
			transform.position += new Vector3(+1,0,0);
			yield return null;
		}
		yield return null;
	}
	
	
}
