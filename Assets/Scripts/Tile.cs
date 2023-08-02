using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Tile : MonoBehaviour
{
    private TextMeshProUGUI textNumeric;

    private int index;
    public int Index
    {
        set => index = value;
        get => index;
    }

    private int numeric;
    public int Numeric
    {
        set
        {
            numeric = value;
            textNumeric.text = numeric.ToString();
        }
        get => numeric;
    }

    private int level;
    public int Level
    {
        set
        {
            level = value;
            Numeric = (1 << level);
            if(level == 0)
            {
                this.gameObject.SetActive(false);
            }
        }
        get => level;
    }

    private bool isActive;
    public bool IsActive
    {
        set
        {
            isActive = value;
            this.gameObject.SetActive(isActive);
        }
        get => isActive;
    }

    public void Setup()
    {
        textNumeric = GetComponentInChildren<TextMeshProUGUI>();
        Level = 0;
        IsActive = false;
    }

    public event Action OnMoveToComplete;

    public void OnMoveTo(Vector3 end)
    {
        StartCoroutine("MoveTo", end);
    }

    private IEnumerator MoveTo(Vector3 end)
    {
        float current = 0;
        float percent = 0;
        float moveTime = 0.2f;
        Vector3 start = GetComponent<RectTransform>().localPosition;

        while(percent < 1.0f)
        {
            current += Time.deltaTime;
            percent = current / moveTime;

            GetComponent<RectTransform>().localPosition = Vector3.Lerp(start, end, percent);

            yield return null;
        }

        OnMoveToComplete.Invoke();
    }

    public bool IsEmpty()
    {
        return Level == 0;
    }
}
