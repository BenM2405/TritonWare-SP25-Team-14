using System.Collections;
using UnityEngine;


public class DialogueLoader : MonoBehaviour
{
    [SerializeField] private ScriptManager scriptManager;
    void Start()
    {
        string levelName = LevelLoader.Instance.levelToLoad;
        Debug.Log($"[DialogueLoader] Attempting to load: Resources/Dialogue/Level{levelName}");

        LinesSO loadedScript = Resources.Load<LinesSO>($"Dialogue/Level{levelName}");

        if (loadedScript != null)
        {
            scriptManager.LoadScript(loadedScript);

            // 💡 Skip START_PUZZLE if we're resuming after puzzle
            if (GameConfig.resumePostPuzzle)
            {
                GameConfig.resumePostPuzzle = false;
                GameConfig.completedPostPuzzle = true;
                SkipUntilAfterPuzzle(loadedScript);
            }

            scriptManager.StartScript();
        }
        else
        {
            Debug.LogError($"[DialogueLoader] Could not find dialogue asset: Dialogue/Level{levelName}.asset");
        }
    }

    void SkipUntilAfterPuzzle(LinesSO linesSO)
    {
        //Skip segments until after the first START_PUZZLE
        int index = 0;
        var allSegments = linesSO.GetFormattedScript(); // You might need to expose this with a getter

        while (index < allSegments.Count)
        {
            if (allSegments[index].lineCommands.Contains(LinesSO.LineCommand.Action_START_PUZZLE))
            {
                index++; // skip the one with START_PUZZLE
                break;
            }
            index++;
        }

        linesSO.SetLineIndex(index);
    }

}
