using UnityEngine;

public class MobModel : MonoBehaviour
{
    [SerializeField] private GameObject model1;
    [SerializeField] private GameObject model2;

    public void ShowModel1()
    {
        model1.SetActive(true);
        model2.SetActive(false);
    }

    public void ShowModel2()
    {
        model1.SetActive(false);
        model2.SetActive(true);
    }
}
