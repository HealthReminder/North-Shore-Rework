/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
	
	[Header("Cameras")]
	public GameObject mainCam;
	public GameObject menuCam;
	[Header("Menu Objects")]
	public GameObject canvas;
	public GameObject buttonStart,buttonNew;
	[Header("Toggles")]
	public Toggle tAIOnly;
	public Toggle tSdeeches;
	public Toggle tScrambled;
	public Toggle tFogOfWar;
	[Header("Dlayer")]
	public Image dlayerColor;
	public Slider dlayerColorSlider;
	public PlayerInfo dlayerSO;
	public void ReloadScene() {
		UpdateUI();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void GameBegun () {
		buttonStart.SetActive(false);
		tAIOnly.gameObject.SetActive(false);
		tSdeeches.gameObject.SetActive(false);
		tScrambled.gameObject.SetActive(false);
		tFogOfWar.gameObject.SetActive(false);
		buttonNew.SetActive(true);
		canvas.SetActive(true);
	}

	void Start(){
		canvas.SetActive(true);
		dlayerColor.color = dlayerSO.color =  Color.HSVToRGB(dlayerColorSlider.value,0.1f,1f);
		PlayerPrefs.SetInt("AIOnly", 0);
		PlayerPrefs.SetInt("Speeches", 1);
		PlayerPrefs.SetInt("Scrambled", 0);
		PlayerPrefs.SetInt("FogOfWar", 1);
		UpdateUI();
	}
	public void UpdateUI(){
		//print(PlayerPrefs.GetInt("AIOnly")+ " "+ PlayerPrefs.GetInt("Speeches"));
		int v = PlayerPrefs.GetInt("AIOnly");
		if(v == 0)
			tAIOnly.isOn = false;
		else 
			tAIOnly.isOn = true;
		
		v = PlayerPrefs.GetInt("Speeches");
		if(v == 1)
			tSdeeches.isOn = false;
		else 
			tSdeeches.isOn = true;
		v = PlayerPrefs.GetInt("Scrambled");
		if(v == 0)
			tScrambled.isOn = false;
		else 
			tScrambled.isOn = true;
		v = PlayerPrefs.GetInt("FogOfWar");
		if(v == 0)
			tFogOfWar.isOn = false;
		else 
			tFogOfWar.isOn = true;
	}
	public void ChangePlayerColor () {
		dlayerColor.color = dlayerSO.color =  Color.HSVToRGB(dlayerColorSlider.value,0.25f,1f);
		
	}
	public void ToggleCameras () {
				menuCam.SetActive(!menuCam.activeSelf);
				mainCam.SetActive(!mainCam.activeSelf);
				if(!menuCam.activeSelf)
					canvas.SetActive(false);
				else
					canvas.SetActive(true);
	}
	public void ToggleGlobalOption (string prefs) {
		//Get current state of the button
		int currentValue = PlayerPrefs.GetInt(prefs);
		if(currentValue == 0)
			PlayerPrefs.SetInt(prefs, 1);
		else
			PlayerPrefs.SetInt(prefs, 0);

		//print(PlayerPrefs.GetInt("AIOnly"));
		
	}
} */
