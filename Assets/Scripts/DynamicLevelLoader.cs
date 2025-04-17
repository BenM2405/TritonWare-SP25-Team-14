using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class DynamicLevelLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    void Start()
    {
        LoadLevelButtons();
    }

    void LoadLevelButtons()
    {
        //Jsons
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");

        foreach (TextAsset level in levelFiles)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            btnObj.name = $"Btn_{level.name}";
            
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = level.name;

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                LevelLoader.Instance.LoadLevel(level.name);
            });
        }
    }

    public void RegenerateButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        LoadLevelButtons();
    }

}
