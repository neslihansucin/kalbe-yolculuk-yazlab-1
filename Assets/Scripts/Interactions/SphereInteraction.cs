
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereInteraction : MonoBehaviour
{
    [SerializeField] private GameObject sphereText;
    [SerializeField] private Transform sphereHoldPoint;

    [Header("Küre Yerleþtirme")]
    [SerializeField] private Transform spherePlacementPoint;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float placementDistance = 2f;

    [Header("Kalp Merkezi Mesajý")]
    [SerializeField] private GameObject heartRepairText;

    [Header("Kalp Iþýklarý")]
    [SerializeField] private HeartLightController heartLightController;

    private PlayerInventory playerInventory;
    private bool playerNearby = false;
    private bool collected = false;
    private bool placed = false;

    private void Start()
    {
        if (sphereText != null)
            sphereText.SetActive(false);

        if (heartRepairText != null)
            heartRepairText.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // F tuþuyla küreyi al
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (playerNearby && !collected)
                CollectSphere();
        }

        // Kalp merkezine yaklaþýnca mesajý göster
        UpdateHeartRepairText();

        // Y tuþuyla küreyi yerleþtir
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            if (collected && !placed)
                PlaceSphere();
        }
    }

    private void UpdateHeartRepairText()
    {
        if (heartRepairText == null ||
            spherePlacementPoint == null ||
            playerTransform == null)
            return;

        float distance = Vector3.Distance(
            playerTransform.position,
            spherePlacementPoint.position
        );

        bool showMessage =
            collected &&
            !placed &&
            distance <= placementDistance;

        if (heartRepairText.activeSelf != showMessage)
            heartRepairText.SetActive(showMessage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        playerInventory = other.GetComponent<PlayerInventory>();

        if (playerInventory != null)
        {
            playerTransform = other.transform;
            playerNearby = true;

            if (sphereText != null)
                sphereText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (sphereText != null)
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

        if (sphereText != null)
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
            floating.StartFloating();
    }

    private void PlaceSphere()
    {
        if (spherePlacementPoint == null || playerTransform == null)
            return;

        float distance = Vector3.Distance(
            playerTransform.position,
            spherePlacementPoint.position
        );

        if (distance > placementDistance)
            return;

        SphereFloating floating = GetComponent<SphereFloating>();

        if (floating != null)
            floating.enabled = false;

        transform.SetParent(spherePlacementPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        placed = true;

        if (heartRepairText != null)
            heartRepairText.SetActive(false);

        // Küre yerleþtirilince üç ýþýðý yak
        if (heartLightController != null)
        {
            heartLightController.TurnOnLights();
        }

        Debug.Log("Küre kalbin merkezine yerleþtirildi!");
    }
}
