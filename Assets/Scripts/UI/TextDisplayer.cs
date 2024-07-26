using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextDisplayer : MonoBehaviour
{
    #region Variables
    [SerializeField] private TMP_Text textCanvas;
    [SerializeField] private List<string> textList = new List<string>();
    [SerializeField] private string text = "";
    [SerializeField] private int currentIndex = 0;

    public List<string> TextList 
    { 
        get { return textList; } 
        set 
        { 
            textList = value; 
            if (textCanvas != null && Index < textList.Count)
            {
                UpdateTextUI();
            }
        }
    }
    public int Index
    {
        get { return currentIndex; }
        set 
        {  
            currentIndex += value; 
            text = textList[currentIndex];
            if (textCanvas != null && currentIndex < textList.Count)
            {
                UpdateTextUI();
            }
        }
    }
    #endregion

    #region Public Functions
    public void NextText(int increaseBy)
    {
        Index = increaseBy;
    }

    public void UpdateTextList(string text)
    {
        textList.Add(text);
        UpdateTextUI();
    }

    public void UpdateTextUI(string newText = "")
    {
        if (newText == "")
        {
            textCanvas.text = text;
        }
        else
        {
            textCanvas.text = newText;
        }
    }

    public void ShowDone()
    {
        textCanvas.color = Color.green;
    }
    #endregion
}
