using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using Firebase.Extensions;
using Firebase;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Database;

public class AuthManager : MonoBehaviour
{
    public FirebaseAuth auth;
    public DatabaseReference dbReference;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;

    public GameObject signUpBtn;
    public GameObject signInBtn;
    public GameObject signOutBtn;
    public GameObject forgotPasswordBtn;
    //public TextMeshProUGUI errorMsgContent;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public async void SignUpNewUser()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (ValidateEmail(email) && ValidatePassword(password))
        {
            //errorMsgContent.gameObject.SetActive(false); // Hide error initially

            FirebaseUser newPlayer = await SignUpNewUserOnly(email, password);

            if (newPlayer != null)
            {
                string username = usernameInput.text;
                await CreateNewSimplePlayer(newPlayer.UserId, newPlayer.Email, username, username);
                await UpdatePlayerDisplayName(username);
                SceneManager.LoadScene(1); // Navigate to the next scene
            }
            else
            {
                // errorMsgContent.text = "Account creation failed. Please check your details.";
                // errorMsgContent.gameObject.SetActive(true);
            }
        }
        else
        {
            // errorMsgContent.text = "Error in signing up: Invalid Email or Password.";
            // errorMsgContent.gameObject.SetActive(true);
        }
    }

    public async Task<FirebaseUser> SignUpNewUserOnly(string email, string password)
    {
        Debug.Log("Sign up method...");

        FirebaseUser newPlayer = null;

        try
        {
            AuthResult authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            newPlayer = authResult.User; // Access the User property
            Debug.LogFormat("Player Details: {0}, {1}", newPlayer.UserId, newPlayer.Email);
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"Firebase SignUp Error: {ex.Message}");
            // errorMsgContent.text = HandleSignUpError(ex);
            // errorMsgContent.gameObject.SetActive(true);
        }

        return newPlayer;
    }


    public async Task UpdatePlayerDisplayName(string displayName)
    {
        if (auth.CurrentUser != null)
        {
            UserProfile profile = new UserProfile { DisplayName = displayName };
            await auth.CurrentUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to update user profile: " + task.Exception);
                }
                else
                {
                    Debug.Log("User profile updated successfully");
                    Debug.LogFormat("Checking current user display name from auth: {0}", GetCurrentUserDisplayName());
                }
            });
        }
    }

    public async Task CreateNewSimplePlayer(string uuid, string email, string displayName, string userName)
    {
        Player newPlayer = new Player(email, displayName, userName);
        Debug.LogFormat("Player details: {0}", newPlayer.PrintPlayer());
        await dbReference.Child("players/" + uuid).SetRawJsonValueAsync(newPlayer.PlayerToJson());
        await UpdatePlayerDisplayName(displayName);
    }

    public string GetCurrentUserDisplayName()
    {
        return auth.CurrentUser?.DisplayName ?? "Unknown User";
    }

    public void SignInUser()
    {
        Debug.Log("Sign in method...");
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (ValidateEmail(email) && ValidatePassword(password))
        {
            auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    // string errorMsg = HandleSignInError(task);
                    // errorMsgContent.text = errorMsg;
                    // errorMsgContent.gameObject.SetActive(true);
                }
                else if (task.IsCompleted)
                {
                    //errorMsgContent.gameObject.SetActive(false);
                    FirebaseUser currentPlayer = task.Result.User;
                    Debug.LogFormat("Welcome to the game: {0} {1}", currentPlayer.UserId, currentPlayer.Email);
                }
            });
        }
        else
        {
            // errorMsgContent.text = "Error in signing in: Invalid Email or Password.";
            // errorMsgContent.gameObject.SetActive(true);
        }
    }

    public void SignOutUser()
    {
        if (auth.CurrentUser != null)
        {
            Debug.LogFormat("Auth user: {0} {1}", auth.CurrentUser.UserId, auth.CurrentUser.Email);

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            auth.SignOut();

            if (currentSceneIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
        }
        Debug.Log("Sign out method...");
    }

    public void ForgotPassword()
    {
        string email = emailInput.text.Trim();
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to send password reset email: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Password reset email sent successfully...");
            }
        });
    }

    public FirebaseUser GetCurrentUser()
    {
        return auth.CurrentUser;
    }

    public bool ValidateEmail(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    public bool ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length >= 6;
    }

    public string HandleSignUpError(FirebaseException ex)
    {
        string errorMsg = "Sign up failed\n";
        AuthError errorCode = (AuthError)ex.ErrorCode;

        switch (errorCode)
        {
            case AuthError.EmailAlreadyInUse:
                errorMsg += "Email already in use.";
                break;
            case AuthError.WeakPassword:
                errorMsg += "Password is weak. Use at least 6 characters.";
                break;
            case AuthError.MissingPassword:
                errorMsg += "Password is missing.";
                break;
            case AuthError.InvalidEmail:
                errorMsg += "Invalid email used.";
                break;
            default:
                errorMsg += $"Issue in authentication: {errorCode}";
                break;
        }

        Debug.LogError("Error message: " + errorMsg);
        return errorMsg;
    }

    public string HandleSignInError(Task<AuthResult> task)
    {
        string errorMsg = "Sign in failed\n";
        if (task.Exception != null)
        {
            FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    errorMsg += "Email is missing.";
                    break;
                case AuthError.WrongPassword:
                    errorMsg += "Incorrect password.";
                    break;
                case AuthError.MissingPassword:
                    errorMsg += "Password is missing.";
                    break;
                case AuthError.InvalidEmail:
                    errorMsg += "Invalid email used.";
                    break;
                case AuthError.UserNotFound:
                    errorMsg += "User not found.";
                    break;
                default:
                    errorMsg += $"Issue in authentication: {errorCode}";
                    break;
            }
        }

        Debug.LogError("Error message: " + errorMsg);
        return errorMsg;
    }
}
