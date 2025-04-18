using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLoader : MonoBehaviour
{
    public List<LevelDialoguePair> dialoguePairs;

    [System.Serializable]
    public class LevelDialoguePair
    {
        public string levelName;
        public LinesSO dialogueScript;
    }

    public ScriptManager scriptManager;

    void Start()
    {
        string levelName = LevelLoader.Instance.levelToLoad;
        foreach (var pair in dialoguePairs)
        {
            if (pair.levelName == levelName)
            {
                scriptManager.LoadScript(pair.dialogueScript);
                scriptManager.StartScript();
                break;
            }
        }
    }
}
