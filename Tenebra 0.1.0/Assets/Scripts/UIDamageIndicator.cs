using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDamageIndicator : MonoBehaviour
{
    public TMP_Text damageText;

    public float moveSpeed;

    public float lifetime = 3f;

    private RectTransform myRect;

    public bool isPlayerText;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);

        myRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerText)
        {
            myRect.anchoredPosition += new Vector2(0f, -moveSpeed * Time.deltaTime);
        }
        else
        {
            myRect.anchoredPosition += new Vector2(0f, moveSpeed * Time.deltaTime);
        }
        
    }
}
