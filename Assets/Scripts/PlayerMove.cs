using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerMove : NetworkBehaviour
{
    private Rigidbody2D rb;

    public const float walkingSpeed = 12f;
    private float movementSpeed;

    private float inputX;
    private float inputY;

    private bool isWalking;
    private bool isIdle;

    public LayerMask interactableMask;

    [HideInInspector]
    public GameObject objPlayerIsNear;

    [HideInInspector]
    [SyncVar(hook = nameof(OnEquipItemChanged))]
    public string equippedItem;

    public GameObject ballObj;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();

        ballObj = this.transform.Find("EquippedItem").transform.Find("Ball").gameObject;
    }

    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (inputX != 0 || inputY != 0)
        {
            inputX = inputX * 0.71f;
            inputY = inputY * 0.71f;

            isWalking = true;
            isIdle = false;
            movementSpeed = walkingSpeed;
        }
        else if (inputX == 0 || inputY == 0)
        {
            isWalking = false;
            isIdle = true;
        }

        Collider2D[] objectsDetected;
        List<Collider2D> objectsDetectedList = new List<Collider2D>();
        ContactFilter2D objContactFilter = new ContactFilter2D();
        objContactFilter.useLayerMask = true;
        objContactFilter.layerMask = interactableMask;

        if (isServer == true)
        {
            PhysicsScene2D physicsScene = gameObject.scene.GetPhysicsScene2D();
            physicsScene.OverlapCircle(transform.position, 3.7f, objContactFilter, objectsDetectedList);
            objectsDetected = objectsDetectedList.ToArray();
        }
        else
        {
            objectsDetected = Physics2D.OverlapCircleAll(transform.position, 3.7f, interactableMask);
        }

        GameObject objShortestDistance = null;
        foreach (var obj in objectsDetected)
        {
            if (obj.GetComponent<NetworkIdentity>().netId != 0)
            {
                if (objShortestDistance == null)
                {
                    objShortestDistance = obj.gameObject;
                }
                else
                {
                    Vector3 colliderOffset = new Vector3(this.GetComponent<BoxCollider2D>().offset.x, this.GetComponent<BoxCollider2D>().offset.y, 0f);
                    Vector3 objShortestColliderOffset = new Vector3(objShortestDistance.GetComponent<BoxCollider2D>().offset.x, objShortestDistance.GetComponent<BoxCollider2D>().offset.y, 0f);

                    float newDist = Vector2.Distance(this.transform.position + colliderOffset, obj.transform.position + colliderOffset);
                    float oldDist = Vector2.Distance(this.transform.position + colliderOffset, objShortestDistance.transform.position + objShortestColliderOffset);

                    if (newDist < oldDist)
                    {
                        objShortestDistance = obj.gameObject;
                    }
                }
            }

            objPlayerIsNear = objShortestDistance;
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer == true)
        {
            Vector2 move = new Vector2(inputX * movementSpeed * Time.deltaTime, inputY * movementSpeed * Time.deltaTime);

            rb.MovePosition(rb.position + move);
        }
    }

    private void OnEquipItemChanged(string oldEquippedItem, string newEquippedItem)
    {
        if (ballObj)
        {
            if (newEquippedItem == "")
            {
                ballObj.SetActive(false);
            }
            else 
            {
                if (newEquippedItem == "Ball")
                {
                    ballObj.SetActive(true);
                }
            }
        }
    }
}
