using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public OsechiBox targetBox;
    public OsechiBox[] sourceBoxes;
    public Text clearText;
    public Button retryButton;

    [Header("Stage Setup")]
    public OkazuData[] availableOkazu;
    public Transform okazuParent;

    private bool isCleared = false;
    private List<Okazu> allOkazu = new List<Okazu>();
    private int totalOkazuCount = 0;

    public bool IsCleared => isCleared;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (clearText != null)
            clearText.gameObject.SetActive(false);

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
            retryButton.onClick.AddListener(Retry);
        }

        SetupStage();
    }

    private void SetupStage()
    {
        allOkazu.Clear();
        var existingOkazu = FindObjectsByType<Okazu>(FindObjectsSortMode.None);
        allOkazu.AddRange(existingOkazu);
        totalOkazuCount = allOkazu.Count;

        Debug.Log($"Found {totalOkazuCount} okazu pieces");
    }

    public void SpawnOkazu(OkazuData data, OsechiBox box, int gridX, int gridY)
    {
        GameObject obj = new GameObject(data.okazuName);
        obj.transform.SetParent(okazuParent != null ? okazuParent : transform);

        var spriteRenderer = obj.AddComponent<SpriteRenderer>();
        var okazu = obj.AddComponent<Okazu>();
        var collider = obj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(data.Width, data.Height);

        okazu.Initialize(data);

        if (box.Place(okazu, gridX, gridY))
        {
            allOkazu.Add(okazu);
            totalOkazuCount++;
        }
        else
        {
            Debug.LogWarning($"Could not place {data.okazuName} at ({gridX}, {gridY})");
            Destroy(obj);
        }
    }

    public void CheckClear()
    {
        if (isCleared) return;
        if (targetBox == null) return;

        int inTargetCount = 0;
        foreach (var okazu in allOkazu)
        {
            if (okazu.CurrentBox == targetBox)
                inTargetCount++;
        }

        Debug.Log($"Okazu in target: {inTargetCount}/{totalOkazuCount}");

        if (inTargetCount == totalOkazuCount && totalOkazuCount > 0)
            OnClear();
    }

    private void OnClear()
    {
        isCleared = true;
        Debug.Log("CLEAR!");

        if (clearText != null)
        {
            clearText.gameObject.SetActive(true);
            clearText.text = "CLEAR!";
        }

        if (retryButton != null)
            retryButton.gameObject.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Retry();
    }
}
