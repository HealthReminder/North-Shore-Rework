using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BattleManager : MonoBehaviour {

	public GameManager gM;
	public AudioManager aMan;

	public GameObject battleCanvas;
	public Text defenderName,defenderDice,defenderTotal,attackerName,attackerDice,attackerTotal;
	public bool isBusy = false;
	public Pointer dointer;
	public bool ignoreUI = true;
	public bool isFastMode = false;

	void Awake()
	{
		aMan = FindObjectOfType<AudioManager>();
	}

	public IEnumerator Battle(Province attacker, Province defender) {
		defenderName.text = defender.owner;
		attackerName.text = attacker.owner;
		defenderTotal.color = new Color(1,1,1,1);
		attackerTotal.color = new Color(1,1,1,1);
		//Declare and clear values
		int attackerValue, defenderValue;
		attackerValue = defenderValue = 0;
		//Clear dice text
		attackerDice.text = defenderDice.text = defenderTotal.text = attackerTotal.text = "";
		//Calculate dice rolls
		//if(attacker.owner == "Player")
		if(defender.isAdjenctToDlayer){
			aMan.PlayTrack("warSounds");
		yield return new WaitForSeconds(0.15f);}

		if(!isFastMode && attacker.isAdjenctToDlayer)
		battleCanvas.SetActive(true);
		string rightText;
		int fakeNumbersIteration = 10;
		yield return null;
		for(int a = 0; a < attacker.troops; a++){
			int value = Random.Range(1,7);
			rightText = attackerDice.text;
			if(!isFastMode&& attacker.isAdjenctToDlayer)
			for(int m = 0; m < fakeNumbersIteration;m++){
				if(attackerDice.text == "")
				attackerDice.text = Random.Range(0,7).ToString();
				else
				attackerDice.text = rightText+" + "+Random.Range(0,7);
				//yield return new WaitForSeconds(0.02f);
			}
			
			//Add dice roll text
			if(attackerDice.text == "")
				attackerDice.text =value.ToString();
			else
				attackerDice.text =  rightText+" + "+value.ToString();
			attackerValue+= value;
			//Change total text
			attackerTotal.text = attackerValue.ToString();

		}
		for(int g = 0; g < defender.troops;g++){
			int value = Random.Range(1,7);
			rightText = defenderDice.text;
			if(!isFastMode&& attacker.isAdjenctToDlayer)
			for(int m = 0; m < fakeNumbersIteration;m++){
				if(attackerDice.text == "")
				defenderDice.text = Random.Range(0,7).ToString();
				else
				defenderDice.text = rightText+" + "+Random.Range(0,7);
				//yield return new WaitForSeconds(0.02f);

			}
			//Add dice roll text
			if(defenderDice.text == "")
				defenderDice.text =value.ToString();
			else
				defenderDice.text =  rightText+" + "+value.ToString();
			defenderValue+= value;
			//Change total text
			defenderTotal.text = defenderValue.ToString();
		}
		

		//Check who wins
		if(defenderValue >= attackerValue){
			//If the defender wins, the attacker loses all of its troops
			if(defender.troops>1)
				defender.troops -=1;
			attacker.troops = 1;
			defender.wasJustAttacked = true;

			defender.UpdateGUI();
			attacker.UpdateGUI();
		} else {
			//If the attacker wins, the defender loses its spot. 
			//The attacking troops moves to the defending spot with one less troop, leaving one behind.
			//Take the province away from the owner
			if(gM.player.pStats.name == defender.owner){
				//Debug.Log("The player is the loser");
				for(int f = gM.player.pStats.provinces.Count-1; f>=0; f--) {
					if(gM.player.pStats.provinces[f] == defender){
						//Debug.Log("Removed province from the player.");
						gM.player.pStats.provinces.RemoveAt(f);
					}else
						Debug.Log("Couldn't find the losing province owned by the player");
					}
			} else {
				gM.AIMan.RemoveProvince(defender);
						
			}
			yield return null;
			//Give the province to another owner
			if(gM.player.pStats.name == attacker.owner){
			//	Debug.Log("The player is the winner");
				gM.player.pStats.provinces.Add(defender);
			} else {
				gM.AIMan.AddProvince(defender,attacker.owner);
			}
			
			defender.troops = attacker.troops-1;
			defender.troops= Mathf.Clamp(defender.troops,1,99);
			attacker.troops = Mathf.Clamp(attacker.troops,1,99);
			attacker.troops = 1;

			defender.ChangeOwnerTo(attacker.owner);
			yield return null;
			defender.UpdateGUI();
			attacker.UpdateGUI();
			foreach(Province h in defender.neighbours)
				h.UpdateGUI();
			//print("Attacker won.");
		}
		defender.turnsOfEstability = 0;
		
		
	//	print("Updated UI.");

		
		if(!isFastMode && attacker.isAdjenctToDlayer)
			yield return new WaitForSeconds(0.1f);

		//while(!Input.anyKey)
		//	yield return null;
		dointer.Clear();
		
		battleCanvas.SetActive(false);
//		print("Done.");
		isBusy=false;
		yield break;
		
	}


}
