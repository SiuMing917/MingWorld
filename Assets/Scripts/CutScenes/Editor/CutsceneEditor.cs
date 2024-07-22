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

            if (GUILayout.Button("��ܰʧ@"))
                cutscene.AddAction(new DialogueAction());
            else if (GUILayout.Button("�H���ʧ@"))
                cutscene.AddAction(new MoveActorAction());
            else if (GUILayout.Button("�H�����V"))
                cutscene.AddAction(new TurnActorAction());
        }

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("�H���ǰe"))
                cutscene.AddAction(new TeleportObjectAction());
            else if (GUILayout.Button("���\�ʧ@"))
                cutscene.AddAction(new EnableObjectAction());
            else if (GUILayout.Button("�����ʧ@"))
                cutscene.AddAction(new DisableObjectAction());
        }

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("NPC����"))
                cutscene.AddAction(new NPCInteractAction());
            else if (GUILayout.Button("�H�J"))
                cutscene.AddAction(new FadeInAction());
            else if (GUILayout.Button("�H�X"))
                cutscene.AddAction(new FadeOutAction());
        }

        base.OnInspectorGUI();
    }
}
