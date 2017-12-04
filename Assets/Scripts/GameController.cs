using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public enum gameStates {initial, setup, inGame, endGame}

    private gameStates nextState, currentState;

    public CellBehaviour[] cellsPrefabs;

    private CellBehaviour[][] cellsInGame;

    public int numberOfRow;
    public int numberOfLines;

    private Vector3[][] cellsPositions;

    public bool autoFit;
    public GameObject backgroundObject;
    public GameObject cellsParent;

    private Vector3 cellSize;

    public float gameTime;
    private float gameTimeAccumulator;

    public float timeToStayInThisSceneAfterEndGame;
    private float timeToStayInThisSceneAfterEndGameAccumulator;

    public GameObject pointInMotionPrefab;
    public Transform pointsInMotionParent;
    private int points;
    public Text pointsIndicator;
    public Image timeIndicator;

    public float pointAnimationTimeMin;
    public float pointAnimationTimeMax;

    public string menuSceneName;

    void Awake(){
        instance = this;
    }

    void Start () {
        currentState = gameStates.initial;
        nextState = gameStates.setup;
    }
	
	void Update () {
        startState();
        updateState();
    }

    //Start of every state
    private void startState(){
        if (currentState == nextState) { return; }

        currentState = nextState;

        switch (nextState){

            case gameStates.setup:
                pointsIndicator.text = 0.ToString();
                FillPositionMatrix();
                PopulateFullTable();
                gameTimeAccumulator = Time.time;
                timeIndicator.fillAmount = 1f;
            break;

            case gameStates.inGame:
                
            break;

            case gameStates.endGame:
                timeToStayInThisSceneAfterEndGameAccumulator = Time.time;

                if(PlayerPrefs.GetInt("scoreRecord") < points){
                    PlayerPrefs.SetInt("scoreRecord", points);
                }
            break;
        }
    }

    //Update of every state
    private void updateState(){
        if (currentState != nextState) { return; }

        switch (currentState){
            case gameStates.setup:
                movetoNextState(gameStates.inGame);
            break;

            case gameStates.inGame:
                timeIndicator.fillAmount = 1f - (Time.time - gameTimeAccumulator) / gameTime;

                if (Time.time - gameTimeAccumulator > gameTime){
                    movetoNextState(gameStates.endGame);
                }
            break;

            case gameStates.endGame:
                if (Time.time - timeToStayInThisSceneAfterEndGameAccumulator > timeToStayInThisSceneAfterEndGame){
                    SceneManager.LoadScene(menuSceneName);
                }
            break;
        }
    }

    public void movetoNextState(gameStates st){
        if (st != currentState){
            nextState = st;
        }
    }

    private void FillPositionMatrix(){

        cellsPositions = new Vector3[numberOfRow][];
        cellsInGame = new CellBehaviour[numberOfRow][];
        for (int i = 0; i < numberOfRow; i++){
            cellsPositions[i] = new Vector3[numberOfLines];
            cellsInGame[i] = new CellBehaviour[numberOfLines];
        }

        if (autoFit){
            float cellSizeX = backgroundObject.transform.localScale.x / numberOfRow;
            float cellSizeY = backgroundObject.transform.localScale.y / numberOfLines;
            cellSize = new Vector3(cellSizeX, cellSizeY, 1f);

            for (int x = 0; x < numberOfRow; x++){
                for (int y = 0; y < numberOfLines; y++){
                    cellsPositions[x][y] = new Vector3((cellSize.x / 2f) + (x * cellSize.x), (cellSize.y / 2f) + (y * cellSize.y), 0f);
                }
            }
        }
        else{
            //In Not autoFit Mode, all cells prefabs need to have the same scale!
            cellSize = new Vector3(cellsPrefabs[0].transform.localScale.x, cellsPrefabs[0].transform.localScale.y, 1f);

            for (int x = 0; x < numberOfRow; x++){
                for (int y = 0; y < numberOfLines; y++){
                    cellsPositions[x][y] = new Vector3((cellsPrefabs[0].transform.localScale.x / 2f) + (x * cellsPrefabs[0].transform.localScale.x), (cellsPrefabs[0].transform.localScale.y / 2f) + (y * cellsPrefabs[0].transform.localScale.y), 0f);
                }
            }
        }
    }

    private void PopulateFullTable(){
        for (int x = 0; x < numberOfRow; x++){
            for (int y = 0; y < numberOfLines; y++){
                InstantiateNewCell(new Vector2Int(x, y));
            }
        }
    }

    private void InstantiateNewCell(Vector2Int pos){
        cellsInGame[pos.x][pos.y] = (CellBehaviour)(Instantiate(cellsPrefabs[Random.Range(0, cellsPrefabs.Length)]));
        cellsInGame[pos.x][pos.y].transform.SetParent(cellsParent.transform);
        cellsInGame[pos.x][pos.y].SetCellSize(cellSize);
        cellsInGame[pos.x][pos.y].SetPosition(pos, cellsPositions[pos.x][pos.y]);
    }

    public void TryToDestroyObjOnClick(Vector2Int coordsOfClick){
        if(currentState != gameStates.inGame){
            return;
        }

        int checkPositionIndex = 1;
        List<CellBehaviour> objectsInHorizontalRow = new List<CellBehaviour>();
        objectsInHorizontalRow.Add(cellsInGame[coordsOfClick.x][coordsOfClick.y]);

        while (coordsOfClick.x + checkPositionIndex < numberOfRow && cellsInGame[coordsOfClick.x + checkPositionIndex][coordsOfClick.y].type == cellsInGame[coordsOfClick.x][coordsOfClick.y].type){
            objectsInHorizontalRow.Add(cellsInGame[coordsOfClick.x + checkPositionIndex][coordsOfClick.y]);
            checkPositionIndex++;
        }
        checkPositionIndex = 1;
        while (coordsOfClick.x - checkPositionIndex >= 0 && cellsInGame[coordsOfClick.x - checkPositionIndex][coordsOfClick.y].type == cellsInGame[coordsOfClick.x][coordsOfClick.y].type){
            objectsInHorizontalRow.Add(cellsInGame[coordsOfClick.x - checkPositionIndex][coordsOfClick.y]);
            checkPositionIndex++;
        }

        int objectsInHorizontalRowCount = objectsInHorizontalRow.Count;

        if (objectsInHorizontalRowCount >= 3){

            for (int i = 0; i < objectsInHorizontalRowCount; i++){
                for (int j = objectsInHorizontalRow[i].currentCoord.y + 1; j < numberOfLines; j++){
                    cellsInGame[objectsInHorizontalRow[i].currentCoord.x][j].SetPosition(new Vector2Int(objectsInHorizontalRow[i].currentCoord.x, j - 1), cellsPositions[objectsInHorizontalRow[i].currentCoord.x][j - 1]);
                    cellsInGame[objectsInHorizontalRow[i].currentCoord.x][j - 1] = cellsInGame[objectsInHorizontalRow[i].currentCoord.x][j];
                }
                InstantiateNewCell(new Vector2Int(objectsInHorizontalRow[i].currentCoord.x, numberOfLines - 1));
            }

            for (int i = 0; i < objectsInHorizontalRowCount; i++){
                CreateMotionPointIndicator(objectsInHorizontalRow[i]);
                Destroy(objectsInHorizontalRow[i].gameObject); 
            }
        }
    }

    private void CreateMotionPointIndicator(CellBehaviour cell){
        GameObject point = Instantiate(pointInMotionPrefab);
        point.transform.SetParent(pointsInMotionParent);
        point.transform.localPosition = Vector3.zero;
        point.transform.position = new Vector3(cell.transform.position.x, cell.transform.position.y, point.transform.position.z);
        point.transform.localScale = Vector3.one;

        Hashtable hash = iTween.Hash(
            "position", pointsIndicator.transform.position,
            "islocal", false,
            "time", Random.Range(pointAnimationTimeMin, pointAnimationTimeMax),
            "easeType", iTween.EaseType.easeInOutQuad,
            "oncomplete", "OnPointAnimationOver",
            "oncompletetarget", this.gameObject,
            "oncompleteparams", point
        );
        iTween.MoveTo(point, hash);
    }

    public void OnPointAnimationOver(object obj)
    {
        Destroy((GameObject)(obj));
        points++;
        pointsIndicator.text = points.ToString();
    }
}