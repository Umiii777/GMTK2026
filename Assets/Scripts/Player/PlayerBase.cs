using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerBase : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if (collision.gameObject.CompareTag("Trap"))
        // {
        //     Debug.Log("Has Dead");
        //     StartCoroutine(Death());
        // }
    }
    // IEnumerator Death()
    // {
    //     yield return new WaitForSeconds(2f);
    //     SceneManager.LoadScene(0);
        
    // }
}
