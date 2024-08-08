using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private Rigidbody bulletRigibody;

    [SerializeField]
    private float moveSpeed = 10f;

    void Start()
    {
        bulletRigibody = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        BulletMove();
    }

    private void BulletMove()
    {
        bulletRigibody.velocity = transform.forward * moveSpeed;
    }
}
