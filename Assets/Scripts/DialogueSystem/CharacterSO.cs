using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "ScriptableObjects/DialogueSystem/CharacterSO", order = 1)]
public class CharacterSO : ScriptableObject
{
    public string Name;
    public Sprite Sprite;   // TODO: maybe add animation capabilities to this
    public Color ColorAccent;
    public string talkingSFX;
    public AnimatorController PortraitSpriteAnimatorController;
}
