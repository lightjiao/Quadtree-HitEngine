using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float Length = 100;

    private void Start()
    {
        Camera.main.orthographicSize = Length / 2;
    }
}