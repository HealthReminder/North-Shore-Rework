using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGUIManager : MonoBehaviour {
    public GameObject arrow_prefab;
    public List<GameObject> arrows;
    public AnimationCurve progress_curve;

    public static BattleGUIManager instance;
    int is_ai_only = 0;
    int current_layer_order = 0;
    private void Awake () {
        Setup (50);
        instance = this;
    }
    private void Setup (int arrow_quantity) {
        is_ai_only = PlayerPrefs.GetInt ("AIOnly");
        GameObject obj;
        for (int i = 0; i < arrow_quantity; i++) {
            obj = Instantiate (arrow_prefab, Vector3.zero, Quaternion.identity);
            obj.transform.parent = transform;
            obj.SetActive (false);
            arrows.Add (obj);
        }
    }
    public void ShowAttack (Vector3 attacker_position, Vector3 defender_position, Color attacker_color) {
        GameObject chosen_arrow = arrows[0];
        arrows.RemoveAt (0);
        arrows.Add (chosen_arrow);
        StartCoroutine (MoveArrowRoutine (attacker_position, defender_position, chosen_arrow.transform, attacker_color));
    }
    float ai_trail_speed = 1;
    float camp_trail_speed = 1;
    IEnumerator MoveArrowRoutine (Vector3 start, Vector3 end, Transform obj, Color color) {
        obj.transform.position = start;
        float progress = 0;

        LineRenderer line = obj.GetComponent<LineRenderer> ();
        yield return null;
        line.sortingOrder = current_layer_order;
        current_layer_order+=1;
        line.SetPosition(0,start);
        line.SetPosition(1,start);
        obj.gameObject.SetActive (true);
        float trail_velocity;
        if (is_ai_only == 1) 
            trail_velocity = ai_trail_speed;
        else 
            trail_velocity = camp_trail_speed;
        
        trail_velocity*=5;
        
        color.a = 1;

        line.startColor = line.endColor = color;
        GradientColorKey[] color_keys = new GradientColorKey[2];

        color_keys[0] = new GradientColorKey(color,0);
        color_keys[1] = new GradientColorKey(color,1);
        
        GradientAlphaKey[] alpha_keys = new GradientAlphaKey[4];
        alpha_keys[0] = new GradientAlphaKey(0,0);
        alpha_keys[1] = new GradientAlphaKey(1,0.25f);
        alpha_keys[2] = new GradientAlphaKey(1,0.9f);
        alpha_keys[3] = new GradientAlphaKey(0,1);

        line.colorGradient.SetKeys(color_keys,alpha_keys);

        while (progress < 1) {
            line.SetPosition(1,Vector3.Lerp (start, end, progress_curve.Evaluate(progress)));
            progress += Time.deltaTime * trail_velocity;
            yield return new WaitForSeconds(Time.deltaTime*(3f/trail_velocity));
        }
        line.SetPosition(1,end);
        yield return new WaitForSeconds (3);
        obj.gameObject.SetActive (false);
        yield break;
    }
}