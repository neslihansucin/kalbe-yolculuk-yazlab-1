
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereInteraction : MonoBehaviour
{
    [SerializeField] private GameObject sphereText;
    [SerializeField] private Transform sphereHoldPoint;

    private PlayerInventory playerInventory;
    private bool playerNearby = false;
    private bool collected = false;

    private void Start()
    {
        sphereText.SetActive(false);
    }



    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("F tuþuna basýldý!");

            if (playerNearby && !collected)
            {
                Debug.Log("Küre alma iþlemi baþladý!");
                CollectSphere();
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory != null)
        {
            playerNearby = true;
            sphereText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            sphereText.SetActive(false);
        }
    }

    private void CollectSphere()
    {
        if (playerInventory == null || sphereHoldPoint == null)
            return;

        playerInventory.CollectSphere();
        collected = true;
        playerNearby = false;

        sphereText.SetActive(false);

        Collider sphereCollider = GetComponent<Collider>();
        if (sphereCollider != null)
            sphereCollider.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }


        transform.SetParent(sphereHoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        SphereFloating floating = GetComponent<SphereFloating>();

        if (floating != null)
        {
            floating.StartFloating();
            Debug.Log("Küre süzülmeye baþladý!");
        }
        else
        {
            Debug.LogWarning("SphereFloating bileþeni bulunamadý!");
        }

    }
}