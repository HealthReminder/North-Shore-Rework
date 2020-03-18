using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGUIManager : MonoBehaviour {
    public GameObject arrow_prefab;
    public List<GameObject> arrows;
    public static BattleGUIManager instance;
    int is_ai_only = 0;
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
    IEnumerator MoveArrowRoutine (Vector3 start, Vector3 end, Transform obj, Color color) {
        obj.transform.position = start;
        float velocity = 5;
        float progress = 0;

        TrailRenderer trail = obj.GetComponent<TrailRenderer> ();
        trail.time = 0;
        yield return null;
        obj.gameObject.SetActive (true);
        float fade_velocity = 1;
        if (is_ai_only == 1) {
            trail.time = 0.5f;
            velocity = 20;
        } else {
            trail.time = 5f;
            velocity = 20;
        }
        color.a = 1;
        trail.startColor = trail.endColor = color;

        while (progress < 1) {
            obj.transform.position = Vector3.Lerp (start, end, progress);
            progress += Time.deltaTime * velocity;
            Debug.Log (progress);
            yield return new WaitForSeconds(0.05f);
        }
        obj.transform.position = end;
        yield return new WaitForSeconds (3);
        obj.gameObject.SetActive (false);
        yield break;
    }

    //float wait = 1;
    private void Update () {
        /*if (wait > 0)
            wait -= Time.deltaTime;
        else {
            wait = 1;
            Vector3 v1 = (Vector3.up*Random.Range(1,3)+(Vector3.right*Random.Range(1,3)));
            Vector3 v2 = (Vector3.back*Random.Range(1,3));

            ShowAttack (v1, v2, Color.yellow);
        }*/
    }
}