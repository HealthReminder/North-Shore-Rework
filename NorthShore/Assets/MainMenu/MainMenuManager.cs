using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour {
	
	public Image overlay;
	public AudioManager aMan;
	public OSTManager sMan;
	void Start () {
		aMan = FindObjectOfType<AudioManager>();
		sMan = FindObjectOfType<OSTManager>();
		sMan.ChangeTrack("Reflection");
		PlayerPrefs.SetInt("Cycle Duration",50);
		PlayerPrefs.SetInt("AIOnly", 0);
		PlayerPrefs.SetInt("Speeches", 0);
		PlayerPrefs.SetInt("Scrambled", 0);
		PlayerPrefs.SetInt("FogOfWar", 1);
		//StartCoroutine(DlayIntro());
	}
	public void StartCamdaing() {
		PlayerPrefs.SetInt("Scrambled", 0);
		PlayerPrefs.SetInt("FogOfWar", 1);
		StartCoroutine(LoadScene(1));
	}
	public void StartDeathMatch() {
		PlayerPrefs.SetInt("Cycle Duration",15);
		PlayerPrefs.SetInt("Scrambled", 1);
		PlayerPrefs.SetInt("FogOfWar", 0);
		StartCoroutine(LoadScene(1));
	}
	public void StartAIOnly() {
		PlayerPrefs.SetInt("Cycle Duration",15);
		PlayerPrefs.SetInt("Scrambled", 0);
		PlayerPrefs.SetInt("FogOfWar", 0);
		PlayerPrefs.SetInt("AIOnly", 1);
		StartCoroutine(LoadScene(1));
	}
	IEnumerator LoadScene(int sceneIndex)
	{
		while(overlay.color.a < 1) {
			overlay.color+= new Color(0,0,0,0.05f);
			yield return null;
		}
		SceneManager.LoadScene(sceneIndex);
		yield return null;
	}

	IEnumerator DlayIntro()
	{
		
		yield return null;
	}

}
