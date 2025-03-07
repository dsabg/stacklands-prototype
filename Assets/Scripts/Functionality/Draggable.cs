using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //������һ����ֻ̬���� Plane ���󣬱�ʾһ��ˮƽ�棬������Ϊ  -z �᷽�򣬲��Ҹ�ƽ����ԭ��ľ���Ϊ 0
    public static readonly Plane Board = new Plane(Vector3.back, 0);

    public event Action DragStarted;//ֻ���ڲ�����
    public event Action<Vector2> DragEnded;

    public Action<PointerEventData> PointerDownCallback;//�����ⲿ����
    public Action<PointerEventData> PointerUpCallback;

    public Vector2 TopLeft = new Vector2(-9, 5.5f);
    public Vector2 BottomRight = new Vector2(9, -4.5f);
    private Vector2 _CursorOffset;

    private bool _IsDragging = false;

    private (Ray ray, float distance) RaycastBoard(Vector3 origin)//Ԫ��
    {
        Ray ray = Camera.main.ScreenPointToRay(origin);//����Ļ��һ����������ռ��д������λ��ָ��õ������
        bool isIntersecting = Board.Raycast(ray, out float distance);

        if (!isIntersecting)
        {
            throw new System.Exception("No intersection between board and cursor.");
        }
        return (ray, distance);
    }

    private Vector3 PosFromRayAndDistance(Ray ray, float distance)
    {
        return Camera.main.transform.position + (ray.direction * distance);
    }

    private void Awake()
    {
        if (PointerDownCallback == null) PointerDownCallback = StartDrag;//Ĭ�ϻص�����
        if (PointerUpCallback == null) PointerUpCallback = StopDrag;
    }

    private void Update()
    {
        if (_IsDragging)
        {
            
            (Ray ray, float distance) = RaycastBoard(Mouse.current.position.ReadValue());
            Vector3 hitPos = PosFromRayAndDistance(ray, distance);
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(hitPos.x - _CursorOffset.x, hitPos.y - _CursorOffset.y, currentPos.z);
            
        }
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, TopLeft.x, BottomRight.x);
        pos.y = Mathf.Clamp(pos.y, BottomRight.y, TopLeft.y);
        transform.position = pos;
    }

    public void StartDrag(PointerEventData eventData)
    {
        Debug.Assert(!_IsDragging, "StartDrag called while already dragging.");//����Ϊfalseʱ�׳��쳣
        (Ray ray, float distance) = RaycastBoard(eventData.position);
        Vector3 hitPos = PosFromRayAndDistance(ray, distance);
        _IsDragging = true;
        DragStarted?.Invoke();
        _CursorOffset = new Vector2(hitPos.x - transform.position.x, hitPos.y - transform.position.y);
    }

    public void StopDrag(PointerEventData eventData)
    {
        _IsDragging = false;
        DragEnded?.Invoke(_CursorOffset);
    }

    public void OnPointerDown(PointerEventData eventData)//�û�����ָ�루���������������ʱ������
    {
        PointerDownCallback?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUpCallback?.Invoke(eventData);
    }
}
