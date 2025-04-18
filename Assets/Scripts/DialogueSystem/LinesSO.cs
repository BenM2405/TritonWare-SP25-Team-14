using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "LinesSO", menuName = "ScriptableObjects/DialogueSystem/LinesSO", order = 2)]
public class LinesSO : ScriptableObject
{
    // allow input of raw text
    // private methods to format it for scriptSO
    // public methods to run test format, and add method to allow scriptSO

    [TextArea(15, 20)]
    [SerializeField] private string rawText = "";
    [SerializeField] private List<CharacterLine> formattedScript = new List<CharacterLine>();
    [SerializeField] private List<CharacterSO> characters;
    public string BackgroundMusicPath;
    public string sceneID;
    public string NextToLoad;
    public bool isNextScenePuzzle;
    private int lineIndex = 0;

    public enum LineCommand
    {
        Action_WAIT,        // wait a brief period for a slight pause
        Action_CONTINUE,    // immediate move to next segment, regardless of player input
        Action_START_PUZZLE, // start a puzzle after dialogue is done
        Format_THINK,       // to show the person thinking (i.e. italics)
        Format_SAD,         // to show the person sad (normal text but may change sprite)
        Format_YELL,        // to show the person yelling (i.e. bold)
        Format_NORMAL,      // to show the person normal speaking (i.e. resets italics and bold)
        EMPTY               // empty in case a dialogue line exists
    }

    public void LoadLines()
    {
        Debug.Log("Formatting script...");
        format(rawText);
        Debug.Log($"Formatted {formattedScript.Count} dialogue segments.");
        if (formattedScript == null)
        {
            Debug.LogError("Formatted script not found!");
        }
    }

    public CharacterLine NextSegment()
    {
        if (lineIndex >= formattedScript.Count)
        {
            return null;
        }
        return formattedScript[lineIndex++];
    }

    public void SetLineIndex(int i) => lineIndex = i;

    private void format(string rawText)
    {
        formattedScript.Clear();

        if (rawText == null)
        {
            Debug.LogError("No raw text found!");
        }

        Debug.Log("Loading raw lines...");
        string[] rawTextLines = rawText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in rawTextLines) { Debug.Log(line); }
        CharacterSO previousCharacter = null;
        for (int i = 0; i < rawTextLines.Length; i++)
        {
            CharacterLine nextCharacterLine = new CharacterLine();

            string[] temp = rawTextLines[i].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            string rawDialogue;
            if (temp.Length == 2)
            {
                string characterName = temp[0];
                rawDialogue = temp[1];

                // Find CharacterSO that matches the raw text
                foreach (CharacterSO character in characters)
                {
                    if (characterName == character.Name)
                    {
                        nextCharacterLine.Character = character;
                        previousCharacter = character;
                        break;
                    }
                }
            }
            else
            {
                if (previousCharacter == null)
                {
                    Debug.LogError("Unable to find a character!");
                }
                nextCharacterLine.Character = previousCharacter;
                rawDialogue = temp[0];
            }



            // Adds the lines or commands from the rest of the raw dialogue to the CharacterLine object
            string[] splitText = rawDialogue.Split(new Char[] { '{', '}', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string split in splitText)
            {
                switch (split)
                {
                    case "WAIT":
                        nextCharacterLine.lineCommands.Add(LineCommand.Action_WAIT);
                        nextCharacterLine.lines.Add(null);
                        break;

                    case "CONTINUE":
                        nextCharacterLine.lineCommands.Add(LineCommand.Action_CONTINUE);
                        nextCharacterLine.lines.Add(null);
                        break;

                    case "START_PUZZLE":
                        nextCharacterLine.lineCommands.Add(LineCommand.Action_START_PUZZLE);
                        nextCharacterLine.lines.Add(null);
                        break;

                    case "THINK":
                        nextCharacterLine.lineCommands.Add(LineCommand.Format_THINK);
                        nextCharacterLine.lines.Add(null);
                        break;

                    case "YELL":
                        nextCharacterLine.lineCommands.Add(LineCommand.Format_YELL);
                        nextCharacterLine.lines.Add(null);
                        break;

                    case "SAD":
                        nextCharacterLine.lineCommands.Add(LineCommand.Format_SAD);
                        nextCharacterLine.lines.Add(null);
                        break;

                    case "NORMAL":
                        nextCharacterLine.lineCommands.Add(LineCommand.Format_NORMAL);
                        nextCharacterLine.lines.Add(null);
                        break;

                    default:
                        nextCharacterLine.lineCommands.Add(LineCommand.EMPTY);
                        nextCharacterLine.lines.Add(split.Trim() + " ");
                        break;
                }
            }
            formattedScript.Add(nextCharacterLine);
        }
        Debug.Log("Script formatted!");
    }

    public void debugLines()
    {
        Debug.Log(formattedScript);
        for (int i = 0; i < formattedScript.Count; i++)
        {
            CharacterLine currentLine = formattedScript[i];
            Debug.Log("Character: " + currentLine.Character.Name);

            for (int j = 0; j < currentLine.lines.Count; j++)
            {
                string dialogue = currentLine.lines[j];
                if (dialogue == null)
                {
                    Debug.Log("Command: " + currentLine.lineCommands[j].ToString());
                }
                else
                {
                    Debug.Log('"' + dialogue + '"');
                }
            }
        }
    }
    public List<CharacterLine> GetFormattedScript() => formattedScript;

    public class CharacterLine
    {
        public CharacterSO Character;
        public List<string> lines;
        public List<LineCommand> lineCommands;

        public CharacterLine()
        {
            Character = null;
            lines = new List<string>();
            lineCommands = new List<LineCommand>();
        }


    }
}
