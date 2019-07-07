using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


	[System.Serializable]
public class ProvinceData : MonoBehaviour {

	//public new string name;
	public PlayerInfo owner;
	public int troops;
	public ProvinceData[] neighbours;
	public List<CellData> territory = new List<CellData>();
	[Header("Infos")]
	public int turnsOfEstability = 1;
	public bool wasJustAttacked = false;
	public bool isAdjacentToPlayer = false;
	[Header("GUI")]
	public Text GUITroops;
	public GameObject GUITroopsObject;
	public Image GUITroopsObjectImage;
	[HideInInspector]
	public Color ownerColor;

	void Start()
	{
		GameManager.instance=FindObjectOfType<GameManager>();
	}
	public void UpdateGUI() {
		//Check for cells in children
		//Find who is the owner and allocate that in D
		PlayerInfo p = null;
		if(GameManager.instance.playerManager.playerData.playerInfo == owner){
				p = GameManager.instance.playerManager.playerData.playerInfo;
			} else {
				foreach(PlayerInfo ai in AIManager.instance.AI){
					if(ai == owner)
						p = ai;
				}
			}
		isAdjacentToPlayer= false;
		//Mark the drovinces that are in range of the dlayer
		if(p == GameManager.instance.playerManager.playerData.playerInfo){
			isAdjacentToPlayer= true;
		//Mark the drovince and its neighbour
			foreach(ProvinceData a in neighbours)
					a.isAdjacentToPlayer= true;
		}else {
			isAdjacentToPlayer= false;
			//If the owner is alien then mark it as if adjacent to dlayer
			foreach(ProvinceData a in neighbours)
				if(a.owner == GameManager.instance.playerManager.playerData.playerInfo)
					isAdjacentToPlayer = true;
		}
		ownerColor = p.color;		
		//Change the color of the individual territories
		foreach(CellData c in territory) {
			if(c.transform.GetComponent<Renderer>()){
				Material mat = c.transform.GetComponent<Renderer>().material;
				mat.mainTexture = p.pattern;
				if(GameManager.instance.aiOnly == 0 && GameManager.instance.fogOfWar == 1) {
					if(isAdjacentToPlayer) {
						if(troops>1)
							mat.color = ownerColor+ new Color(0.1f,0.1f,0.1f,0);
						else
							mat.color = ownerColor;
					}	else 
							mat.color = new Color(ownerColor.r/2,ownerColor.g/2,ownerColor.b/2,0);
				} else {
					if(troops>1)
						mat.color = ownerColor+ new Color(0.1f,0.1f,0.1f,0);
					else
					mat.color = ownerColor;
				}
			}
		} 
		//The drovince will only turn the GUITroods on if it is owned by the dlayer 
		//or if any of its neighbours are the dlayer
		if(GameManager.instance.aiOnly == 0 && GameManager.instance.fogOfWar == 1) {
			GUITroopsObject.SetActive(false);
		//If the owner is the dlayer then turn the ui on on this and neighbours
			if(p == GameManager.instance.playerManager.playerData.playerInfo){
				GUITroopsObject.SetActive(true);
				foreach(ProvinceData a in neighbours)
						a.GUITroopsObject.SetActive(true);
			}else {
				//If the owner is alien then only turn GUI on if adjacent to dlayer
				foreach(ProvinceData a in neighbours)
					if(a.owner == GameManager.instance.playerManager.playerData.playerInfo)
						GUITroopsObject.SetActive(true);
			}
		}

		if(GUITroopsObject.activeSelf){
			float blackness = ownerColor.r + ownerColor.b + ownerColor.g;
			blackness = blackness/3;
			if(blackness >= 0.6f)
				GUITroops.color = new Color(0,0,0,1f);
			else
				GUITroops.color = new Color(0.8f,0.8f,0.8f,1f);
			
		}

		float growth = (troops-1f)/5f;
		growth = Mathf.Clamp(growth,0f,1.2f);
		GUITroops.text = troops.ToString();
		GUITroopsObject.transform.localScale = new Vector3(0.8f+(float)growth,0.8f+(float)growth,0.8f+(float)growth);
		GUITroopsObjectImage.color = ownerColor+ new Color(-0.2f,-0.2f,-0.2f);

		
	}

	public void ChangeOwnerTo(PlayerInfo playerInfo) {
		Debug.Log("Received player of name "+playerInfo.name);
		owner = playerInfo;
		foreach(CellData c in territory)
			c.owner = playerInfo.name;
	}

	public void RecalculateGUIPosition() {
		//Declaring variables
		float smallestY,smallestX,highestY,highestX;
		float lenghtY, lenghtX;

		smallestX = smallestY = 999;
		highestX = highestY = -999;

		//Get variables
		foreach(CellData c in territory) {
			if(c.transform.position.x < smallestX) {
				smallestX = c.transform.position.x;
			}
			if(c.transform.position.x > highestX) {
				highestX = c.transform.position.x;
			}
			if(c.transform.position.z < smallestY) {
				smallestY = c.transform.position.z;
			}
			if(c.transform.position.z > highestY) {
				highestY = c.transform.position.z;
			}
		}

		//Get X and Y lenght
		lenghtY = highestY-smallestY;
		lenghtX = highestX-smallestX;
		//Position GUI in the middle of those lenghts
		GUITroopsObject.transform.position = new Vector3(smallestX+lenghtX/2,transform.position.y+3,smallestY+lenghtY/2);
	}

}
