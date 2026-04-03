using System;
using System.Collections;
using UnityEngine;

public class ExplObj : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(dead());
    }

    private IEnumerator dead()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
