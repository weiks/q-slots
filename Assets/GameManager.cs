using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QuartersSDK;
using PlayFab;
using PlayFab.ClientModels;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    private bool QuartersSetupCompleted = false;

    private int quartersBalance;

    //Awake is always called before any Start functions
    void Awake()
    {
        ////Check if instance already exists
        //if (instance == null)

        //    //if not, set instance to this
        //    instance = this;

        ////If instance already exists and it's not this:
        //else if (instance != this)

        //    //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
        //    Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        PlayerPrefs.DeleteKey("QuartersRefreshToken");

        LoadSceneAsync();
        LoginToPlayFab();
    }

    void LoginToPlayFab() {
        //login user to playfab title using device id
        LoginWithCustomIDRequest loginRequest = new LoginWithCustomIDRequest();
        loginRequest.CustomId = SystemInfo.deviceUniqueIdentifier;
        loginRequest.CreateAccount = true;

        PlayFabClientAPI.LoginWithCustomID(loginRequest, delegate (LoginResult result) {
            Debug.Log("Playfab user logged in: " + result.PlayFabId);
        }, delegate (PlayFabError error) {
            Debug.LogError(error.ErrorMessage);
        });
    }

    void LoadSceneAsync() {
        //Start loading the Scene asynchronously and output the progress bar
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene() {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Game");
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;
        // Debug.Log("Pro :" + asyncOperation.progress);
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            // Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                // Debug.Log("Scene is ready");

                //Wait to you press the space key to activate the Scene
                if (QuartersSetupCompleted)
                {
                    //quartersBalance = PlayerPrefs.GetInt("quartersBalance");

                    //Activate the Scene
                    asyncOperation.allowSceneActivation = true;

                }
            }

            yield return null;
        }
    }

    public void Credits() {
        Debug.Log("loading credits..");
        SceneManager.LoadScene("Credits");
    }

    public void GetAccountBalance() {
        Quarters.Instance.GetAccountBalance(delegate (User.Account.Balance balance) {
            quartersBalance = (int)balance.quarters;
            Debug.Log("Quarters balance: " + (int)balance.quarters);
            PlayerPrefs.SetInt("quartersBalance", quartersBalance);
            Debug.Log("loading game..");
            QuartersSetupCompleted = true;
        }, delegate (string error) {

        });
    }

    public void Deauthorize () {
        Quarters.Instance.Deauthorize();
    }

    public void Play () {
        //if (Quarters.Instance.IsAuthorized) {
            //GetAccountBalance();
        //} else {
            Debug.Log("authorizing quarters..");
            Quarters.Instance.Authorize(OnAuthorizationSuccess, OnAuthorizationFailed);
        //}
    }

    private void OnAuthorizationSuccess()
    {
        Debug.Log("OnAuthorizationSuccess");
        GetAccountBalance();
    }

    private void OnAuthorizationFailed(string error)
    {
        Debug.LogError("OnAuthorizationFailed: " + error);

    }
}
