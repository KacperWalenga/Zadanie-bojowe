using System;
using UnityEngine;
using UnityEngine.UI;

public class MobModel : MonoBehaviour
{
    [SerializeField] private Image model1;
    [SerializeField] private Image model2;

    private void Awake()
    {
        model1.enabled = false;
        model2.enabled = false;
    }

    public void ShowModel1()
    {
        model1.enabled = true;
        model2.enabled = false;
    }

    public void ShowModel2()
    {
        model1.enabled = false;
        model2.enabled = true;
    }

    public void SetColor(Color color)
    {
        model1.color = color;
        model2.color = color;
    }
}
