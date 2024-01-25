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
	[CustomEditor(typeof(Actors))]
	public class ActorsEditor : Editor
	{

		// Serialized clone property of Dialogue.actorsList
		private SerializedProperty ActorsList;

		// Must implement completely custom behavior of how to display and edit ReorderableLists
		private ReorderableList actorsList;

		// Reference to the actual Dialogue instance this Inspector belongs to
		private Actors Actors;
		Actor          InspectorSelectedActor;
		int            InspectorSelectedActorIndex;

		// Called when the Inspector is opened (when the ScriptableObject is selected)
		private void OnEnable()
		{
			// Get the target as the type you are actually using
			Actors = (Actors) target;

			// Link in serialized fields to their according SerializedProperties
			ActorsList = serializedObject.FindProperty(nameof(Actors.actorsList));

			// Setup and configure the actorsList that will be used to display the content of the ActorsList 
			actorsList                       = new ReorderableList(serializedObject, ActorsList, false, true, true, true);
			actorsList.drawHeaderCallback    = DrawActorListHeader;
			actorsList.onAddCallback         = OnActorListAdd;
			actorsList.drawElementCallback   = DrawActorListElement;
			actorsList.elementHeightCallback = OnActorElementHeight;
			actorsList.onSelectCallback      = OnActorListSelect;
			actorsList.onRemoveCallback      = OnActorListRemove;
		}


		//actorsList Callbacks


		#region actorsList Callbacks

		void DrawActorListHeader(Rect rect)
		{
			var fieldWidth = (rect.width - 14) / 4;
			EditorGUI.LabelField(new Rect(rect.x + 14,                  rect.y, fieldWidth,         rect.height), "Actor Name");
			EditorGUI.LabelField(new Rect(rect.x + 14 + fieldWidth + 2, rect.y, 3 * fieldWidth - 2, rect.height), "Actor Description");
		}

		void OnActorListAdd(ReorderableList list)
		{
			// This adds the new element but copies all values of the select or last element in the list
			list.serializedProperty.arraySize++;
			var newActorName = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1).FindPropertyRelative("actorName");
			var newActorDes  = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1).FindPropertyRelative("actorDescription");
			newActorName.stringValue = "";
			newActorDes.stringValue  = "";
		}

		void DrawActorListElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			var nameControl        = "ActorName"        + index;
			var descriptionControl = "ActorDescription" + index;
			var actor              = Actors.actorsList[index];
			var actorProperty      = ActorsList.GetArrayElementAtIndex(index).FindPropertyRelative("actorName");
			var fieldWidth         = rect.width / 4;
			var actorName          = actor.actorName;
			// Get all actors as string[]
			var availableIDs = Actors.actorsList;

			// store the original GUI.color
			var color = GUI.color;
			// Tint the field in red for invalid values
			// either because it is empty or a duplicate
			if (string.IsNullOrWhiteSpace(actorProperty.stringValue) || availableIDs.Count(item => string.Equals(item, actorProperty.stringValue)) > 1)
			{
				GUI.color = Color.red;
			}

			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName(nameControl);
			actorName = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight), GUIContent.none, actorName);
			if (EditorGUI.EndChangeCheck())
			{
				actor.actorName = actorName;
			}

			// reset to the default color
			GUI.color = color;
			var description = actor.actorDescription;
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName(descriptionControl);
			description = EditorGUI.TextField(new Rect(rect.x + fieldWidth + 2, rect.y + 2, 3 * fieldWidth - 2, EditorGUIUtility.singleLineHeight), GUIContent.none, description);
			if (EditorGUI.EndChangeCheck())
			{
				actor.actorDescription = description;
			}

			Actors.actorsList[index].actorId = index;

			// If the value is invalid draw a HelpBox to explain why it is invalid
			if (string.IsNullOrWhiteSpace(actorProperty.stringValue))
			{
				rect.y += EditorGUI.GetPropertyHeight(actorProperty);
				EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Name may not be empty!", MessageType.Error);
			}
			else if (availableIDs.Count(item => Equals(item, actorProperty.stringValue)) > 1)
			{
				rect.y += EditorGUI.GetPropertyHeight(actorProperty);
				EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Duplicate! ID has to be unique!", MessageType.Error);
			}

			EditorGUI.EndDisabledGroup();
			var focusedControl = GUI.GetNameOfFocusedControl();
			if (string.Equals(nameControl, focusedControl) || string.Equals(descriptionControl, focusedControl))
			{
				InspectorSelectedActor      = actor;
				InspectorSelectedActorIndex = index;
			}
		}

		float OnActorElementHeight(int index)
		{
			var element      = ActorsList.GetArrayElementAtIndex(index).FindPropertyRelative("actorName");
			var availableIDs = Actors.actorsList;
			var height       = EditorGUI.GetPropertyHeight(element);
			if (string.IsNullOrWhiteSpace(element.stringValue) || availableIDs.Count(item => string.Equals(item, element.stringValue)) > 1)
			{
				height += EditorGUIUtility.singleLineHeight;
			}

			return height;
		}

		void OnActorListSelect(ReorderableList list)
		{
			InspectorSelectedActor      = Actors.actorsList[list.index];
			InspectorSelectedActorIndex = list.index;
		}

		void OnActorListRemove(ReorderableList list)
		{
			var actor = Actors.actorsList[list.index];
			if (actor == null) return;
			var deletedLastOne = list.count == 1;
			ReorderableList.defaultBehaviours.DoRemoveButton(list);
			list.DoLayoutList();
			if (deletedLastOne)
			{
				InspectorSelectedActor      = null;
				InspectorSelectedActorIndex = 0;
			}
			else
			{
				InspectorSelectedActor      = (list.index < list.count) ? Actors.actorsList[list.index] : (list.count > 0) ? Actors.actorsList[list.count - 1] : null;
				InspectorSelectedActorIndex = (list.index < list.count) ? list.index : (list.count                    > 0) ? list.count - 1 : 0;
			}
		}

		#endregion


		public override void OnInspectorGUI()
		{
			DrawScriptField();

			// load real target values into SerializedProperties
			serializedObject.Update();
			actorsList.DoLayoutList();
			if (InspectorSelectedActor != null && Actors.actorsList.Length > 0)
			{
				EditorGUILayout.Space(5);
				EditorGUILayout.LabelField("Actor Details", EditorStyles.boldLabel);
				EditorGUILayout.Space(5);
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.IntField(new GUIContent("ActorID", "Internal ID. Change at your own risk."), Actors.actorsList[InspectorSelectedActorIndex].actorId + 1);
				EditorGUI.EndDisabledGroup();
				// get the current element's SerializedProperty
				var nameProperty        = ActorsList.GetArrayElementAtIndex(InspectorSelectedActorIndex).FindPropertyRelative("actorName");
				var occupationProperty  = ActorsList.GetArrayElementAtIndex(InspectorSelectedActorIndex).FindPropertyRelative("actorOccupation");
				var descriptionProperty = ActorsList.GetArrayElementAtIndex(InspectorSelectedActorIndex).FindPropertyRelative("actorDescription");
				var avatarProperty      = ActorsList.GetArrayElementAtIndex(InspectorSelectedActorIndex).FindPropertyRelative("actorAvatar");

				// Get all actors as string[]
				var availableIDs = Actors.actorsList;

				// store the original GUI.color
				var color = GUI.color;
				// Tint the field in red for invalid values
				// either because it is empty or a duplicate
				if (string.IsNullOrWhiteSpace(nameProperty.stringValue) || availableIDs.Count(item => string.Equals(item, nameProperty.stringValue)) > 1)
				{
					GUI.color = Color.red;
				}

				EditorGUILayout.PropertyField(nameProperty, GUILayout.Height(20));
				nameProperty.stringValue = nameProperty.stringValue;

				// reset to the default color
				GUI.color = color;
				EditorGUILayout.PropertyField(occupationProperty, GUILayout.Height(20));
				occupationProperty.stringValue = occupationProperty.stringValue;
				EditorGUILayout.LabelField("Actor Description");
				EditorStyles.textField.wordWrap = true; // This sets the wordwrap value of the property
				descriptionProperty.stringValue = EditorGUILayout.TextArea(descriptionProperty.stringValue, GUILayout.Height(50));
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Actor Avatar");
				avatarProperty.objectReferenceValue = EditorGUILayout.ObjectField(avatarProperty.objectReferenceValue, typeof(Sprite), false, GUILayout.Height(100), GUILayout.Width(100));
				EditorGUILayout.EndHorizontal();
			}

			// Write back changed values into the real target
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawScriptField()
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((Actors) target), typeof(Actors), false);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.Space();
		}

	}

	#endif


}