using UnityEngine;
using System.IO;
using UnityEngine.UI;
using OpenAI;

public class SaveSystem : MonoBehaviour
{

    [SerializeField] private InputField apiKeyInputField;
    [SerializeField] private InputField backstoryInputField;
    [SerializeField] private Button saveConfigButton;
    [SerializeField] private Button openConfigButton;
    [SerializeField] private GameObject configPanel;

    private PlayerData data;
    public PlayerData PlayerData => data;

    private string filePath;

    public static SaveSystem Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        filePath = Application.persistentDataPath + "/savefile.json";
        data = Load();
        if (data == null)
        {
            configPanel.SetActive(true);
        }
        else
        {
            ChatGPT.Instance.Init(data.backstory, data.apiKey);
        }
        Debug.Log("Save path: " + Application.persistentDataPath + "/savefile.json");
    }

    private void Start() 
    {
        saveConfigButton.onClick.AddListener(SaveConfig);
        openConfigButton.onClick.AddListener(OpenConfig);   
    }

    public void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public PlayerData Load()
    {
        if (File.Exists(filePath))
        {
            
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            return null;
        }
    }

    private void SaveConfig()
    {
        data = new PlayerData();
        data.apiKey = apiKeyInputField.text;
        data.backstory = backstoryInputField.text;
        Save(data);
        configPanel.SetActive(false);
        ChatGPT.Instance.Init(data.backstory, data.apiKey);
    }

    private void OpenConfig()
    {
        configPanel.SetActive(true);
        apiKeyInputField.text = data.apiKey;
        backstoryInputField.text = data.backstory;
        
    }
}