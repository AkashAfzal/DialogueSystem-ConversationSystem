using System.Collections.Generic;
using UnityEngine;


namespace GameDevUtils.DialogueSystem
{


	[CreateAssetMenu(fileName = "Conversation", menuName = "DialogueSystem/Conversations", order = 2)]
	public class Conversation : ScriptableObject
	{

		public Actors         actors;
		public List<Dialogue> dialogues;

	}


}