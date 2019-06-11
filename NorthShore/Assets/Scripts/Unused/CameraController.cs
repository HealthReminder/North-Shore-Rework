using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {


	// How long the object should shake for.
	public float shakeDuration = 5f;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float decreaseFactor = 1.0f;

	Vector3 originalPos;

	//Make it a singleton
    public static CameraController instance;
    void Awake(){
		//Singleton pattern
		if  (instance == null){
			DontDestroyOnLoad(gameObject);
			instance = this;
		}	
		else if (instance != this)
			Destroy(gameObject);
		
	}
	void Start()
	{
		originalPos = transform.localPosition;
	}



	void Update()
	{
		if (shakeDuration > 0)
		{
			transform.localPosition = new Vector3(originalPos.x +(Random.Range(-0.25f,0.25f) * shakeDuration),originalPos.y+(Random.Range(-0.25f,0.25f) * shakeDuration),originalPos.z);

			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		else
		{
			shakeDuration = 0f;
			transform.localPosition = originalPos;
		}
	}
}