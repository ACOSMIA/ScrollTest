using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scroll : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    [Serializable]
    struct ItemInfo
    {
        public string name;
        public Sprite sprite;
        public ItemInfo(string name,Sprite sprite){
            this.name=name;
            this.sprite=sprite;
        }
    }
    [Tooltip("选项预制体")]
    [SerializeField] private GameObject itemPrefab;
    [Tooltip("选项父物体")]
    [SerializeField] private RectTransform itemParent;
    [Tooltip("描述文字")]
    [SerializeField] private Text descriptionText;
    [Tooltip("选项信息")]
    [SerializeField] private ItemInfo[] itemInfos;
    [Tooltip("显示数量")]
    [SerializeField] private int displayNum;
    [Tooltip("显示间隔")]
    [SerializeField] private int itemGap;
    [Tooltip("移动插值")]
    [SerializeField] private float moveSmooth;
    [Tooltip("拖动速度")]
    [SerializeField] private int dragSpeed;
    [Tooltip("缩放倍率")]
    [SerializeField] private float scaleMultiplying;
    [Tooltip("透明度倍率")]
    [SerializeField] private float alphaMultiplying;

    public event Action<int> SelectAction;

    private CellItem[] items;
    private float displayWidth;
    [SerializeField]
    private int offsetTimes;
    private bool isDrag;
    private int currentItemIndex;
    private float[] distances;
    private float selectItemX;
    private bool isSelectMove;
    private bool isSelected;

    

    

    private void Start() {
        Init();
        MoveItems(0);//移动列表到偏移为0
    }

    //初始化
    private void Init()
    {
        displayWidth=(displayNum-1)*itemGap;//计算显示的总宽度
        items=new CellItem[displayNum];
        //创建指定数量的元素，获取他们的CellItem组件
        for(int i=0;i<displayNum;i++){
            CellItem item=Instantiate(itemPrefab,itemParent).GetComponent<CellItem>();
            item.cellIndex=i;//创建的排序
            items[i]=item;//赋值给items数组
        }
        
    }

    private void Update() {
        if(!isDrag){
            Adsorption();
        }

        //这里根据拖拽移动的x量，是否大于一个元素的间隔，来判断是否需要移动列表
        int currenOffsetTimes=Mathf.FloorToInt(itemParent.localPosition.x/itemGap);
        // Debug.Log(itemParent.localPosition.x);
        if(currenOffsetTimes!=offsetTimes){
            //判断当前x的移动量是否等于原来的offsetTimes值，不等于就要移动元素
            offsetTimes=currenOffsetTimes;
            MoveItems(offsetTimes);//这里根据offsetTimes刷新元素显示，因为元素直接保持相同相对距离，所以不影响显示
        }
        ItemsControl();
    }

    //设置ItemInfo的信息
    public void SetItemInfo(string[] names,Sprite[] sprites){
        if(names.Length!=sprites.Length){
            Debug.Log("数据不完整");
            return;
        }

        //根据参数设置好一个ItemInfo数组
        itemInfos=new ItemInfo[names.Length];
        for(int i=0;i<itemInfos.Length;i++){
            itemInfos[i]=new ItemInfo(names[i],sprites[i]);
        }

        SelectAction=null;
        isSelected=false;

    }


    //移动列表 ，用于插入删除元素的更新和刚打开列表的初始化，是瞬间的
    /// <summary>
    /// 
    /// </summary>
    /// <param name="offsetTimes">偏移次数</param>
    private void MoveItems(int offsetTimes)
    {
        //把所有选项移动到正确位置
        for(int i=0;i<displayNum;i++){
            float x=itemGap*(i-offsetTimes)- displayWidth/2;//注意这
            items[i].rectTransform.localPosition= new Vector2(x,itemParent.localPosition.y);
        }

        int middle;
        //意义不明 貌似是计算出最前（中间）选项
        //我懂了，是根据偏移决定中间的选项最开始是谁，若没有偏移则最开始就是0
        if(offsetTimes>0){
            //但是这个算式在偏移为0的情况下只会等于长度
            middle = itemInfos.Length -offsetTimes%itemInfos.Length;
        }
        else{
            middle=-offsetTimes%itemInfos.Length;
        }

        int infoIndex=middle;//获取的信息序号
        //这里从中间开始遍历，给中间和右边的显示元素赋值，把itemInfos里的数据一个个给items组件设置显示
        //i代表显示的序号，从中间显示的开始往右边走；infoIndex表示获取的信息序号，若无偏移则从第0个开始获取，往上增加
        for(int i=Mathf.FloorToInt(displayNum/2f);i<displayNum;i++){
            if(infoIndex>=itemInfos.Length){
                infoIndex=0;//显示元素的超过数据总数，显示为0
            }
            items[i].SetInfo(itemInfos[infoIndex].sprite,itemInfos[infoIndex].name,infoIndex,this);
            infoIndex++;
        }
        //从中间的左边一个开始反向赋值，显示序号从右往左赋值，信息序号从最大赋值到中间
        //infoIndex从偏移的小一个开始，若无偏移则从最大元素开始
        infoIndex=middle-1;
        for(int i=Mathf.FloorToInt(displayNum/2f)-1;i>=0;i--){
            if(infoIndex<=-1){
                //若小于0，则变到最大的序号
                infoIndex=itemInfos.Length-1;
            }
            items[i].SetInfo(itemInfos[infoIndex].sprite,itemInfos[infoIndex].name,infoIndex,this);
            infoIndex--;
        }
    }

    public void Select(int itemIndex,int infoIndex, RectTransform recTransform) 
    { 
        //这个isSelected意义不明
        if(!isSelected&&itemIndex==currentItemIndex){
            //若选择的等于最前显示的
            SelectAction?.Invoke(infoIndex);
            isSelected=true;
            Debug.Log("Select"+(infoIndex+1));
        }
        else{
            //选择的不是最前显示的
            //移动选项，这里是赋值给selectItemX直接移动x轴移动量
            isSelectMove=true;
            selectItemX = recTransform.localPosition.x;
            
        }

    }

    public void Adsorption(){
        float targetX=itemParent.localPosition.x;
        if(!isSelectMove){
            float distance=itemParent.localPosition.x%itemGap;//除余，拿多出来的部分
            int times = Mathf.FloorToInt(itemParent.localPosition.x/itemGap);//整除，算当前在第几个
            if(distance>0){
                if(distance<itemGap/2){
                    //除余值小于间隔的一半，吸附到前面去
                    targetX=times*itemGap;
                }
                else{
                    targetX=(times+1)*itemGap;
                }
            }
            else if(distance<0){
                //移动为负数的时候
                if(distance<-itemGap/2){
                    targetX=times*itemGap;
                }
                else{
                    targetX=(times+1)*itemGap;
                }
            }
        }
        else{
            targetX=-selectItemX;//赋值，为什么是负数？
           
        }
        

        //代替lerp函数，阶梯式达到目标x
        if(itemParent.localPosition.x>targetX+10){
            itemParent.localPosition=new Vector2(itemParent.localPosition.x-moveSmooth,itemParent.localPosition.y);
        }
        else if(itemParent.localPosition.x<targetX-10){
            itemParent.localPosition=new Vector2(itemParent.localPosition.x+moveSmooth,itemParent.localPosition.y);
        }
        else if(itemParent.localPosition.x!=targetX){
            itemParent.localPosition=new Vector2(targetX,itemParent.localPosition.y);
        }
    }
    
    //控制每个选项的透明度和缩放，获取中间的选项
    //update中调用
    private void ItemsControl(){
        distances = new float[displayNum];
        for (int i = 0; i< displayNum; i++){
            //获得每个元素和中心的距离
            float distance = Mathf.Abs(items[i].rectTransform. position.x - transform.position.x);
            distances[i] = distance;
            float scale = 1 - distance * scaleMultiplying;//距离越远大小越小
            items[i].rectTransform.localScale = new Vector3(scale,scale,1);//设置大小
            items[i].SetAlpha(1 - distance * alphaMultiplying);//距离越远alpha越低
        }

        float minDistance = itemGap*displayNum;
        int minIndex=0;
        //找出距中间最小距离的那个元素
        for (int i =0; i < displayNum; i++){
            if (distances[i]< minDistance){
                minDistance = distances[i];
                minIndex = i;
            }
        }
        //获取最前元素信息
        descriptionText.text = items [minIndex].infoIndex.ToString();
        currentItemIndex = items[minIndex].cellIndex;



    }

    public void OnDrag(PointerEventData eventData)
    {
        isSelectMove=false;
        itemParent.localPosition=new Vector2(itemParent.localPosition.x+eventData.delta.x*dragSpeed,
                                        itemParent.localPosition.y);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag=true;
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrag=false;
    }

    
}
