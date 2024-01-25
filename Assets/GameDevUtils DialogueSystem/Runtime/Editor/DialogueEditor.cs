using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;


namespace GameDevUtils.DialogueSystem
{


// This editor script needs to be either wrapped by #if UNITY_EDITOR or placed in a folder called "Editor"
	#if UNITY_EDITOR
	[CustomEditor(typeof(Conversation))]
	public class DialogueEditor : Editor
	{

		// // Serialized clone property of Dialogue.actorsList
		private Actors Actors;

		// Serialized clone property of Dialogue.DialogueItems
		private SerializedProperty DialogueItems;

		// Must implement completely custom behavior of how to display and edit ReorderableLists
		private ReorderableList actorsList;
		private ReorderableList dialogueItemsList;

		// Reference to the actual Dialogue instance this Inspector belongs to
		private Conversation Conversation;
		bool                 DialogueExpanded;


		// Called when the Inspector is opened (when the ScriptableObject is selected)
		private void OnEnable()
		{
			// Get the target as the type you are actually using
			Conversation = (Conversation) target;

			// Link in serialized fields to their according SerializedProperties
			DialogueItems = serializedObject.FindProperty(nameof(Conversation.dialogues));

			// Setup and configure the dialogueItemsList that will be used to display the content of the DialogueItems 
			dialogueItemsList                       = new ReorderableList(serializedObject, DialogueItems, true, true, true, true);
			dialogueItemsList.drawHeaderCallback    = DrawDialogueListHeader;
			dialogueItemsList.onAddCallback         = OnDialogueListAdd;
			dialogueItemsList.drawElementCallback   = DrawDialogueListElement;
			dialogueItemsList.elementHeightCallback = OnDialogueElementHeight;
		}

		//dialogueItemsList Callbacks


		#region dialogueItemsList Callbacks

		void DrawDialogueListHeader(Rect rect)
		{
			// As the header we simply want to see the usual display name of the DialogueItems
			EditorGUI.LabelField(rect, "Dialogues", EditorStyles.boldLabel);
		}

		void OnDialogueListAdd(ReorderableList list)
		{
			// This adds the new element but copies all values of the select or last element in the list
			list.serializedProperty.arraySize++;
			var newElement     = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
			var leftSideActor  = newElement.FindPropertyRelative(nameof(Dialogue.leftSideActor));
			var rightSideActor = newElement.FindPropertyRelative(nameof(Dialogue.rightSideActor));
			var text           = newElement.FindPropertyRelative(nameof(Dialogue.dialogueText));
			leftSideActor.intValue  = -1;
			rightSideActor.intValue = -1;
			text.stringValue        = "";
		}

		void DrawDialogueListElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (Conversation.actors != null)
			{
				// get the current element's SerializedProperty
				var element = DialogueItems.GetArrayElementAtIndex(index);
				EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, 20), $"DIALOGUE{index + 1}", EditorStyles.boldLabel);

				// Get the nested property fields of the DialogueElement class
				var leftSideActor     = element.FindPropertyRelative(nameof(Dialogue.leftSideActor));
				var rightSideActor    = element.FindPropertyRelative(nameof(Dialogue.rightSideActor));
				var focusedSides      = element.FindPropertyRelative(nameof(Dialogue.focusedActor));
				var haveMultipleLines = element.FindPropertyRelative(nameof(Dialogue.haveMultipleLines));
				var text              = element.FindPropertyRelative(nameof(Dialogue.dialogueText));
				var dialogueLines     = element.FindPropertyRelative(nameof(Dialogue.dialogueLines));
				var popUpHeight       = EditorGUI.GetPropertyHeight(leftSideActor) / 2;
				// Get the existing actor names as GuiContent[]
				var availableOptions = Conversation.actors.actorsList.Select(item => new GUIContent(item.actorName)).ToArray();

				// store the original GUI.color
				var color = GUI.color;

				// if the value is invalid tint the next field red else reset color
				GUI.color = leftSideActor.intValue < 0 ? Color.red : color;

