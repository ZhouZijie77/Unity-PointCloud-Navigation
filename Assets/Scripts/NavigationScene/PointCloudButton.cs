using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PointCloudButton : MonoBehaviour
{
    Button point_cloudButton;
    // Start is called before the first frame update
    void Start()
    {
        point_cloudButton = GetComponent<Button>();
        point_cloudButton.onClick.AddListener(onClick);
    }

    // Update is called once per frame
    
    void onClick()
    {
        //Debug.Log("!!!!");
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        //Debug.Log("sfad");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/PointCloud");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }


}
