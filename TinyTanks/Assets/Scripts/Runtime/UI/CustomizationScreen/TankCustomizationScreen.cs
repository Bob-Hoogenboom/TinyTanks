using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TankCustomizationScreen : MonoBehaviour
{
    [Header("Tank Type Data")]
    public MenuTankData smallTankData;
    public MenuTankData mediumTankData;
    public MenuTankData heavyTankData;

    [Header("Preview Root")]
    public Transform bodyPreviewParent;
    public Transform cupolaPreviewParent;

    [Header("UI References")]
    public Transform skinButtonsParent; 
    public GameObject skinButtonprefab; 


    private MenuTankData _currentTankData;
    private GameObject currentBody;
    private GameObject currentCupola;
    private int bodyIndex;
    private int cupolaIndex;
    private int materialIndex;

    private readonly List<GameObject> _spawnedDecalButtons = new List<GameObject>();


    void Start()
    {
        SelectTankType(mediumTankData); // Default to small tank
    }

    public void SelectTankType(MenuTankData tankData)
    {
        _currentTankData = tankData;
        bodyIndex = 0;
        cupolaIndex = 0;
        materialIndex = 0;
        UpdatePreview();

        BuildMaterialButtons();
    }

    public void NextBody()
    {
        bodyIndex = (bodyIndex + 1) % _currentTankData.bodyOptions.Count;
        UpdatePreview();
    }

    public void NextCupola()
    {
        cupolaIndex = (cupolaIndex + 1) % _currentTankData.cupolaOptions.Count;
        UpdatePreview();
    }

    public void SelectMaterial(int index)
    {
        materialIndex = index;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        // Clean up old parts
        foreach (Transform child in bodyPreviewParent) Destroy(child.gameObject);
        foreach (Transform child in cupolaPreviewParent) Destroy(child.gameObject);

        // Instantiate new body
        currentBody = Instantiate(_currentTankData.bodyOptions[bodyIndex], bodyPreviewParent);
        currentCupola = Instantiate(_currentTankData.cupolaOptions[cupolaIndex], cupolaPreviewParent);

        // Apply material
        ApplyMaterialToTank(_currentTankData.availableMaterials[materialIndex].material);
    }

    private void ApplyMaterialToTank(Material mat)
    {
        Renderer[] bodyRenderers = bodyPreviewParent.GetComponentsInChildren<Renderer>();
        Renderer[] cupolaRenderers = cupolaPreviewParent.GetComponentsInChildren<Renderer>();

        foreach (var rend in bodyRenderers)
            rend.material = mat;

        foreach (var rend in cupolaRenderers)
            rend.material = mat;
    }

    private void BuildMaterialButtons()
    {
        // Clear previous buttons
        foreach (var btn in _spawnedDecalButtons)
        {
            Destroy(btn);
        }
        _spawnedDecalButtons.Clear();

        // Spawn new buttons
        for (int i = 0; i < _currentTankData.availableMaterials.Count; i++)
        {
            int index = i; // local copy for lambda
            GameObject newButton = Instantiate(skinButtonprefab, skinButtonsParent);
            _spawnedDecalButtons.Add(newButton);

            Button btnComponent = newButton.GetComponent<Button>();
            btnComponent.onClick.AddListener(() => SelectMaterial(index));  //fill in button click action event


            // Optional: visualize material color on button if it has an Image
            Image img = newButton.GetComponentsInChildren<Image>()[1];
            if (img)
            {
                img.color = _currentTankData.availableMaterials[index].color;
                Debug.Log("Ja heeft ie");
            }
        }
    }
}
