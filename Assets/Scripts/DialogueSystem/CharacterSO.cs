using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "ScriptableObjects/DialogueSystem/CharacterSO", order = 1)]
public class CharacterSO : ScriptableObject
{
    public string Name;
    public Sprite Sprite;   // TODO: maybe add animation capabilities to this
                            // TODO: Insert FMOD SFX Event for speaking sfx
    public Sprite ColorAccent;
}
