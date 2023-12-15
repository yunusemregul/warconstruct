using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttachPoint : MonoBehaviour
{
    public bool isAttached = false;
    public List<BlockType> notSupportedBlockTypes = new List<BlockType>();
    public GameObject player;
    float sphereSize = 1f;
    void Start()
    {
        if (player != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>());
        }
    }

    void Update()
    {
        
    }
    public void CheckForSnap()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sphereSize / 2);
        foreach (Collider collider in colliders)
        {

            AttachPoint otherAttachPoint = collider.GetComponent<AttachPoint>();
            if (otherAttachPoint != null && collider.gameObject.layer != LayerMask.NameToLayer("HoloAttachPoint")) 
            {
                otherAttachPoint.isAttached = true;
                isAttached = true;
            }

        }
    }
}
