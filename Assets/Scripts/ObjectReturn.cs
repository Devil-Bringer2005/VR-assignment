using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ObjectReturn : MonoBehaviour
{
    [Header("Return Settings")]
    [Tooltip("Time in seconds before an unplaced object returns.")]
    [SerializeField] private float returnDuration = 5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Coroutine returnTimerCoroutine;

    // True when the correct socket has accepted the object
    private bool placedInSocket = false;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

   

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Check who selected the object
        IXRSelectInteractor interactor = args.interactorObject;

       

        if (interactor is XRSocketInteractor)
        {
        
            StopReturnTimer();

            Debug.Log(
                gameObject.name +
                " was selected by a socket."
            );

            return;
        }

        placedInSocket = false;

        // Cancel any old timer
        StopReturnTimer();

        // Start a completely fresh timer
        returnTimerCoroutine = StartCoroutine(ReturnTimer());

        Debug.Log(
            gameObject.name +
            " grabbed. Timer reset to " +
            returnDuration +
            " seconds."
        );
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        IXRSelectInteractor interactor = args.interactorObject;


        if (interactor is XRSocketInteractor)
        {
            // If this was a correctly completed placement,
            // don't return the object.
            if (placedInSocket)
            {
                StopReturnTimer();

                Debug.Log(
                    gameObject.name +
                    " remains in completed socket."
                );

                return;
            }

            return;
        }

        if (placedInSocket)
        {
            // Correctly placed object should stay.
            StopReturnTimer();
            return;
        }

        // Start the return countdown.
        StopReturnTimer();

        returnTimerCoroutine = StartCoroutine(ReturnTimer());

        Debug.Log(
            gameObject.name +
            " released. Return timer started: " +
            returnDuration +
            " seconds."
        );
    }

    private IEnumerator ReturnTimer()
    {
        float timer = returnDuration;

        while (timer > 0f)
        {
            // Correct placement cancels the timer
            if (placedInSocket)
            {
                yield break;
            }

            timer -= Time.deltaTime;

            yield return null;
        }

        // Timer finished
        if (placedInSocket)
        {
            yield break;
        }

        // Don't return if a socket currently owns the object
        if (IsAttachedToSocket())
        {
            yield break;
        }

        // Return to original position
        TeleportToOriginalPosition();

        returnTimerCoroutine = null;
    }

   
    private bool IsAttachedToSocket()
    {
        if (grabInteractable == null)
            return false;

        foreach (IXRSelectInteractor interactor
                 in grabInteractable.interactorsSelecting)
        {
            if (interactor is XRSocketInteractor)
            {
                return true;
            }
        }

        return false;
    }

   
    public void SetPlacedInSocket(bool placed)
    {
        placedInSocket = placed;

        if (placed)
        {
            // Absolutely make sure no return timer is running.
            StopReturnTimer();

            // Stop physics movement
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log(
                gameObject.name +
                " marked as PLACED IN SOCKET."
            );
        }
        else
        {
            Debug.Log(
                gameObject.name +
                " marked as NOT PLACED."
            );
        }
    }

  
    private void StopReturnTimer()
    {
        if (returnTimerCoroutine != null)
        {
            StopCoroutine(returnTimerCoroutine);
            returnTimerCoroutine = null;
        }
    }

  
    public void TeleportToOriginalPosition()
    {
        StopReturnTimer();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
        }

        transform.SetPositionAndRotation(
            originalPosition,
            originalRotation
        );

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = false;
        }

        Debug.Log(
            gameObject.name +
            " teleported to original position."
        );
    }
}
