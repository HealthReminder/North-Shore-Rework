using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInput : MonoBehaviour {
	public bool wasInitialized = false;
	public bool isBusy = false;
	public Camera playerCam;
	public ProvinceData attacker, defender;

	public BattleManager bM;
	public ProvinceData province;

	[SerializeField]
	public AICurrentStats pStats;

	[Header("Menu")]
	public MenuManager menuMan;
	ProvinceData lastHoveredProvince = null;

	
	 void Update() {
		 if(wasInitialized){
		if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)) {
			menuMan.ToggleCameras();
		}
		if(Input.GetKeyDown(KeyCode.M)) {
			if(Time.timeScale != 20){
				FindObjectOfType<BattleManager>().isFastMode = true;
				Time.timeScale = 20;
				print(Time.timeScale);
			} else {
				FindObjectOfType<BattleManager>().isFastMode = false;
				Time.timeScale = 1;
				print(Time.timeScale);
			}
			
		}
		
		if(!isBusy){
			//If the dlayer resses R the scene will load again
			//if(Input.GetKeyDown(KeyCode.R)){
			//	SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			//}
			//If the dlayer dresses escade the game will dause and the cams will be toggled.
			//This will only hadden when there is a game already, otherwise the only way to do it is to click
			//"New game" and make dlayerIndut enabled again
				Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				

				if (Physics.Raycast(ray, out hit, Mathf.Infinity)){
					//Debug.DrawLine(ray.origin, hit.point);
					if(hit.transform.parent){
					//Checks if hit a cell
						if(hit.transform.parent.GetComponent<ProvinceData>()){
						
						//If it hits a province change the referenced
						province = hit.transform.parent.GetComponent<ProvinceData>();

						//Get the new hovered province
						if(lastHoveredProvince!= null){
							if(province!=lastHoveredProvince){
							//	print("Hitting a different province");
								lastHoveredProvince.GUITroopsObjectImage.color = lastHoveredProvince.ownerColor+ new Color(-0.2f,-0.2f,-0.2f);
								//Get new one
								lastHoveredProvince = province;
								province.GUITroopsObjectImage.color = province.GUITroopsObjectImage.color + new Color(0.3f,0.3f,0.3f);
							} 
								//print("Hitting the same province");
						}else{ 
							//print("Hitting the first province ever");
							lastHoveredProvince = province;
						}
						

					}
			
				}
			}
			if(Input.GetMouseButtonUp(0)){
				if(province != null) {
					//The player is selecting an attacker.
						if(province.owner == "Player") {
							//If it had another cell attacker, deselect it.
							if(attacker!= null) {
								foreach(ProvinceData p in attacker.neighbours)
									p.transform.position+= new Vector3(0,-0.6f,0);
								attacker.transform.position+= new Vector3(0,-1.2f,0);
								attacker = null;
							}
							//Also clears the cell it is defender.
							if(defender != null){
									defender.transform.position+= new Vector3(0,-0.6f,0);
									defender = null;
							}
							//Only selects the clicked cell if it has more than 1 troop.
							if(province.troops >1){
								attacker = province;
								PointerController.instance.SetAttacker(hit.point+ new Vector3(0,1,0));
								attacker.transform.position+= new Vector3(0,1.2f,0);
								foreach(ProvinceData p in attacker.neighbours)
									p.transform.position+= new Vector3(0,0.6f,0);
							}
							
						//The player is selecting the defender.
						} else {
							//Can only select if it has an attacker already.
							if(attacker != null) {
								//Clears the defender if it has one.
								if(defender != null){
									defender.transform.position+= new Vector3(0,-0.6f,0);
									defender = null;
								}
								//Only selects the defender if it has a neighbouring attacker (troops cannot go diagonally).
								foreach(ProvinceData p in attacker.neighbours)
									if(p == province){
										defender = province;
										PointerController.instance.SetDefender(hit.point+ new Vector3(0,1,0));
										defender.transform.position+= new Vector3(0,0.6f,0);
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
		if(attacker != null && defender != null)
			bM.isBusy = true;
			StartCoroutine(bM.Battle(attacker,defender));
			while(bM.isBusy){
				isBusy = true;
				yield return null;
			}
			isBusy = false;
            if(attacker!= null) {
				foreach(ProvinceData p in attacker.neighbours)
									p.transform.position+= new Vector3(0,-0.6f,0);
				attacker.transform.position+= new Vector3(0,-1.2f,0);
				attacker = null;
			}
			if(defender != null){
				defender.transform.position+= new Vector3(0,-0.6f,0);
				defender = null;
			}
			yield break;
	}
/*
		if(Input.GetKeyDown(KeyCode.A)) {
			if(attacker != null && defender != null)
				bM.Battle(attacker,defender);
            if(attacker!= null) {
				attacker.transform.position+= new Vector3(0,-0.6f,0);
				attacker = null;
			}
			if(defender != null){
				defender.transform.position+= new Vector3(0,-0.35f,0);
				defender = null;
			}
		}
    } */

}
