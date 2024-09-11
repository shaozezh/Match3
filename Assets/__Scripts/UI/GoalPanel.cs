using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GoalPanel : MonoBehaviour
{
    public Image image;
    public Sprite sprite;
    public Text text;
    public string stringTag;
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    private void Setup()
    {
        image.sprite = sprite;
        text.text = stringTag;
    }
}
