using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace GameDevUtils.DialogueSystem
{


	public class DialogueController : MonoBehaviour
	{

		[Tooltip("Select or drag the Dialogue.asset here")] [SerializeField] Conversation    loadedConversation;
		[SerializeField]                                                     Button          skipButton;
		[SerializeField]                                                     TextMeshProUGUI leftSideActorNameText;
		[SerializeField]                                                     TextMeshProUGUI rightSideActorNameText;
		[SerializeField]                                                     TextMeshProUGUI leftSideActorOccupationText;
		[SerializeField]                                                     TextMeshProUGUI rightSideActorOccupationText;
		[SerializeField]                                                     TextMeshProUGUI dialogueTextDisplay;
		[SerializeField]                                                     Image           leftSideActorImage;
		[SerializeField]                                                     Image           rightSideActorImage;
		[SerializeField]                                                     GameObject      leftSideNameArea;
		[SerializeField]                                                     GameObject      rightSideNameArea;
		[SerializeField]                                                     float           typingdelay;


		private int DialogueIndex     = 0;
		private int DialogueLineIndex = 0;

		Actor[]  ActorsList      => loadedConversation.actors.actorsList;
		Dialogue CurrentDialogue => loadedConversation.dialogues[DialogueIndex];


		void Start()
		{
			dialogueTextDisplay.text = "";
			RefreshUIForDialogueData();
			if (CurrentDialogue.haveMultipleLines)
			{
				StartCoroutine(Type(CurrentDialogue.dialogueLines[DialogueLineIndex]));
			}
			else
			{
				StartCoroutine(Type(CurrentDialogue.dialogueText));
			}

			skipButton.onClick.AddListener(SkipButton_OnClick);
		}

		IEnumerator Type(string sentence)
		{
			foreach (char letter in sentence)
			{
				dialogueTextDisplay.text += letter;
				yield return new WaitForSeconds(typingdelay);
			}
		}

		public void NextSentence()
		{
			if (DialogueIndex >= loadedConversation.dialogues.Count) return;
			StopAllCoroutines();
			if (CurrentDialogue.haveMultipleLines && DialogueLineIndex < CurrentDialogue.dialogueLines.Length)
			{
				RefreshUIForDialogueData();
				dialogueTextDisplay.text = "";
				StartCoroutine(Type(CurrentDialogue.dialogueLines[DialogueLineIndex]));
				DialogueLineIndex++;
			}
			else
			{
				DialogueLineIndex = 0;
				DialogueIndex++;
				dialogueTextDisplay.text = "";
				if (DialogueIndex < loadedConversation.dialogues.Count)
				{
					if (CurrentDialogue.haveMultipleLines)
					{
						RefreshUIForDialogueData();
						StartCoroutine(Type(CurrentDialogue.dialogueLines[DialogueLineIndex]));
						DialogueLineIndex++;
					}
					else
					{
						RefreshUIForDialogueData();
						StartCoroutine(Type(CurrentDialogue.dialogueText));
					}
				}
				else
				{
					StartCoroutine(Type("Conversation Completed"));
				}
			}
		}

		private void RefreshUIForDialogueData()
		{
			leftSideActorNameText.text        = ActorsList[CurrentDialogue.leftSideActor].actorName;
			rightSideActorNameText.text       = ActorsList[CurrentDialogue.rightSideActor].actorName;
			leftSideActorOccupationText.text  = ActorsList[CurrentDialogue.leftSideActor].actorOccupation;
			rightSideActorOccupationText.text = ActorsList[CurrentDialogue.rightSideActor].actorOccupation;
			leftSideActorImage.sprite         = ActorsList[CurrentDialogue.leftSideActor].actorAvatar;
			rightSideActorImage.sprite        = ActorsList[CurrentDialogue.rightSideActor].actorAvatar;
			leftSideActorImage.color          = (int) CurrentDialogue.focusedActor == 0 ? Color.white : Color.grey;
			rightSideActorImage.color         = (int) CurrentDialogue.focusedActor == 1 ? Color.white : Color.grey;
			leftSideNameArea.SetActive((int) CurrentDialogue.focusedActor  == 0);
			rightSideNameArea.SetActive((int) CurrentDialogue.focusedActor == 1);
		}

		private void SkipButton_OnClick()
		{
			DialogueIndex = loadedConversation.dialogues.Count;
			StopAllCoroutines();
			dialogueTextDisplay.text = "";
			StartCoroutine(Type("Conversation Completed"));
		}

	}


}