using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptManager : MonoBehaviour
{
    [SerializeField] private LinesSO scriptLines;
    private CharacterSO currentCharacter;
    private bool isSceneRunning = false;

    void Start()
    {
        loadLines();
        debugLines();
    }

    public void SwapScripts(LinesSO linesSO)
    {
        scriptLines = linesSO;
    }

    // call NextLine (returns Character line) until it returns null (end of script)
    // send the next line into display handling
    // if the call returns null, call end scene handling (whether it transfers to gameplay or another scene idk)

    public void StartScript()
    {
        if (!isSceneRunning)
        {
            isSceneRunning = true;
            NextLine();
        }
        else
        {
            Debug.LogError("Scene is already running! Use NextLine()");
        }
    }

    public void NextLine()
    {
        if (isSceneRunning)
        {
            // load next line
        }
        else
        {
            Debug.LogError("Start script first! - StartScript()");
        }
    }












    [ContextMenu("Debug Lines")]
    private void debugLines()
    {
        scriptLines.debugLines();
    }

    [ContextMenu("Load Lines")]
    private void loadLines()
    {
        scriptLines.LoadLines();
    }
}
