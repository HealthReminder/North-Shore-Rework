using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerView : MonoBehaviour
{
	[Header("Canvas")]
	public GameObject nextTurnButtonObject;
	public Image overlay;
	[Header("Pointer View")]
	public LineRenderer pointer_Line;
	public Transform pointer_Defender,pointer_Attacker;

    [Header("Ending View")]
	[SerializeField] Image ending_WinImg;
	[SerializeField] Image ending_LoseImg;

    public static PlayerView instance;
    private void Awake() {
        instance = this;
    }
	#region Events 
	public void OnPlayerTurn(string state) {
		if(state == "OnStart"){
			nextTurnButtonObject.SetActive(true);
		} else if(state == "OnEnd"){
			nextTurnButtonObject.SetActive(false);
		}
	}
	public void FadeOverlay(int state) {
		StopCoroutine(FadeOverlayRoutine(-1));
		StartCoroutine(FadeOverlayRoutine(state));
	}
	#endregion
	#region Pointer View
	public void SetDefender(Vector3 newDos) {
		pointer_Line.SetPosition(0,pointer_Attacker.transform.position);
		pointer_Line.SetPosition(1,newDos);
		pointer_Defender.position = newDos;
		pointer_Defender.gameObject.SetActive(true);
		pointer_Line.enabled=true;
	}
	public void SetAttacker(Vector3 newDos) {
		Clear();
		transform.position = newDos;
		pointer_Attacker.gameObject.SetActive(true);
		
	}
	public void Clear() {
		pointer_Line.enabled=false;
		pointer_Attacker.gameObject.SetActive(false);
		pointer_Defender.gameObject.SetActive(false);
	}
	#endregion
    #region Ending View
	public IEnumerator PlayerWin () {
		ending_WinImg.color += new Color(0,0,0,-1);
		ending_WinImg.gameObject.SetActive(true);
		while(ending_WinImg.color.a <1){
			ending_WinImg.color += new Color(0,0,0,0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		while(!Input.anyKey)

			yield return null;
		while(ending_WinImg.color.a >0){
			ending_WinImg.color += new Color(0,0,0,-0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(0);
		yield break;
	}
	
	public IEnumerator PlayerLose () {
		ending_LoseImg.color += new Color(0,0,0,-1);
		ending_LoseImg.gameObject.SetActive(true);
		while(ending_LoseImg.color.a <1){
			ending_LoseImg.color += new Color(0,0,0,0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		while(!Input.anyKey)

			yield return null;
		while(ending_LoseImg.color.a >0){
			ending_LoseImg.color += new Color(0,0,0,-0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(0);
		yield break;
	}
	#endregion

	IEnumerator FadeOverlayRoutine(int state) {
		Debug.Log("Fading to "+state);
		if(state == 0) {
			while(overlay.color.a < 1){
				overlay.color+= new Color(0,0,0,2f*Time.deltaTime);
				yield return null;
			}
		} else if(state == 1) {
			while(overlay.color.a > 0){
			overlay.color+= new Color(0,0,0,-1f*Time.deltaTime);
			yield return null;
			}
		}
		Debug.Log("Finished to "+state);
		yield break;
	}
}
