using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogueController : MonoBehaviour
{
    public Text dialogueText;
    public Voice[] Voices;

    public static DialogueController instance;
    public Queue<string> sentences;

    IEnumerator sentenceTyperCoroutine;

    #region SINGLETON
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    private void Start()
    {
        sentences = new Queue<string>();
    }
    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Starting a conversation with " + dialogue.name);

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
        }
        else
        {
            string sentence = sentences.Dequeue();

            if (sentenceTyperCoroutine != null) { StopCoroutine(sentenceTyperCoroutine); }

            sentenceTyperCoroutine = TypeSentence(sentence, GetVoiceByName("Companion"));
            StartCoroutine(sentenceTyperCoroutine);
        }
    }

    IEnumerator TypeSentence(string sentence, Voice voice)
    {
        print("bruh");
        dialogueText.text = "";
        foreach (string word in GameMath.DivideSentenceIntoWords(sentence))
        {
            if (word == "/")
            {
                dialogueText.text += ". ";
                yield return new WaitForSeconds(voice.slashPause);
            }
            else if(word == "." || word == "!" || word == "?")
            {
                dialogueText.text += word + " ";
                yield return new WaitForSeconds(voice.endOfSentencePause);
            }
            else if (word == ":" || word == ",")
            {
                dialogueText.text += word + " ";
                yield return new WaitForSeconds(voice.commaPause);
            }
            else
            {
                yield return new WaitForSeconds(voice.spacePause);
                dialogueText.text += " " + word;
            }
        }
    }
    void EndDialogue()
    {
        Debug.Log("End of conversation");
    }
    Voice GetVoiceByName(string name)
    {
        Voice V = Array.Find(Voices, Voice => Voice.name == name);
        return V;
    }
}
[System.Serializable]
public struct Voice
{
    public string name;
    public float endOfSentencePause;
    public float commaPause;
    public float slashPause;
    public float spacePause;
}
