using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogTester : MonoBehaviour
{
    public GrafItem dialog;
    public GameObject textBox;
    public Text text;
    public Button[] antworteMöglichkeiten;

    public void Start()
    {
        foreach(var button in antworteMöglichkeiten)
        {
            button.onClick.AddListener(() => Antworten(button.GetComponent<ButtonData>().buttonGuid));
        }
    }
    public void StartDialog()
    {
        textBox.SetActive(true);
        dialog.StardDialog();
        text.text = dialog.getCurrendNode().DE_DialogueText;
        var antworten = dialog.getValidAnswers();
        for (int i = 0; i < antworten.Count; i++)
        {
            antworteMöglichkeiten[i].gameObject.SetActive(true);
            antworteMöglichkeiten[i].GetComponentInChildren<Text>().text = antworten.ElementAt(i).DE_Text;
            antworteMöglichkeiten[i].GetComponent<ButtonData>().buttonGuid = antworten.ElementAt(i);
        }
    }
    public void Antworten(NodeLinkData nodeLink)
    {
        Debug.Log("Click");
        antworteMöglichkeiten.ToList().ForEach(e => {
            e.gameObject.SetActive(false);
        });
        dialog.GoPath(nodeLink);
        text.text = dialog.getCurrendNode().DE_DialogueText;
        var antworten = dialog.getValidAnswers();
        for (int i = 0; i < antworten.Count; i++)
        {
            antworteMöglichkeiten[i].gameObject.SetActive(true);
            antworteMöglichkeiten[i].GetComponentInChildren<Text>().text = antworten.ElementAt(i).DE_Text;
            antworteMöglichkeiten[i].GetComponent<ButtonData>().buttonGuid = antworten.ElementAt(i);
        }
    }
}

