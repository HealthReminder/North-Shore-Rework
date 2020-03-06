using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] public class AIReaction {
	[SerializeField] public float key;
	[SerializeField] public Sprite reaction_image;
	
}
[System.Serializable]
[CreateAssetMenu(fileName = "Player", menuName = "DiceWars/Player", order = 0)]
public class PlayerInfo : ScriptableObject {
	
	public new string name;
	public Color color;
	public Texture2D pattern;

	public AnimationCurve aggressiveness;
	public AnimationCurve expansiveness;

	//[SerializeField] public List<AIReaction> reactions;

	
}
