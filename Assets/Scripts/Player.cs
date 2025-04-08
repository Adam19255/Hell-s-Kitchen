using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    public static Player Instance { get; private set; } // Singleton instance of the Player class

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public ClearCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private GameInputs gameInputs;
    [SerializeField] private LayerMask countersLayerMask;

    private float playerRadius = 0.7f;
    private float playerHeight = 2f;
    private bool isWalking;
    private Vector3 lastInteratctionPosition;
    private ClearCounter selectedCounter;

    private void Awake() {
        if ( Instance != null) {
            Debug.LogError("There is more than one Player instance.");
        }
        Instance = this; // Set the singleton instance to this instance
    }

    private void Start() {
        gameInputs.OnInteract += GameInputs_OnInteract;
    }

    private void GameInputs_OnInteract(object sender, System.EventArgs e) {
        if (selectedCounter != null) {
            selectedCounter.Interact();
        }
    }

    private void Update() {
        HandleMovment();
        HandleInteractions();
    }

    public bool IsWalking() {
        return isWalking;
    }

    private void HandleInteractions() {
        Vector2 inputVector = gameInputs.GetMovementVectorNordmalized();

        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);

        if (movement != Vector3.zero) {
            lastInteratctionPosition = movement;
        }

        float interactionDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteratctionPosition, out RaycastHit raycastHit, interactionDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter)) {
                //  Has a ClearCounter component
                if (clearCounter != selectedCounter) {
                    SetSelectedCounter(clearCounter);
                }
            }
            else {
                SetSelectedCounter(null);
            }
        }
        else {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovment() {
        Vector2 inputVector = gameInputs.GetMovementVectorNordmalized();

        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y); // Convert to 3D movement to prevent player from moving up and down
        float moveDistence = moveSpeed * Time.deltaTime;

        bool collision = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movement, moveDistence); // Check for collision with other objects

        if (collision) {
            // Cannot move to that direction
            // Check only the X and Z axis
            Vector3 movementX = new Vector3(movement.x, 0, 0).normalized;
            collision = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementX, moveDistence);

            if (!collision) {
                // Move only in the X axis
                movement = movementX;
            }
            else {
                // Check only the Z axis
                Vector3 movementZ = new Vector3(0, 0, movement.z).normalized;
                collision = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementZ, moveDistence);

                if (!collision) {
                    // Move only in the Z axis
                    movement = movementZ;
                }
                else {
                    // Cannot move in any direction
                }
            }
        }

        if (!collision) {
            // There is no collision, so we can move the player
            transform.position += movement * moveDistence;
        }


        isWalking = movement != Vector3.zero; // Check if the player is moving

        transform.forward = Vector3.Slerp(transform.forward, movement, Time.deltaTime * rotateSpeed); // Rotate the player to face the direction of movement
    }

    private void SetSelectedCounter(ClearCounter selectedCounter) {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });
    }
}
