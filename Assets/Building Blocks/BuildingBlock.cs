using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    public BlockType blockType;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        foreach (Transform child in transform)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(child.position, 0.2f);
        }
    }
    public void UpdateSnapPoints()
    {
        foreach(Transform child in transform)
        {
            AttachPoint attachPoint = child.GetComponent<AttachPoint>();
            if (child != null)
            {
                attachPoint.CheckForSnap();
            }

        }
    }
}
