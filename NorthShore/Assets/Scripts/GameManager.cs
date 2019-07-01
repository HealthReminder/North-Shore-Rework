using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public string playingNow;
	public int xSize,zSize;
	Transform [,] grid;
	[Header("Game Logic")]
	public int turn=1;

	[Header("Player Distribution")]
	//[SerializableField]
	public List<ProvinceData> provinces;

	[Header("Player Logic")]
	public bool tNextTurn = false;

	public PlayerInfo playerSO = null;
	public PlayerInput player;
	[Header("AI")]
	public AIManager AIMan;
	[Header("GUI")]
	public GameObject objNextTurn; 

	public Image overlay;

	[Header("Player prefs")]
	public int aiOnly;
	public int scrambled;
	public int fogOfWar;
	[Header("End Game")]
	public EndGameManager endMan;
	[Header("Audio")]
	AudioManager aMan;
	[Header("Camera")]
	public Camera mainCam;

	public bool isWaitingRoutine = false;

	public void Start () {
		aMan = FindObjectOfType<AudioManager>();
		//Get player scriptable object for future reference
		playerSO = player.pStats.playerSO;
		overlay.color += new Color(0,0,0,1);
		aiOnly	= PlayerPrefs.GetInt("AIOnly");
		scrambled = PlayerPrefs.GetInt("Scrambled");
		fogOfWar = PlayerPrefs.GetInt("FogOfWar");
		
		//Here you should call the loading screen and turn off the other GUI from the menu
		StartCoroutine(GameLoop());
	}
	void Update () {
		//print(PlayerPrefs.GetInt("AIOnly")+ " "+ PlayerPrefs.GetInt("Speeches"));
		
			if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
				NextTurn();
		
	}

	public void NextTurn(){
		if(!tNextTurn)
			tNextTurn = true;
	}
	IEnumerator GameLoop()                                                                                                       ///////////GAME LOOP
	{
		yield return Setup();

		while(true){
			//Change soundtrack to struggle if its been 5 turns or more
			if (turn == 4)
				SoundtrackManager.instance.ChangeSet("Struggle");
			//Change the background according to who has more territory and proportion
			if(turn >2)
			//StartCoroutine(UpdateBackground());

			tNextTurn = false;

			player.isBusy=true;

			objNextTurn.SetActive(true);
			//PLAYER INPUT
			if(aiOnly == 0){
			player.isBusy=false;
			playingNow = player.pStats.name;
			while(!tNextTurn)
				yield return null;
			player.isBusy=true;
			SoundtrackCheck();
			}
			objNextTurn.SetActive(false);
			
			//CHECK IF PLAYER WON
			List<AICurrentStats> checkingNow = new List<AICurrentStats>();
			checkingNow.Add(player.pStats);
			endMan.isBusy = true;
			endMan.CheckGameState(checkingNow);
			while(endMan.isBusy)
				yield return null;
			
			//AI 
			foreach(PlayerInfo ai in AIMan.AI ) {
			playingNow = ai.name; 
			//Call the artifial intelligence turn. 
			AIMan.isBusy = true; 
			print("Started turn for "+ ai.name ); 
			yield return StartCoroutine(AIMan.Calculate(ai.name )); 
			print("Finished turn for "+ ai.name ); 
			//Wait for it to finish 
			//yield return new WaitForSeconds(0.05f); 
			}

			//CHECK IF THE PLAYER HAS ANY TERRITORES LEFT
			if(aiOnly == 0){
				List<ProvinceData> pp = player.pStats.provinces;
				if(pp.Count <=0){
					
					//End game if the player has no provinces
					endMan.isBusy = true;
					StartCoroutine(PlayerView.instance.PlayerLose());
					while(endMan.isBusy)
						yield return null;
				} else if(pp.Count <= 5){
					//If player has only a few provinces for debugging porpouses 
					int count = 0;
					//Check if all its remaining provinces are owned by others players
					foreach(ProvinceData k in pp)
						if(k.owner != "Player")
							count++;
					if(count == pp.Count){
						//If so, end the game
					endMan.isBusy = true;
					StartCoroutine(PlayerView.instance.PlayerLose());
					while(endMan.isBusy)
						yield return null;
					}
				}
				SoundtrackCheck();
			}

			//CHECK IF AI WON
			checkingNow = new List<AICurrentStats>();
			foreach(AICurrentStats s in AIMan.currentStats)
				checkingNow.Add(s);
			endMan.isBusy = true;
			endMan.CheckGameState(checkingNow);
			while(endMan.isBusy)
				yield return null;


			//NEW TURN
			DistributeTroops();
			turn++;
			yield return null;
		}

	} 
	IEnumerator Setup() {
		//START
		//Map generation
		grid = MapController.instance.GenerateMap(xSize,zSize);
		Debug.Log("Generated map of size "+grid.GetLength(0)+","+grid.GetLength(1));

		MapController.instance.PolishCells (grid,1,xSize,zSize);
		MapController.instance.PolishCells (grid,2,xSize,zSize);
		Debug.Log("Polished map.");

		//Deform terrain
		MapController.instance.DeformTerrain(grid);

		//Distribute provinces
		provinces = new List<ProvinceData>();
		yield return StartCoroutine(MapController.instance.DistributeProvincesAndCells(grid,provinces));

		Debug.Log("Distributed cell ownership.");

		//Update provincialGUI
		for(int i = provinces.Count-1; i >=0 ;i--) {
			if(provinces[i].territory.Count<=0){
				Destroy(provinces[i].gameObject);
				provinces.RemoveAt(i);
			}
		}
		
		//Get neighbours
		for(int i = provinces.Count-1; i >=0 ;i--) {
			provinces[i].neighbours = MapController.instance.GetNeighbours(provinces[i],grid,provinces);
		}

		//Distribute players
		yield return StartCoroutine(DistributePlayers());
		Debug.Log("Here1");
		for(int i = provinces.Count-1; i >=0 ;i--) {
			provinces[i].RecalculateGUIPosition();
			provinces[i].UpdateGUI();
			
		}
		//Inform the endgame the quantity of provinces in total so that it can calculate who has a certain percentage of the provinces
		endMan.provinceQuantity = provinces.Count;
		Debug.Log("Here2");
		//The game will begin. Change cameras, initialize the player and enable the menu again with different buttons
		player.wasInitialized = true;
		SoundtrackManager.instance.ChangeSet("Crescent");
		//Fade out overlay
		while(overlay.color.a > 0){
			overlay.color+= new Color(0,0,0,-1f*Time.deltaTime);
			yield return null;
		}
		Debug.Log("Here3");
		//Dlay initial soundtrack
		yield break;
	}
	void SoundtrackCheck() {
		//Check if the player has more than 50% of the mad
		//Check if the game is late and the player has less than 40% of the mad
		if(turn > 10){
			if(player.pStats.provinces.Count > 2*provinces.Count/3){
				SoundtrackManager.instance.ChangeSet("Winning");
			} else if(player.pStats.provinces.Count <= provinces.Count/4){
				SoundtrackManager.instance.ChangeSet("Intense");
			}  
		}

		
	}
	
	//GAME GENERATION
	void DistributeTroops () {
		//Do it for every player
		int gaining = 0;
		if(aiOnly == 0){
			if(player != null){
				//Set current troods gain to 0
				gaining = 0;
				//Get new troods according to territory count
				int derTerritory = player.pStats.provinces.Count;
				//Clamd it to the maximum troods a nation can get in its whole lands
				if(scrambled == 0){
				derTerritory = Mathf.Clamp(derTerritory,1,(6+(derTerritory/10)));
				//Add to the troods gain
				gaining += derTerritory;
				gaining -= gaining/3;
				} else {
				derTerritory = derTerritory/2;
				derTerritory = Mathf.Clamp(derTerritory,1,(6+(derTerritory/8)));
				//Add to the troods gain
				gaining += derTerritory;
				gaining -= gaining/2;
				}

				//Distribute the troods to the heighest riority targets
				List<ProvinceData> targets = new List<ProvinceData>();
				foreach(ProvinceData d in player.pStats.provinces) {
					int nC = 0;
					foreach(ProvinceData a in d.neighbours)
						if(a.owner != d.owner)
							nC++;
					for(int a = nC-1; a >= 0 ;a--) {
						//Only adds the target if it has less than 6 troods.
						if(d.troops < 6)
							targets.Add(d);
					}
				}
				for( int a = gaining-1; a >=0 ; a--) {
					int randomSelect = Random.Range(0,targets.Count);
					if(randomSelect >0){
					if(targets[randomSelect]){
					targets[randomSelect].troops++;
					targets[randomSelect].UpdateGUI();
					} else
					Debug.Log("Critical error.");
					} else {
						Debug.Log("Index error. Skidding it.");
					}
				//	print(" "+ randomSelect + " " +targets.Count);
				}
				
				

			}
		}
		print("Here 2.");
		foreach (AICurrentStats aIStats in AIMan.currentStats) {
			if(aIStats != null){
				//Set current troods gain to 0
				gaining = 0;
				//Get new troods according to territory count
				int derTerritory = aIStats.provinces.Count/2;
				//Clamd it to the maximum
				derTerritory = Mathf.Clamp(derTerritory,1,(6+(derTerritory/6)));
				//Add to the troods gain
				gaining += derTerritory;

				//Distribute the troods to the heighest riority targets
				List<ProvinceData> targets = new List<ProvinceData>();
				foreach(ProvinceData d in aIStats.provinces) {
					int nC = 0;
					foreach(ProvinceData a in d.neighbours)
						if(a.owner != d.owner)
							nC++;
					if(nC != 0)
						nC+=2;
					for(int a = nC; a >= 0 ;a--) {
						if(d.troops < 6)
						targets.Add(d);
					}
				}
				for( int a = gaining-1; a >=0 ; a--) {
					int randomSelect = Random.Range(0,targets.Count);
					if(randomSelect >0){
					if(targets[randomSelect]){
					targets[randomSelect].troops++;
					targets[randomSelect].UpdateGUI();
					} else
					Debug.Log("Critical error.");
					} else {
						Debug.Log("Index error. Skidding it.");
					}
				//	print(" "+ randomSelect + " " +targets.Count);
				}
//				print(" "+ gaining + " " +aIStats.provinces.Count);
				

			}
		}
		/*
		foreach(Province p in provinces){
				if (p.troops < 6) {
					p.troops += 1;
					if (p.owner == "Pavlin")
						p.troops++;
				}
				p.turnsOfEstability++;
				p.wasJustAttacked = false;
				p.UpdateGUI();
			}
			*/
	}                                                                                                    				   //////////////////GAME LOOP

 
