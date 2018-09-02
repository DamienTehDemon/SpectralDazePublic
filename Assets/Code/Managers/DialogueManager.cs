﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    public Sprite[] CharacterSprites;
    public List<AudioClip> BeeGibberish;
    public List<AudioClip> StichesGibberish;

    public Image CharacterImage { get; set; }
    public Text CharacterName { get; set; }
    public Text DialogueText { get; set; }

    DateTime OnEnterTime;

    public Conversation TestConversation { get; set; }

    public GameObject DialogueParentObj;
    public Animator TextAnimator;
    public AudioClip NextDialogueSfxClip;

    public bool IsDialogueActive = false;

    private Queue<DialogueLine> DialogueQueue;

    public AudioSource myAudioSource;

    public bool IsQueueEmpty
    {
        get
        {
            if (!DialogueQueue.Any()) return true;
            else
                return false;
        }
    }

    private void Start()
    {
        CharacterImage = GameObject.FindGameObjectWithTag("CharacterPortrait").GetComponent<Image>();
        CharacterName = GameObject.FindGameObjectWithTag("CharacterName").GetComponent<Text>();
        DialogueText = GameObject.FindGameObjectWithTag("DialogueText").GetComponent<Text>();
        TextAnimator = GameObject.FindGameObjectWithTag("DialogueText").GetComponent<Animator>();
        myAudioSource = GetComponent<AudioSource>();

        DialogueParentObj.SetActive(false);
        DialogueQueue = new Queue<DialogueLine>();
    }

    public void StartDialogue(Conversation dialogue)
    {
        DialogueQueue = new Queue<DialogueLine>(dialogue.DialogueLines);
        OnEnterTime = DateTime.Now;
        CycleDialogue();
    }


    public void DisplayText()
    {
        ShowDialogueBox();
        TextAnimator.Play("Anim_NextDialogue");

        var Line = DialogueQueue.Dequeue();

        CharacterImage.sprite = GetCharacterSprite(Line.LineCharacter);
        CharacterName.text = GetCharacterName(Line.LineCharacter);
        DialogueText.text = Line.LineText;
        PlayVoice(Line.LineCharacter);
    }

    private Sprite GetCharacterSprite(Character lineCharacter)
    {
        switch (lineCharacter)
        {
            case Character.Be:
                return CharacterSprites[0];
            case Character.Stitches:
                return CharacterSprites[1];
        }
        return CharacterSprites[0];
    }

    private string GetCharacterName(Character lineCharacter)
    {
        switch (lineCharacter)
        {
            case Character.Be:
                return "Be:";
            case Character.Stitches:
                return "Stiches:";
        }
        return "???:";
    }

    private void PlayVoice(Character lineCharacter)
    {
        AudioClip tmpAudioClip;
        var rnd = new System.Random();

        switch (lineCharacter)
        {
            case  Character.Be:
                tmpAudioClip = BeeGibberish[rnd.Next(BeeGibberish.Count - 1)];
                break;
            default:
                tmpAudioClip = StichesGibberish[rnd.Next(StichesGibberish.Count - 1)];
                break;
        }

        myAudioSource.Stop();
        myAudioSource.clip = tmpAudioClip;
        myAudioSource.Play();
    }

    public void ShowDialogueBox()
    {
        DialogueParentObj.SetActive(true);
    }

    public void HideDialogueBox()
    {
        DialogueParentObj.SetActive(false);
    }

    public void CycleDialogue()
    {
        if (IsQueueEmpty)
            HideDialogueBox();
        else
            DisplayText();
    }
}
