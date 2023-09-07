using UnityEngine;
using TMPro;

public class BlockScript : MonoBehaviour
{
    Transform block;
    TextMeshPro heightLabel;
    string newText;
    bool textChanged;
    float height;
    float targetHeight;
    Color color;
    Color targetColor;
    float lerpVal;

    void Awake()
    {
        block = transform.GetChild(0);
        heightLabel = transform.GetChild(1).GetComponent<TextMeshPro>();
        SetHeight(0.1f);
        heightLabel.text = "";
        color = new Color(0.25f, 0.25f, 0.25f, 1);
        block.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        lerpVal = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (lerpVal < 1)
        {
            lerpVal = Mathf.Lerp(lerpVal, 1, 0.02f);
            if (lerpVal > 0.99f) { lerpVal = 1; }
            SetHeight(Mathf.Lerp(height, targetHeight, lerpVal));
            color = Color.Lerp(color, targetColor, lerpVal);
            block.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        }
        if (textChanged)
        {
            heightLabel.text = newText;
            textChanged = false;
        }
    }
    public void SetTargetHeight(int _targetHeight, int _gridSize)
    {
        if (_targetHeight > 0)
        {
            targetHeight = _targetHeight;
            newText = targetHeight.ToString();
            targetColor = GetColor(_targetHeight, _gridSize);
        }
        else
        {
            targetHeight = 0.1f;
            newText = "";
            targetColor = new Color(0.25f, 0.25f, 0.25f, 1);
        }
        textChanged = newText != heightLabel.text;
        lerpVal = 0;
    }
    void SetHeight(float _height)
    {
        height = _height;
        block.localScale = new Vector3(0.75f, height, 0.75f);
        block.localPosition = Vector3.up * height * 0.5f;
        heightLabel.transform.localPosition = Vector3.up * (0.5f + height);
    }
    Color GetColor(int _targetHeight, int _gridSize)
    {
        switch (_gridSize)
        {
            case 1: return Color.HSVToRGB(Random.value, Random.Range(0.75f, 1), 1);
            case 2: return Color.HSVToRGB(new float[2] { 345, 225 }[_targetHeight - 1] / 360f, 1, 1);
            case 3: return Color.HSVToRGB(new float[3] { 315, 45, 210 }[_targetHeight - 1] / 360f, 0.9f, 1);
            case 4: return Color.HSVToRGB(new float[4] { 345, 45, 105, 225 }[_targetHeight - 1] / 360f, 1, 1);
            case 5: return Color.HSVToRGB(new float[5] { 345, 45, 105, 225, 270 }[_targetHeight - 1] / 360f, _targetHeight == 5 ? 0.9f : 1, 1);
            case 6: return Color.HSVToRGB(new float[6] { 345, 45, 105, 225, 270, 315 }[_targetHeight - 1] / 360f, _targetHeight > 4 ? 0.9f : 1, 1);
            case 7: return Color.HSVToRGB(new float[7] { 345, 25, 45, 105, 225, 270, 315 }[_targetHeight - 1] / 360f, _targetHeight > 5 ? 0.9f : 1, 1);
            case 8: return Color.HSVToRGB(new float[8] { 345, 25, 45, 105, 195, 225, 270, 315 }[_targetHeight - 1] / 360f, _targetHeight > 6 ? 0.9f : 1, 1);
            case 9: return Color.HSVToRGB(new float[9] { 345, 25, 45, 105, 140, 195, 225, 270, 315 }[_targetHeight - 1] / 360f, _targetHeight > 7 ? 0.9f : 1, 1);
            default: return Color.white;
        }
    }
}