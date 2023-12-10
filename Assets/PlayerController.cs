using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0.1f, 9f)][SerializeField] float sensitivity = 2f;
	[Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
	Vector2 rotation = Vector2.zero;
	const string xAxis = "Mouse X"; //Strings in direct code generate garbage, storing and re-using them creates no garbage
	const string yAxis = "Mouse Y";
    public float jumpForce = 5f;
    public float speed = 5f;
    public float holoDistance = 5f;
    public GameObject playerCamera;
    public GameObject[] buildingBlocks;
    private GameObject currentBuildingBlock;
    private Material floorMaterial;


    void Start()
    {
        floorMaterial = (Material)Resources.Load("Floor", typeof(Material));
        currentBuildingBlock = buildingBlocks[0];
    }

    public float Sensitivity {
		get { return sensitivity; }
		set { sensitivity = value; }
	}

    void Update()
    {
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        playerCamera.transform.localRotation = xQuat * yQuat;

        Vector3 moveDirection = playerCamera.transform.forward * Input.GetAxis("Vertical") + playerCamera.transform.right * Input.GetAxis("Horizontal");
        moveDirection.y = 0f;
        moveDirection.Normalize();
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            CreateFloor();
        }

        moveHoloAround();

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            currentBuildingBlock = buildingBlocks[0];
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            currentBuildingBlock = buildingBlocks[1];
        }
    }

    private void moveHoloAround()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        Vector3 finalHoloPosition = Vector3.zero;
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
        Collider[] hitColliders = Physics.OverlapBox(finalHoloPosition, currentBuildingBlock.transform.localScale);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.tag == "BuildingBlock") {
                Vector3 distanceSubstraction = hitCollider.transform.position - finalHoloPosition;
                if (distanceSubstraction.magnitude < minimumDistance) {
                    minimumDistance = distanceSubstraction.magnitude;
                    closestCollider = hitCollider;
                }
            }
        }

        if (closestCollider != null) {
            Vector3 subtraction = closestCollider.transform.position - finalHoloPosition;
            subtraction.Normalize();

            if (Math.Abs(subtraction.x) > Math.Abs(subtraction.z)) {
                finalHoloPosition = closestCollider.transform.position - Math.Sign(subtraction.x) * (closestCollider.transform.right * currentBuildingBlock.transform.localScale.x);
            } else {
                finalHoloPosition = closestCollider.transform.position - Math.Sign(subtraction.z) * (closestCollider.transform.forward * currentBuildingBlock.transform.localScale.z);
            }
        }

        finalHoloPosition += currentBuildingBlock.GetComponent<BuildingBlock>().attachOffset;

        currentBuildingBlock.transform.position = finalHoloPosition;
    }

    void CreateFloor()
    {
        GameObject clonedHolo = Instantiate(currentBuildingBlock);
        clonedHolo.transform.position = currentBuildingBlock.transform.position;
        clonedHolo.AddComponent<BoxCollider>();
        clonedHolo.GetComponent<BoxCollider>().tag = "BuildingBlock";
        clonedHolo.GetComponent<Renderer>().material = floorMaterial;
    }

    void Jump()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}