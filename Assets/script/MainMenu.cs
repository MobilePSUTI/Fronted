using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private RectTransform uiPanel;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button forgotPasswordButton;

    [Header("Settings")]
    [SerializeField] private float minPasswordLength = 6;
    [SerializeField] private float maxPasswordLength = 30;
    [SerializeField] private float uiMoveDuration = 0.3f;

    private bool isProcessing;
    private Vector2 originalPanelPosition;
    private Coroutine movePanelCoroutine;

    private void Awake()
    {
        originalPanelPosition = uiPanel.anchoredPosition;

        // Initialize UI
        errorText.text = "";
        loginButton.onClick.AddListener(OnLoginButtonClick);
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordClick);

        // Add input field validation
        emailInput.onValueChanged.AddListener(ValidateEmailField);
        passwordInput.onValueChanged.AddListener(ValidatePasswordField);
    }

    private void Start()
    {
        // Check for saved session
        if (UserSession.HasSavedSession())
        {
            StartCoroutine(TryAutoLogin());
        }
    }

    #region Input Validation
    private void ValidateEmailField(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            SetInputFieldState(emailInput, false, "Email is required");
            return;
        }

        bool isValid = IsValidEmail(email);
        SetInputFieldState(emailInput, isValid, isValid ? "" : "Invalid email format");
        UpdateLoginButtonState();
    }

    private void ValidatePasswordField(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            SetInputFieldState(passwordInput, false, "Password is required");
            return;
        }

        bool isValid = password.Length >= minPasswordLength && password.Length <= maxPasswordLength;
        string error = isValid ? "" : $"Password must be {minPasswordLength}-{maxPasswordLength} characters";
        SetInputFieldState(passwordInput, isValid, error);
        UpdateLoginButtonState();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private void SetInputFieldState(TMP_InputField field, bool isValid, string errorMessage)
    {
        ColorBlock colors = field.colors;
        colors.normalColor = isValid ? Color.white : new Color(1, 0.8f, 0.8f);
        colors.selectedColor = isValid ? Color.white : new Color(1, 0.8f, 0.8f);
        field.colors = colors;

        if (!isValid && field.isFocused)
        {
            if (!string.IsNullOrEmpty(errorMessage) && errorText.text != errorMessage)
            {
                errorText.text = errorMessage;
            }
        }
    }

    private void UpdateLoginButtonState()
    {
        bool emailValid = IsValidEmail(emailInput.text);
        bool passwordValid = passwordInput.text.Length >= minPasswordLength &&
                            passwordInput.text.Length <= maxPasswordLength;

        loginButton.interactable = emailValid && passwordValid && !isProcessing;
    }
    #endregion

    #region Login Process
    private IEnumerator TryAutoLogin()
    {
        string savedToken = PlayerPrefs.GetString("AuthToken");
        string savedUserId = PlayerPrefs.GetString("UserId");

        if (string.IsNullOrEmpty(savedToken) || string.IsNullOrEmpty(savedUserId))
        {
            yield break;
        }

        SetProcessingState(true);

        yield return StartCoroutine(ApiClient.Instance.GetUserData(
            savedUserId,
            savedToken,
            (success, userData) => {
                if (success)
                {
                    userData.token = savedToken;
                    UserSession.CurrentUser = userData;
                    ProceedAfterLogin();
                }
                else
                {
                    UserSession.ClearSession();
                    SetProcessingState(false);
                }
            }
        ));
    }

    private void OnLoginButtonClick()
    {
        if (isProcessing) return;

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // Final validation
        if (!IsValidEmail(email))
        {
            errorText.text = "Please enter a valid email address";
            return;
        }

        if (password.Length < minPasswordLength || password.Length > maxPasswordLength)
        {
            errorText.text = $"Password must be {minPasswordLength}-{maxPasswordLength} characters";
            return;
        }

        StartCoroutine(LoginProcess(email, password));
    }

    private IEnumerator LoginProcess(string email, string password)
    {
        SetProcessingState(true);
        errorText.text = "";

        yield return StartCoroutine(ApiClient.Instance.Login(
            email,
            password,
            (success, token) => {
                if (!success)
                {
                    errorText.text = token; // Error message from server
                    SetProcessingState(false);
                    return;
                }

                // Get user data after successful login
                StartCoroutine(GetUserDataAfterLogin(token));
            }
        ));
    }

    private IEnumerator GetUserDataAfterLogin(string token)
    {
        yield return StartCoroutine(ApiClient.Instance.GetUserData(
            "current", // Special endpoint for current user
            token,
            (success, userData) => {
                if (!success)
                {
                    errorText.text = "Failed to load user data";
                    SetProcessingState(false);
                    return;
                }

                userData.token = token;
                UserSession.CurrentUser = userData;

                // Check if avatar needs to be loaded
                if (string.IsNullOrEmpty(userData.avatarUrl))
                {
                    ProceedAfterLogin();
                }
                else
                {
                    StartCoroutine(LoadUserAvatar(userData.avatarUrl));
                }
            }
        ));
    }

    private IEnumerator LoadUserAvatar(string avatarUrl)
    {
        yield return StartCoroutine(ApiClient.Instance.DownloadAvatar(
            avatarUrl,
            (success, texture) => {
                if (success)
                {
                    UserSession.CachedAvatar = texture;
                }
                ProceedAfterLogin();
            }
        ));
    }

    private void ProceedAfterLogin()
    {
        if (UserSession.CurrentUser == null)
        {
            SetProcessingState(false);
            return;
        }

        switch (UserSession.CurrentUser.role.ToLower())
        {
            case "student":
                if (string.IsNullOrEmpty(UserSession.CurrentUser.avatarUrl))
                {
                    SceneManager.LoadScene("MenuAvatar");
                }
                else
                {
                    SceneManager.LoadScene("StudentsScene");
                }
                break;

            case "teacher":
                SceneManager.LoadScene("PrepodModel");
                break;

            case "applicant":
                SceneManager.LoadScene("ApplicantScene");
                break;

            default:
                errorText.text = "Unknown user role";
                SetProcessingState(false);
                break;
        }
    }
    #endregion

    #region UI Management
    private void SetProcessingState(bool processing)
    {
        isProcessing = processing;
        loadingIndicator.SetActive(processing);
        emailInput.interactable = !processing;
        passwordInput.interactable = !processing;
        loginButton.interactable = !processing;
        registerButton.interactable = !processing;
        forgotPasswordButton.interactable = !processing;
    }

    private void OnRegisterButtonClick()
    {
        if (isProcessing) return;
        SceneManager.LoadScene("RegistrationScene");
    }

    private void OnForgotPasswordClick()
    {
        if (isProcessing) return;
        SceneManager.LoadScene("PasswordResetScene");
    }

    public void OnInputFieldSelected(TMP_InputField inputField)
    {
        if (Application.isMobilePlatform)
        {
            if (movePanelCoroutine != null)
            {
                StopCoroutine(movePanelCoroutine);
            }
            movePanelCoroutine = StartCoroutine(AdjustForKeyboard(inputField));
        }
    }

    public void OnInputFieldDeselected()
    {
        if (Application.isMobilePlatform)
        {
            if (movePanelCoroutine != null)
            {
                StopCoroutine(movePanelCoroutine);
            }
            movePanelCoroutine = StartCoroutine(MovePanelToOriginalPosition());
        }
    }

    private IEnumerator AdjustForKeyboard(TMP_InputField inputField)
    {
        yield return new WaitForSeconds(0.1f);

        if (!TouchScreenKeyboard.isSupported || !TouchScreenKeyboard.visible)
        {
            yield break;
        }

        RectTransform inputRect = inputField.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        inputRect.GetWorldCorners(corners);
        float inputFieldBottom = corners[0].y;

        float keyboardHeight = GetKeyboardHeight();
        float screenHeight = Screen.height;
        float keyboardTop = screenHeight - keyboardHeight;

        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 inputFieldBottomScreen = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        float inputFieldBottomY = inputFieldBottomScreen.y;

        if (inputFieldBottomY < keyboardTop + 50) // 50 pixels buffer
        {
            float offset = (keyboardTop + 50 - inputFieldBottomY) / canvas.scaleFactor;
            Vector2 targetPosition = originalPanelPosition + new Vector2(0, offset);

            yield return StartCoroutine(MovePanelToPosition(targetPosition));
        }
    }

    private IEnumerator MovePanelToPosition(Vector2 targetPosition)
    {
        float elapsedTime = 0f;
        Vector2 startPosition = uiPanel.anchoredPosition;

        while (elapsedTime < uiMoveDuration)
        {
            uiPanel.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / uiMoveDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        uiPanel.anchoredPosition = targetPosition;
    }

    private IEnumerator MovePanelToOriginalPosition()
    {
        yield return StartCoroutine(MovePanelToPosition(originalPanelPosition));
    }

    private float GetKeyboardHeight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityView = unityActivity.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
            AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
            unityView.Call("getWindowVisibleDisplayFrame", rect);
            return Screen.height - rect.Call<int>("height");
        }
#else
        return TouchScreenKeyboard.area.height;
#endif
    }
    #endregion

    private void OnDestroy()
    {
        // Clean up event listeners
        emailInput.onValueChanged.RemoveAllListeners();
        passwordInput.onValueChanged.RemoveAllListeners();
        loginButton.onClick.RemoveAllListeners();
        registerButton.onClick.RemoveAllListeners();
        forgotPasswordButton.onClick.RemoveAllListeners();
    }
}