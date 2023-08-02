using System.Collections;
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
        }
        get => level;
    }

    public void Setup()
    {
        textNumeric = GetComponentInChildren<TextMeshProUGUI>();
        Level = 0;
    }

    public void OnMoveTo(Vector3 end)
    {
        StartCoroutine("MoveTo", end);
    }

    private IEnumerator MoveTo(Vector3 end)
    {
        float current = 0;
        float percent = 0;
        float moveTime = 0.5f;
        Vector3 start = GetComponent<RectTransform>().localPosition;

        while(percent < 1.0f)
        {
            current += Time.deltaTime;
            percent = current / moveTime;

            GetComponent<RectTransform>().localPosition = Vector3.Lerp(start, end, percent);

            yield return null;
        }
    }

    public bool IsEmpty()
    {
        return Level == 0;
    }
}
