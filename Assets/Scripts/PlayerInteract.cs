using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour 
{
    public float interactDistance = 3f;
    public LayerMask interactableLayer;
    public string interactKey = "e";
    public TMP_Text interactText; // Assign a UI Text element in the Inspector
    private GameObject currentObject;

    void Update()
    {
        CheckForObject();
        InteractWithObject();
    }

    void CheckForObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                if (currentObject != hit.collider.gameObject)
                {
                    // Reset the outline for the previous object
                    if (currentObject != null)
                    {
                        Outline prevOutline = currentObject.GetComponent<Outline>();
                        if (prevOutline != null) prevOutline.enabled = false;
                    }

                    // Assign the new object
                    currentObject = hit.collider.gameObject;

                    // Enable new object's outline
                    Outline outline = currentObject.GetComponent<Outline>();
                    if (outline != null) outline.enabled = true;

                    // Get the InteractableObject component
                    InteractableObject interactable = currentObject.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        // Show interaction text with the unique message
                        interactText.text = interactable.message + " (Press E)";
                    }
                    interactText.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            // Reset the object if no interactable object is found
            if (currentObject != null)
            {
                Outline outline = currentObject.GetComponent<Outline>();
                if (outline != null) outline.enabled = false;

                currentObject = null;
                interactText.gameObject.SetActive(false);
            }
        }
    }

    void InteractWithObject()
    {
        if (currentObject != null && Input.GetKeyDown(interactKey))
        {
            Debug.Log("Interacted with " + currentObject.name);
            // You can add specific interaction logic here if needed
        }
    }
}
