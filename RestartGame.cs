namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class RestartGame : MonoBehaviour {

        private void Awake() {
            GameServices.ResetCreated();
            Destroy(FindObjectOfType<GameServices>()?.gameObject);
            
        }

        private IEnumerator Start() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene("LoadingScene");
        }
    }

}