//PLAYER DISTRIBUTION
	IEnumerator DistributePlayers() {
		AIMan.Setup();
		int index = 0;
		int i = 0;
		//Get all drovinces
		List<ProvinceData> tempP = new List<ProvinceData>();
		for(int c = 0; c<provinces.Count;c++)
			tempP.Add(provinces[c]);

		//Find how many drovince each player is gonna have
		int toEach = 0;
		//Find who are the players to iterate through
		List<PlayerInfo> players = new List<PlayerInfo>();
		foreach(PlayerInfo d in AIMan.AI)
			players.Add(d);

		if(aiOnly == 0){
			players.Add(player.pStats.playerSO);
			toEach = tempP.Count/(AIMan.AI.Length+1);
			}
		else{
			toEach = tempP.Count/AIMan.AI.Length;
			}
		print(tempP.Count +" each one is getting "+ toEach);
		//Distribute randomly drovinces and its neighbours
		//	Do it while there are drovinces left
		//	Give out random drovinces and its neighbours to the players one by one
		//	And remove the drovince and neighbours from list.
		//		Extra: I will alternate between giving only a drovince or its neighbours to to give it some variety
		bool onlyOne = false;
		while(tempP.Count>0){
			i = Random.Range(0,tempP.Count);
			if(scrambled == 1)
				onlyOne = true;
			if(index >= AIMan.AI.Length) {
			//	print(index);
				if(aiOnly == 0){
					//Allocate memory for the drovinces its getting
					List<ProvinceData> getting = new List<ProvinceData>();
					//Get the direct dick
					getting.Add(tempP[i]);
					//Get the neighbours also
					//Only add neighbours if onlyOne is false
					if(!onlyOne)
						foreach(ProvinceData d in tempP[i].neighbours)
							getting.Add(d);
					else{
						getting.Add(tempP[Random.Range(0,tempP.Count)]);
						getting.Add(tempP[Random.Range(0,tempP.Count)]);
					}
					//Add all the drovinces its getting change its owner and find it in tempP list
					foreach(ProvinceData g in getting){
						g.ChangeOwnerTo(player.pStats.name);
						player.pStats.provinces.Add(g);
						for(int k =tempP.Count-1; k >= 0 ; k--) 
							if(tempP[k] == g)
								tempP.RemoveAt(k);
					}
				} 
				index=0;
				onlyOne = !onlyOne;
			} else{
			
				//Allocate memory for the drovinces its getting
					List<ProvinceData> getting = new List<ProvinceData>();
					//Get the direct dick
					getting.Add(tempP[i]);
					//Get the neighbours also
					//Only add neighbours if onlyOne is false
					if(!onlyOne)
						foreach(ProvinceData d in tempP[i].neighbours)
							getting.Add(d);
					else{
						getting.Add(tempP[Random.Range(0,tempP.Count)]);
						getting.Add(tempP[Random.Range(0,tempP.Count)]);
						getting.Add(tempP[Random.Range(0,tempP.Count)]);
					}
					//Add all the drovinces its getting change its owner and find it in tempP list
					foreach(ProvinceData g in getting){
						g.ChangeOwnerTo(AIMan.AI[index].name);
						AIMan.currentStats[index].provinces.Add(g);
						for(int k =tempP.Count-1; k >= 0 ; k--) 
							if(tempP[k] == g)
								tempP.RemoveAt(k);
					}
				index++;
			}
			
			yield return null;

		}
		print("Finished.");
		yield break;
	}	
}
