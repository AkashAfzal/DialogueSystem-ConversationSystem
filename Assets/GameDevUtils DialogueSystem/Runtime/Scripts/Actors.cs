using UnityEngine;


namespace GameDevUtils.DialogueSystem
{


	[CreateAssetMenu(fileName = "Actors", menuName = "DialogueSystem/Actors", order = 1)]
	public class Actors : ScriptableObject
	{
		public Actor[] actorsList;
	}

	[System.Serializable]
	public class Actor
	{
		public int    actorId;
		public string actorName;
		public string actorOccupation;
		public string actorDescription;
		public Sprite actorAvatar;

	}


}