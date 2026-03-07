using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 50; //ancho
    public int height = 30; //Alto
    public float updateTime = 0.1f; //veolocidad de tiempo 
    public float cellspawn = 0.95f;
    public GameObject cellPrefab; // es un prefabricodo es lo de clonacion 
    public TMP_Text text; 

    private bool[,] grid;  // es un aray de dos dimenciones de la posicion inicion y lo de generacion  si esta en pocion 
    private bool[,] nextGrid; // es un aray es a siguiente refilla si est aviva o muerta 
    private GameObject[,] cellObjects; // visul del codigo 
    private float timer; 
    private bool isPaused = false;


    void Start() // es inicio del onjeto 
    {
        grid = new bool[width, height]; // pocion 
        nextGrid = new bool[width, height]; // nueva pision o nueva celuda 
        cellObjects = new GameObject[width, height]; // visual

        InputManager.Instance.OnPause += TogglePause; // llamado del boton y la cion del boton 
        InputManager.Instance.OnRestart += RestartSimulation;
        InputManager.Instance.OnClear += ClearSimulation;
        InputManager.Instance.OnToggleCell += ToggleCellInput;
        InputManager.Instance.onNext += NextVGrid;

        GenerateGrid(); // gegerar las celdas 
        RandomizeGrid(); // aletorio de celulas 
    }

    void Update() //actualizacion del codigo
    {
        if (isPaused) return; // preguntasn si el pausado 
        
        //timer += Time.deltaTime;// bucle de la simulador 
        //if (timer >= updateTime) 
        //{
            //Step(); // son los lasos y las negaracion de la celula 
            //UpdateVisuals(); //apartado visual 
            //timer = 0f;
        //}
    }


    void NextVGrid(){
        Step(); // son los lasos y las negaracion de la celula 
        UpdateVisuals(); //apartado visual 
        timer = 0f;
    }

    void TogglePause()
    {
        isPaused = !isPaused; // 
        Debug.Log(isPaused ? "Simulación pausada" : "Simulación reanudada"); // if en una sola linea 
    }

    void ToggleCellInput()
    {
        // Si hay mouse disponible (PC), usar clic real
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            HandleMouseClick();
            return;
        }

        // Si no hay mouse, usar el centro de la cámara
        Vector3 camPos = Camera.main.transform.position;
        int x = Mathf.RoundToInt(camPos.x);
        int y = Mathf.RoundToInt(camPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];
        UpdateVisuals();
    }


    void ClearSimulation()// llaman otros eventos 
    {
        Debug.Log("Limpiando simulación...");
        ClearGrid();
        timer = 0f;
    }

    void RestartSimulation()// llaman a otros eventos
    {
        Debug.Log("Reiniciando simulación...");
        RandomizeGrid();
        timer = 0f;
    }

    void GenerateGrid() // genera las celdas
    {
        for (int x = 0; x < width; x++) // llena por columnas 
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity); // crear un nuevo onjero de un pfefed  Quaternion.identity es decir rotaciones en 0 este no se bloqueo de guimbal
                cell.transform.parent = transform; // que el padrea va cer igaul a la celuda que todo va salir den el punto de git managet
                cellObjects[x, y] = cell; // llenar el aray vicual de las celulas 
            }
        }
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = false;
            }
        }
        UpdateVisuals();
    }

    void RandomizeGrid() // 
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Random.value > cellspawn; // cada cedula tiene un 5 parcierto de aparecer una parcar en una celda es el manejo de posibilidades 
            }
        }
        UpdateVisuals();
    }

    void Step() // cada paso recorer toda la grilla y las reglas
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int aliveNeighbors = CountAliveNeighbors(x, y); // cuente cunatos vecinoas que esten vivos
                bool alive = grid[x, y]; // tomas la picoon donde est 

                if (alive && (aliveNeighbors < 2 || aliveNeighbors > 3)) // condiciones de muerte 
                    nextGrid[x, y] = false; // Muere
                else if (!alive && aliveNeighbors == 3) 
                    nextGrid[x, y] = true;  // Nace
                else
                    nextGrid[x, y] = alive; // Se mantiene
            }
        }

        // Swap grids
        var temp = grid; 
        grid = nextGrid;
        nextGrid = temp;
    }

    int CountAliveNeighbors(int x, int y)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++) // limitador de vecions en la malla mas pqueña
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (grid[nx, ny]) count++;
                }
            }
        }

        return count;
    }

    void HandleMouseClick()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];
        UpdateVisuals();
    }



    void UpdateVisuals() // apartado visual de la celula 
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var rend = cellObjects[x, y].GetComponent<SpriteRenderer>();
                rend.color = grid[x, y] ? Color.black : Color.white;
            }
        }

    }
}
