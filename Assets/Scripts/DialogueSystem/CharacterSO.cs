using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "ScriptableObjects/DialogueSystem/CharacterSO", order = 1)]
public class CharacterSO : ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public Color ColorAccent;
    public string talkingSFX;
}
