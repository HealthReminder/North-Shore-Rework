using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour {

	public PlayerInfo[] AI;
	[SerializeField]
	public PlayerData[] currentStats;

	public float current_turn = 0;

	public static AIManager instance;
	private void Awake () {
		instance = this;
	}
	public void Setup () {
		currentStats = new PlayerData[AI.Length];
		for (int c = 0; c < AI.Length; c++)
			currentStats[c] = new PlayerData {
				playerInfo = AI[c],
					provinces = new List<ProvinceData> ()
			};

	}

	public IEnumerator Calculate (PlayerInfo currentAIInfo, int current_turn, int cycle_duration) {
		print ("Called turn calculation.");
		PlayerData ai = currentStats[0];
		foreach (PlayerData a in currentStats)
			if (a.playerInfo == currentAIInfo)
				ai = a;

		//First of all: Choose the provinces from which you wanna attack.
		//If the player is all defensive it won't choose any.
		//If the player is just defensive it will only counter-attack.
		//If the player is aggressive it will attack on all provinces that it can win.
		if (ai.provinces.Count > 0) {

			//Get a random neighbour from these provinces.

			//Compare troops.
			//If the player is all aggressive it will attack at all costs.

			//Battle and wait for it.
			//If the player is expansive it will keep attacking.
			//If the player is not expansive it will rest.

			//Get proggress in current cycle
			while (current_turn > cycle_duration)
				current_turn -= cycle_duration;

			//Initialize the agressiveness with the progress in current cycle
			//Get porcentage of time gone
			float agressiveness = current_turn;
			agressiveness = agressiveness / cycle_duration;
			agressiveness = (float) ai.playerInfo.aggressiveness.Evaluate ((float) agressiveness);
			//If the Player is not ultra defensive get which provinces are vulnerable to an attack
			List<ProvinceData> attackingW = new List<ProvinceData> ();
			List<ProvinceData> aiProvinces = ai.provinces;
			//is ULTRA DEFENSIVE test
			if (agressiveness > -0.5f)
				foreach (ProvinceData prov in aiProvinces) {
					//The player is in a defensive stance and may only counter attack.
					if (agressiveness <= 0f) {
						bool hasEnemy = false;
						ProvinceData[] provNeighbours = prov.neighbours;
						//Check if province has a frontier with an enemy
						foreach (ProvinceData neighbour in provNeighbours) {
							if (neighbour.owner != prov.owner)
								hasEnemy = true;
						}
						//If so add it to the cells it is counter attacking
						if (hasEnemy)
							if (prov.troops > 1) {
								if (prov.wasJustAttacked) {
									attackingW.Add (prov);
								}
							}
					}
					//AGGRESSIVE ISTANCE
					//The player is in an aggressive stance and may attack.
					else {
						//Check if province has a frontier with an enemy
						bool hasEnemy = false;
						ProvinceData[] provNeighbours = prov.neighbours;
						foreach (ProvinceData neighbour in provNeighbours) {
							if (neighbour.owner != prov.owner)
								hasEnemy = true;
						}
						//If so add it to the cells it is attacking
						if (hasEnemy)
							if (prov.troops > 1) {
								attackingW.Add (prov);
							}
					}

				}
			//print(ai.playerInfo.name+" has "+ normalizedAggr+" and can attack "+attackingW.Count);

			//While there are provinces the AI can use to attack it will attack.

			//The dlayer can attack with some rovinces
			//We need to take a drovince and attack something if it can
			//If it cannot attack any then break
			//If it can attack and win with more than 1 troods then add the gotten drovince to the list.

			//While there are attackables drovince
			int index;
			while (attackingW.Count > 0) {
				//Get a random drovince from the list
				index = Random.Range (0, attackingW.Count);
				ProvinceData attacker = attackingW[index];
				ProvinceData defender = null;

				//Only checks if the drovince has enough troods e is owned by it
				if (attacker.troops > 1 && attacker.owner == ai.playerInfo) {
					//GUI selected
					attacker.transform.position += new Vector3 (0, 1.2f, 0);
					List<ProvinceData> neis = new List<ProvinceData> ();
					//Get all neighbours
					for (int a = attacker.neighbours.Length - 1; a >= 0; a--) {
						if (attacker.neighbours[a].owner != attacker.owner)
							neis.Add (attacker.neighbours[a]);
					}
					//GUI neighbours
					ProvinceData[] provNeighbours = attacker.neighbours;
					foreach (ProvinceData p in provNeighbours)
						p.transform.position += new Vector3 (0, 0.6f, 0);
					//While there are neighbours in temdorary list NEIS 
					int i;
					while (neis.Count > 0) {
						print ("There are neighbours left.");
						//Get a random neighbour
						PlayerView.instance.SetAttacker (attacker.transform.position + new Vector3 (0, 2, 0));
						i = Random.Range (0, neis.Count);
						//Only attacks the cell if it has more or equal number of troods and is an enemy
						yield return null;
						if (attacker.troops > 1)
							if ((neis[i].troops < attacker.troops && neis[i].owner != attacker.owner && agressiveness <= 0) || (agressiveness < 0.5f && neis[i].troops <= attacker.troops && neis[i].owner != attacker.owner) || (agressiveness >= 0.5f && neis[i].owner != attacker.owner)) {
								defender = neis[i];
								PlayerView.instance.SetDefender (defender.transform.position + new Vector3 (0, 2, 0));
								//GUI attacking
								defender.transform.position += new Vector3 (0, 0.6f, 0);
								yield return StartCoroutine (GameController.instance.Battle (attacker, defender));
								//Now check if you lost
								if (defender.owner != attacker.owner) {
									// You lost
									//Cannot attack anymore
									//print(ai.name +" Just attacked " + defender.owner + "and lost");
								} else {
									// You won. Now check if it has troods enough to attack again
									// If so add it to this list
									BattleGUIManager.instance.ShowAttack (attacker.transform.position + Vector3.up * 2, defender.transform.position + Vector3.up * 2, attacker.owner.color);
									if (defender.troops > 1) {
										//print(ai.name +" Just attacked " + defender.owner + "and won");
										attackingW.Add (defender);
									}
								}
								if (attacker.troops <= 1) {
									neis.Clear ();
									defender.transform.position += new Vector3 (0, -0.6f, 0);
									break;
								}
								defender.transform.position += new Vector3 (0, -0.6f, 0);
							}
						//Remove random neighbour from list
						neis.RemoveAt (i);
						yield return null;
					}
					//Deactivate GUIs
					attacker.transform.position += new Vector3 (0, -1.2f, 0);
					foreach (ProvinceData p in attacker.neighbours)
						p.transform.position += new Vector3 (0, -0.6f, 0);

					//Checks all the neighbours the attacker has
				}

				attackingW.RemoveAt (index);
				//Remove the drovince you just got
				yield return null;
			}

			yield return null;
		}
		Debug.Log ("Is done attacking");
		yield break;
	}
	public void RemoveProvince (ProvinceData removingProvince) {
		//		print("Removing province");
		foreach (PlayerData a in currentStats) {
			//	print("Province quantities of "+a.name+" is "+a.provinces.Count);
			for (int b = a.provinces.Count - 1; b >= 0; b--) {
				//				Debug.Log(removingProvince + " comparing to "+a.provinces[b]);
				if (a.provinces[b] == removingProvince) {
					//Debug.Log("Found right province.");
					a.provinces.RemoveAt (b);
				}
			}

		}
	}
	public void AddProvince (ProvinceData addingProvince, string newOwner) {
		//print("Adding province.");
		//bool added = false;
		foreach (PlayerData a in currentStats) {
			//print("Province quantities of "+a.name+" is "+a.provinces.Count);
			if (a.playerInfo.name == newOwner) {
				a.provinces.Add (addingProvince);
				//	added = true;
			}
			//if(added == false)
			//Debug.Log("Could not add the province.");

		}
	}
}