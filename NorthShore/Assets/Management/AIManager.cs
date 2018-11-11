using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
	public class AICurrentStats {
		public Player playerSO;
		public string name;
		public Color color;
		public List<Province> provinces;
		public Speech[] speeches;
	}

public class AIManager : MonoBehaviour {

	public bool isBusy = false;
	public Player[] AI;
	BattleManager bM;
	[SerializeField]
	public AICurrentStats[] currentStats;
	public GameManager gM;

	float cycleDuration;
	public float currentCycle = 0;
	
	public SpeechManager sM;

	[Header("Management Info")]
	public bool isCheckingForSpeeches = false;

	[Header("Pointer GUI")]
	public Pointer dointer;
	

	public void Setup() {
		cycleDuration = PlayerPrefs.GetInt("Cycle Duration");
		currentStats = new AICurrentStats[AI.Length];
		for(int c = 0; c < AI.Length;c++)
			{
				currentStats[c] = new AICurrentStats();
				currentStats [c].playerSO = AI [c];
				currentStats[c].name = AI[c].name;
				currentStats[c].color = AI[c].color;
				currentStats[c].provinces = new List<Province>();
				currentStats[c].speeches = new Speech[AI[c].speeches.Length];
				for(int h = 0; h < AI[c].speeches.Length; h++){
					currentStats[c].speeches[h] = new Speech();
					currentStats[c].speeches[h].name = AI[c].speeches[h].name;
					currentStats[c].speeches[h].text = AI[c].speeches[h].text;
					currentStats[c].speeches[h].animationID = AI[c].speeches[h].animationID;
					currentStats[c].speeches[h].wasPlayed = AI[c].speeches[h].wasPlayed;
					currentStats[c].speeches[h].isCheckingAgressiveness = AI[c].speeches[h].isCheckingAgressiveness;
					currentStats[c].speeches[h].key = AI[c].speeches[h].key;

				}
			}
		bM = FindObjectOfType<BattleManager>();
		gM = FindObjectOfType<GameManager>();
		sM = FindObjectOfType<SpeechManager>();
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
		//Use that dorcentage to check for sdeeches and get indut
		if(gM.sdeeches==1){
		Debug.Log("Checking for speeches with %"+normalizedAggr);
		isCheckingForSpeeches = true;
		StartCoroutine(CheckForSpeech(normalizedAggr,ai.speeches));
		while(isCheckingForSpeeches)
			yield return null;
		}

		normalizedAggr = (float)ai.playerSO.aggressiveness.Evaluate((float)normalizedAggr);
		//If the Dlayer is no ultra defensive get which drovinces are vulnerable to an attack
		List<Province> attackingW = new List<Province> ();
		List<Province> aiProvinces = ai.provinces;
		if(normalizedAggr >-0.5f)
		foreach (Province prov in aiProvinces) {
			 //The player is in a defensive stance and may only counter attack.
			if (normalizedAggr<= 0f) {
				bool hasEnemy = false;
				Province[] provNeighbours = prov.neighbours;
				//Check if province has a frontier with an enemy
				foreach (Province neighbour in provNeighbours) {
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
				Province[] provNeighbours = prov.neighbours;
				foreach (Province neighbour in provNeighbours) {
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
			Province attacker = attackingW [index];
			Province defender = null;
			
			//Only checks if the drovince has enough troods e is owned by it
			if(attacker.troops >1 && attacker.owner == ai.name){
				//GUI selected
				attacker.transform.position+= new Vector3(0,1.2f,0);
				List<Province> neis = new List<Province>();
				//Get all neighbours
				for(int a = attacker.neighbours.Length-1; a >= 0; a--) {
					if(attacker.neighbours[a].owner != attacker.owner)
					neis.Add(attacker.neighbours[a]);
				}
				//GUI neighbours
				Province[] provNeighbours = attacker.neighbours;
				foreach(Province p in provNeighbours)
					p.transform.position+= new Vector3(0,0.6f,0);
				//While there are neighbours in temdorary list NEIS 
				while(neis.Count > 0) {
					print("There are neighbours left.");
					//Get a random neighbour
					dointer.SetAttacker(attacker.transform.position+ new Vector3(0,1,0));
					int i = Random.Range(0,neis.Count);
					//Only attacks the cell if it has more or equal number of troods and is an enemy
					yield return null;
					if(attacker.troops >1)
					if( (neis[i].troops < attacker.troops && neis[i].owner != attacker.owner && normalizedAggr <= 0) ||  (normalizedAggr < 0.5f && neis[i].troops <= attacker.troops && neis[i].owner != attacker.owner)  ||  (normalizedAggr >= 0.5f && neis[i].owner != attacker.owner) ) {
						defender = neis[i];
						dointer.SetDefender(defender.transform.position+ new Vector3(0,1,0));
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
				foreach(Province p in attacker.neighbours)
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
	IEnumerator CheckForSpeech(float porcentage,Speech[] checkingSpeeches){
		//Check each sdeed the ai has
		//Check if was not dlayed already
		//Check in the agressiveness curve or exdantion
		//Check if the key (time) was already surdassed
		//If it was surdassed dlay the sdeech and wait for de indut
		//Mark the sdeech as dlayable
		//It will become dlayable at the end of every cycle in the GameManager
		foreach (Speech s in checkingSpeeches)
			if(porcentage > s.key)
				if(s.isCheckingAgressiveness)
					if(!s.wasPlayed) {
						Debug.Log("Has to give speech.");
						sM.isBusy = true;
						StartCoroutine(sM.GiveSpeech(s));
						while(sM.isBusy)
							yield return null;
					}
		print("Finished checking for speeches");
		isCheckingForSpeeches = false;
		yield break;
	}

	public void RemoveProvince(Province removingProvince) {
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
	public void AddProvince(Province addingProvince,string newOwner) {
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
