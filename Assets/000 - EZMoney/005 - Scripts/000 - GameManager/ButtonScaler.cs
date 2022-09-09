using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScaler : MonoBehaviour
{
    public void Start()
    {
        
    }
    public void PushButtonDown()
    {
        gameObject.transform.localScale = new Vector2(0.95f, 0.95f);
    }

    public void PushButtonUp()
    {
        gameObject.transform.localScale = new Vector2(1f, 1f);
    }
}
