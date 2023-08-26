using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CellItem : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    public Image image;
    public Text nameText;
    public CanvasGroup canvasGroup;

    public int cellIndex;
    public int infoIndex;

    public Scroll scroll;
    public RectTransform rectTransform;

    public bool isDrag=false;

    public void SetInfo(Sprite sprite , string name ,int infoIndex, 
        Scroll scroll)
    {
        image.sprite=sprite;
        nameText.text=name;
        this.infoIndex=infoIndex;
        this.scroll=scroll;
    }

    public void SetAlpha(float alpha){
        canvasGroup.alpha=alpha;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDrag=true;//若拖动则执行拖动方法
        scroll.OnDrag(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag=false;//按下鼠标时先判断为不在拖拽，方便点击切换
        scroll.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!isDrag){
            //松开也没有拖动，则点击切换到当前元素
            scroll.Select(cellIndex,infoIndex,rectTransform);
        }
        scroll.OnPointerUp(eventData);
    }
}
