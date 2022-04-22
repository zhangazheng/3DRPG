using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;
    public event Action<Vector3> OnMouseClicked;

    [SerializeField] private Texture2D point, doorway, attact, target, arrow;

    RaycastHit hitInfo;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetCursorTexture();
        MouseControl();
    }
    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo))
        {
            switch(hitInfo.collider.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.ForceSoftware);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attact, new Vector2(16, 16), CursorMode.ForceSoftware);
                    break;
            }
        }
    }
    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            OnMouseClicked?.Invoke(hitInfo.point);
        }
    }
}
