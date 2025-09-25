using UnityEngine;

public class GridTargetMarked : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

    public void SetVisibleGridMarked(bool _isActive)
    {
        meshRenderer.enabled = _isActive;
    }
}
