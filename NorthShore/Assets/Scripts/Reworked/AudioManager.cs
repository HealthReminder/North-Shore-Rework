
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	[HideInInspector]	public static AudioManager instance;
	[Range(0,1)] public float generalVolume = 1;
	public AnimationCurve rollofCurve;
	public int sourceQuantity;
	
	[SerializeField]	public List<Track> tracks;

	int currentSource = 0;

	AudioSource[] sources;

	[System.Serializable]	public class Track {
		public string name;
		[Range(0,3)]
		public float volume = 1;
		[Range(0.01f,3)]
		public float pitch = 1;
		public AudioClip track;
		
		[Header("Clipping")]
		public Vector2 randomStart;
		public float playFor = 0;
	}

	float lastIndex;

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

		sources = new AudioSource[sourceQuantity];
		for(int a = 0; a< sources.Length; a++){
			sources[a] = gameObject.AddComponent<AudioSource>();
			sources[a].playOnAwake = false;
		}
			

	}
	public void PlayTrack(string name){
		if(name != null && name!= "") {
			Track currentTrack = tracks[0];
			foreach(Track s in tracks) {
				if(s.name == name)
					currentTrack = s;
			}
			StartCoroutine(Play(currentTrack.track,currentTrack.pitch,currentTrack.volume,currentTrack.randomStart,currentTrack.playFor));
		}		
	}

	IEnumerator Play(AudioClip s, float pitch, float volume, Vector2 randomStart, float playFor) {
		AudioSource curSrc = sources[currentSource];
		curSrc.pitch = pitch;
		curSrc.volume = volume*generalVolume;
		curSrc.clip = s;
		curSrc.time = Random.Range(randomStart.x*s.length,randomStart.y*s.length);
		curSrc.Play();

		AudioSource oldSource = null;
		if(playFor != 0){
			oldSource = curSrc;
		}

		currentSource++;
		if(currentSource >= sources.Length)
			currentSource = 0;
		
		if(playFor != 0){
			yield return new WaitForSeconds(playFor);
			float drog = 0;
			while(drog <= 1) {
				drog+=Time.deltaTime*0.5f;
				oldSource.volume = rollofCurve.Evaluate(drog)*generalVolume;
				yield return null;
			}
			oldSource.volume=0;
		}		
		yield break;
	}
}
