using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutscene))]

public class CutsceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var cutscene = target as Cutscene;

        using (var scope = new GUILayout.HorizontalScope()) 
        {

            if (GUILayout.Button("對話動作"))
                cutscene.AddAction(new DialogueAction());
            else if (GUILayout.Button("人物動作"))
                cutscene.AddAction(new MoveActorAction());
            else if (GUILayout.Button("人物面向"))
                cutscene.AddAction(new TurnActorAction());
        }

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("人物傳送"))
                cutscene.AddAction(new TeleportObjectAction());
            else if (GUILayout.Button("允許動作"))
                cutscene.AddAction(new EnableObjectAction());
            else if (GUILayout.Button("關閉動作"))
                cutscene.AddAction(new DisableObjectAction());
        }

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("NPC互動"))
                cutscene.AddAction(new NPCInteractAction());
            else if (GUILayout.Button("淡入"))
                cutscene.AddAction(new FadeInAction());
            else if (GUILayout.Button("淡出"))
                cutscene.AddAction(new FadeOutAction());
        }

        base.OnInspectorGUI();
    }
}
