using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float characterTextSpeed = 20f;

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

    void OnDisable()
    {
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        StopAllCoroutines();
    }

    public void LoadScript(LinesSO lines)
    {
        scriptLines = lines;
        scriptLines.LoadLines();
        isSceneRunning = true;
    }

    [ContextMenu("Start Script")]
    public void StartScript()
    {
        animator.gameObject.SetActive(true);
        StopAllCoroutines();

        if (!scriptLines.BackgroundMusicPath.Equals(LevelLoader.Instance.musicPath))
        {
            LevelLoader.Instance.StopMusic();
            LevelLoader.Instance.SetMusic(scriptLines.BackgroundMusicPath);
            LevelLoader.Instance.StartMusic();
        }

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("safeStop", 1);
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
            StopAllCoroutines();
            isQueuedToStartPuzzle = false;
            isSegmentRunning = false;
            animator.SetBool("isSceneRunning", false);
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
    }

    public void ReadLines()
    {
        StopAllCoroutines();
        characterTalkingEvent.start();
        StartCoroutine(TypeSentence());
    }

    public void EndDialogue()
    {
        isSceneRunning = false;
        animator.SetBool("isSceneRunning", false);
        LevelLoader.Instance.StopMusic();
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
            LevelLoader.Instance.LoadLevel(scriptLines.NextToLoad);
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
        if (character == null)
        {
            Debug.LogWarning("[ScriptManager] Skipping character load because CharacterSO is null.");
            return;
        }

        characterName.text = character.Name;
        colorAccent.color = character.ColorAccent;
        characterTalkingEvent = FMODUnity.RuntimeManager.CreateInstance(character.talkingSFX);

        characterPortrait.sprite = character.Sprite;

        var anim = characterPortrait.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetInteger("state", state);
        }
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
                    yield return new WaitForSeconds(0.5f);
                    characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    EnableContinueButton();
                    isSegmentRunning = false;
                    yield break;
                }

                sentence = nextSegment.lines[lineIndex];
                if (sentence == null)
                {
                    yield return runLineCommand(nextSegment.lineCommands[lineIndex]);
                }

                lineIndex++;
            } while (sentence == null);

            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("safeStop", 0);
            characterPortrait.GetComponent<Animator>()?.SetBool("isSpeaking", true);

            for (int i = 0; i < sentence.Length; i++)
            {
                dialogueText.text += sentence[i];
                yield return new WaitForSeconds(1 / characterTextSpeed);
            }

            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("safeStop", 1);
            characterPortrait.GetComponent<Animator>()?.SetBool("isSpeaking", false);
        }
    }

    private void playerSkipSegment()
    {
        StopAllCoroutines();
        characterTalkingEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        characterPortrait.GetComponent<Animator>()?.SetBool("isSpeaking", false);

        string temp = "";
        for (int i = 0; i < nextSegment.lines.Count; i++)
        {
            if (nextSegment.lineCommands[i] == LinesSO.LineCommand.Action_START_PUZZLE)
            {
                runLineCommand(LinesSO.LineCommand.Action_START_PUZZLE);
                dialogueText.text = "";
                isSegmentRunning = false;
                return;
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
                isQueuedToStartPuzzle = true;
                NextSegment();
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

        characterPortrait.GetComponent<Animator>()?.SetInteger("state", state);
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
