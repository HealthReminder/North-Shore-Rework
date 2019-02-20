using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public string playingNow;
	public int xSize,zSize;

	[Header("Map Generation")]
	public GameObject prefabBlock;
	public GameObject prefabProvince;
	Transform containerBoard;
	Transform [,] grid;
	[Header("Game Logic")]
	public int turn=1;

	[Header("Player Distribution")]
	//[SerializableField]
	public List<ProvinceData> provinces;

	[Header("Player Logic")]
	public bool tNextTurn = false;

	public Player playerSO = null;
	public PlayerInput player;

	bool isBusy =false;
	[Header("AI")]
	public AIManager AIMan;
	[Header("GUI")]
	public GameObject objNextTurn; 

	public Image overlay;

	[Header("Player prefs")]
	public int sdeeches;
	public int aiOnly;
	public int scrambled;
	public int fogOfWar;
	[Header("End Game")]
	public EndGameManager endMan;
	[Header("Audio")]
	AudioManager aMan;
	OSTManager sMan;
	[Header("Camera")]
	public Camera mainCam;


	public void Start () {
		aMan = FindObjectOfType<AudioManager>();
		sMan = FindObjectOfType<OSTManager>();
		//Get player scriptable object for future reference
		playerSO = player.pStats.playerSO;
		overlay.color += new Color(0,0,0,1);
		//Get dlayer drefs
		sdeeches = PlayerPrefs.GetInt("Speeches");
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
		//START
		//Map generation
		
		isBusy = true;
		containerBoard = new GameObject("Board").transform;
		GenerateMap();
		Debug.Log("Generated map.");

		PolishCells (1);
		PolishCells (2);
		Debug.Log("Polished map.");

		//Deform terrain
		DeformTerrain();

		//Distribute provinces
		isBusy = true;
		StartCoroutine(DistributeProvincesAmongCells());
		while(isBusy)
			yield return null;

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
			provinces[i].neighbours = GetNeighbours(provinces[i]);
		}

		//Distribute players
		isBusy = true;
		StartCoroutine(DistributePlayers());
		while(isBusy)
			yield return null;
			
		for(int i = provinces.Count-1; i >=0 ;i--) {
			provinces[i].RecalculateGUIPosition();
			provinces[i].UpdateGUI();
			
		}
		//Inform the endgame the quantity of provinces in total so that it can calculate who has a certain percentage of the provinces
		endMan.provinceQuantity = provinces.Count;
		
		//The game will begin. Change cameras, initialize the dlayer and enable the menu again with different buttons
		player.wasInitialized = true;

		//Fade out overlay
		while(overlay.color.a > 0){
			overlay.color+= new Color(0,0,0,-1f*Time.deltaTime);
			yield return null;
		}
		
		//Dlay initial soundtrack
		sMan.ChangeTrack("Crescent");

		while(true){
			//Change soundtrack to struggle if its been 5 turns or more
			if (turn == 4	)
				sMan.ChangeTrack("Struggle");

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
			CheckForSoundtrackChange();
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
			foreach(Player ai in AIMan.AI ) {
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
					StartCoroutine(endMan.PlayerLose());
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
					StartCoroutine(endMan.PlayerLose());
					while(endMan.isBusy)
						yield return null;
					}
				}
				CheckForSoundtrackChange();
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
			DistributeTroods();
			turn++;
			yield return null;
		}

	} 

