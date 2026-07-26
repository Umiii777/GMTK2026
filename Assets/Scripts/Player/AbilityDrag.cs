using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDrag : MonoBehaviour
{
    public Camera mainCam;
    public float frequency = 5f;
    public float damping = 1f;

    private TargetJoint2D targetJoint;
    private BaseObject draggingObj;

    private void Update()
    {
        if (PauseManager.IsPaused)
        {
            if (targetJoint != null)
                OnDropThings();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryDragThings();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnDropThings();
        }
    }
    void FixedUpdate()
    {
        if (PauseManager.IsPaused)
            return;

        if (targetJoint)
        {
            targetJoint.target = GetMouseWorldPosition2D();
        }
    }
    private void TryDragThings()
    {
        Vector2 worldPos = GetMouseWorldPosition2D();
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider)
        {
            BaseObject targetObj = hit.collider.gameObject?.GetComponent<BaseObject>();
            Rigidbody2D rb = hit.collider.attachedRigidbody;
            if(targetObj ==null)
            {
                return;
            }
            if (rb && targetObj.canDrag)
            {
                targetJoint = rb.gameObject.AddComponent<TargetJoint2D>();
                targetJoint.frequency = frequency;
                targetJoint.dampingRatio = damping;
                targetJoint.autoConfigureTarget = false;
                targetJoint.target = worldPos;
            }
        }
    }
    private void OnDropThings()
    {
        if (targetJoint)
            Destroy(targetJoint);

    }
    private Vector2 GetMouseWorldPosition2D()
    {
        Vector3 pos = Input.mousePosition;

        // 以下两行是为了匹配老电视滤镜的鱼眼透视特效 by ZJWCY at 2026/7/26 14:27
        if (mainCam.GetComponent<OldTelevision>().enabled)
            pos = TransformMousePos(pos);

        pos.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(pos);
    }
    private Vector2 TransformMousePos(Vector2 mousePos)
    {
        var distortion = OldTelevision.Distortion;
        var resolution = Screen.currentResolution;

        mousePos.x /= resolution.width;
        mousePos.y /= resolution.height;

        var x = mousePos.x;
        mousePos.x += Mathf.Pow(mousePos.y * 2f - 1f, 2f) * distortion * (mousePos.x - 0.5f);
        mousePos.y += Mathf.Pow(x * 2f - 1f, 2f) * distortion * (mousePos.y - 0.5f);

        mousePos.x *= resolution.width;
        mousePos.y *= resolution.height;

        return mousePos;
    }
}
