using TMPro;
using UnityEngine;

public class EdgeSignScript : MonoBehaviour
{
    TextMeshPro _text2D;
    TextMeshPro _text3D;
    TextMeshPro text2D
    {
        get
        {
            if (!_text2D) { _text2D = transform.GetChild(0).GetComponentInChildren<TextMeshPro>(); }
            return _text2D;
        }
    }
    TextMeshPro text3D
    {
        get
        {
            if (!_text3D) { _text3D = transform.GetChild(1).GetComponentInChildren<TextMeshPro>(); }
            return _text3D;
        }
    }
    string newText;
    bool textChanged;
    private void Update()
    {
        bool topDown = Camera.main.transform.rotation.eulerAngles.x == 90;
        transform.GetChild(0).gameObject.SetActive(topDown);
        transform.GetChild(1).gameObject.SetActive(!topDown);
        if (textChanged)
        {
            text2D.text = text3D.text = newText;
            textChanged = false;
        }
    }
    public void SetValue(int n)
    {
        newText = n > 0 ? n.ToString() : "";
        textChanged = !_text3D || newText != text3D.text;
    }
}