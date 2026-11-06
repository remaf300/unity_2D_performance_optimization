using System.Collections;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    private float maxDistance = 22;

    [Header("Pickup System")]
    [SerializeField] private float pickupRangeRadius;
    [SerializeField] private LayerMask playerLayer;
    private bool isPickedup;
    private bool isInRange;
    private bool isIndicationsVisible;

    [Header("Item Name Indication")]
    [SerializeField] private GameObject itemNameIndicationPrefab;
    [SerializeField] private ParticleSystem interactParticles;
    private ItemNameIndication currentItemNameIndication;

    [Header("Outline")]
    [SerializeField] private float outlineThickness = 2;
    [SerializeField] private Material outlineMaterial;
    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;
    
    private PickupBehaviour playerPickupBehaviour;
    private Item itemScript;
    private Coroutine currentRoutine;
    private Rigidbody2D rb;
    private float currentDistance;
   

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        itemScript = GetComponent<Item>();
        Init();
    }

    private void Init()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Crée une array et établit sa longueur grace au nombre de spriteRenderers récupérés dans Awake
        materials = new Material[spriteRenderers.Length];

        // ajoute les materials des sprite renderers dans materials
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            // Applique l'outline sur les spriteRenderers
            spriteRenderers[i].material = outlineMaterial;
            materials[i] = spriteRenderers[i].material;
        }

        SetOutline(0);
    }

    private void Start()
    {
        playerPickupBehaviour = PlayerMovement.instance.GetComponent<PickupBehaviour>();

        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (isInRange && !isPickedup && !Menu.isPaused && !playerPickupBehaviour.isPicking)
        {
            Pickup();
        }
    }

    private void Update()
    {
        // if player is in range, show indications
        if(isInRange)
        {
            if (!isIndicationsVisible)
            {
                isIndicationsVisible = true;

                // Instantiate name indication
                currentItemNameIndication = Instantiate(itemNameIndicationPrefab, transform.position, Quaternion.Euler(0, 0, -transform.rotation.z)).GetComponent<ItemNameIndication>();
                currentItemNameIndication.transform.parent = transform;
                currentItemNameIndication.UpdateIndication(itemScript.itemData.name, itemScript.itemData.isDiscovered);

                // Show outline
                SetOutline(outlineThickness);

                // Show particles
                interactParticles.Play();
            }

            currentItemNameIndication.transform.rotation = Quaternion.Euler(0, 0, -transform.rotation.z);

            // Si on récupère un item et qu'il vient d'être découvert
            if (playerPickupBehaviour.isPicking)
                currentItemNameIndication.UpdateIndication(itemScript.itemData.name, itemScript.itemData.isDiscovered);
        }
        else // if player is not in range, hide indications
        {
            if (isIndicationsVisible)
            {
                isIndicationsVisible = false;

                // Hide name indication
                currentItemNameIndication.DestroySelf();

                // Hide outline
                SetOutline(0);

                // Hide particles
                interactParticles.GetComponent<ParticleSystem>().Stop();
            }
        }

        currentDistance = Vector3.Distance(playerPickupBehaviour.transform.position, transform.position);
        isInRange = currentDistance <= pickupRangeRadius;
    }

    private void FixedUpdate()
    {
        if (currentRoutine == null) currentRoutine = StartCoroutine(HandleRigidbody());
    }

    IEnumerator HandleRigidbody()
    {
        yield return new WaitForSeconds(1f);

        // if the player is near the item
        if (currentDistance < maxDistance)
        {
            // we reactivate the physics
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.None;
        }
        else if (rb.bodyType != RigidbodyType2D.Kinematic && rb.linearVelocity.magnitude < 1f)
        {
            // we freeze the object
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        currentRoutine = null;
    }

    private void Pickup()
    {
        isPickedup = true;
        interactParticles.transform.parent = null;
        interactParticles.Stop();
        currentItemNameIndication.DestroySelf();

        // Ajouter l'objet dans l'inventaire
        playerPickupBehaviour.DoPickup(itemScript);
    }

    public void ReactivateItem()
    {
        isPickedup = false;
        interactParticles.Play();
    }

    public void DestroyItem()
    {
        GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;

        Destroy(interactParticles.gameObject);
        Destroy(gameObject);
    }

    private void SetOutline(float newOutlineThickness)
    {
        // Set the flash amount
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_OutlineThickness", newOutlineThickness);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pickupRangeRadius);
    }
}
