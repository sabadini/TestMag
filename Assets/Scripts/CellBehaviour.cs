using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviour : MonoBehaviour {

    public enum cellType { box, sphere, cylinder, capsule}

    public cellType type;

    public Vector2Int currentCoord;
    private Vector2Int lastCoord = new Vector2Int(-1,-1);

    private Ray ray;

    private Camera inputCamera;
    public Collider targetCollider;

    public float spawnAnimationTime;
    public float moveAnimationTime;

    private Vector3 cellSize;

    private void Awake(){
        inputCamera = Camera.main;

        this.transform.localScale = Vector3.zero;
    }

    void Start () {
        Hashtable hash = iTween.Hash(
            "scale", cellSize,
            "islocal", true,
            "time", spawnAnimationTime,
            "easeType", iTween.EaseType.easeInOutCubic
        );
        iTween.ScaleTo(this.gameObject, hash);
    }
	
	void Update (){
        //Check input
        if(Input.GetMouseButtonDown(0)){
            ray = inputCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (targetCollider.Raycast(ray, out hit, 1000))
            {
                ClickOnThisObject();
            }
        }

    }

    private void ClickOnThisObject(){
        #if UNITY_EDITOR
            Debug.Log("Clicked on " + type + " on Position: x=" + currentCoord.x + "  y=" + currentCoord.y);
        #endif
        GameController.instance.TryToDestroyObjOnClick(currentCoord);   
    }

    public void SetPosition(Vector2Int intPos, Vector3 pos){
        if(lastCoord.x >= 0){

            lastCoord = currentCoord;
            currentCoord = intPos;

            Hashtable hash = iTween.Hash(
                "position", pos,
                "islocal", true,
                "time", moveAnimationTime,
                "easeType", iTween.EaseType.spring
            );
            iTween.MoveTo(this.gameObject, hash);
        }
        else{
            currentCoord = intPos;
            lastCoord = currentCoord;
            this.transform.localPosition = pos;
        }
    }

    public void SetCellSize(Vector3 size){
        cellSize = size;
    }
}