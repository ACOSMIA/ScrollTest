using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas : MonoBehaviour
{
    public Scroll scroll;
    private void Awake() {
        if(scroll!=null){
            int num=100;
            string[] names=new string[num];
            Sprite[] sprites=new Sprite[num];
            for(int i=0;i<num;i++){
                names[i]=(i).ToString();
                sprites[i]=null;
            }
            scroll.SetItemInfo(names,sprites);
            scroll.SelectAction+=index=>{
                print(index);
            };
        }
        
    }
}
