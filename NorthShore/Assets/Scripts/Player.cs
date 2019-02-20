using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Player", menuName = "DiceWars/Player", order = 0)]
public class Player : ScriptableObject {
	
	public new string name;
	public Color color;
	public Texture2D pattern;

	public AnimationCurve aggressiveness;
	public AnimationCurve expansiveness;	
	public Speech[] speeches;

	
}
