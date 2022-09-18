using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyBox;
using PlayFab;
using PlayFab.ServerModels;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    //==================================================================================
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                    _instance = new GameObject().AddComponent<GameManager>();
            }

            return _instance;
        }
    }

    [field: SerializeField] public List<GameObject> GameMangerObj { get; set; }
    [field: SerializeField] public bool CheatsActivated { get; set; }
    [field: SerializeField] public bool DebugMode { get; set; }
    [SerializeField] private string SceneToLoad;
    [field: SerializeField] public bool PanelActivated { get; set; }

    [field: Header("CAMERA")]
    [field: SerializeField] public Camera MyUICamera { get; set; }
    [field: SerializeField] public Camera MainCamera { get; set; }

    [field: Header("ERROR")]
    [field: SerializeField] private GameObject DualLogInErrorPanel { get; set; }
    [field: SerializeField] private TextMeshProUGUI SpecialErrorTMP { get; set; }
    [field: SerializeField] private GameObject ErrorPanel { get; set; }
    [field: SerializeField] private TextMeshProUGUI ErrorTMP { get; set; }

    [field: Header("MISCELLANEOUS SCRIPTS")]
    [field: SerializeField] public SceneController SceneController { get; set; }
    [field: SerializeField] public AnimationsLT AnimationsLT { get; set; }
    [field: SerializeField] public InputManager InputManager { get; set; }
    [field: SerializeField] public AudioManager BGMAudioManager { get; set; }
    [field: SerializeField] public AudioManager SFXAudioManager { get; set; }

    [field: Header("CHARACTERS")]
    [field: SerializeField] public List<CharacterData> AllCharacters { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool CanUseButtons { get; set; }

    //==================================================================================

    private void Awake()
    {
        if (_instance != null)
        {
            for (int a = 0; a < GameMangerObj.Count; a++)
                Destroy(GameMangerObj[a]);
        }

        for (int a = 0; a < GameMangerObj.Count; a++)
            DontDestroyOnLoad(GameMangerObj[a]);
    }

    private void Start()
    {
        if (DebugMode)
            SceneController.CurrentScene = SceneToLoad;
        else
        {
            SceneController.CurrentScene = "EntryScene";

            PlayFabServerAPI.GetTitleData(new GetTitleDataRequest(),
                resultCallback =>
                {
                    if (resultCallback.Data.ContainsKey("Version") && resultCallback.Data["Version"] != Application.version)
                        DisplaySpecialErrorPanel("Game is outdated. Please update.");
                },
                errorCallback =>
                {

                });
        }
    }

    #region ERRORS
    public void DisplayDualLoginErrorPanel()
    {
        DisplaySpecialErrorPanel("You have logged into another device");
    }

    public void DisplaySpecialErrorPanel(string _message)
    {
        DualLogInErrorPanel.SetActive(true);
        SpecialErrorTMP.text = _message;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void DisplayErrorPanel(string errorMessage)
    {
        ErrorPanel.SetActive(true);
        PanelActivated = true;
        ErrorTMP.text = errorMessage;
    }

    public void CloseErrorPanel()
    {
        ErrorPanel.SetActive(false);
        PanelActivated = false;
    }
    #endregion

    public string DeserializeStringValue(string value, string key)
    {
        Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

        return result[key];
    }

    public int DeserializeIntValue(string value, string key)
    {
        Dictionary<string, int> result = JsonConvert.DeserializeObject<Dictionary<string, int>>(value);

        return result[key];
    }

    public string SerializeIntValue(List<string> keyValues, List<int> values)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();

        for (int a = 0; a < keyValues.Count; a++)
            dict.Add(keyValues[a], values[a]);

        return JsonConvert.SerializeObject(dict);
    }

    public CharacterData GetProperCharacter(string characterID)
    {
        foreach(CharacterData character in AllCharacters)
            if (character.animalID == characterID)
                return character;
        return null;
    }
}
