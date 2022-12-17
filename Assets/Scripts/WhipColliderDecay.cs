using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipColliderDecay : MonoBehaviour
{
    private void OnEnable()
    {
        print("Creating whip collider");
        StartCoroutine(KillTime());
    }

    IEnumerator KillTime()
    {
        yield return new WaitForSeconds(.3f);
        print("Destroying whip collider");
        Destroy(this.gameObject);
    }
}
