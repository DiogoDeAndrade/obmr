using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Building Generation")]
    public Camera       baseCamera;
    public float        margin = 300.0f;
    public GameObject[] buildingPrefabs;
    public Vector2      widthRange;
    public Vector2      heightRange;
    public Gradient     colors;
    public int          layer;
    [Header("Scrolling")]
    public float        scrollSpeedScale = 1.0f;
    public float        scrollSpeed = 0.0f;
    
    List<SpriteRenderer>    buildings;
    List<SpriteRenderer>    buildingsToDelete;
    float                   cameraLimitX;

    // Start is called before the first frame update
    void Start()
    {
        buildings = new List<SpriteRenderer>();
        buildingsToDelete = new List<SpriteRenderer>();
        cameraLimitX = baseCamera.aspect * baseCamera.orthographicSize;

        GenStartingBuildings();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 moveVector = new Vector3(-scrollSpeed * scrollSpeedScale * Time.fixedDeltaTime, 0.0f, 0.0f);

        foreach (var building in buildings)
        {
            building.transform.Translate(moveVector);

            if (building.transform.position.x < -cameraLimitX - margin)
            {
                buildingsToDelete.Add(building);
            }
        }
    }

    private void Update()
    {
        foreach (var building in buildingsToDelete)
        {
            buildings.Remove(building);
            Destroy(building.gameObject);
        }
        buildingsToDelete.Clear();

        var lastBuilding = buildings[buildings.Count - 1];
        if (lastBuilding.bounds.max.x < (cameraLimitX + margin))
        {
            buildings.Add(CreateBuilding(lastBuilding.bounds.max.x));
        }
    }

    void GenStartingBuildings()
    {
        float x = -cameraLimitX - margin;

        while (x < cameraLimitX + margin)
        {
            SpriteRenderer  building = CreateBuilding(x);

            x += building.bounds.extents.x * 2.0f;

            buildings.Add(building);
        }
    }

    SpriteRenderer CreateBuilding(float x)
    {
        int r = Random.Range(0, buildingPrefabs.Length);

        GameObject building = Instantiate(buildingPrefabs[r], transform);

        building.transform.position = new Vector3(x, transform.position.y, transform.position.z);
        building.transform.localScale = new Vector3(Random.Range(widthRange.x, widthRange.y), 
                                                    Random.Range(heightRange.x, heightRange.y), 
                                                    1.0f);

        SpriteRenderer sprite = building.GetComponentInChildren<SpriteRenderer>();
        if (sprite)
        {
            float c = Random.Range(0.0f, 1.0f);

            sprite.color = colors.Evaluate(c);
            sprite.sortingOrder = layer;

            return sprite;
        }

        return null;
    }
}
