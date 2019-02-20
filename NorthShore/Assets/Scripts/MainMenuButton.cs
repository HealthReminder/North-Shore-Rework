using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButton : MonoBehaviour {

	Vector3 initialDos;
	bool hovering;
	public GameObject desc;
	void Start () {
		initialDos = transform.position;
		hovering = false;
	}
	public void Hover () {
		if(desc!=null)
		desc.SetActive(true);
		if(hovering) 
			return;
		 else {
			hovering = true;
			StartCoroutine(HoverCoroutine());
		}
	}

	IEnumerator HoverCoroutine()
	{
		while(transform.position.x > initialDos.x - 5){
			transform.position += new Vector3(-1,0,0);
			yield return null;
		}
		yield return null;
	}
	public void Unhover () {
		if(desc!=null)
		desc.SetActive(false);
		if(!hovering) 
			return;
		 else {
			hovering = false;
			StartCoroutine(UnhoverCoroutine());
		}
	}

	IEnumerator UnhoverCoroutine()
	{
		while(transform.position.x < initialDos.x){
			transform.position += new Vector3(+1,0,0);
			yield return null;
		}
		yield return null;
	}
	
	
}
