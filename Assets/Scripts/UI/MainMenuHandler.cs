using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class MainMenuHandler : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _recordText;
    
    public void LoadScene(int sceneId)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneId);
    }
    public void ClearRecord()
    {
        PlayerPrefs.SetInt("Record", 0);
        UpdateRecordText();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += (scene, mode) => OnLevelLoaded(scene);
    }

    private void OnLevelLoaded(Scene scene)
    {
        UpdateRecordText();
    }

    private void UpdateRecordText()
    {
        int record = PlayerPrefs.GetInt("Record", 0);
        _recordText.text = $"RECORD: {record.ToString("D6")}";
    }
}
