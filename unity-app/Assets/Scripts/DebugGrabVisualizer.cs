using UnityEngine;

// Simple visualizer to help debug two-hand grab/pull detection.
// Attach this to the same GameObject as BreathBlockBehavior (the cube).
public class DebugGrabVisualizer : MonoBehaviour
{
    public string leftAnchorName = "LeftControllerAnchor";
    public string rightAnchorName = "RightControllerAnchor";
    public float nearRadius = 0.18f;
    public float pullThreshold = 0.12f;

    Transform leftAnchor;
    Transform rightAnchor;
    float initialDistance = 0f;
    bool tracking = false;

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        FindAnchorsIfMissing();
        if (rend != null) rend.material.color = Color.white;
    }

    void Update()
    {
        if (leftAnchor == null || rightAnchor == null) FindAnchorsIfMissing();

        if (leftAnchor == null || rightAnchor == null)
        {
            // can't detect
            if (rend != null) rend.material.color = Color.gray;
            return;
        }

        float dl = Vector3.Distance(leftAnchor.position, transform.position);
        float dr = Vector3.Distance(rightAnchor.position, transform.position);

        bool bothNear = dl <= nearRadius && dr <= nearRadius;

        if (bothNear && !tracking)
        {
            tracking = true;
            initialDistance = Vector3.Distance(leftAnchor.position, rightAnchor.position);
            if (rend != null) rend.material.color = Color.yellow; // near and tracking
        }
        else if (tracking)
        {
            float current = Vector3.Distance(leftAnchor.position, rightAnchor.position);
            if (current - initialDistance > pullThreshold)
            {
                if (rend != null) rend.material.color = Color.green; // pull triggered
            }
            else
            {
                if (rend != null) rend.material.color = Color.yellow; // still tracking
            }

            // stop tracking if anchors moved away from the block
            if (!(Vector3.Distance(leftAnchor.position, transform.position) <= nearRadius && Vector3.Distance(rightAnchor.position, transform.position) <= nearRadius))
            {
                tracking = false;
                if (rend != null) rend.material.color = Color.white;
            }
        }
        else
        {
            if (rend != null) rend.material.color = Color.white;
        }
    }

    void FindAnchorsIfMissing()
    {
        if (leftAnchor == null && !string.IsNullOrEmpty(leftAnchorName))
        {
            var go = GameObject.Find(leftAnchorName);
            if (go != null) leftAnchor = go.transform;
        }
        if (rightAnchor == null && !string.IsNullOrEmpty(rightAnchorName))
        {
            var go = GameObject.Find(rightAnchorName);
            if (go != null) rightAnchor = go.transform;
        }
    }
}
