using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class ClickHandler : MonoBehaviour
{
    public Button zoomButton;
    public Button rotateButton;
    public Button shiftButton;
    public Button quitButton;
    public VisualController visualController;
    public TextMeshProUGUI zoomTMP;
    public TextMeshProUGUI rotateTMP;
    // Start is called before the first frame update
    void Start()
    {
        zoomButton.onClick.AddListener(ZoomOnClick);
        rotateButton.onClick.AddListener(RotateOnClick);
        shiftButton.onClick.AddListener(ShiftOnClick);
        quitButton.onClick.AddListener(QuitOnClick);
    }

    void ZoomOnClick()
    {
        visualController.isZooming = !visualController.isZooming;
        if (visualController.isZooming)
        {
            zoomTMP.text = "Translate";
        }
        else
        {
            zoomTMP.text = "Zoom";
        }
    }
    void QuitOnClick()
    {
        //Debug.Log("sfasdf");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void RotateOnClick()
    {
        visualController.isRotating = !visualController.isRotating;
        if (visualController.isRotating)
        {
            rotateTMP.text = "Default";
        }
        else
        {
            rotateTMP.text = "Rotate";
        }
    }

    void ShiftOnClick()
    {
        //Debug.Log("sfdsf");
        TimeCount.t1 = DateTime.Now;
        StartCoroutine(LoadScene());
        //SceneManager.LoadScene(1);

    }


    IEnumerator LoadScene()
    {
        //Debug.Log("sdfdsf");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/Navigation");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
