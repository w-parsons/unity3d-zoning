using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/**
 * Controls display of the UI elements.
 */
public class UIManager : MonoBehaviour
{
    TextMeshProUGUI areasText, squaresText;
    Button drawButton, deleteButton;
    ZoneManager zm;

    // Start is called before the first frame update
    void Start()
    {
        areasText = GameObject.Find("DistinctAreasText").GetComponent<TextMeshProUGUI>();
        squaresText = GameObject.Find("TotalSquaresText").GetComponent<TextMeshProUGUI>();
        drawButton = GameObject.Find("DrawButton").GetComponent<Button>();
        deleteButton = GameObject.Find("DeleteButton").GetComponent<Button>();
        DrawZone dz = GameObject.Find("DrawZone").GetComponent<DrawZone>();
        zm = GameObject.Find("ZoneManager").GetComponent<ZoneManager>();

        drawButton.onClick.AddListener(delegate
        {
            dz.setDrawingMode(DrawZone.DrawingMode.Draw);
        });

        deleteButton.onClick.AddListener(delegate
        {
            dz.setDrawingMode(DrawZone.DrawingMode.Delete);
        });
    }

    // Update is called once per frame
    void Update()
    {
        areasText.text = "Distinct Areas: " + zm.getDistinctAreas();
        squaresText.text = "Total Squares: " + zm.getSizeOfAllRects();
    }
}
