using UnityEngine;


namespace GameDevUtils.DialogueSystem
{


	[System.Serializable]
	public class Dialogue
	{

		public enum FocusedActor
		{

			Left  = 0,
			Right = 1

		}


		[Header("ACTOR")] 
		[Tooltip("Left side actor")]  
		public int leftSideActor;  // Set Left ActorID index according to the Actor
		[Tooltip("Right side actor")] 
		public int rightSideActor; // Set Right ActorID index according to the Actor

		[Tooltip("Which actor is focused now or which actor is talking right now")] 
		public FocusedActor focusedActor;

		[Header("Dialogue")] 
		public bool haveMultipleLines;
		[Tooltip("What the Actor is saying")]
		[TextArea] 
		public string   dialogueText;
		public string[] dialogueLines;

	}


}