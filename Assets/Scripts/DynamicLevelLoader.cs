using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

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
                PlayStoryLevel(level.name);
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

    public void PlayStoryLevel(string levelName)
    {
        LevelLoader.Instance.levelToLoad = levelName;
        GameConfig.isStoryMode = true;
        SceneManager.LoadScene("DialogueScene");
    }


}
