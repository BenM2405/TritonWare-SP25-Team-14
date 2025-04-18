using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class ScriptManager : MonoBehaviour
{
    [SerializeField] private LinesSO scriptLines;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Image colorAccent;
    [SerializeField] private Button continueButton;
    [SerializeField] private Animator animator;
    private FMOD.Studio.EventInstance characterTalkingEvent;
    [SerializeField] private float characterTextSpeed = 20f; // Formula: (How many seconds per letter printed) = 1 / characterTextSpeed
    private LinesSO.CharacterLine nextSegment;

    private bool isSceneRunning = false;
    private bool isSegmentRunning = false;
    private int state = 0;


    // TODO: Have the dialogue pop up and pop down when scene is playing / ending, implement sprite animations for talking, Lerp or Slerp colors to transition between color accents
    // maybe implementing sound talking sfx swaps, speed increases / decreases, who knows lol

    void Start()
    {
        loadLines();
        StopAllCoroutines();
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        animator.SetBool("isSceneRunning", false);
    }
    void Update()
    {
        if (isSegmentRunning && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            playerSkipSegment();
        }
    }


    public void LoadScript(LinesSO linesSO)
    {
        scriptLines = linesSO;
        scriptLines.LoadLines();
    }

    [ContextMenu("Start Script")]
    public void StartScript()
    {
        if (!isSceneRunning)
        {
            isSceneRunning = true;
            animator.SetBool("isSceneRunning", true);
            scriptLines.SetLineIndex(0);
            NextSegment();
        }
        else
        {
            Debug.LogError("Scene is already running! Use NextSegment()");
        }
    }

    [ContextMenu("Next Segment")]
    public void NextSegment()
    {
        if (isSceneRunning)
        {
            //Debug.Log("Loading next segment!");
            isSegmentRunning = true;
            DisableContinueButton();
            StopAllCoroutines();
            nextSegment = scriptLines.NextSegment();
            if (nextSegment != null)
            {
                LoadCharacterData(nextSegment.Character);
                ReadLines();
            }
            else
            {
                isSegmentRunning = false;
                EndDialogue();
            }
        }
        else
        {
            Debug.LogError("Start script first! - StartScript()");
        }
    }

    public void ReadLines()
    {
        //Debug.Log("Reading lines...");
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        StopAllCoroutines();
        characterTalkingEvent.start();
        StartCoroutine(TypeSentence());
    }

    public void EndDialogue()
    {
        isSceneRunning = false;
        animator.SetBool("isSceneRunning", false);
        Debug.Log("Dialogue has ended!");

        GameConfig.isStoryMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }

    private void LoadCharacterData(CharacterSO character)
    {
        //Debug.Log("Loading character data...");
        characterName.text = character.Name;
        colorAccent.color = character.ColorAccent;
        characterTalkingEvent = FMODUnity.RuntimeManager.CreateInstance(character.talkingSFX);

        Animator portraitAnimator = characterPortrait.GetComponent<Animator>();

        if (character.PortraitSpriteAnimatorController != null)
        {
            characterPortrait.GetComponent<Animator>().runtimeAnimatorController = character.PortraitSpriteAnimatorController;
            characterPortrait.GetComponent<Animator>().SetInteger("state", state);
        }
        else if (portraitAnimator != null)
        {
            portraitAnimator.runtimeAnimatorController = null;
        }
        characterPortrait.sprite = character.Sprite;
    }

    IEnumerator TypeSentence()
    {
        dialogueText.text = "";
        int lineIndex = 0;
        string sentence;
        while (true)
        {
            do
            {
                if (lineIndex >= nextSegment.lines.Count)
                {
                    Debug.Log("Reached end of segment!");
                    yield return new WaitForSeconds(0.5f);  // Trying to prevent popping noise from occurring by allowing safeStop before stopping
                    characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    EnableContinueButton();
                    isSegmentRunning = false;
                    yield break;
                }

                //Debug.Log("Reading line " + lineIndex);
                sentence = nextSegment.lines[lineIndex];
                if (sentence == null)
                {
                    //Debug.Log("Dialogue not found, running command!");
                    yield return runLineCommand(nextSegment.lineCommands[lineIndex]);
                }
                lineIndex++;
            } while (sentence == null);
            //Debug.Log("Printing line dialogue " + lineIndex);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("safeStop", 0);
            characterPortrait.GetComponent<Animator>().SetBool("isSpeaking", true);
            for (int i = 0; i < sentence.Length; i++)
            {
                dialogueText.text += sentence[i];
                yield return new WaitForSeconds(1 / characterTextSpeed);
            }

            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("safeStop", 1);
            characterPortrait.GetComponent<Animator>().SetBool("isSpeaking", false);
        }
    }

    private void playerSkipSegment()
    {
        StopAllCoroutines();
        Debug.Log("Skipped segment!");
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        characterPortrait.GetComponent<Animator>().SetBool("isSpeaking", false);
        EnableContinueButton();
        String temp = "";
        for (int i = 0; i < nextSegment.lines.Count; i++)
        {
            temp += nextSegment.lines[i];
        }
        dialogueText.text = temp;
        isSegmentRunning = false;
    }

    private object runLineCommand(LinesSO.LineCommand lineCommand)
    {
        // TODO: Special command logic handled here
        Debug.Log(lineCommand.ToString());

        switch (lineCommand)
        {
            case LinesSO.LineCommand.Action_WAIT:
                return new WaitForSeconds(1f);

            case LinesSO.LineCommand.Action_CONTINUE:
                StopAllCoroutines();
                nextSegment = scriptLines.NextSegment();
                LoadCharacterData(nextSegment.Character);
                characterTalkingEvent.start();
                StartCoroutine(TypeSentence());
                break;

            case LinesSO.LineCommand.Format_THINK:
                state = 3;
                dialogueText.fontStyle = FontStyles.Italic;
                break;

            case LinesSO.LineCommand.Format_SAD:
                state = 2;
                dialogueText.fontStyle = FontStyles.Normal;
                break;

            case LinesSO.LineCommand.Format_YELL:
                state = 1;
                dialogueText.fontStyle = FontStyles.Bold;
                break;

            case LinesSO.LineCommand.Format_NORMAL:
                state = 0;
                dialogueText.fontStyle = FontStyles.Normal;
                break;
        }
        characterPortrait.GetComponent<Animator>().SetInteger("state", state);
        return null;
    }

    private void DisableContinueButton()
    {
        continueButton.GetComponent<ContinueButton>().SetActive(false);
    }

    private void EnableContinueButton()
    {
        continueButton.GetComponent<ContinueButton>().SetActive(true);
    }

    [ContextMenu("Debug Lines")]
    private void debugLines()
    {
        scriptLines.debugLines();
    }

    private void loadLines()
    {
        scriptLines.LoadLines();
    }
}
