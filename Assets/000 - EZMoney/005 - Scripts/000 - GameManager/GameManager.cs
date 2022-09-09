using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyBox;

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
            SceneController.CurrentScene = "GameplayScene";
            /*if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
                PlayFabSettings.TitleId = "C1147";*/
        }
    }

    #region ERRORS
    public void DisplayDualLoginErrorPanel()
    {
        DualLogInErrorPanel.SetActive(true);
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
        if (SceneController.CurrentScene == "AdventureScene")
            StartCoroutine(DelayPanelReactivation());
        else
        {
            ErrorPanel.SetActive(false);
            PanelActivated = false;
        }
    }

    private IEnumerator DelayPanelReactivation()
    {
        ErrorPanel.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        PanelActivated = false;
    }
    #endregion
}
