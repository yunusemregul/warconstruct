using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    public GameObject[] buildingBlocks;
    private GameObject currentBuildingBlock;
    private Material holoMaterial;
    public float holoDistance = 5f;
    public GameObject playerCamera;
    private int currentBlockIndex = 0;
    private GameObject targetAttachPoint;

    void Start()
    {
        holoMaterial = (Material)Resources.Load("Holo", typeof(Material));
        createHolo();
    }

    private void createHolo()
    {
        if (currentBuildingBlock != null)
        {
            Destroy(currentBuildingBlock);
        }
        currentBuildingBlock = Instantiate(buildingBlocks[currentBlockIndex]);
        currentBuildingBlock.GetComponent<Renderer>().material = holoMaterial;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateSelectedBlock();
        }

        moveHoloAround();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentBlockIndex = 0;
            createHolo();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentBlockIndex = 1;
            createHolo();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentBlockIndex = 2;
            createHolo();
        }
    }

    private void moveHoloAround()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        Vector3 finalHoloPosition;
        Debug.DrawRay(ray.origin, ray.direction * holoDistance, Color.red);
        if (Physics.Raycast(ray, out hit) && hit.distance < holoDistance)
        {
            finalHoloPosition = hit.point;
        }
        else
        {
            finalHoloPosition = playerCamera.transform.position + playerCamera.transform.forward * holoDistance;
        }

        Collider closestCollider = null;
        float minimumDistance = float.MaxValue;
        Collider[] hitColliders = Physics.OverlapBox(finalHoloPosition, currentBuildingBlock.transform.localScale / 1.7f);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.tag == "BuildingBlock")
            {
                Vector3 distanceSubstraction = hitCollider.transform.position - finalHoloPosition;
                if (distanceSubstraction.magnitude < minimumDistance)
                {
                    minimumDistance = distanceSubstraction.magnitude;
                    closestCollider = hitCollider;
                }
            }
        }

        if (closestCollider != null)
        {
            float distance = float.MaxValue;
            Transform closestAttachPoint = null;
            foreach (Transform attachPoint in closestCollider.transform)
            {
                if (attachPoint.GetComponent<AttachPoint>().isAttached)
                {
                    continue;
                }
                Vector3 distanceSubstraction = attachPoint.position - finalHoloPosition;
                if (distanceSubstraction.magnitude < distance)
                {
                    distance = distanceSubstraction.magnitude;
                    closestAttachPoint = attachPoint;
                }
            }

            Vector3 normal = closestAttachPoint.position - closestCollider.transform.position;
            normal.Normalize();

            BlockType targetBlockType = closestCollider.GetComponent<BuildingBlock>().blockType;
            BlockType sourceBlockType = currentBuildingBlock.GetComponent<BuildingBlock>().blockType;

            foreach (Transform attachPoint in currentBuildingBlock.transform)
            {
                if (doesAttachPointsNotMatch(attachPoint, closestAttachPoint))
                {
                    continue;
                }
                
                Debug.DrawLine(attachPoint.position, closestAttachPoint.position, Color.red);
                Vector3 attachPointToHoloNormal = attachPoint.position - currentBuildingBlock.transform.position;
                attachPointToHoloNormal.Normalize();

                if ((targetBlockType==BlockType.Foundation && sourceBlockType==BlockType.Wall) || (targetBlockType==BlockType.Wall && sourceBlockType==BlockType.Ceiling) || Vector3.Dot(normal, attachPointToHoloNormal) <= -0.5f)
                {
                    targetAttachPoint = closestAttachPoint.gameObject;
                    currentBuildingBlock.transform.position += closestAttachPoint.position - attachPoint.position;
                    currentBuildingBlock.transform.rotation = closestAttachPoint.rotation;
                    return;
                }
            }
        }

        currentBuildingBlock.transform.position = finalHoloPosition;
    }

    bool doesAttachPointsNotMatch(Transform attachPoint1, Transform attachPoint2)
    {
        return attachPoint1.GetComponent<AttachPoint>().notSupportedBlockTypes.Contains(attachPoint2.parent.GetComponent<BuildingBlock>().blockType) 
        || attachPoint2.GetComponent<AttachPoint>().notSupportedBlockTypes.Contains(attachPoint1.parent.GetComponent<BuildingBlock>().blockType); 
    }

    void OnDrawGizmos()
    {
        if (currentBuildingBlock != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(currentBuildingBlock.transform.position, currentBuildingBlock.transform.localScale / 1.7f * 2);
        }
    }

    void CreateSelectedBlock()
    {
        GameObject foundation = Instantiate(buildingBlocks[currentBlockIndex]);
        foundation.transform.position = currentBuildingBlock.transform.position;
        foundation.transform.rotation = currentBuildingBlock.transform.rotation;
        foundation.GetComponent<BoxCollider>().enabled = true;
        foundation.GetComponent<BoxCollider>().tag = "BuildingBlock";
        if (targetAttachPoint != null)
        {
            targetAttachPoint.GetComponent<AttachPoint>().isAttached = true;
        }
    }
}