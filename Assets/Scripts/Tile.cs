using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    private TextMeshProUGUI textNumeric;

    private int numeric;
    public int Numeric
    {
        set
        {
            numeric = value;
            textNumeric.text = numeric.ToString();
            IsActive = (numeric > 0 ? true : false);
        }
        get => numeric;
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

    public void Setup(int num)
    {
        textNumeric = GetComponentInChildren<TextMeshProUGUI>();
        Numeric = num;
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
        float moveTime = 0.15f;
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
        return Numeric == 0;
    }
}
