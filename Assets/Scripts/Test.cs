using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public bool isReturn;
    private void Update()
    {
        if(isReturn)return;
        if (Input.GetKey(KeyCode.Space))
            animator.SetBool("test", true);
        else animator.SetBool("test", false);
    }
}