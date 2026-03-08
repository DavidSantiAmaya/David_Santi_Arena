using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    // -------------------------------
    // CONFIGURACIÓN DE LA GRILLA
    // -------------------------------
    [Header("Grid Settings")]

    public int width = 50;   // ancho de la grilla
    public int height = 30;  // alto de la grilla

    public float updateTime = 0.1f; // velocidad de actualización automática

    public float cellspawn = 0.95f; // probabilidad inicial de aparición de celdas

    public GameObject cellPrefab; // prefab visual de cada celda

    public TMP_Text text; // texto UI donde mostramos la generación


    // -------------------------------
    // GENERADOR DE ARENA
    // -------------------------------
    [Header("Sand Generator")]

    public bool sandGenerator = true; // activar o desactivar generador

    [Range(0f, 1f)]
    public float spawnChanceTop = 0.1f; // probabilidad de crear arena en la fila superior



    // -------------------------------
    // VARIABLES INTERNAS
    // -------------------------------

    private bool[,] grid;        // estado actual de las celdas
    private bool[,] nextGrid;    // estado de la siguiente generación

    private GameObject[,] cellObjects; // objetos visuales de cada celda

    private float timer; // temporizador para generación automática

    private bool isPaused = false; // estado de pausa

    private int generationCount = 0; // contador de generaciones



    // -------------------------------
    // START
    // Se ejecuta al iniciar la escena
    // -------------------------------
    void Start()
    {
        // crear arrays de la grilla
        grid = new bool[width, height];
        nextGrid = new bool[width, height];
        cellObjects = new GameObject[width, height];

        // conectar eventos del InputManager
        InputManager.Instance.OnPause += TogglePause;
        InputManager.Instance.OnRestart += RestartSimulation;
        InputManager.Instance.OnClear += ClearSimulation;
        InputManager.Instance.OnToggleCell += ToggleCellInput;
        InputManager.Instance.onNext += NextVGrid;

        // crear celdas visuales
        GenerateGrid();

        // generar estado inicial aleatorio
        RandomizeGrid();

        generationCount = 0;
        UpdateGenerationText();
    }



    // -------------------------------
    // UPDATE
    // se ejecuta cada frame
    // -------------------------------
    void Update()
    {
        if (isPaused) return;

        // SI SE MANTIENE PRESIONADA LA TECLA L
        if (Keyboard.current.lKey.isPressed)
        {
            timer += Time.deltaTime;

            if (timer >= updateTime)
            {
                Step();           // calcular nueva generación
                UpdateVisuals();  // actualizar visual
                timer = 0f;       // reiniciar temporizador
            }
        }
    }



    // -------------------------------
    // AVANZAR UNA GENERACIÓN MANUAL
    // -------------------------------
    void NextVGrid()
    {
        Step();
        UpdateVisuals();
        timer = 0f;
    }



    // -------------------------------
    // PAUSAR / REANUDAR
    // -------------------------------
    void TogglePause()
    {
        isPaused = !isPaused;

        Debug.Log(isPaused ?
            "Simulación pausada" :
            "Simulación reanudada");
    }



    // -------------------------------
    // ACTIVAR / DESACTIVAR CELDA
    // -------------------------------
    void ToggleCellInput()
    {
        // si hay mouse disponible
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            HandleMouseClick();
            return;
        }

        // usar centro de cámara
        Vector3 camPos = Camera.main.transform.position;

        int x = Mathf.RoundToInt(camPos.x);
        int y = Mathf.RoundToInt(camPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];

        UpdateVisuals();
    }



    // -------------------------------
    // LIMPIAR SIMULACIÓN
    // -------------------------------
    void ClearSimulation()
    {
        Debug.Log("Limpiando simulación...");

        ClearGrid();

        timer = 0f;

        generationCount = 0;

        UpdateGenerationText();
    }



    // -------------------------------
    // REINICIAR SIMULACIÓN
    // -------------------------------
    void RestartSimulation()
    {
        Debug.Log("Reiniciando simulación...");

        RandomizeGrid();

        timer = 0f;

        generationCount = 0;

        UpdateGenerationText();
    }



    // -------------------------------
    // CREAR OBJETOS VISUALES
    // -------------------------------
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(
                    cellPrefab,
                    new Vector3(x, y, 0),
                    Quaternion.identity
                );

                cell.transform.parent = transform;

                cellObjects[x, y] = cell;
            }
        }
    }



    // -------------------------------
    // BORRAR TODA LA GRILLA
    // -------------------------------
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



    // -------------------------------
    // GENERAR ESTADO INICIAL ALEATORIO
    // -------------------------------
    void RandomizeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Random.value > cellspawn;
            }
        }

        UpdateVisuals();
    }



    // -------------------------------
    // CALCULAR SIGUIENTE GENERACIÓN
    // (SIMULACIÓN DE ARENA)
    // -------------------------------
    void Step()
    {

        // limpiar grilla siguiente
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nextGrid[x, y] = false;
            }
        }


        // recorrer grilla
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                if (!grid[x, y]) continue;


                // bajar
                if (y > 0 && !grid[x, y - 1])
                {
                    nextGrid[x, y - 1] = true;
                }
                else
                {

                    bool moved = false;

                    // abajo derecha
                    if (x < width - 1 && y > 0 && !grid[x + 1, y - 1])
                    {
                        nextGrid[x + 1, y - 1] = true;
                        moved = true;
                    }

                    // abajo izquierda
                    else if (x > 0 && y > 0 && !grid[x - 1, y - 1])
                    {
                        nextGrid[x - 1, y - 1] = true;
                        moved = true;
                    }

                    if (!moved)
                    {
                        nextGrid[x, y] = true;
                    }
                }
            }
        }


        // intercambiar grids
        var temp = grid;
        grid = nextGrid;
        nextGrid = temp;


        // generar arena arriba
        GenerateTopRow();


        // aumentar generación
        generationCount++;

        UpdateGenerationText();
    }



    // -------------------------------
    // GENERADOR DE ARENA
    // FILA SUPERIOR
    // -------------------------------
    void GenerateTopRow()
    {
        if (!sandGenerator) return;

        int topRow = height - 1;

        for (int x = 0; x < width; x++)
        {
            if (!grid[x, topRow])
            {
                if (Random.value < spawnChanceTop)
                {
                    grid[x, topRow] = true;
                }
            }
        }
    }



    // -------------------------------
    // ACTIVAR CELDA CON MOUSE
    // -------------------------------
    void HandleMouseClick()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];

        UpdateVisuals();
    }



    // -------------------------------
    // ACTUALIZAR COLORES VISUALES
    // -------------------------------
    void UpdateVisuals()
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



    // -------------------------------
    // ACTUALIZAR TEXTO DE GENERACIÓN
    // -------------------------------
    void UpdateGenerationText()
    {
        if (text == null)
        {
            Debug.LogWarning("TMP_Text no asignado.");
            return;
        }

        text.text = "Generación: " + generationCount.ToString();
    }

}
