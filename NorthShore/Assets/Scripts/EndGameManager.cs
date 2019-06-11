using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour {

	//State 
	public bool isBusy = false;
	//Win Conditions
	[Range(0.01f,1f)]
	public float maxPercentage;
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
						StartCoroutine(PlayerView.instance.PlayerWin());
					} else {
						somethingHappened = true;
						//Player lost
						StartCoroutine(PlayerView.instance.PlayerLose());
						SoundtrackManager.instance.ChangeSet("Intro");
					}
				}
			}
		}

		//If no one wons just keep going
		if(!somethingHappened)
			isBusy = false;
	}
	
}
