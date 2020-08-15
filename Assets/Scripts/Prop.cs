using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public bool kickable = true;
    public bool dragable = true;
    public bool storable = false;
    public bool interactable = false;
    public bool equippable = false;
    public Slot slot = Slot.none;
    public float maxDistanceError = 1f;
    public Sprite preview;
    public string Name;
    public string description;
    public ItemType itemType;


    public Key key;
    public Flashlight flashlight;

    #region HOVER ANIMATION VARS
    public bool isHovering = false;
    [HideInInspector]
    public bool oldIsHovering;
    public Transform hoverDestinationTransform;
    [HideInInspector]
    public ActionUponArrival arrivalAction;
    [SerializeField]
    float hoverSpeed = 8f;
    float hoverRotationSpeed = 180;
    public bool useDestinationTransformRotation = false;
    public float size;

    #endregion

    #region COMPONENTS
    GameObject tempGOWithCloneRB;
    Rigidbody myRigidbody;
    Rigidbody cloneRigidBody;
    PlayerController playerController;
    Animator animator;
    Door currentDoor;
    #endregion

    #region MISC.

    #region SAW
    [HideInInspector] bool sawForward;
    [HideInInspector] bool inSawAnimation;
    [SerializeField] int requiredSaws = 30;
    [SerializeField] DoorFalling linkedDoor;
    #endregion

    #endregion

    public void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        CreateCloneRigidbodyOnTempGO();
        playerController = FindObjectOfType<PlayerController>();
        animator = GetComponent<Animator>();

        if (itemType == ItemType.Flashlight)
        {
            flashlight.light = GetComponentInChildren<Light>();
            flashlight.initialIntensity = flashlight.light.intensity;
        }
    }
    public void Update()
    {
        #region UPDATE HOVER POSITION
        if (isHovering && hoverDestinationTransform != null) {
            Vector3 direction = (hoverDestinationTransform.position - transform.position).normalized;
            float leftOverDistance = (hoverDestinationTransform.position - transform.position).magnitude;

            if (useDestinationTransformRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, hoverDestinationTransform.rotation, hoverRotationSpeed * Time.deltaTime);
            }
            if ((direction * hoverSpeed * Time.deltaTime).magnitude < leftOverDistance)
            {
                transform.position += direction * hoverSpeed * Time.deltaTime;
            }
            else
            {
                transform.position = hoverDestinationTransform.position;
            }

            if (transform.position == hoverDestinationTransform.position && (transform.rotation == hoverDestinationTransform.rotation || useDestinationTransformRotation == false)) {

                if (arrivalAction == ActionUponArrival.addToInventory)
                {
                    Inventory.instance.AddToInventory(gameObject);
                }
                if (arrivalAction == ActionUponArrival.stopHovering)
                {
                    Inventory.instance.DisableHoverAnimation(gameObject);
                }
                if (arrivalAction == ActionUponArrival.enableKeyAnimation)
                {
                    animator.enabled = true;
                    animator.SetBool("Enabled",true);
                }
                if (arrivalAction == ActionUponArrival.disablePhysics)
                {
                    SetHoverMode(HoverMode.stuck);
                }
                if (arrivalAction == ActionUponArrival.disableProp)
                {
                    SetHoverMode(HoverMode.physicsOn);
                    gameObject.SetActive(false);
                }
                if (arrivalAction == ActionUponArrival.keepHovering)
                {
                    SetHoverMode(HoverMode.hovering);
                }
            }
        }
        #endregion
        oldIsHovering = isHovering;

        if (itemType == ItemType.Flashlight)
        {
            
        }
    }
    public void Use()
    {
        if (itemType == ItemType.Key)
        {
            RaycastHit Rayhit = GameMath.RaycastFromCenterOfScreen(playerController.maxInteractDistance);
            if (Rayhit.collider != null && Rayhit.collider.gameObject.GetComponent<Door>() != null) {

                currentDoor = Rayhit.collider.gameObject.GetComponent<Door>();

                if (currentDoor.locked && currentDoor.lockId == key.keyId)
                {
                    Transform targetTransform = Rayhit.collider.gameObject.transform.Find("keyHole").transform;
                    Inventory.instance.HoverFromInventory(Inventory.instance.selectedSlot, playerController.storerTransfrom.position, targetTransform, true, ActionUponArrival.enableKeyAnimation);
                    transform.parent = Rayhit.collider.gameObject.transform;
                }
                if (currentDoor.locked && currentDoor.lockId != key.keyId) {

                }

            }
        }
        else if (itemType == ItemType.Gun)
        {
            print("bang");
        }
        else if (itemType == ItemType.Trinket)
        {
            print("...");
        }
        else if (itemType == ItemType.Saw)
        {
            if (sawForward)
            {
                transform.Translate(0, 0, 0.2f);
                sawForward = false;
            }
            else
            {
                transform.Translate(0, 0, -0.2f);
                sawForward = true;
                requiredSaws--;
            }

            if (requiredSaws <= 0)
            {
                transform.Translate(0, 0, -0.1f);
                myRigidbody.isKinematic = false;
                linkedDoor.FallDown();
                gameObject.layer = LayerMask.NameToLayer("Prop");
            }
        }
        else if (itemType == ItemType.Flashlight) 
        {
            if (flashlight.light.intensity != 0)
            {
                flashlight.light.intensity = 0f;
            }
            else
            {
                flashlight.light.intensity = flashlight.initialIntensity;
            }
        }
    }
    public void SetHoverMode(HoverMode hoverMode)
    {
        if (hoverMode == HoverMode.hovering)
        {
            isHovering = true;
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            gameObject.layer = LayerMask.NameToLayer("Prop");

            #region SAVE AND DELETE RIGIDBODY
            if (myRigidbody != null)
            {
                myRigidbody.useGravity = false;
                CreateCloneRigidbodyOnTempGO();
                SetRigidbodyEqual(cloneRigidBody, myRigidbody);
                Destroy(myRigidbody);
                myRigidbody = null;
            }
            //GetComponent<Rigidbody>().isKinematic = true;
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
            //GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            #endregion
        }
        if (hoverMode == HoverMode.physicsOn)
            {
            isHovering = false;
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
            gameObject.layer = LayerMask.NameToLayer("Prop");

            #region LOAD RIGIDBODY AND DELETE SAVE
            if (myRigidbody == null) {
                myRigidbody = gameObject.AddComponent<Rigidbody>();
                SetRigidbodyEqual(myRigidbody, cloneRigidBody);
                Destroy(tempGOWithCloneRB);
                myRigidbody.angularDrag = 0f;
                myRigidbody.drag = 0f;
                myRigidbody.useGravity = true;
            }
            #endregion
        }
        if (hoverMode == HoverMode.stuck)
        {
            isHovering = false;
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
            #region SAVE AND DELETE RIGIDBODY
            if (myRigidbody != null)
            {
                CreateCloneRigidbodyOnTempGO();
                SetRigidbodyEqual(cloneRigidBody, myRigidbody);
                Destroy(myRigidbody);
                myRigidbody = null;
            }
            #endregion
        }

    }
    #region KEY STUFF
    public void OpenAnimationDone() {
        animator.Play("start");
        animator.enabled = false;
        SetHoverMode(HoverMode.stuck);
        currentDoor.locked = false;

    }
    #endregion

    //DO NOT EXTEND THIS IF YOU WANT TO KEEP YOUR SANITY:
    #region RIGIDBODY BULLSHIT
    private void SetRigidbodyEqual(Rigidbody theRigidbodyToChange, Rigidbody theRigidbodyToCopy) {
        theRigidbodyToChange.angularDrag = theRigidbodyToCopy.angularDrag;
        theRigidbodyToChange.angularVelocity = theRigidbodyToCopy.angularVelocity;
        theRigidbodyToChange.centerOfMass = theRigidbodyToCopy.centerOfMass;
        theRigidbodyToChange.collisionDetectionMode = theRigidbodyToCopy.collisionDetectionMode;
        theRigidbodyToChange.constraints = theRigidbodyToCopy.constraints;
        theRigidbodyToChange.detectCollisions = theRigidbodyToCopy.detectCollisions;
        theRigidbodyToChange.drag = theRigidbodyToCopy.drag;
        theRigidbodyToChange.freezeRotation = theRigidbodyToCopy.freezeRotation;
        theRigidbodyToChange.inertiaTensor = theRigidbodyToCopy.inertiaTensor;
        theRigidbodyToChange.inertiaTensorRotation = theRigidbodyToCopy.inertiaTensorRotation;
        theRigidbodyToChange.interpolation = theRigidbodyToCopy.interpolation;
        theRigidbodyToChange.mass = theRigidbodyToCopy.mass;
        theRigidbodyToChange.maxDepenetrationVelocity = theRigidbodyToCopy.maxDepenetrationVelocity;
        theRigidbodyToChange.name = theRigidbodyToCopy.name;
        theRigidbodyToChange.sleepThreshold = theRigidbodyToCopy.sleepThreshold;
        theRigidbodyToChange.solverIterations = theRigidbodyToCopy.solverIterations;
        theRigidbodyToChange.solverVelocityIterations = theRigidbodyToCopy.solverVelocityIterations;
        theRigidbodyToChange.useGravity = theRigidbodyToCopy.useGravity;
    }
    private void CreateCloneRigidbodyOnTempGO() {
        tempGOWithCloneRB = new GameObject("temp");
        tempGOWithCloneRB.AddComponent<Rigidbody>();
        tempGOWithCloneRB.hideFlags = HideFlags.HideInHierarchy;
        SetRigidbodyEqual(tempGOWithCloneRB.GetComponent<Rigidbody>(), myRigidbody);
        cloneRigidBody = tempGOWithCloneRB.GetComponent<Rigidbody>();
        tempGOWithCloneRB.SetActive(false);
    }
    #endregion
}
public enum ItemType
{
    Key,
    Gun,
    Trinket,
    Saw,
    Flashlight
}
[System.Serializable]
public struct Key {
    public int keyId;
}
[System.Serializable]
public struct Flashlight
{
    public bool on;
    public float initialIntensity;
    public Light light;
}
public enum ActionUponArrival {
    stopHovering,
    addToInventory,
    enableKeyAnimation,
    disablePhysics,
    disableProp,
    keepHovering
}
public enum HoverMode {
    hovering,
    physicsOn,
    stuck
}
[System.Serializable]
public enum Slot
{
    none,
    hand,
    head,
    back

}
