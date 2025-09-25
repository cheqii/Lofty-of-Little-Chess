using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;

    private void Start()
    {
        // at first try to get sprite renderer from the object itself
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            // if not found, try to get from children
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // if still not found, log a warning
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not found on {gameObject.name} or its children.");
            return;
        }

        // store the default material
        defaultMaterial = spriteRenderer.material;
    }

    public void SetSelected(bool _selected)
    {
        if (_selected)
        {
            spriteRenderer.material = highlightMaterial;
        }
        else
        {
            spriteRenderer.material = defaultMaterial;
        }
    }
}
