using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerView : MonoBehaviour
{
	[Header("Canvas")]
	public GameObject nextTurnButtonObject;
	public Image overlay;
	[Header("Pointer View")]
	public LineRenderer battle_Line;
	Vector3 battle_Defender,battle_Attacker;
	public Transform battle_Target;

    [Header("Ending View")]
	public UnityEvent onLoseEvent;
	public UnityEvent onWinEvent;

    public static PlayerView instance;
    private void Awake() {
        instance = this;
    }
	private void Start() {
		if(PlayerPrefs.GetInt("AIOnly") == 1)
			nextTurnButtonObject.SetActive(false);
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
	public void SetDefender(Vector3 defenderPos) {
		return;
		battle_Line.SetPosition(0,battle_Attacker);
		battle_Line.SetPosition(1,defenderPos);
		battle_Defender = defenderPos;
		battle_Line.enabled=true;
		battle_Target.position = defenderPos;
		battle_Target.gameObject.SetActive(true);
	}
	public void SetAttacker(Vector3 attackerPos) {
		return;
		ClearBattleGUI();
		battle_Attacker = attackerPos;
		battle_Target.position = attackerPos;
		battle_Target.gameObject.SetActive(true);
		
	}
	public void ClearBattleGUI() {
		battle_Line.enabled = false;
		battle_Target.gameObject.SetActive(false);
	}
	#endregion
    #region Ending View
	public void PlayerWin () {
		SoundtrackManager.instance.ChangeSet("Winning");
		AudioManager.instance.PlayTrack("Player_Win");
		onWinEvent.Invoke();
	}
	
	public void PlayerLose () {
		SoundtrackManager.instance.ChangeSet("Intro");
		AudioManager.instance.PlayTrack("Player_Lose");
		onLoseEvent.Invoke();
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
