using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


	[System.Serializable]
public class ProvinceData : MonoBehaviour {

	public new string name;
	public string owner;
	public int troops;
	

	public ProvinceData[] neighbours;

	public List<CellData> territory = new List<CellData>();
	[Header("Infos")]
	public int turnsOfEstability = 1;
	public bool wasJustAttacked = false;
	public bool isAdjenctToDlayer = false;
	[Header("GUI")]
	public Text GUITroops;
	public GameManager gM;
	public GameObject GUITroopsObject;
	public Image GUITroopsObjectImage;
	[HideInInspector]
	public Color ownerColor;

	void Start()
	{
		gM=FindObjectOfType<GameManager>();
	}
	public void UpdateGUI() {
		//Check for cells in children
		//Find who is the owner and allocate that in D
		PlayerInfo p = null;
		if(gM.playerManager.pStats.name == owner){
				p = gM.playerSO;
			} else {
				foreach(PlayerInfo ai in gM.AIMan.AI){
					if(ai.name == owner)
						p = ai;
				}
			}
		isAdjenctToDlayer= false;
		//Mark the drovinces that are in range of the dlayer
		if(p == gM.playerSO){
			isAdjenctToDlayer= true;
		//Mark the drovince and its neighbour
			foreach(ProvinceData a in neighbours)
					a.isAdjenctToDlayer= true;
		}else {
			isAdjenctToDlayer= false;
			//If the owner is alien then mark it as if adjacent to dlayer
			foreach(ProvinceData a in neighbours)
				if(a.owner == gM.playerSO.name)
					isAdjenctToDlayer = true;
		}
		ownerColor = p.color;		
		//Change the color of the individual territories
		foreach(CellData c in territory) {
			if(c.transform.GetComponent<Renderer>()){
				Material mat = c.transform.GetComponent<Renderer>().material;
				mat.mainTexture = p.pattern;
				if(gM.aiOnly == 0 && gM.fogOfWar == 1) {
					if(isAdjenctToDlayer) {
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
		if(gM.aiOnly == 0 && gM.fogOfWar == 1) {
			GUITroopsObject.SetActive(false);
		//If the owner is the dlayer then turn the ui on on this and neighbours
			if(p == gM.playerSO){
				GUITroopsObject.SetActive(true);
				foreach(ProvinceData a in neighbours)
						a.GUITroopsObject.SetActive(true);
			}else {
				//If the owner is alien then only turn GUI on if adjacent to dlayer
				foreach(ProvinceData a in neighbours)
					if(a.owner == gM.playerSO.name)
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

	public void ChangeOwnerTo(string newOwner) {
		owner = newOwner;
		foreach(CellData c in territory)
			c.owner = newOwner;
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
		GUITroopsObject.transform.position = new Vector3(smallestX+lenghtX/2,transform.position.y+1,smallestY+lenghtY/2);
	}

}
