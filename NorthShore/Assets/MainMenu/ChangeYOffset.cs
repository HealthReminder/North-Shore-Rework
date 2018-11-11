﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeYOffset : MonoBehaviour {
// Scroll main texture based on time

     public float scrollSpeed = 0.5f;
     Image rend;

    void Start()
    {
        rend = GetComponent<Image>();
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        rend.material.mainTextureOffset = new Vector2(0, offset);
    }
}