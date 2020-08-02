using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField]
    GameObject itemSlot;
    [SerializeField]
    float itemSlotCellSize = 1f;
    public int selectedSlot = 0;
    int prevSelectedSlot = 0;
    int prevInventoryItemCount;
    int inventorySize;
    public Prop selectedProp;
    PlayerController currentPlayerController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else {
            Destroy(this);
        }
    }
    private void Start()
    {
        currentPlayerController = FindObjectOfType<PlayerController>();
    }
    private void Update()
    {
        if (Input.mouseScrollDelta.y > 0.1)
        {
            selectedSlot += 1;
        }
        if (Input.mouseScrollDelta.y < -0.1)
        {
            selectedSlot -= 1;
        }

        #region SET INVENTORY BY NUMBERS
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            selectedSlot = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedSlot = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedSlot = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectedSlot = 3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedSlot = 4;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            selectedSlot = 5;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            selectedSlot = 6;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            selectedSlot = 7;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            selectedSlot = 8;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            selectedSlot = 9;
        }

        #endregion

        #region CLAMP


        if (selectedSlot > MetaManager.playerInventory.Count - 1)
        {
            selectedSlot = MetaManager.playerInventory.Count - 1;
        }
        if (selectedSlot < 0)
        {
            selectedSlot = 0;
        }
        #endregion

        #region UPDATE DISPLAY
        if (prevSelectedSlot != selectedSlot || prevInventoryItemCount != MetaManager.playerInventory.Count) {
            OnInventoryChange();
        }
        prevSelectedSlot = selectedSlot;
        prevInventoryItemCount = MetaManager.playerInventory.Count;
        #endregion  
    }
    void OnInventoryChange() {
        if (MetaManager.playerInventory.Count != 0)
        {
            selectedProp = MetaManager.playerInventory[selectedSlot].GetComponent<Prop>();
        }
        else {
            selectedProp = null;
        }
        UpdateInventoryDisplay();
        currentPlayerController.RefreshHeldObject();
    }
    public void StartHoverAnimation(Transform destination, GameObject gm,Transform parent, ActionUponArrival actionUponArrival, bool useDestinationTransformRotation) {
        Prop item = gm.GetComponent<Prop>();
            item.arrivalAction = actionUponArrival;
            item.gameObject.transform.parent = parent;
            item.hoverDestinationTransform = destination;
            item.useDestinationTransformRotation = useDestinationTransformRotation;
            item.SetHoverMode(HoverMode.hovering);
    }
    // V a workaround if you want to input a vector 3 instead V
    public void StartHoverAnimation(Vector3 destination, Quaternion rotation, GameObject gm, Transform parent, ActionUponArrival actionUponArrival, bool useDestinationTransformRotation)
    {
        GameObject newGameobject= new GameObject("temp");
        Transform transform = newGameobject.transform;
        transform.position = destination;
        transform.rotation = rotation;

        //newGameobject.hideFlags = HideFlags.HideInHierarchy;
        StartCoroutine(DestroyAfterDelay(5, newGameobject));

        StartHoverAnimation(transform, gm, parent, actionUponArrival, useDestinationTransformRotation);
    }
    public void StartHoverAnimation(Vector3 destination, GameObject gm, Transform parent, ActionUponArrival actionUponArrival)
    {
        StartHoverAnimation(destination, Quaternion.Euler(Vector3.zero), gm, parent, actionUponArrival, false);
    }
    public void DisableHoverAnimation(GameObject gm)
    {
        Prop item = gm.GetComponent<Prop>();
        if (item.storable == true)
        {
            item.gameObject.transform.parent = null;
            item.hoverDestinationTransform = null;
            item.SetHoverMode(HoverMode.physicsOn);
        }
    }
    public bool AddToInventory(GameObject gameObject) {
        
        Prop item = gameObject.GetComponent<Prop>();
        if (item != null && item.storable == true)
        {
            MetaManager.playerInventory.Add(gameObject);
            gameObject.SetActive(false);
            UpdateInventoryDisplay();
            DisableHoverAnimation(gameObject);
            return true;
        }
        else { return false; }
    }
    public void RemoveFromInventory(int id, Vector3 startPos) {
        GameObject gameObject = MetaManager.playerInventory[id];
        gameObject.transform.position = startPos;
        gameObject.transform.parent = null;
        gameObject.SetActive(true);
        MetaManager.playerInventory.RemoveAt(id);
        UpdateInventoryDisplay();
    }
    public void ActivateFromInventory(int id, Vector3 startPos)
    {
        GameObject gameObject = MetaManager.playerInventory[id];
        gameObject.transform.position = startPos;
        gameObject.transform.parent = null;
        gameObject.SetActive(true);
    }
    public void HoverFromInventoryKeepingReference(int id, Vector3 startPos, Transform destinationTransform, bool useDestinationTransformRotation, ActionUponArrival actionUponArrival)
    {
        StartHoverAnimation(destinationTransform, MetaManager.playerInventory[id], null, actionUponArrival, useDestinationTransformRotation);
        ActivateFromInventory(id, startPos);
    }
    public void HoverFromInventory(int id, Vector3 startPos, Transform destinationTransform, ActionUponArrival actionUponArrival)
    {
        StartHoverAnimation(destinationTransform.position, destinationTransform.rotation, MetaManager.playerInventory[id], null, actionUponArrival, false);
        RemoveFromInventory(id, startPos);
    }
    public void HoverFromInventory(int id, Vector3 startPos, Transform destinationTransform, bool useDestinationTransformRotation, ActionUponArrival actionUponArrival)
    {
        StartHoverAnimation(destinationTransform, MetaManager.playerInventory[id], null, actionUponArrival, useDestinationTransformRotation);
        RemoveFromInventory(id, startPos);
    }
    public void HoverFromInventory(int id, Vector3 startPos, Vector3 endPos, Quaternion endRot, bool useDestinationTransformRotation, ActionUponArrival actionUponArrival) {
        StartHoverAnimation(endPos, endRot, MetaManager.playerInventory[id],null, actionUponArrival, useDestinationTransformRotation);
        RemoveFromInventory(id, startPos);
    }
    public void HoverFromInventory(int id, Vector3 startPos, Vector3 endPos, ActionUponArrival actionUponArrival)
    {
        StartHoverAnimation(endPos, MetaManager.playerInventory[id], null, actionUponArrival);
        RemoveFromInventory(id, startPos);
    }
    public void ThrowFromInventory(int id, Vector3 startPos, Vector3 startingImpusle) {
        GameObject gameObject = MetaManager.playerInventory[id];
        DisableHoverAnimation(gameObject);
        RemoveFromInventory(id, startPos);
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        rigidbody.AddTorque(Random.insideUnitSphere * startingImpusle.magnitude * 1.4f, ForceMode.Impulse);
        rigidbody.AddForce(startingImpusle,ForceMode.Impulse);

    }
    public void UpdateInventoryDisplay()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        int x = 0;
        for (int i = 0; i < MetaManager.playerInventory.Count; i++)
        {
            GameObject newItemSlot = Instantiate(itemSlot, transform);

            RectTransform itemSlotRectTransform = newItemSlot.GetComponent<RectTransform>();
            itemSlotRectTransform.anchoredPosition = new Vector2((x - selectedSlot) * itemSlotCellSize,0);
            x++;

            newItemSlot.transform.Find("preview").GetComponent<Image>().sprite = MetaManager.playerInventory[i].GetComponent<Prop>().preview;
        }
    }
    public IEnumerator DestroyAfterDelay(float delay, GameObject gmo) {
        yield return new WaitForSeconds(delay);
        Destroy(gmo);
    }
    
}
