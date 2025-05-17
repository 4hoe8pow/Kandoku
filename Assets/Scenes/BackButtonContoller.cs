using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonController : MonoBehaviour
{
    public void GoToTitleScene()
    {
        SceneManager.LoadScene("Title");
    }
}