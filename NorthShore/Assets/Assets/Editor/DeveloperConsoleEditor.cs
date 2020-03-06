using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeveloperConsole))]
public class DeveloperConsoleEditor : Editor {

    int player_Map_Size = 1;
    float labelWidth = 50f;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        GUILayout.Space(10f);
        GUILayout.Label("Update and Reset player prefs", EditorStyles.boldLabel); 
        GUILayout.Space(20f); 

        GUILayout.BeginHorizontal();
        GUILayout.Label("Map_Size", GUILayout.Width(labelWidth)); 
        player_Map_Size = (EditorGUILayout.IntField(player_Map_Size));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Update")) //8
        {
            PlayerPrefs.SetInt("Player_Map_Size", player_Map_Size);
            Debug.Log("PlayerPrefs Saved");
        }

        if (GUILayout.Button("Reset")) //10
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs Reset");
        }

        GUILayout.EndHorizontal();
    }
}