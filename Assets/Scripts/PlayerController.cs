using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Looking around")]
    [SerializeField]
    float sensitivity = 100;
    Camera FPSCam;
    float mouseDeltaX = 0;
    float mouseDeltaY = 0;

    [Header("Movement")]
    public float acceleration = 10f;
    public float dampAcceleration = 15f;
    public float maxSpeed = 1f;
    public float jumpHight = 1f;
    Vector3 speedVector;

    [Header("Grabbing")]
    [HideInInspector]
    public GameObject grabbedObject;
    Prop grabbedProp;
    Rigidbody grabbedRigidbody;
    bool isGrabbed = false;
    [SerializeField]
    public Transform grabberTransform;
    [SerializeField]
    public Transform storerTransfrom;
    [SerializeField]
    public Transform throwerTransform;
    [SerializeField]
    public Transform holderTransform;
    [SerializeField]
    float grabSpeed = 2f;
    [SerializeField]
    float grabMoveSpeed = 0.3f;
    [SerializeField]
    float throwImpulse = 2f;
    [SerializeField]
    float maxGrabDistance = 1f;
    [SerializeField]
    float minGrabDistance = 0.2f;
    [SerializeField]
    float maxGrabDistanceError = 0.8f;
    [SerializeField]
    float grabErrorActivateDelay = 2;
    float grabErrorActivateTimer;
    [SerializeField]
    public float maxInteractDistance = 1.4f;
    [SerializeField]
    LayerMask layersTheGrabberCantEnter;

    [Header("Holding")]
    Prop heldProp;
    bool holdingObject;
    IEnumerator currentSwitchCoroutine;
    bool switching = false;

    [Header("Misc.")]
    [SerializeField]
    float minKickSpeed = 8f;
    [SerializeField]
    float kickImpulse = 2f;
    [SerializeField]
    float kickDelay = 0.3f;
    float kickTimer = 0f;

    #region self-reference
    CharacterController myCharacterController;
    #endregion

    void Start()
    {
        #region GETTING COMPONENTS
        FPSCam = GetComponentInChildren<Camera>();
        myCharacterController = GetComponent<CharacterController>();
        #endregion

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        if (isGrabbed)
        {
            UpdateGrabbedPosition(grabbedObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            GetComponent<DialogueTrigger>().TriggerDialogue();
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            DialogueController.instance.DisplayNextSentence();
        }

        #region MOUSE ROTATION UPDATE
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        #region old numbers
        float thisOldXRot = transform.localRotation.eulerAngles.x;
        float thisOldYRot = transform.localRotation.eulerAngles.y;
        float thisOldZRot = transform.localRotation.eulerAngles.z;

        float CamOldXRot = FPSCam.transform.localRotation.eulerAngles.x;
        float CamOldYRot = FPSCam.transform.localRotation.eulerAngles.y;
        float CamOldZRot = FPSCam.transform.localRotation.eulerAngles.z;
        #endregion

        mouseDeltaY -= mouseY;
        mouseDeltaX += mouseX;

        mouseDeltaY = Mathf.Clamp(mouseDeltaY, -90,90);

        transform.localRotation = Quaternion.Euler(thisOldXRot, mouseDeltaX, thisOldZRot);
        FPSCam.transform.localRotation = Quaternion.Euler(mouseDeltaY, CamOldYRot, CamOldZRot);
        #endregion

        #region POSITION UPDATE

        if (Input.GetButton("Jump") && myCharacterController.isGrounded)
        {

            speedVector.y = Mathf.Sqrt(-2 * jumpHight * Physics.gravity.y);
        }

        Vector3 moveDir = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
       
        if (myCharacterController.isGrounded) {
            #region DAMP SPEED
            if (moveDir == Vector3.zero)
            {
                if (new Vector2(speedVector.x, speedVector.z).magnitude > dampAcceleration * Time.deltaTime)
                {
                    Vector2 flatDir = new Vector2(speedVector.x, speedVector.z).normalized;
                    speedVector += new Vector3(flatDir.x, 0, flatDir.y).normalized * -dampAcceleration * Time.deltaTime;
                }
                else
                {
                    speedVector.x = 0;
                    speedVector.z = 0;
                }
            }
            #endregion
            #region STOP FALLING
            if (speedVector.y < 0)
            {
                speedVector.y = -0.05f;
            }
            #endregion
        }

       
        speedVector += Physics.gravity * Time.deltaTime;
        #region MOVE
        if (myCharacterController.isGrounded) {
            Vector3 horizontalSpeed = new Vector3(speedVector.x, 0, speedVector.z);

            if (horizontalSpeed.magnitude < maxSpeed) {
                speedVector += moveDir * acceleration * Time.deltaTime;
            }
            if (horizontalSpeed.magnitude >= maxSpeed)
            {
                if ((speedVector + moveDir * acceleration * Time.deltaTime).magnitude > maxSpeed) {
                    speedVector = (speedVector + moveDir * acceleration * Time.deltaTime).normalized * maxSpeed;

                }
                else {
                    speedVector += moveDir * acceleration * Time.deltaTime;
                }
            }
        }
        #endregion

        myCharacterController.Move(speedVector * Time.deltaTime);

        #endregion

        #region INTERACTION
        
        if (Input.GetButtonDown("Pick up")) {

            if (!isGrabbed)
            {
                RaycastHit rayHit;
                rayHit = GameMath.RaycastFromCenterOfScreen(maxInteractDistance);

                if (rayHit.collider != null && rayHit.collider.gameObject.GetComponent<Prop>() != null)
                {
                    GameObject hitGameObject;
                    hitGameObject = rayHit.collider.gameObject;
                    Prop hitProp = rayHit.collider.gameObject.GetComponent<Prop>();

                    if (hitProp.interactable)
                        hitProp.Use();

                        #region DOORS
                    Door door = hitGameObject.GetComponent<Door>();
                    if (door != null)
                    {
                        if ((door.closed && !door.locked) || !door.closed)
                        {
                            door.Open();
                            GrabWithMaxDistance(hitGameObject);
                            print("Opened");
                        }

                        if (door.locked)
                        {
                            door.Jiggle();
                        }
                    }
                    #endregion
                    else if (hitProp.dragable)
                    {
                        Grab(hitGameObject);
                    }
                    else if (hitProp.storable) 
                    {
                        HoverToInventory(hitGameObject);
                    }
                    
                }
            }
            else if(grabbedProp.storable){
                HoverToInventory(grabbedObject);
                UnGrab();
            }
            else
            {
                UnGrab();
            }
        }
        if (Input.GetButtonDown("Drop"))
        {
            if (isGrabbed) 
            {
                UnGrab();
            }
            else if (MetaManager.playerInventory.Count != 0)
            {
                RaycastHit rayHit;
                rayHit = GameMath.RaycastFromCenterOfScreen(maxInteractDistance);
                float size = MetaManager.playerInventory[Inventory.instance.selectedSlot].GetComponent<Prop>().size;
                bool doThrow;
                if (rayHit.collider != null)
                    doThrow = false;//Inventory.instance.HoverFromInventory(Inventory.instance.selectedSlot, storerTransfrom.transform.position, rayHit.point + rayHit.normal * size, ActionUponArrival.stopHovering);
                else
                    doThrow = true; //Inventory.instance.ThrowFromInventory(Inventory.instance.selectedSlot, throwerTransform.position, throwerTransform.forward * throwImpulse * Inventory.instance.selectedProp.GetComponent<Rigidbody>().mass);
                if (heldProp != null && Inventory.instance.selectedProp == heldProp)
                {
                    //throw out held prop
                    if (!doThrow)
                        Inventory.instance.HoverFromInventory(Inventory.instance.selectedSlot, storerTransfrom.transform.position, rayHit.point + rayHit.normal * size, ActionUponArrival.stopHovering);
                    else
                        Inventory.instance.ThrowFromInventory(Inventory.instance.selectedSlot, throwerTransform.position, throwerTransform.forward * throwImpulse * Inventory.instance.selectedProp.GetComponent<Rigidbody>().mass);
                    heldProp = null;
                }
                else
                {
                    if (!doThrow)
                        Inventory.instance.HoverFromInventory(Inventory.instance.selectedSlot, storerTransfrom.transform.position, rayHit.point + rayHit.normal * size, ActionUponArrival.stopHovering);
                    else
                        Inventory.instance.ThrowFromInventory(Inventory.instance.selectedSlot, throwerTransform.position, throwerTransform.forward * throwImpulse * Inventory.instance.selectedProp.GetComponent<Rigidbody>().mass);
                }

            }
        }
        if (Input.GetButtonDown("Use") && MetaManager.playerInventory.Count != 0) {
            MetaManager.playerInventory[Inventory.instance.selectedSlot].GetComponent<Prop>().Use();
        }
        #endregion

        #region GRABBING

        #region PREVENT GLITCHYNESS
        if (isGrabbed)
        {
            float distanceError = (grabberTransform.position - grabbedRigidbody.position).magnitude;
            if (((distanceError > maxGrabDistanceError) || (distanceError > grabbedObject.GetComponent<Prop>().maxDistanceError)) && grabErrorActivateTimer < 0)
            {
                UnGrab();
                return;
            }

            /*
            if (Physics.CheckSphere(grabberTransform.position, 0, layersTheGrabberCantEnter, QueryTriggerInteraction.Ignore))
            {
                unGrab();
            }
            */

            #region UNGRAB IF SOMETHING IS IN THE WAY
           RaycastHit[] raycastHits;
           Vector3 difference = grabberTransform.position - grabbedObject.transform.position;
           Vector3 direction = difference.normalized;
           raycastHits = Physics.RaycastAll(grabbedObject.transform.position, direction,difference.magnitude, layersTheGrabberCantEnter);
           if (raycastHits.Length != 0) {
               foreach (RaycastHit hit in raycastHits) {
                   if (hit.collider.gameObject != grabbedObject) 
                   {
                       UnGrab();
                       return;
                   }
               }
           }
            #endregion
        }
        #endregion

        grabberTransform.localPosition += new Vector3(0,0,Input.mouseScrollDelta.y * grabMoveSpeed);
        #region CLAMPING

        if (grabberTransform.localPosition.z > maxGrabDistance) {
            grabberTransform.localPosition = new Vector3(0, 0, maxGrabDistance);
        }
        if (grabberTransform.localPosition.z < minGrabDistance)
        {
            grabberTransform.localPosition = new Vector3(0, 0, minGrabDistance);
        }

        #endregion

        #endregion

        kickTimer -= Time.deltaTime;
        grabErrorActivateTimer -= Time.deltaTime;
    }
    public void RefreshHeldObject()
    {
        if (MetaManager.playerInventory.Count != 0)
        {
            //print(Inventory.instance.selectedProp.equippable);
            //print(Inventory.instance.selectedProp.slot);
            if (heldProp != Inventory.instance.selectedProp)
            {
                if (currentSwitchCoroutine == null)
                {
                    currentSwitchCoroutine = SwitchHeldObject();
                    StartCoroutine(currentSwitchCoroutine);
                }
            }
        }
        IEnumerator WhipOutObject()
        {
            Inventory.instance.HoverFromInventoryKeepingReference(Inventory.instance.selectedSlot, storerTransfrom.transform.position, holderTransform, true, ActionUponArrival.disablePhysics);
            heldProp = Inventory.instance.selectedProp;
            yield return new WaitWhile(() => heldProp.isHovering);
        }
        IEnumerator PutAwayObject()
        {
            if (heldProp != null)
            {
                HoverToPlayerAndDisable(heldProp.gameObject);
                yield return new WaitWhile(() => heldProp.isHovering);
                print("setToNull");
                heldProp = null;
            }
        }
        IEnumerator SwitchHeldObject()
        {
            if (heldProp != null)
            {
                HoverToPlayerAndDisable(heldProp.gameObject);
                yield return new WaitWhile(() => heldProp.isHovering);
                print("setToNull");
                heldProp = null;
            }
            if (Inventory.instance.selectedProp.equippable == true && Inventory.instance.selectedProp.slot == Slot.hand)
            {
                Inventory.instance.HoverFromInventoryKeepingReference(Inventory.instance.selectedSlot, storerTransfrom.transform.position, holderTransform, true, ActionUponArrival.disablePhysics);
                heldProp = Inventory.instance.selectedProp;
                heldProp.gameObject.transform.parent = holderTransform;
                yield return new WaitWhile(() => heldProp.isHovering);
            }
            currentSwitchCoroutine = null;
        }
    }
    #region KICKING
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.rigidbody != null && kickTimer < 0) {
            if (hit.gameObject.GetComponent<Prop>() == null || hit.gameObject.GetComponent<Prop>().kickable == true) {
                kickTimer = kickDelay;
                Vector3 relativeSpeed = speedVector - hit.rigidbody.velocity;
                if (relativeSpeed.magnitude > minKickSpeed) {
                    hit.rigidbody.AddForceAtPosition((relativeSpeed.magnitude - minKickSpeed) * speedVector.normalized * kickImpulse, hit.point, ForceMode.Impulse);
                    print("bruh");
                }
            }
        }
    }
    #endregion
    public void Grab(GameObject grabbedGameObject) {
        if (!isGrabbed) {
            Prop prop = grabbedGameObject.GetComponent<Prop>();
            if (prop != null) {
                grabbedObject = grabbedGameObject;
                grabbedProp = prop;
                grabbedRigidbody = grabbedGameObject.GetComponent<Rigidbody>();
                grabbedRigidbody.angularDrag = 40f;
                grabbedRigidbody.drag = 10f;
                grabbedRigidbody.useGravity = false;
                isGrabbed = true;
                grabErrorActivateTimer = grabErrorActivateDelay;

                grabberTransform.localPosition = new Vector3(0, 0, maxGrabDistance / 2);
            }
        }
        else {
            UnGrab();
        }
    }
    void GrabWithMaxDistance(GameObject grabbedGameObject) {
        Grab(grabbedGameObject);
        grabberTransform.localPosition = new Vector3(0, 0, maxGrabDistance);
    }
    public void UnGrab()
    {
        isGrabbed = false;
        grabbedRigidbody.angularDrag = 0f;
        grabbedRigidbody.drag = 0f;
        grabbedRigidbody.useGravity = true;
        grabbedObject = null;
        grabbedRigidbody = null;
    }
    public void UpdateGrabbedPosition(GameObject gm)
    {
        grabbedRigidbody.velocity = (grabberTransform.position - grabbedObject.transform.position) * grabSpeed;
        if (grabbedRigidbody.velocity.magnitude < 0.1f) {
            grabbedRigidbody.velocity = Vector3.zero;
        }
    }
    void HoverToInventory(GameObject gameObject) {
        Inventory.instance.StartHoverAnimation(storerTransfrom, gameObject, transform, ActionUponArrival.addToInventory, false);
    }
    void HoverToPlayerAndDisable(GameObject gameObject)
    {
        Inventory.instance.StartHoverAnimation(storerTransfrom, gameObject, transform, ActionUponArrival.disableProp, false);
    }
}

