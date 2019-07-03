using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public static GameController instance;
	private void Awake() {
		instance = this;
	}
	public IEnumerator Battle(ProvinceData attacker, ProvinceData defender) {
		
		//Player shots if visible
		if(defender.isAdjacentToPlayer){
			AudioManager.instance.PlayTrack("Player_Shots");
			yield return new WaitForSeconds(0.1f);
		}

		//Calculate dice rolls
		int attackerValue, defenderValue;
		attackerValue = defenderValue = 0;
		for(int a = 0; a < attacker.troops; a++){
			int value = Random.Range(1,7);
			attackerValue+= value;
		}
		for(int g = 0; g < defender.troops;g++){
			int value = Random.Range(1,7);
			defenderValue+= value;
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
			if(GameManager.instance.playerManager.playerData.playerInfo == defender.owner){
				//Debug.Log("The player is the loser");
				List<ProvinceData> playerProvinces = GameManager.instance.playerManager.playerData.provinces;
				for(int f = playerProvinces.Count-1; f>=0; f--) {
					if(playerProvinces[f] == defender){
						//Debug.Log("Removed province from the player.");
						playerProvinces.RemoveAt(f);
					}else	Debug.Log("ERROR - Couldn't find the losing province owned by the player");
				}
			} else {
				AIManager.instance.RemoveProvince(defender);	
			}
			//Give the province to another owner
			if(GameManager.instance.playerManager.playerData.playerInfo.name == attacker.owner.name){
			//	Debug.Log("The player is the winner");
				GameManager.instance.playerManager.playerData.provinces.Add(defender);
			} else {
				AIManager.instance.AddProvince(defender,attacker.owner.name);
			}
			
			defender.troops = attacker.troops-1;
			defender.troops= Mathf.Clamp(defender.troops,1,99);
			attacker.troops = Mathf.Clamp(attacker.troops,1,99);
			attacker.troops = 1;

			defender.ChangeOwnerTo(attacker.owner);
			yield return null;
			defender.UpdateGUI();
			attacker.UpdateGUI();
			foreach(ProvinceData h in defender.neighbours)
				h.UpdateGUI();
			//print("Attacker won.");
		}
		defender.turnsOfEstability = 0;
		PlayerView.instance.Invoke("ClearBattleGUI",0.2f);
		yield break;
		
	}
	
}
