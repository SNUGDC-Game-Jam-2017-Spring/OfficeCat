using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WarningAnim : MonoBehaviour {

    public Sprite warning;
    public int warningNum;
    int currentWarningCount;

    private void Update()
    {
        if(Level.WarningCount != currentWarningCount)
        {
            if(Level.WarningCount == warningNum)
            {
                WarningAnimation();
            }
        }
        currentWarningCount = Level.WarningCount;
    }

    void WarningAnimation()
    {
        GetComponent<Image>().sprite = warning;
        GetComponent<RectTransform>().DOPunchScale(Vector3.one * 1.5f, 0.4f, 5, 1);
    }
}
