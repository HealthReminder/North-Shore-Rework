using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] public struct WinCondition{
	[Range(0.01f,1f)]
	public float maxPercentage;

}

public class GameManager : MonoBehaviour {
	[SerializeField] public WinCondition winCondition;

	public string playingNow;
	public int xSize,zSize;
	Transform [,] grid;
	[Header("Game Logic")]
	public int turn=1;

	[Header("Player Distribution")]
	//[SerializableField]
	public List<ProvinceData> provinces;

	[Header("Player Logic")]
	public bool nextTurnPlayerInput = false;

	public PlayerInfo playerSO = null;
	public PlayerManager playerManager;

	[Header("AI")]
	public AIManager AIMan;

	[Header("Player prefs")]
	public int aiOnly;
	public int scrambled;
	public int fogOfWar;
	[Header("Audio")]
	AudioManager aMan;
	[Header("Camera")]
	public Camera mainCam;

	public bool isWaitingRoutine = false;

	public void Start () {
		aMan = FindObjectOfType<AudioManager>();
		//Get player scriptable object for future reference
		playerSO = playerManager.pStats.playerSO;
		PlayerView.instance.overlay.color += new Color(0,0,0,1);
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
		if(!nextTurnPlayerInput)
			nextTurnPlayerInput = true;
	}
	IEnumerator GameLoop()                                                                                                       ///////////GAME LOOP
	{
		yield return Setup();

		while(true){
			//PLAYER TURN
			nextTurnPlayerInput = false;
			playerManager.isBusy=true;
			if(aiOnly == 0){
				PlayerView.instance.OnPlayerTurn("OnStart");
				playerManager.isBusy=false;
				playingNow = playerManager.pStats.name;
				while(!nextTurnPlayerInput)
					yield return null;
				playerManager.isBusy=true;
				SoundtrackCheck();
				PlayerView.instance.OnPlayerTurn("OnEnd");
			}
			yield return CheckGameState(AIMan.currentStats);
			//AI TURN
			foreach(PlayerInfo ai in AIMan.AI ) {
				playingNow = ai.name; 
				yield return StartCoroutine(AIMan.Calculate(ai.name )); 
			}

			yield return CheckGameState(AIMan.currentStats);

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
		//The game will begin. Change cameras, initialize the player and enable the menu again with different buttons
		playerManager.wasInitialized = true;
		SoundtrackManager.instance.ChangeSet("Crescent");
		//Fade out overlay
		PlayerView.instance.FadeOverlay(1);
		Debug.Log("Here3");
		//Dlay initial soundtrack
		yield break;
	}
	public IEnumerator CheckGameState (AICurrentStats[] players) {
		if(turn <= 5)
			yield break;
		bool somethingHappened = false;
		foreach(AICurrentStats a in players) {
			if(!somethingHappened){
				print("Checking if "+a.name+" won with" +(float)a.provinces.Count/(float)provinces.Count);
				if((float)a.provinces.Count/(float)provinces.Count >= winCondition.maxPercentage){
					if(a.name == playerSO.name){
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
		if(aiOnly == 0){
			//End game if the player has no provinces
			if(playerManager.pStats.provinces.Count <=0){
				yield return PlayerView.instance.PlayerLose();
			} else if (playerManager.pStats.provinces.Count <= 5){
				//If player has only a few provinces for debugging porpouses 
				int count = 0;
				//Check if all its remaining provinces are owned by others players
				foreach(ProvinceData k in playerManager.pStats.provinces)
					if(k.owner != "Player")
						count++;
				if(count == playerManager.pStats.provinces.Count){
					//If so, end the game
					yield return PlayerView.instance.PlayerLose();
				}
			}
			SoundtrackCheck();
		}
		while(somethingHappened)
			yield return null;
		//If no one wons just keep going
		yield break;
	}
	void SoundtrackCheck() {
		//Check if the player has more than 50% of the mad
		//Check if the game is late and the player has less than 40% of the mad
		if(turn == 4){
			SoundtrackManager.instance.ChangeSet("Struggle");
		} else if(turn > 10){
			if(playerManager.pStats.provinces.Count > 2*provinces.Count/3){
				SoundtrackManager.instance.ChangeSet("Winning");
			} else if(playerManager.pStats.provinces.Count <= provinces.Count/4){
				SoundtrackManager.instance.ChangeSet("Intense");
			}  
		}

		
	}
	
	//GAME GENERATION
	void DistributeTroops () {
		//Do it for every player
		int gaining = 0;
		if(aiOnly == 0){
			if(playerManager != null){
				//Set current troods gain to 0
				gaining = 0;
				//Get new troods according to territory count
				int derTerritory = playerManager.pStats.provinces.Count;
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
				foreach(ProvinceData d in playerManager.pStats.provinces) {
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
				}
			}
		}
	}                     
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
			players.Add(playerManager.pStats.playerSO);
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
						g.ChangeOwnerTo(playerManager.pStats.name);
						playerManager.pStats.provinces.Add(g);
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
