using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private GameObject dialogueManager;
    private Vector3 previousPosition;

    public void TriggerNextSegment()
    {
        dialogueManager.GetComponent<ScriptManager>().NextSegment();
    }

    public void SetActive(bool active)
    {
        if (!active)
        {
            previousPosition = transform.localPosition;
            transform.localPosition = new Vector3(0, -10000, 0);
        }
        else
        {
            transform.localPosition = previousPosition;
        }
    }
}
