using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PuzzleSocket : MonoBehaviour
{
    [Header("Socket Settings")]
    public PuzzleShape requiredShape;

    [Header("XR Socket")]
    public XRSocketInteractor socketInteractor;

    [Header("Socket Materials")]
    public Material emptyMaterial;       
    public Material correctMaterial;     
    public Material incorrectMaterial;   

    [Header("Socket Renderer")]
    public Renderer socketRenderer;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip errorSound;

    [Header("Cannot Attach UI")]
    public GameObject cannotAttachUIPrefab;
    public Transform errorUISpawnPoint;
    public float errorUIDuration = 2f;

    private bool completed = false;
    private PuzzleObject currentObject;

    private void Awake()
    {
        // Automatically find XR Socket Interactor
        if (socketInteractor == null)
        {
            socketInteractor = GetComponent<XRSocketInteractor>();
        }

        // Listen for an object actually being attached
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnSocketSelectEntered);
        }
    }

    private void Start()
    {
        SetSocketMaterial(emptyMaterial);
    }

    private void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnSocketSelectEntered);
        }
    }

   

    private void OnTriggerEnter(Collider other)
    {
        if (completed)
            return;

        PuzzleObject puzzleObject =
            other.GetComponentInParent<PuzzleObject>();

        if (puzzleObject == null)
            return;

        currentObject = puzzleObject;

       

        if (puzzleObject.shape == requiredShape)
        {
            SetSocketMaterial(correctMaterial);

            // Enable socket so correct object can attach
            if (socketInteractor != null)
            {
                socketInteractor.enabled = true;
            }

            Debug.Log("Correct object near socket.");
        }

       
        else
        {
            SetSocketMaterial(incorrectMaterial);

            // Disable socket so wrong object cannot attach
            if (socketInteractor != null)
            {
                socketInteractor.enabled = false;
            }

            PlayErrorSound();
            SpawnCannotAttachUI();

            Debug.Log(
                "Cannot attach " +
                puzzleObject.shape +
                " to " +
                requiredShape +
                " socket."
            );
        }
    }

    

    private void OnTriggerExit(Collider other)
    {
        if (completed)
            return;

        PuzzleObject puzzleObject =
            other.GetComponentInParent<PuzzleObject>();

        if (puzzleObject == null)
            return;

        if (puzzleObject == currentObject)
        {
            currentObject = null;

            // Reset socket material
            SetSocketMaterial(emptyMaterial);

            // Enable socket again
            if (socketInteractor != null)
            {
                socketInteractor.enabled = true;
            }
        }
    }

   

    private void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        if (completed)
            return;

        // Get the object that was attached
        GameObject selectedObject = args.interactableObject.transform.gameObject;

        PuzzleObject puzzleObject =
            selectedObject.GetComponent<PuzzleObject>();

        // Sometimes PuzzleObject may be on a parent
        if (puzzleObject == null)
        {
            puzzleObject =
                selectedObject.GetComponentInParent<PuzzleObject>();
        }

        if (puzzleObject == null)
            return;

        // Safety check
        if (puzzleObject.shape != requiredShape)
        {
            Debug.LogWarning("Wrong object somehow entered socket.");

            // Remove wrong object
            if (socketInteractor != null)
            {
                socketInteractor.interactionManager.SelectExit(
                    socketInteractor,
                    args.interactableObject
                );
            }

            PlayErrorSound();
            return;
        }

        // Correct object
        CorrectPlacement(puzzleObject);
    }

   

    private void CorrectPlacement(PuzzleObject puzzleObject)
    {
        if (completed)
            return;

        if (puzzleObject == null)
            return;

        // Extra safety check
        if (puzzleObject.shape != requiredShape)
        {
            return;
        }

        completed = true;

        currentObject = puzzleObject;

        // Keep socket green
        SetSocketMaterial(correctMaterial);

        // Make sure socket remains enabled
        if (socketInteractor != null)
        {
            socketInteractor.enabled = true;
        }

        // Snap object exactly to socket
        puzzleObject.transform.SetPositionAndRotation(
            transform.position,
            transform.rotation
        );

        // Tell ObjectReturn that the object was successfully placed
        ObjectReturn objectReturn =
            puzzleObject.GetComponent<ObjectReturn>();

        if (objectReturn != null)
        {
            objectReturn.SetPlacedInSocket(true);
        }

        // Disable grabbing
        XRGrabInteractable grabInteractable =
            puzzleObject.GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Success sound
        PlaySuccessSound();

        Debug.Log(
            "Correct object placed in " +
            requiredShape +
            " socket!"
        );

        // Notify Puzzle Manager
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.SocketCompleted();
        }
    }

   
    private void PlaySuccessSound()
    {
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }
    }

    

    private void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
    }

    

    private void SpawnCannotAttachUI()
    {
        if (cannotAttachUIPrefab == null)
            return;

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        if (errorUISpawnPoint != null)
        {
            spawnPosition = errorUISpawnPoint.position;
            spawnRotation = errorUISpawnPoint.rotation;
        }

        GameObject errorUI = Instantiate(
            cannotAttachUIPrefab,
            spawnPosition,
            spawnRotation
        );

        Destroy(errorUI, errorUIDuration);
    }

    

    private void SetSocketMaterial(Material material)
    {
        if (socketRenderer != null && material != null)
        {
            socketRenderer.material = material;
        }
    }

   
    public void ResetSocket()
    {
        completed = false;
        currentObject = null;

        // Reset socket appearance
        SetSocketMaterial(emptyMaterial);

        // Make sure socket is enabled
        if (socketInteractor != null)
        {
            socketInteractor.enabled = true;
        }

        Debug.Log(
            gameObject.name +
            " socket reset."
        );
    }


}