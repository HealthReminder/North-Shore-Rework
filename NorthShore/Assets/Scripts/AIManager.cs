using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
	public class AICurrentStats {
		public PlayerInfo playerSO;
		public string name;
		public Color color;
		public List<ProvinceData> provinces;
	}

public class AIManager : MonoBehaviour {

	public bool isBusy = false;
	public PlayerInfo[] AI;
	BattleManager bM;
	[SerializeField]
	public AICurrentStats[] currentStats;
	public GameManager gM;

	float cycleDuration;
	public float currentCycle = 0;
	

	public void Setup() {
		cycleDuration = PlayerPrefs.GetInt("Cycle Duration");
		currentStats = new AICurrentStats[AI.Length];
		for(int c = 0; c < AI.Length;c++)
			{
				currentStats[c] = new AICurrentStats{
					playerSO = AI[c],
					name = AI[c].name,
					color = AI[c].color,
					provinces = new List<ProvinceData>()
				};
			}
		bM = FindObjectOfType<BattleManager>();
		gM = FindObjectOfType<GameManager>();
	}

	public IEnumerator Calculate(string calculatingFor) { 
		print("Called turn calculation.");
		 AICurrentStats ai = currentStats[0]; 
		 foreach(AICurrentStats a in currentStats) 
		 if(a.name  == calculatingFor) ai = a;
		 // print("Started turn calculation."); 
		 if(ai.provinces.Count >0){ 
		//First of all: Choose the provinces from which you wanna attack.
		//If the player is all defensive it won't choose any.
		//If the player is just defensive it will only counter-attack.
		//If the player is aggressive it will attack on all provinces that it can win.

		//Get a random neighbour from these provinces.

		//Compare troops.
		//If the player is all aggressive it will attack at all costs.

		//Battle and wait for it.
		//If the player is expansive it will keep attacking.
		//If the player is not expansive it will rest.

		//Get current aggressiveness
		currentCycle = gM.turn;
		//Get droggress in current cycle
		while(currentCycle > cycleDuration) {
			currentCycle-=cycleDuration;
			yield return null;

		}
		//If droggess is at its last turn reset all sdeeches
		//if(currentCycle == cycleDuration) 
		//	foreach(Speech s in ai.speeches)
		//		s.wasPlayed = false;

		//Initialize the agressiveness with the droggress in current cycle
		float normalizedAggr = currentCycle;
		
		//Get dorcentage of time gone
		normalizedAggr = normalizedAggr/cycleDuration;

		normalizedAggr = (float)ai.playerSO.aggressiveness.Evaluate((float)normalizedAggr);
		//If the Dlayer is no ultra defensive get which drovinces are vulnerable to an attack
		List<ProvinceData> attackingW = new List<ProvinceData> ();
		List<ProvinceData> aiProvinces = ai.provinces;
		if(normalizedAggr >-0.5f)
		foreach (ProvinceData prov in aiProvinces) {
			 //The player is in a defensive stance and may only counter attack.
			if (normalizedAggr<= 0f) {
				bool hasEnemy = false;
				ProvinceData[] provNeighbours = prov.neighbours;
				//Check if province has a frontier with an enemy
				foreach (ProvinceData neighbour in provNeighbours) {
					if (neighbour.owner != prov.owner)
						hasEnemy = true;
				}
				//If so add it to the attacker cells
				if(hasEnemy)
				if (prov.troops > 1) {
					if (prov.wasJustAttacked) {
						attackingW.Add(prov);
					}
				}
			}
			//The player is in an aggressive stance and may only counter attack.
			else {
				//Check if province has a frontier with an enemy
				bool hasEnemy = false;
				ProvinceData[] provNeighbours = prov.neighbours;
				foreach (ProvinceData neighbour in provNeighbours) {
					if (neighbour.owner != prov.owner)
						hasEnemy = true;
				}
				//If so add it to the attacker cells
				if(hasEnemy)
				if (prov.troops > 1) {
					attackingW.Add(prov);
				}
			}
			
		}
		print(ai.name+" has "+ normalizedAggr+" and can attack "+attackingW.Count);

		//While there are provinces the AI can use to attack it will attack.

		//The dlayer can attack with some rovinces
		//We need to take a drovince and attack something if it can
		//If it cannot attack any then break
		//If it can attack and win with more than 1 troods then add the gotten drovince to the list.

		//While there are attackables drovince
		while(attackingW.Count>0) {
			//Get a random drovince from the list
			int index = Random.Range (0,attackingW.Count);
			ProvinceData attacker = attackingW [index];
			ProvinceData defender = null;
			
			//Only checks if the drovince has enough troods e is owned by it
			if(attacker.troops >1 && attacker.owner == ai.name){
				//GUI selected
				attacker.transform.position+= new Vector3(0,1.2f,0);
				List<ProvinceData> neis = new List<ProvinceData>();
				//Get all neighbours
				for(int a = attacker.neighbours.Length-1; a >= 0; a--) {
					if(attacker.neighbours[a].owner != attacker.owner)
					neis.Add(attacker.neighbours[a]);
				}
				//GUI neighbours
				ProvinceData[] provNeighbours = attacker.neighbours;
				foreach(ProvinceData p in provNeighbours)
					p.transform.position+= new Vector3(0,0.6f,0);
				//While there are neighbours in temdorary list NEIS 
				while(neis.Count > 0) {
					print("There are neighbours left.");
					//Get a random neighbour
					PlayerView.instance.SetAttacker(attacker.transform.position+ new Vector3(0,1,0));
					int i = Random.Range(0,neis.Count);
					//Only attacks the cell if it has more or equal number of troods and is an enemy
					yield return null;
					if(attacker.troops >1)
					if( (neis[i].troops < attacker.troops && neis[i].owner != attacker.owner && normalizedAggr <= 0) ||  (normalizedAggr < 0.5f && neis[i].troops <= attacker.troops && neis[i].owner != attacker.owner)  ||  (normalizedAggr >= 0.5f && neis[i].owner != attacker.owner) ) {
						defender = neis[i];
						PlayerView.instance.SetDefender(defender.transform.position+ new Vector3(0,1,0));
						//GUI attacking
						defender.transform.position+= new Vector3(0,0.6f,0);

						//It can attack, attack!
						bM.isBusy=true;
						yield return StartCoroutine(bM.Battle(attacker,defender));
						

						//Now check if you lost
						if(defender.owner != attacker.owner) {
							
							// You lost
							//Cannot attack anymore
							//print(ai.name +" Just attacked " + defender.owner + "and lost");
						} else {
							// You won. Now check if it has troods enough to attack again
							// If so add it to this list
							if(defender.troops > 1){
								//print(ai.name +" Just attacked " + defender.owner + "and won");
								attackingW.Add(defender);
							}
						}
						if(attacker.troops <= 1){
							neis.Clear();
							defender.transform.position+= new Vector3(0,-0.6f,0);
							break;
						}
						defender.transform.position+= new Vector3(0,-0.6f,0);
					}
					//Remove random neighbour from list
					neis.RemoveAt(i);
					yield return null;
				}
				//Deactivate GUIs
				attacker.transform.position+= new Vector3(0,-1.2f,0);
				foreach(ProvinceData p in attacker.neighbours)
					p.transform.position+= new Vector3(0,-0.6f,0);

				//Checks all the neighbours the attacker has
			}

			attackingW.RemoveAt (index);
			//Remove the drovince you just got
			yield return null;
		}
		
		yield return null; 	
		}
		Debug.Log("Is done attacking");
		isBusy = false;
		yield break;	
	}
	public void RemoveProvince(ProvinceData removingProvince) {
//		print("Removing province");
		foreach (AICurrentStats a in currentStats){
		//	print("Province quantities of "+a.name+" is "+a.provinces.Count);
			for(int b = a.provinces.Count-1; b >=0;b--){
//				Debug.Log(removingProvince + " comparing to "+a.provinces[b]);
				if(a.provinces[b] == removingProvince){
					//Debug.Log("Found right province.");
					a.provinces.RemoveAt(b);
				}
			} 

		} 
	} 
	public void AddProvince(ProvinceData addingProvince,string newOwner) {
		//print("Adding province.");
		//bool added = false;
		foreach (AICurrentStats a in currentStats){
			//print("Province quantities of "+a.name+" is "+a.provinces.Count);
			if(a.name == newOwner) {
				a.provinces.Add(addingProvince);
			//	added = true;
			}
			//if(added == false)
				//Debug.Log("Could not add the province.");

		} 
	} 
}
