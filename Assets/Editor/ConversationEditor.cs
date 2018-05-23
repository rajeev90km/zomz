using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Conversation))]
public class ConversationEditor : Editor 
{
    private ReorderableList _list;

    private void OnEnable()
    {
        _list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("AllDialogues"),
                true, true, true, true);
        _list.elementHeight = 220f;
        _list.drawElementCallback += DrawElement;
        _list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Conversation Entities");
        };
    }

    private void DrawElement(Rect rect, int index, bool active, bool focused)
    {
        float _labelWidth = 60f;
        float _textBoxWidth = 170f;
        float _padding = 5f;

        var element = _list.serializedProperty.GetArrayElementAtIndex(index);

        //Name
        EditorGUI.LabelField(new Rect(rect.x, rect.y + _padding, _labelWidth, EditorGUIUtility.singleLineHeight), "Name");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + _padding, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("CharacterName"), GUIContent.none);

        //Name
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 2 * _padding + EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "Text");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 2 * _padding + EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("Text"), GUIContent.none);

        //Avatar
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 3 * _padding + 2 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "Avatar");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 3 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("Avatar"), GUIContent.none);
        
        //Char Position
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 4 * _padding + 3 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "Char Pos");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 4 * _padding + 3 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("CharPosition"), GUIContent.none);
       
        //Background Image
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 5 * _padding + 4 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "BG Image");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 5 * _padding + 4 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("BackgroundImage"), GUIContent.none);

        //VO
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 6 * _padding + 5 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "VO");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 6 * _padding + 5 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("DialogueVO"), GUIContent.none);

        //BG Music
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 7 * _padding + 6 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "BG Music");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 7 * _padding + 6 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("BackgroundMusic"), GUIContent.none);

        //SFX
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 8 * _padding +7 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "SFX");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 8 * _padding + 7 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("SFX"), GUIContent.none);

        //SFX
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 9 * _padding + 8 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "SFX");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 9 * _padding + 8 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("SFX"), GUIContent.none);

        // Exit SFX
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 10 * _padding + 9 * EditorGUIUtility.singleLineHeight, _labelWidth, EditorGUIUtility.singleLineHeight), "Exit SFX");
        EditorGUI.PropertyField(
            new Rect(rect.x + _labelWidth, rect.y + 10 * _padding + 9 * EditorGUIUtility.singleLineHeight, _textBoxWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("ExitSFX"), GUIContent.none);
    }

    public override void OnInspectorGUI()
    {
        Conversation conversation = (Conversation)target;
        conversation.PauseGamePlay = EditorGUILayout.Toggle("Pause Gameplay", conversation.PauseGamePlay);

        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }    
}
