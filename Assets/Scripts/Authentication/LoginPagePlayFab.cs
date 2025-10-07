using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
public class LoginPagePlayFab : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TopText;
    [SerializeField] TextMeshProUGUI SystemText;
    [Header("Login")]
    [SerializeField] TMP_InputField EmailLoginInput;
    [SerializeField] TMP_InputField PasswordLoginInput;
    [SerializeField] GameObject LoginPage;
    [Header("Register")]
    [SerializeField] TMP_InputField UserRegisterInput;
    [SerializeField] TMP_InputField EmailRegisterInput;
    [SerializeField] TMP_InputField PasswordRegisterInput;
    [SerializeField] TMP_InputField ConfirmPasswordRegisterInput;
    [SerializeField] GameObject RegisterPage;
    [Header("Recovery")]
    [SerializeField] TMP_InputField EmailRcoveryInput;
    [SerializeField] GameObject RecoveryPage;
    [Header("CreateAccount")]
    [SerializeField] GameObject CreateAccountPage;

    [Header("Scene Flow")]
    [SerializeField] string nextSceneName = "EducationBuilding";
    private void Awake()
    {
        // Set TitleId 1 lần cho chắc (đổi sang TitleId của bạn nếu khác)
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = "4C053";
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    #region Button Functions
    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(UserRegisterInput.text) || string.IsNullOrEmpty(EmailRegisterInput.text) || string.IsNullOrEmpty(PasswordRegisterInput.text) || string.IsNullOrEmpty(ConfirmPasswordRegisterInput.text))
        {
            SystemText.text = "Please fill in all fields";
            return;
        }
        if (PasswordRegisterInput.text != ConfirmPasswordRegisterInput.text)
        {
            SystemText.text = "Passwords do not match";
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {
            Email = EmailRegisterInput.text,
            Password = PasswordRegisterInput.text,
            RequireBothUsernameAndEmail = true,
            Username = UserRegisterInput.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }
    public void Login()
    {
        if (string.IsNullOrEmpty(EmailLoginInput.text) || string.IsNullOrEmpty(PasswordLoginInput.text))
        {
            SystemText.text = "Email and password are required";
            return;
        }
        var request = new LoginWithEmailAddressRequest
        {
            Email = EmailLoginInput.text,
            Password = PasswordLoginInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        SystemText.text = "Login Successful";
        Debug.Log("Login Successful");
        // Proceed to the next scene or main menu
        // 1) Lưu trạng thái đăng nhập để scene sau đọc
        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.SetLogin(result);
        }
        else
        {
            // Phòng khi bạn quên đặt GlobalGameState trong scene Login
            Debug.LogWarning("[Login] GlobalGameState is missing in the Login scene. Add it to avoid losing login state.");
        }

        // 2) Chuyển scene
        if (!string.IsNullOrEmpty("MainGame"))
        {
            SceneManager.LoadScene("MainGame");
        }
        else
        {
            // fallback: sang scene kế tiếp trong Build Settings
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void RecoverUser()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = EmailRcoveryInput.text,
            TitleId = PlayFabSettings.staticSettings.TitleId // Replace with your PlayFab title ID
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnRecoverSuccess, OnError);
    }

    private void OnRecoverSuccess(SendAccountRecoveryEmailResult result)
    {
        OpenLoginPage();
        SystemText.text = "Recovery Email Sent";
    }

    private void OnError(PlayFabError error)
    {
        SystemText.text = error.ErrorMessage;
        Debug.Log("Error: " + error.GenerateErrorReport());
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        // SystemText.text = "New Accoubt Created";
        OpenCreateAccountPage();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenLoginPage()
    {
        CreateAccountPage.SetActive(false);
        LoginPage.SetActive(true);
        RegisterPage.SetActive(false);
        RecoveryPage.SetActive(false);
        TopText.text = "Login";
    }
    public void OpenRegisterPage()
    {
        LoginPage.SetActive(false);
        RegisterPage.SetActive(true);
        RecoveryPage.SetActive(false);
        CreateAccountPage.SetActive(false);
        TopText.text = "Resgister";
    }
    public void OpenRecoveryPage()
    {
        LoginPage.SetActive(false);
        RegisterPage.SetActive(false);
        RecoveryPage.SetActive(true);
        CreateAccountPage.SetActive(false);
        TopText.text = "Recovery";
    }
    public void OpenCreateAccountPage()
    {
        CreateAccountPage.SetActive(true);
        LoginPage.SetActive(false);
        RegisterPage.SetActive(false);
        RecoveryPage.SetActive(false);
        TopText.text = "Create Account";
    }
    #endregion
}
