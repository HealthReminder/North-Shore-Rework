using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionView : MonoBehaviour
{
    public void CheckForView(PlayerInfo player, int current_turn, int cycle_duration) {
        //Get proggress in current cycle
		while (current_turn > cycle_duration) 
			current_turn -= cycle_duration;
        float current_key = current_turn/cycle_duration;
        
        //foreach (AIReaction r in player.reactions)
        //    if(r.key == current_key)
        //        SpawnView(r,Vector3.zero);
    }

    public void SpawnView(AIReaction reaction, Vector3 spawn_position) {
        Debug.Log("Spawning view on :"+spawn_position);
        //new GameObject("Reaction GUI").AddComponent<SpriteRenderer>().sprite = reaction.reaction_image;
    }

    public static ReactionView instance;
    private void Awake() {
        instance = this;
    }
}
