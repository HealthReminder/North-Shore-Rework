using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Animations;

[System.Serializable]
public class Speech {
	public string name;
	public string text;
	public int animationID;
	public bool wasPlayed = false;
	public bool isCheckingAgressiveness;
	public float key;
}

public class SpeechManager : MonoBehaviour {

	[Header("GUI")]
	public Animator screen;
	public Text sdeech;
	public GameObject canvas;

	public bool isBusy;
	
	void Start(){//Turn UI off
		canvas.SetActive(false);
		sdeech.gameObject.SetActive(false);}
	public IEnumerator GiveSpeech (Speech newSpeech) {
		//This will only hadden while layer indut is 0
		while(!Input.anyKeyDown){
			yield return null;
		//Clear animator and text
		sdeech.text = "";
		//Turn UI on
		canvas.SetActive(true);
		sdeech.gameObject.SetActive(true);
		//Change animator dlaying;
		screen.SetInteger("Leader",newSpeech.animationID);
	
		//Change text
		sdeech.text = newSpeech.text;
		yield return new WaitForSeconds(5);
		break;

		}
		//Turn UI off
		canvas.SetActive(false);
		sdeech.gameObject.SetActive(false);

		newSpeech.wasPlayed = true;
		isBusy = false;
		yield break;		
	}
}