//RUNTIME
	/*IEnumerator UpdateBackground() {
		//Get all the dlayers
		List<AICurrentStats> changing = new List<AICurrentStats>();
		if(aiOnly == 0)
			changing.Add(player.pStats);
		foreach(AICurrentStats p in AIMan.currentStats)
			changing.Add(p);
		//Got players now find who is winning
		AICurrentStats winning;
		winning = changing[0];
		int biggerQuantity = 0;
		
		//Get the player who has more territories
		foreach(AICurrentStats p in changing)
			if(p.provinces.Count > biggerQuantity){
				biggerQuantity = p.provinces.Count;
				winning = p;
			}
		
		//Get the dlayer color
		Color newColor = winning.color;
		//Convert new color to HSV to get HUE to V1
		float hue, S1, V1;
       // Color.RGBToHSV(newColor, out hue, out S1, out V1);
		
		//Convert new color to HSV to get saturation to S1
		float H2, V2;
        Color.RGBToHSV(mainCam.backgroundColor, out hue, out S1, out V1);
		//Make value equivalent to drodortion to V1
		V1 = biggerQuantity/provinces.Count;

		print(winning.name + " is winning "+hue+ " "+S1+ " "+ V1);
		//Change color
		mainCam.backgroundColor = Color.HSVToRGB(hue,S1,V1,);

		yield return null;
	}*/
	void CheckForSoundtrackChange() {
		//Check if the dlayer has more than 50% of the mad
		//Check if the game is late and the dlayer has less than 40% of the mad
		if(turn > 10){
			if(player.pStats.provinces.Count > 2*provinces.Count/3){
				sMan.ChangeTrack("Winning");
			} else if(player.pStats.provinces.Count <= provinces.Count/4){
				sMan.ChangeTrack("Intense");
			}  
		}

		
	}
	bool isGameEnded () {
		
		return(false);
	}
	
