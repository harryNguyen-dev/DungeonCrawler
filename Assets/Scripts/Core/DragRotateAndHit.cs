using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BulletHellTemplate
{
    /// <summary>
    /// This script allows rotation of a child object inside a containerPassItems when dragging or holding,
    /// or triggers an animation on click without dragging.
    /// Attach this script to an Image or Button (set to transparent).
    /// </summary>
    public class DragRotateAndHit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public static DragRotateAndHit Singleton;

        [Header("Container Settings")]
        [Tooltip("The containerPassItems that holds the child object to be rotated.")]
        public Transform container;

        [Tooltip("Rotation speed of the containerPassItems.")]
        public float rotateSpeed = 4f;

        [Tooltip("Audio clip to play when triggering a hit animation.")]
        public AudioClip hitClip;

        [Tooltip("Audio mixer tag to identify the audio group.")]
        public string hitTag = "master";

        // private CharacterModel childCharacterModel;

        private bool isDragging = false;
        private bool isHolding = false;

        private Vector2 startPointerPosition;
        private Quaternion initialRotation;

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupChild();
        }

        /// <summary>
        /// Sets up the child object inside the containerPassItems and retrieves its Animator and/or CharacterModel.
        /// </summary>
        /// <summary>
        /// Initializes references to the first child of the containerPassItems.
        /// Tries to find Animator and CharacterModel in the child, and falls back to the containerPassItems if not found.
        /// </summary>
        private void SetupChild()
        {
            if (container == null)
            {
                Debug.LogError("No containerPassItems assigned. Please assign a containerPassItems in the inspector.");
                return;
            }

            if (container.childCount > 0)
            {
                Transform child = container.GetChild(0);
                
                // childCharacterModel = child.GetComponent<CharacterModel>();            

                // if (childCharacterModel == null)
                    // childCharacterModel = container.GetComponent<CharacterModel>();
              
            }
            else
            {
                Debug.LogError("No child found in the containerPassItems.");
            }

            initialRotation = container.rotation;
        }


        /// <summary>
        /// Called when the pointer is pressed down on the UI element.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            startPointerPosition = eventData.position;
            isDragging = false;
            isHolding = true;
        }

        /// <summary>
        /// Called when dragging the UI element. Rotates the containerPassItems based on horizontal drag movement.
        /// </summary>
        /// <param name="eventData">Pointer event data containing the drag information.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (container == null || container.childCount == 0)
                return;

            Vector2 dragDirection = eventData.position - startPointerPosition;
            if (dragDirection.x != 0)
            {
                float rotationAmount = -rotateSpeed * dragDirection.x * Time.deltaTime;
                container.Rotate(Vector3.up, rotationAmount);
            }

            isDragging = true;
        }

        private void Update()
        {
            // If holding but not dragging, rotate the containerPassItems slowly
            if (isHolding && !isDragging)
            {
                container.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Called when the pointer is released. If not dragging, triggers the hit animation.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            isHolding = false;

            // If it was not dragging, trigger a "Hit" animation
            if (!isDragging)
            {
                // Play audio if available
                // if (AudioManager.Singleton != null)
                // {
                //     AudioManager.Singleton.PlayAudio(hitClip, hitTag);
                // }

                // // Check if the child has a CharacterModel with Playable animations
                // if (childCharacterModel != null)
                // {
                //     EventBus.Publish(new AnimationOnReceiveDamageEvent(childCharacterModel));                
                // }               
                // else
                // {
                //     Debug.LogWarning("No CharacterModel found to trigger the hit animation.");
                // }
            }
        }

        /// <summary>
        /// Resets the containerPassItems rotation to its original state.
        /// </summary>
        public void ResetRotation()
        {
            if (container != null)
            {
                container.rotation = initialRotation;
            }
            else
            {
                Debug.LogError("No containerPassItems assigned to reset rotation.");
            }
        }

        /// <summary>
        /// Updates the character after a new one is loaded into the containerPassItems.
        /// </summary>
        public void UpdateCharacter()
        {
            UpdateCharacterAsync().Forget(); 
        }

        /// <summary>
        /// Waits a frame to ensure the new character is instantiated before setup.
        /// Uses UniTask to avoid issues when changing scenes or exiting play mode.
        /// </summary>
        private async UniTaskVoid UpdateCharacterAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, token);

            ResetRotation();
            SetupChild();

            // if (childCharacterModel == null)
            // {
            //     Debug.LogError("No CharacterModel found after character update. Check the containerPassItems's child.");
            // }
        }
    }
}
