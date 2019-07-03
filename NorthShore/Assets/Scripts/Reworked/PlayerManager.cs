using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour {
	public bool isOn = false;
	public bool isBusy = false;
	public Camera playerCam;
	ProvinceData attackingProvince, defendingProvince;
	public ProvinceData currentSelectedProvince;

	[SerializeField]
	public PlayerData playerData;

	ProvinceData lastHoveredProvince = null;

	
	 void Update() {
		if(isOn){
			if(!isBusy){
				Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				

				if (Physics.Raycast(ray, out hit, Mathf.Infinity)){
					//Debug.DrawLine(ray.origin, hit.point);
					if(hit.transform.parent){
					//Checks if hit a cell
						if(hit.transform.parent.GetComponent<ProvinceData>()){
						
						//If it hits a province change the referenced
						currentSelectedProvince = hit.transform.parent.GetComponent<ProvinceData>();

						//Get the new hovered province
						if(lastHoveredProvince!= null){
							if(currentSelectedProvince!=lastHoveredProvince){
								//print("Hitting a different province");
								lastHoveredProvince.GUITroopsObjectImage.color = lastHoveredProvince.ownerColor+ new Color(-0.2f,-0.2f,-0.2f);
								lastHoveredProvince = currentSelectedProvince;
								currentSelectedProvince.GUITroopsObjectImage.color = currentSelectedProvince.GUITroopsObjectImage.color + new Color(0.3f,0.3f,0.3f);
							} 
								//print("Hitting the same province");
						}else{ 
							//print("Hitting the first province ever");
							lastHoveredProvince = currentSelectedProvince;
						}
						

					}
			
				}
			}
			if(Input.GetMouseButtonUp(0)){
				if(currentSelectedProvince != null) {
					//The player is selecting an attacker.
						if(currentSelectedProvince.owner == playerData.playerInfo) {
							//If it had another cell attacker, deselect it.
							if(attackingProvince!= null) {
								foreach(ProvinceData p in attackingProvince.neighbours)
									p.transform.position+= new Vector3(0,-0.6f,0);
								attackingProvince.transform.position+= new Vector3(0,-1.2f,0);
								attackingProvince = null;
							}
							//Also clears the cell it is defender.
							if(defendingProvince != null){
									defendingProvince.transform.position+= new Vector3(0,-0.6f,0);
									defendingProvince = null;
							}
							//Only selects the clicked cell if it has more than 1 troop.
							if(currentSelectedProvince.troops >1){
								attackingProvince = currentSelectedProvince;
								PlayerView.instance.SetAttacker(attackingProvince.transform.position+ new Vector3(0,2,0));
								attackingProvince.transform.position+= new Vector3(0,1.2f,0);
								foreach(ProvinceData p in attackingProvince.neighbours)
									p.transform.position+= new Vector3(0,0.6f,0);
							}
							
						//The player is selecting the defender.
						} else {
							//Can only select if it has an attacker already.
							if(attackingProvince != null) {
								//Clears the defender if it has one.
								if(defendingProvince != null){
									defendingProvince.transform.position+= new Vector3(0,-0.6f,0);
									defendingProvince = null;
								} else PlayerView.instance.ClearBattleGUI();	
								//Only selects the defender if it has a neighbouring attacker (troops cannot go diagonally).
								foreach(ProvinceData p in attackingProvince.neighbours)
									if(p == currentSelectedProvince){
										defendingProvince = currentSelectedProvince;
										PlayerView.instance.SetDefender(defendingProvince.transform.position+ new Vector3(0,2,0));
										defendingProvince.transform.position+= new Vector3(0,0.6f,0);
										StartCoroutine(CallAttack());
									}
											
							}
						}

				}
				

				
			}
		}
	}
	}

	IEnumerator CallAttack() {
		if(attackingProvince != null && defendingProvince != null)
				yield return StartCoroutine(GameController.instance.Battle(attackingProvince,defendingProvince));
		if(attackingProvince!= null) {
			foreach(ProvinceData p in attackingProvince.neighbours)
				p.transform.position+= new Vector3(0,-0.6f,0);
			attackingProvince.transform.position+= new Vector3(0,-1.2f,0);
			attackingProvince = null;
		}
		if(defendingProvince != null){
			defendingProvince.transform.position+= new Vector3(0,-0.6f,0);
			defendingProvince = null;
		}
		yield break;
	}

}