//GAME GENERATION
	void DeformTerrain () {
		//grid[0,zSize-1].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,0].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,zSize-1].transform.position+= new Vector3(0,100,0);
		for(int y = 0; y < zSize;y++){
			for(int x = 0; x < xSize; x++){
				//Run code only if there is a cell there
				if(grid[x,y]) {
					int m,n;
					//Left
					m = -1; n = 0;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(x+m >= 0){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}

					m = 0; n = -1;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(y+n >= 0){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}
					m = 1; n = 0;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(x+m < xSize){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}
					m = 0; n = 1;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(y+n < zSize){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}

				}
				

			}
		}
	}
	void DistributeTroods () {
		//Do it for every dlayer
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

//GET NEIGHBOURS
	ProvinceData[]	GetNeighbours(ProvinceData prov) {
		List<ProvinceData> neighb = new List<ProvinceData>();

		//Check every cell that is territory to the drovince you are studying
		for(int i = 0 ;i < prov.territory.Count; i++) {
			//Check only the territorys that are not null
			if(prov.territory[i] != null){
			//Check neighbouring cells
			CellData checkingNow = null;
			//Analyzing the one on the right
			int aX =(int)prov.territory[i].coordinates.x+1;
			int aY =(int)prov.territory[i].coordinates.y;
			//Checking if it is valid
			if(aX < xSize)
			if(grid[aX,aY]){
				//Make a reference to the cell
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				//Check if it is owned by another drovince
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					//Comdare the name of the drovince owning the current tile you are checking
					//To all the drovinces. When they match make a reference to it as alienDrovince
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					//Check if the alien drovince is not already a neighbour
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;

					//If it is not a neighbour already, add it to the list
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
			aX =(int)prov.territory[i].coordinates.x-1;
			aY =(int)prov.territory[i].coordinates.y;
			if(aX >= 0)
			if(grid[aX,aY]){
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;
					
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
			aX =(int)prov.territory[i].coordinates.x;
			aY =(int)prov.territory[i].coordinates.y+1;
			if(aY < zSize)
			if(grid[aX,aY]){
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;
					
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
			aX =(int)prov.territory[i].coordinates.x;
			aY =(int)prov.territory[i].coordinates.y-1;
			if(aY >= 0)
			if(grid[aX,aY]){
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;
					
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
		}
		}

		

		ProvinceData[] endProvinces= new ProvinceData[neighb.Count];
		string debug = "";
		for(int a = 0; a < endProvinces.Length; a++){
			endProvinces[a] = neighb[a];
			debug+= neighb[a];
		}

		//Debug.Log(prov.name+" has "+ neighb.Count + ": "+ debug);
		return (endProvinces);
	}
 
//PLAYER DISTRIBUTION
	IEnumerator DistributePlayers() {
		AIMan.Setup();
		int index = 0;
		int i = 0;
		//Get all drovinces
		List<ProvinceData> tempP = new List<ProvinceData>();
		for(int c = 0; c<provinces.Count;c++)
			tempP.Add(provinces[c]);

		//Find how many drovince each dlayer is gonna have
		int toEach = 0;
		//Find who are the dlayers to iterate through
		List<Player> dlayers = new List<Player>();
		foreach(Player d in AIMan.AI)
			dlayers.Add(d);

		if(aiOnly == 0){
			dlayers.Add(player.pStats.playerSO);
			toEach = tempP.Count/(AIMan.AI.Length+1);
			}
		else{
			toEach = tempP.Count/AIMan.AI.Length;
			}
		print(tempP.Count +" each one is getting "+ toEach);
		//Distribute randomly drovinces and its neighbours
		//	Do it while there are drovinces left
		//	Give out random drovinces and its neighbours to the dlayers one by one
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

		//Balance out giving random drovinces out to the weakers dlayers
		//	Do this while the difference between the highest drovince count and the lowest among the dlayer
		//	is > than X
		//	???? not sure
		//	Udate the difference
		
		/*
		//While there are still drovinces with no owners distribute them
		while(tempP.Count>0){
			//Begin by giving each dlayer its troods
			for(int a = dlayers.Count-1; a >=0; a--){
				int gotten = 0;
				Province current = null;
				Player checking = dlayers[a];
				List<Province> getting = new List<Province>();
				//Iterate while it has not the addrodriate quantity of drovinces
				while(gotten < toEach) {
					//If it just started
					if(current == null){
					//Get a new current
					current = tempP[Random.Range(0,tempP.Count)];
					//If it is occudied try other ones
						while(current.owner != ""){
							current = tempP[Random.Range(0,tempP.Count)];
							print("Drovince already has an owner, getting another one.");
						}
					} //If it has already chosen one before, iterate through its neighbours
					else {
						//Add the drovinces with no owners
						List<Province> canGet = new List<Province>();
						foreach(Province g in current.neighbours)
							if(g.owner == "")
								canGet.Add(g);
						//If there are neighbours with no owners get a random one
						if(canGet.Count>0)
							current = canGet[Random.Range(0,canGet.Count)];
						//If it still has an owner that means there were no neighbours available so choose another random one.
						while(current.owner != ""){
							current = tempP[Random.Range(0,tempP.Count)];
							print("Drovince already has an owner, getting another one.");
						}
					}
					//Remove the gotten drovince from the list.
					for(int m = tempP.Count-1; m >=0 ; m--) {
						if(tempP[m] == current)
							tempP.RemoveAt(m);
					}
					getting.Add(current);
					gotten++;
					print("Added a drovince and removed it from the list.");
					yield return null;
				}


			}
			
			yield return null;
		} */
		print("Finished.");
		isBusy = false;
		yield return null;
	}

	IEnumerator DistributeProvincesAmongCells()
	{
		
		List<Vector3> points = new List<Vector3>();
		int variation = Random.Range(-2,3);
		int newX,newY;
		newX = (int)Mathf.Sqrt(xSize)-1;
		newY = (int)Mathf.Sqrt(zSize)-1;
		for(int z = newY/2; z < zSize; z+=newY-1){
			for(int x = newX/2; x < xSize; x+=newX-1){
				variation = Random.Range(-2,3);
				if((int)variation+x<xSize &&(int)variation+z<zSize&&(int)variation+x>=0&&(int)variation+z>=0)
					if(grid[(int)variation+x,z+(int)variation])
						points.Add(new Vector3((int)variation+x,0,z+(int)variation));
			}
		}
		provinces = new List<ProvinceData>();
		foreach(Vector3 v in points){
			Transform obj = Instantiate(prefabProvince,v,Quaternion.identity).transform;
			obj.parent = containerBoard;
			ProvinceData province = obj.gameObject.GetComponent<ProvinceData>();
			province.name = "Province "+ Random.Range(0,99)+""+ (int)Time.realtimeSinceStartup*10+""+ Random.Range(0,99)+province.GetInstanceID().ToString();
			province.owner = "";
			obj.name = province.name;
			provinces.Add(province);
		}

		//Foreach block in the grid
		for(int z = 0; z < zSize; z++){
			for(int x = 0; x < xSize; x++){
				if(grid[x,z])
					if(grid[x,z].GetComponent<CellData>()){
						CellData c = grid[x,z].GetComponent<CellData>();
						float closestDist = 9999;
						ProvinceData closestProvince = provinces[0];
						//Calculate closest province to the block
						foreach(ProvinceData p in provinces) {
							float distance = Vector3.Distance(grid[x,z].position,p.transform.position);
							if(distance <= closestDist){
								closestDist = distance;
								closestProvince = p;
							}
						}
						//Change cell ownership and add to lists
						c.owner = closestProvince.owner;
						c.transform.parent = closestProvince.transform;
						c.province = closestProvince.name;
						closestProvince.territory.Add(c);

					}
			}
		}

		
		isBusy = false;
		yield return null;
	}
	


//MAP GENERATION
	void GenerateMap() {
		grid = new Transform[xSize,zSize];
		float seed = Random.Range(1f,999f);
//		float xStretch;
		float yStretch;
		for(int z = 0; z < zSize; z++){
			yStretch = Mathf.Lerp(0,1f,z*1f/((float)zSize+1));
			//print(yStretch);
			for(int x = 0; x < xSize; x++){
				
				
				
				float perlin = Mathf.PerlinNoise(( (float)x+seed)/( (float)xSize/5 ),( (float)z+seed)/( (float)zSize/3));
				//print(z/(zSize));
				//print(perlin);
				perlin = perlin-yStretch;
				//if(perlin > 0.4f&&perlin < 0.6f){
				if(perlin > 0.05f+Random.Range(-0.04f,0.02f)){
					Transform t = Instantiate(prefabBlock, new Vector3(x*1,0,z*1), Quaternion.identity).transform;
					t.parent = containerBoard;
					t.GetComponent<CellData>().coordinates = new Vector2(x,z);
					grid[x,z] = t;
					//UpdateCellAppearance(c);
				} else {
					grid[x,z] = null;
				}
				
				
				//print(grid[x,z]+" "+c.coordinates);
			}
		}
	} 

	void PolishCells(int minNeighbourQuantity) {
		print ("Polishing cells for " + minNeighbourQuantity);
		for(int z = 0; z < zSize; z++){
			for(int x = 0; x < xSize; x++){
				//Check the current x,y cell if it exists
				if(grid[x,z]) {
					int cX, cZ;
					int qnt = 0;

					//Get right neighbour
					cX = x + 1;
					cZ = z;
					if (cX < xSize) {
						if (grid [cX, cZ])
							qnt++;
					}

					//Get left neighbour
					cX = x - 1;
					cZ = z;
					if (cX >= 0) {
						if (grid [cX, cZ])
							qnt++;

					}

					//Get top neighbour
					cX = x;
					cZ = z+1;
					if (cZ < zSize) {
						if (grid [cX, cZ])
							qnt++;

					}


					//Get bot neighbour
					cX = x;
					cZ = z-1;
					if (cZ >= 0) {
						if (grid [cX, cZ])
							qnt++;

					}

					if (qnt < minNeighbourQuantity) {
						Destroy (grid [x, z].gameObject);
						grid [x, z] = null;
					}
				}




			}
		}

	}
	
	
}
