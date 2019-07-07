﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
	public class PlayerData {
		public PlayerInfo playerInfo;
		//public string name;
		//public Color color;
		public List<ProvinceData> provinces;
	}
[System.Serializable] public struct WinCondition{
	[Range(0.01f,1f)]	public float maxPercentage;
}

public class GameManager : MonoBehaviour {
	[SerializeField] public WinCondition winCondition;
	public bool isMatchEnded = false;
	public string playingNow;
	public int mapSizeX,mapSizeY;
	public PlayerManager playerManager;
	public PlayerData[] allPlayers;
	Transform [,] grid;
	[Header("Game Logic")]
	public int turn=1;

	[Header("Player Distribution")]
	//[SerializableField]
	public List<ProvinceData> provinces;

	//[Header("Player Logic")]
	[HideInInspector]public bool nextTurnPlayerInput = false;

	
	[Header("AI")]
	public AIManager AIMan;

	[Header("Player prefs")]
	public int aiOnly;
	public int isScrambleMode;
	public int fogOfWar;

	public static GameManager instance;
	private void Awake() {
		instance = this;
	}
	public void Start () {
		//Get player scriptable object for future reference
		PlayerView.instance.overlay.color += new Color(0,0,0,1);
		aiOnly	= PlayerPrefs.GetInt("AIOnly");
		isScrambleMode = PlayerPrefs.GetInt("Scrambled");
		fogOfWar = PlayerPrefs.GetInt("FogOfWar");
		//Here you should call the loading screen and turn off the other GUI from the menu
		StartCoroutine(GameLoop());
	}
	void Update () {
		//print(PlayerPrefs.GetInt("AIOnly")+ " "+ PlayerPrefs.GetInt("Speeches"));
		
			if(Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
				NextTurn();

			if(Input.GetKeyDown(KeyCode.Alpha9))
				PlayerView.instance.PlayerWin();
			if(Input.GetKeyDown(KeyCode.Alpha0))
				PlayerView.instance.PlayerLose();
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
				if(playerManager.playerData.provinces.Count > 0){
					PlayerView.instance.OnPlayerTurn("OnStart");
					playerManager.isBusy=false;
					playingNow = playerManager.playerData.playerInfo.name;
					while(!nextTurnPlayerInput)
						yield return null;
					playerManager.isBusy=true;
					PlayerView.instance.OnPlayerTurn("OnEnd");
				}
			}
			yield return CheckGameState();
			//AI TURN
			foreach(PlayerInfo ai in AIMan.AI ) {
				playingNow = ai.name; 
				yield return StartCoroutine(AIMan.Calculate(ai)); 
			}

			yield return CheckGameState();

			//NEW TURN
			GameController.instance.DistributeTroops(allPlayers,isScrambleMode);
			turn++;
			yield return null;
		}

	} 
	IEnumerator Setup() {
		//START
		//Map generation
		grid = MapController.instance.GenerateMap(mapSizeX,mapSizeY);
		Debug.Log("Generated map of size "+grid.GetLength(0)+","+grid.GetLength(1));

		MapController.instance.PolishCells (grid,1,mapSizeX,mapSizeY);
		MapController.instance.PolishCells (grid,2,mapSizeX,mapSizeY);
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

		//Set initial troops to 2 
		foreach (ProvinceData p in provinces)
			p.troops = 2;

		//Distribute players
		yield return StartCoroutine(DistributePlayers());
		Debug.Log("Here1");
		for(int i = provinces.Count-1; i >=0 ;i--) {
			provinces[i].RecalculateGUIPosition();
			provinces[i].UpdateGUI();
			
		}
		//The game will begin. Change cameras, initialize the player and enable the menu again with different buttons
		playerManager.isOn = true;
		SoundtrackManager.instance.ChangeSet("Crescent");
		//Fade out overlay
		PlayerView.instance.FadeOverlay(1);
		Debug.Log("Here3");
		//Dlay initial soundtrack
		
		
		yield break;
	}
	public IEnumerator CheckGameState () {
		SoundtrackCheck();
		if(isMatchEnded)
			yield break;
		if(turn <= 5)
			yield break;

		foreach(PlayerData a in allPlayers) {
			print("Checking if "+a.playerInfo.name+" won with" +(float)a.provinces.Count/(float)provinces.Count);
			if((float)a.provinces.Count/(float)provinces.Count >= winCondition.maxPercentage){
				if(a.playerInfo == playerManager.playerData.playerInfo)
					PlayerView.instance.PlayerWin();
				else {
					if(aiOnly == 0){
						isMatchEnded = true;
						PlayerView.instance.PlayerLose();
					}
					else {
						isMatchEnded = true;
						PlayerView.instance.PlayerWin();
					}
				}
					
				//SoundtrackManager.instance.ChangeSet("Intro");
			}
		}
		if(aiOnly == 0){
			//End game if the player has no provinces
			if(playerManager.playerData.provinces.Count <=0){
				isMatchEnded = true;
				PlayerView.instance.PlayerLose();
			} else if (playerManager.playerData.provinces.Count <= 5){
				//If player has only a few provinces for debugging porpouses 
				int count = 0;
				//Check if all its remaining provinces are owned by others players
				foreach(ProvinceData k in playerManager.playerData.provinces)
					if(k.owner != playerManager.playerData.playerInfo)
						count++;
				if(count == playerManager.playerData.provinces.Count){
					//If so, end the game
					isMatchEnded = true;
					PlayerView.instance.PlayerLose();
				}
			}
		}
		//If no one wons just keep going
		yield break;
	}
	void SoundtrackCheck() {
		//Check if the player has more than 50% of the mad
		//Check if the game is late and the player has less than 40% of the mad
		if(!isMatchEnded){
			if(aiOnly == 1){
				if(turn >= 10)
					SoundtrackManager.instance.ChangeSet("Intense");
			}else if(turn == 4){
				SoundtrackManager.instance.ChangeSet("Struggle");
			} else if(turn > 10){
				if(playerManager.playerData.provinces.Count > 2*provinces.Count/3){
					SoundtrackManager.instance.ChangeSet("Winning");
				} else if(playerManager.playerData.provinces.Count <= provinces.Count/4){
					SoundtrackManager.instance.ChangeSet("Intense");
				}  
			}
		}

		
	}       
	IEnumerator DistributePlayers() {
		AIMan.Setup();
		if(aiOnly == 1)
			allPlayers = AIMan.currentStats;
		else if(aiOnly == 0){
			allPlayers = new PlayerData[AIMan.currentStats.Length+1];
			for (int s = 0; s < allPlayers.Length; s++)
			{
				if(s < AIMan.currentStats.Length)
					allPlayers[s] = AIMan.currentStats[s];
				else 
					allPlayers[s] = playerManager.playerData;
			}
		}

		int index = 0;
		int randomIndex = 0;
		//Get all drovinces
		List<ProvinceData> tempProvinces = new List<ProvinceData>();
		for(int c = 0; c<provinces.Count;c++)
			tempProvinces.Add(provinces[c]);

		//Find how many drovince each player is gonna have
		int initialProvinceCount = 0;

		initialProvinceCount = tempProvinces.Count/allPlayers.Length;
			
		print(tempProvinces.Count +" each one is getting "+ initialProvinceCount);
		//Distribute randomly drovinces and its neighbours
		//	Do it while there are drovinces left
		//	Give out random drovinces and its neighbours to the players one by one
		//	And remove the drovince and neighbours from list.
		//		Extra: I will alternate between giving only a drovince or its neighbours to to give it some variety
		bool onlyOne = false;
		while(tempProvinces.Count > 0){
			randomIndex = Random.Range(0,tempProvinces.Count);
			if(isScrambleMode == 1)
				onlyOne = true;
			
			//Allocate memory for the drovinces its getting
			List<ProvinceData> provincesGaining = new List<ProvinceData>();
			//Get the direct dick
			provincesGaining.Add(tempProvinces[randomIndex]);
			//Get the neighbours also
			//Only add neighbours if onlyOne is false
			if(!onlyOne)
				foreach(ProvinceData d in tempProvinces[randomIndex].neighbours)
					provincesGaining.Add(d);
			else{
				provincesGaining.Add(tempProvinces[Random.Range(0,tempProvinces.Count)]);
				provincesGaining.Add(tempProvinces[Random.Range(0,tempProvinces.Count)]);
				provincesGaining.Add(tempProvinces[Random.Range(0,tempProvinces.Count)]);
			}
			//Add all the drovinces its getting change its owner and find it in tempP list
			foreach(ProvinceData g in provincesGaining){
				g.ChangeOwnerTo(allPlayers[index].playerInfo);
				allPlayers[index].provinces.Add(g);
				for(int k =tempProvinces.Count-1; k >= 0 ; k--) 
					if(tempProvinces[k] == g)
						tempProvinces.RemoveAt(k);
			}
			index++;
			if(index >= allPlayers.Length)
				index = 0;
			
			
			yield return null;
		}
		foreach (PlayerData p in allPlayers)
			for (int i = p.provinces.Count-1; i >= 0; i--)
				if(p.provinces[i].owner != p.playerInfo)
					p.provinces.RemoveAt(i);	

		for (int o = 0; o < 5; o++)
		{
			List<ProvinceData> extraProvinces = new List<ProvinceData>();
			foreach (PlayerData p in allPlayers)
				if(p.provinces.Count > initialProvinceCount+1)
					for (int i = 0; i < p.provinces.Count-initialProvinceCount; i++)
						extraProvinces.Add(p.provinces[p.provinces.Count-1-i]);
					
			foreach (PlayerData p in allPlayers)
				if(p.provinces.Count <= initialProvinceCount)
					for (int i = 0; i < 1+initialProvinceCount-p.provinces.Count; i++)
						if(extraProvinces.Count>0){
							int randomI = Random.Range(0,extraProvinces.Count);
							p.provinces.Add(extraProvinces[randomI]);
							extraProvinces[randomI].owner = p.playerInfo;
						}
			yield return null;
		}
		
					
			
		foreach (PlayerData p in allPlayers)
			for (int i = p.provinces.Count-1; i >= 0; i--)
				if(p.provinces[i].owner != p.playerInfo)
					p.provinces.RemoveAt(i);

		foreach (PlayerData p in allPlayers)
				Debug.Log(p.playerInfo.name +" is getting provinces "+p.provinces.Count);
		print("Finished.");
		yield break;
	}	
}
