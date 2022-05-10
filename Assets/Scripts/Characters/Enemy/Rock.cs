using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    private Rigidbody rb;
    [Header("Basic Settings")]
    [SerializeField] private float force;

    public GameObject target;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Fly2Taget();
    }
    public void Fly2Taget()
    {
        if(target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }
        Vector3 dir = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }
}
