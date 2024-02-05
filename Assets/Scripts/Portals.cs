using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portals : MonoBehaviour
{
    public Collider portal1;
    public Collider portal2;
    Rigidbody mainBody;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.tag.Equals("Player"))
        {
            if ((portal1.transform.position - other.transform.position).magnitude < (portal2.transform.position - other.transform.position).magnitude)
            {
                other.attachedRigidbody.transform.position = portal2.transform.position + portal2.transform.rotation * Vector3.forward * 2;
                other.attachedRigidbody.transform.rotation = portal2.transform.rotation;
                other.attachedRigidbody.velocity = portal2.transform.rotation * Vector3.forward * other.attachedRigidbody.velocity.magnitude;
            }
            else
            {
                other.attachedRigidbody.transform.position = portal1.transform.position + portal1.transform.rotation * Vector3.forward * 2;
                other.attachedRigidbody.transform.rotation = portal1.transform.rotation;
                other.attachedRigidbody.velocity = portal1.transform.rotation * Vector3.forward * other.attachedRigidbody.velocity.magnitude;
            }
        }
    }
}
