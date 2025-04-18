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
    [SerializeField] private GameObject levelLoader;
    private FMOD.Studio.EventInstance characterTalkingEvent;
    private FMOD.Studio.EventInstance backgroundMusicEvent;
    [SerializeField] private float characterTextSpeed = 20f; // Formula: (How many seconds per letter printed) = 1 / characterTextSpeed
    private LinesSO.CharacterLine nextSegment;

    private bool isSceneRunning = false;
    private bool isSegmentRunning = false;
    private bool isQueuedToStartPuzzle = false;
    private int state = 0;

    void Start()
    {
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
        isSceneRunning = true;
    }

    [ContextMenu("Start Script")]
    public void StartScript()
    {
        animator.gameObject.SetActive(true);

        StopAllCoroutines();
        backgroundMusicEvent = FMODUnity.RuntimeManager.CreateInstance(scriptLines.BackgroundMusicPath);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("safeStop", 1);
        backgroundMusicEvent.start();
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        isSceneRunning = true;
        animator.SetBool("isSceneRunning", true);
        NextSegment();
    }



    [ContextMenu("Next Segment")]
    public void NextSegment()
    {
        if (isQueuedToStartPuzzle)
        {
            isQueuedToStartPuzzle = false;
            isSegmentRunning = false;
            animator.SetBool("isSceneRunning", false); // hides the dialogue box if animated
            dialogueText.text = "";
            characterName.text = "";
            characterPortrait.sprite = null;
            LevelLoader.Instance.LoadLevel(scriptLines.NextToLoad);
            return;
        }

        if (isSceneRunning)
        {
            animator.SetBool("isSceneRunning", true);
            isSegmentRunning = true;
            DisableContinueButton();
            StopAllCoroutines();

            nextSegment = scriptLines.NextSegment(); // ← now we know it's safe
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
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        StopAllCoroutines();
        characterTalkingEvent.start();
        StartCoroutine(TypeSentence());
    }

    public void EndDialogue()
    {
        isSceneRunning = false;
        animator.SetBool("isSceneRunning", false);
        backgroundMusicEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        StartCoroutine(DelayForNextScene());
    }

    IEnumerator DelayForNextScene()
    {
        yield return new WaitForSeconds(0.5f);

        if (GameConfig.completedPostPuzzle)
        {
            GameConfig.completedPostPuzzle = false;
            GameConfig.resumePostPuzzle = false;
            GameConfig.isStoryMode = false;
            GameConfig.openStoryCanvas = true;
            SceneManager.LoadScene("MainMenu");
        }
        else if (scriptLines.isNextScenePuzzle)
        {
            GameConfig.isStoryMode = true;

            if (levelLoader != null)
            {
                levelLoader.GetComponent<LevelLoader>().LoadLevel(scriptLines.NextToLoad);
            }

            SceneManager.LoadScene("PuzzleScene");
        }
        else
        {
            GameConfig.isStoryMode = false;
            GameConfig.resumePostPuzzle = false;
            GameConfig.openStoryCanvas = true;

            SceneManager.LoadScene("MainMenu");
        }
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
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        characterPortrait.GetComponent<Animator>().SetBool("isSpeaking", false);

        string temp = "";
        for (int i = 0; i < nextSegment.lines.Count; i++)
        {
            if (nextSegment.lineCommands[i] == LinesSO.LineCommand.Action_START_PUZZLE)
            {
                runLineCommand(LinesSO.LineCommand.Action_START_PUZZLE);
                dialogueText.text = ""; // Don’t show further text
                isSegmentRunning = false;
                return; // ← stop here to wait for user
            }
            temp += nextSegment.lines[i];
        }

        dialogueText.text = temp;
        isSegmentRunning = false;
        EnableContinueButton();
    }


    private object runLineCommand(LinesSO.LineCommand lineCommand)
    {
        Debug.Log(lineCommand.ToString());

        switch (lineCommand)
        {
            case LinesSO.LineCommand.Action_WAIT:
                return new WaitForSeconds(1f);

            case LinesSO.LineCommand.Action_CONTINUE:
                StopAllCoroutines();
                isSegmentRunning = false;
                EnableContinueButton();
                break;

            case LinesSO.LineCommand.Action_START_PUZZLE:
                //TODO: This section needs to start the puzzle found in the scene. On the puzzle side, upon level completion, the script needs to call NextSegment() located here
                Debug.Log("[ScriptManager] START_PUZZLE command received.");
                // this should only be called when the dialogue is actively in a puzzle
                // make sure when skipping, it does not skip this format
                // when continue is hit, hide the dialogue box and start the puzzle
                isQueuedToStartPuzzle = true;
                EnableContinueButton();
                // when puzzle is complete, call NextSegment() which should show the dialogue box
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

    public void ShowDialogueBox()
    {
        animator.gameObject.SetActive(true);
        animator.SetBool("isSceneRunning", true);
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
