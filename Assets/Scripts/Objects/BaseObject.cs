using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : MonoBehaviour
{
    //定义可拖拽类型
    public enum ObjectMassType
    {
        Heavy,
        Light,
        normal
    }
    public enum DragMoveType
    {
        Free,
        Xlimited,
        Ylimited
    }

    [Header("DragConfig")]
    private Rigidbody2D rb;

    public ObjectMassType massType;
    public DragMoveType dragMoveType;

    public bool canDrag = false;

    public bool limitDragDistance = false;
    public float maxDragDistance = 3f;

    private Vector2 initialPos;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyMassConfig();

    }
    public void OnDragStart()
    {
        initialPos = rb.position;
    }

    public Vector2 MododifyDrag(Vector2 targetPos)
    {
        Vector2 resultPos = targetPos;

        switch (dragMoveType)
        {
            case DragMoveType.Xlimited:
                resultPos.x = rb.position.x;
                break;
            case DragMoveType.Ylimited:
                resultPos.y = rb.position.y;
                break;
        }
        if (limitDragDistance)
        {
            Vector2 offset = resultPos - initialPos;
            if (offset.magnitude > maxDragDistance)
            {
                resultPos = initialPos + offset.normalized * maxDragDistance;
            }
        }
        return resultPos;
    }
    private void ApplyMassConfig()
    {
        switch (massType)
        {
            case ObjectMassType.Light:
                rb.mass = 0.5f;
                rb.gravityScale = 0.8f;
                break;
            case ObjectMassType.Heavy:
                rb.mass = 5f;
                rb.gravityScale = 2.5f;
                break;
            case ObjectMassType.normal:
                rb.mass = 1f;
                rb.gravityScale = 1f;
                break;
        }
    }

}
