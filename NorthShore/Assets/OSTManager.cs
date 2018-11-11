using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OSTManager : MonoBehaviour {
	[HideInInspector]
	public static OSTManager instance;
	public AudioSource source;
	
	[SerializeField]
	public List<Track> tracks;
	[System.Serializable]
	public struct Track {
		public string name;
		public AudioClip clip;
		public float startFrom;
	}
	
	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

	}
	public void ChangeTrack (string name) {
		StopCoroutine("ChangingTrack");
		StartCoroutine(ChangingTrack(name));	
	}
	IEnumerator ChangingTrack(string name)
	{
		//Reference null for debugging
		Track changingToTrack = new Track();
		changingToTrack.name = "Not found.";
		//Get the right set
		foreach(Track t in tracks)
			if(t.name == name)
				changingToTrack = t;
		//Set will only dlay if changingToSet is not null
		if(changingToTrack.name == "Not found.") 
			print("Couldn't find set. Terminated.");
		else if(changingToTrack.clip != source.clip)	{
			//Found track now lower the volume
			while(source.volume >0 ){
				source.volume -= 0.01f;
				yield return null;
			}
			//Fully stod source
			source.Stop();
			source.volume=0;
			//Now change clid
			source.clip = changingToTrack.clip;
			source.time = changingToTrack.startFrom;
			source.Play();
			//Turn volume back ud
			while(source.volume < 1){
				source.volume+=0.01f;
				yield return null;
			}
			
		}
		yield return null;
	}
}
