using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerView : MonoBehaviour
{
    [Header("Ending")]
	[SerializeField] Image canvasWin;
	[SerializeField] Image canvasLose;

    public static PlayerView instance;
    private void Awake() {
        instance = this;
    }
    // Update is called once per frame
	public IEnumerator PlayerWin () {
		canvasWin.color += new Color(0,0,0,-1);
		canvasWin.gameObject.SetActive(true);
		while(canvasWin.color.a <1){
			canvasWin.color += new Color(0,0,0,0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		while(!Input.anyKey)

			yield return null;
		while(canvasWin.color.a >0){
			canvasWin.color += new Color(0,0,0,-0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(0);
		yield break;
	}
	
	public IEnumerator PlayerLose () {
		canvasLose.color += new Color(0,0,0,-1);
		canvasLose.gameObject.SetActive(true);
		while(canvasLose.color.a <1){
			canvasLose.color += new Color(0,0,0,0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		while(!Input.anyKey)

			yield return null;
		while(canvasLose.color.a >0){
			canvasLose.color += new Color(0,0,0,-0.05f);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(0);
		yield break;
	}
}