				// Draw the Popup so you can select from the existing Actors names
				leftSideActor.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y + 25, rect.width, popUpHeight), new GUIContent(leftSideActor.displayName), leftSideActor.intValue, availableOptions);

				// if the value is invalid tint the next field red else reset color
				GUI.color = rightSideActor.intValue < 0 ? Color.red : color;

				// Draw the Popup so you can select from the existing Actors names
				rightSideActor.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y + 25 + popUpHeight, rect.width, popUpHeight), new GUIContent(rightSideActor.displayName), rightSideActor.intValue, availableOptions);

				// reset the GUI.color
				GUI.color = color;

				// Draw the focused actor side from the enum list
				EditorGUI.PropertyField(new Rect(rect.x, rect.y + 25 + popUpHeight * 2, rect.width, popUpHeight), focusedSides);
				rect.y += popUpHeight;
				
				
				// Display options foldout
				haveMultipleLines.boolValue = EditorGUI.Toggle(new Rect(rect.x, rect.y + 50 + popUpHeight, rect.width, popUpHeight), "Have Multiple Lines", haveMultipleLines.boolValue);
				if (haveMultipleLines.boolValue)
				{
					//Draw Dialogue lines string array
					var dialogueLinesHeight = EditorGUI.GetPropertyHeight(dialogueLines);
					EditorGUI.PropertyField(new Rect(rect.x, rect.y + 80 + popUpHeight, rect.width, dialogueLinesHeight), dialogueLines, true);
				}
				else
				{
					// Draw the single Dialogue Text field
					// since we use a PropertyField it will automatically recognize that this field is tagged [TextArea]
					// and will choose the correct drawer accordingly
					var textHeight = EditorGUI.GetPropertyHeight(text);
					EditorGUI.PropertyField(new Rect(rect.x, rect.y + 75 + popUpHeight, rect.width, textHeight), text);
				}

				
				
				
			}
			else
			{
				GUI.color = Color.red;
				EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Please Pass The Actors Data Before!", MessageType.Error);
			}
		}

		float OnDialogueElementHeight(int index)
		{
			var element            = DialogueItems.GetArrayElementAtIndex(index);
			var leftSideActorName  = element.FindPropertyRelative(nameof(Dialogue.leftSideActor));
			var rightSideActorName = element.FindPropertyRelative(nameof(Dialogue.rightSideActor));
			var focusedActor       = element.FindPropertyRelative(nameof(Dialogue.focusedActor));
			var haveMultipleLines  = element.FindPropertyRelative(nameof(Dialogue.haveMultipleLines));
			var dialogueText       = element.FindPropertyRelative(nameof(Dialogue.dialogueText));
			var dialogueLines      = element.FindPropertyRelative(nameof(Dialogue.dialogueLines));

			float dialogueHeight = haveMultipleLines.boolValue ? EditorGUI.GetPropertyHeight(dialogueLines) : EditorGUI.GetPropertyHeight(dialogueText);

			//height of entire dialog items section
			return EditorGUI.GetPropertyHeight(leftSideActorName) + EditorGUI.GetPropertyHeight(rightSideActorName) + EditorGUI.GetPropertyHeight(focusedActor) + EditorGUI.GetPropertyHeight(haveMultipleLines) + dialogueHeight + EditorGUIUtility.singleLineHeight;
		}

		#endregion


		public override void OnInspectorGUI()
		{
			DrawScriptField();
			Conversation.actors = (Actors) EditorGUILayout.ObjectField("Actors", Conversation.actors, typeof(Actors), true);
			EditorGUILayout.LabelField("Dialogues Data", EditorStyles.boldLabel);

			// load real target values into SerializedProperties
			serializedObject.Update();
			dialogueItemsList.DoLayoutList();

			// Write back changed values into the real target
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
			{
				EditorUtility.SetDirty(Conversation);
			}
		}

		private void DrawScriptField()
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((Conversation) target), typeof(Conversation), false);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.Space();
		}

	}
	#endif


}