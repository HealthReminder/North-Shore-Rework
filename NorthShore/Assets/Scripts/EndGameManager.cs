using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour {

	//State 
	public bool isBusy = false;
	//Win Conditions
	[Range(0.01f,1f)]
	public float maxPercentage;
	//Win
	public Image canvasWin, canvasLose;
	public GameManager gM;
	public int provinceQuantity;

	public void CheckGameState (List<AICurrentStats> players) {
		bool somethingHappened = false;
		foreach(AICurrentStats a in players) {
			if(!somethingHappened){
				print("Checking if "+a.name+" won with" +(float)a.provinces.Count/(float)provinceQuantity);
				if((float)a.provinces.Count/(float)provinceQuantity >= maxPercentage){
					if(a.name == gM.playerSO.name){
						somethingHappened = true;
						//Player won
						StartCoroutine(PlayerWin());
					} else {
						somethingHappened = true;
						//Player lost
						StartCoroutine(PlayerLose());
						SoundtrackManager.instance.ChangeSet("Intro");
					}
				}
			}
		}

		//If no one wons just keep going
		if(!somethingHappened)
			isBusy = false;
	}
	
	// Update is called once per frame
	public IEnumerator PlayerWin () {
		canvasWin.color += new Color(0,0,0,-1);
		canvasWin.gameObject.SetActive(true);
		while(canvasWin.color.a <1){
			canvasWin.color += new Color(0,0,0,0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		while(!Input.anyKey)

			yield return null;
		while(canvasWin.color.a >0){
			canvasWin.color += new Color(0,0,0,-0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(0);
		yield break;
	}
	
	public IEnumerator PlayerLose () {
		canvasLose.color += new Color(0,0,0,-1);
		canvasLose.gameObject.SetActive(true);
		while(canvasLose.color.a <1){
			canvasLose.color += new Color(0,0,0,0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		while(!Input.anyKey)

			yield return null;
		while(canvasLose.color.a >0){
			canvasLose.color += new Color(0,0,0,-0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(0);
		yield break;
	}
}
