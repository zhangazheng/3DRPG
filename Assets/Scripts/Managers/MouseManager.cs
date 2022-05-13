using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MouseManager : Singleton<MouseManager>
{
    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    [SerializeField] private Texture2D point, doorway, attact, target, arrow;

    RaycastHit hitInfo;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
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
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.ForceSoftware);
                    break;
            }
        }
    }
    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            switch (hitInfo.collider.tag)
            {
                case "Ground":
                    OnMouseClicked?.Invoke(hitInfo.point);
                    break;
                case "Enemy":
                    OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
                    break;
                case "Attackable":
                    OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
                    break;
                case "Portal":
                    OnMouseClicked?.Invoke(hitInfo.point);
                    break;
            }
        }
    }
}
