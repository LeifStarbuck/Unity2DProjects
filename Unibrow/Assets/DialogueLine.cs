using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Color speakerColor = Color.white;

    [TextArea(2, 6)]
    public string text;
}