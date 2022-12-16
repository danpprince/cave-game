using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whipColiiderDecay : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(KillTime());
    }

    IEnumerator KillTime()
    {
        yield return new WaitForSeconds(.3f);
        Destroy(this.gameObject);
    }


}
