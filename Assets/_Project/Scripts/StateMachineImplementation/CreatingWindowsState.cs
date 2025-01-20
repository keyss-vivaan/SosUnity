using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public class CreatingWindowsState : ARState
{
    public CreatingWindowsState(StateManager manager) : base(manager) { }
 
    private GameObject windowObjectSpawned;
    private GameObject selectedWall;
    private bool isWindowStart = true;
 
    private GameObject tempLeftDimension;
    private GameObject tempRightDimension;
    private GameObject tempTopDimension;
    private GameObject tempBottomDimension;
 
    public override void Enter()
    {
        Debug.Log("Creating windows Now");
        arManager.FinishWindowsButton.SetActive(true);
        arManager.UndoButton.SetActive(true);
        arManager.UndoButton.GetComponent<Button>().onClick.RemoveAllListeners();
        arManager.UndoButton.GetComponent<Button>().onClick.AddListener(OnUndo);
 
        arManager.FinishWindowsButton.GetComponent<Button>().onClick.RemoveAllListeners();
        arManager.FinishWindowsButton.GetComponent<Button>().onClick.AddListener(OnFinishWindows);
 
 
 
 
        tempLeftDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab);
        tempRightDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab);
        tempTopDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab);
        tempBottomDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab);
 
        tempLeftDimension.transform.position = new Vector3(1000, 1000, 1000);
        tempRightDimension.transform.position = new Vector3(1000, 1000, 1000);
        tempTopDimension.transform.position = new Vector3(1000, 1000, 1000);
        tempBottomDimension.transform.position = new Vector3(1000, 1000, 1000);
 
    }
 
    public override void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() || !arManager.TryGetTouchPosition(out Vector2 touchPosition))
        {
            return;
        }
 
        Ray ray = Camera.main.ScreenPointToRay(arManager.ScreenCenter());
        RaycastHit hit;
#if UNITY_EDITOR
 
if (Physics.Raycast(ray, out hit) && Input.GetMouseButtonDown(0) && hit.collider.tag == "Wall") // Check for mouse button press on a "Wall"
{
    var hitPose = hit.point;
 
    if (!isWindowStart)
    {
        // First tap: Start creating the object
        isWindowStart = true;
 
        selectedWall = hit.collider.gameObject;
        windowObjectSpawned = GameObject.Instantiate(arManager.windowObject, hitPose, Quaternion.identity);
        windowObjectSpawned.AddComponent<windowClass>();
        arManager.WindowsObjects.Add(windowObjectSpawned);
 
        // Initialize LineRenderer
        for (int i = 0; i < windowObjectSpawned.GetComponent<LineRenderer>().positionCount; i++)
        {
            windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(i, hitPose);
        }
    }
    else
    {
        // Second tap: Finalize the object
        isWindowStart = false;
 
        if (hit.collider.gameObject == selectedWall)
        {
            windowObjectSpawned.GetComponent<LineRenderer>().loop = true;
 
            Vector3 temp1 = windowObjectSpawned.GetComponent<LineRenderer>().GetPosition(0);
 
            // Update the LineRenderer to form a rectangular shape
            windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(1, new Vector3(temp1.x, hitPose.y, temp1.z));
            windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(2, hitPose);
            windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(3, new Vector3(hitPose.x, temp1.y, hitPose.z));
 
            windowClass tempWindow = new windowClass();
            for (int i = 0; i < windowObjectSpawned.GetComponent<LineRenderer>().positionCount; i++)
            {
                tempWindow.points[i] = windowObjectSpawned.GetComponent<LineRenderer>().GetPosition(i);
            }
            windowObjectSpawned.GetComponent<windowClass>().points = SortWindowPoints(selectedWall.GetComponent<wallClass>(), tempWindow).points;
        }
 
        // Display dimensions and select window type
        DisplayDimensions(windowObjectSpawned, selectedWall);
        SelectWindowType(windowObjectSpawned);
    }
}
else if (isWindowStart)
{
    // During dragging, adjust the LineRenderer dynamically
    Vector3 deviceMovement = Input.acceleration; // Simulate dragging using device motion
    Vector3 newHitPose = new Vector3(
        windowObjectSpawned.transform.position.x + deviceMovement.x,
        windowObjectSpawned.transform.position.y,
        windowObjectSpawned.transform.position.z + deviceMovement.z
    );
 
    for (int i = 0; i < windowObjectSpawned.GetComponent<LineRenderer>().positionCount; i++)
    {
        windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(i, newHitPose);
    }
}
 
#else
 
if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Wall")
{
    var hitPose = hit.point;
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
 
        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (!isWindowStart)
                {    
                    arManager.setsecondtapDoorwindowInstruction.SetActive(true);
                    arManager.doortapicon.SetActive(true);
                    // First tap: Start creating the object
                    isWindowStart = true;
 
                    selectedWall = hit.collider.gameObject;
                    windowObjectSpawned = GameObject.Instantiate(arManager.windowObject, hitPose, Quaternion.identity);
                    windowObjectSpawned.AddComponent<windowClass>();
                    arManager.WindowsObjects.Add(windowObjectSpawned);
 
                    for (int i = 0; i < windowObjectSpawned.GetComponent<LineRenderer>().positionCount; i++)
                    {
                        windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(i, hitPose);
                    }
                }
                else
                {
                    // Second tap: Finalize the object
                    isWindowStart = false;
 
                    if (hit.collider.gameObject == selectedWall)
                    {    
                        // arManager.setsecondtapDoorwindowInstruction.SetActive(false);

                        windowObjectSpawned.GetComponent<LineRenderer>().loop = true;
 
                        Vector3 temp1 = windowObjectSpawned.GetComponent<LineRenderer>().GetPosition(0);
 
                        windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(1, new Vector3(temp1.x, hitPose.y, temp1.z));
                        windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(2, hitPose);
                        windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(3, new Vector3(hitPose.x, temp1.y, hitPose.z));
 
                        windowClass tempWindow = new windowClass();
                        for (int i = 0; i < windowObjectSpawned.GetComponent<LineRenderer>().positionCount; i++)
                        {
                            tempWindow.points[i] = windowObjectSpawned.GetComponent<LineRenderer>().GetPosition(i);
                        }
                        windowObjectSpawned.GetComponent<windowClass>().points = SortWindowPoints(selectedWall.GetComponent<wallClass>(), tempWindow).points;
                    }
 
                    DisplayDimensions(windowObjectSpawned, selectedWall);
                    SelectWindowType(windowObjectSpawned);
                }
                break;
 
            case TouchPhase.Moved:
                if (isWindowStart)
                {
                    Vector3 deviceMovement = Input.acceleration;
                    Vector3 newHitPose = new Vector3(
                        windowObjectSpawned.transform.position.x + deviceMovement.x,
                        windowObjectSpawned.transform.position.y,
                        windowObjectSpawned.transform.position.z + deviceMovement.z
                    );
 
                    for (int i = 0; i < windowObjectSpawned.GetComponent<LineRenderer>().positionCount; i++)
                    {
                        windowObjectSpawned.GetComponent<LineRenderer>().SetPosition(i, newHitPose);
                    }
                }
                break;
        }
    }
}
 
#endif
 
    }
 
    public override void Exit()
    {
        // Debug.Log("Finished creating windows");
        arManager.FinishWindowsButton.SetActive(false);
        arManager.UndoButton.SetActive(false);
 
        GameObject.Destroy(tempLeftDimension);
        GameObject.Destroy(tempRightDimension);
        GameObject.Destroy(tempTopDimension);
        GameObject.Destroy(tempBottomDimension);
 
        arManager.screenCenter.SetActive(false);
        arManager.UndoButton.SetActive(false);
        arManager.FinishWindowsButton.SetActive(false);
        arManager.windowTypePanel.SetActive(false);
 
 
    }
 
    public override void OnFinishWindows()
    {
        arManager.SetState(new Creating3DModelState(arManager));
    }
 
    public override void OnUndo()
    {
        if (arManager.WindowsObjects.Count > 0)
        {
            GameObject lastWindow = arManager.WindowsObjects[arManager.WindowsObjects.Count - 1];
            GameObject.Destroy(lastWindow);
            arManager.WindowsObjects.RemoveAt(arManager.WindowsObjects.Count - 1);
 
            selectedWall.GetComponent<wallClass>().windows.RemoveAt(selectedWall.GetComponent<wallClass>().windows.Count - 1);
        }
    }
 
    private void SelectWindowType(GameObject windowObj)
    {
        windowClass window = windowObj.GetComponent<windowClass>();
        arManager.UndoButton.SetActive(false);
        arManager.FinishWindowsButton.SetActive(false);
        arManager.windowTypePanel.SetActive(true);
 
        for (int i = 0; i < arManager.windowTypePanel.GetComponentsInChildren<Button>().Length; i++)
        {
            string buttonName = arManager.windowTypePanel.GetComponentsInChildren<Button>()[i].name;
            arManager.windowTypePanel.GetComponentsInChildren<Button>()[i].GetComponent<Button>()
                .onClick.AddListener(() => OnSelectWindowType(buttonName, windowObj));
        }
 
    }
 
    private void OnSelectWindowType(string windowTypeSelected, GameObject window)
    {
        Debug.Log(windowTypeSelected);
        window.GetComponent<windowClass>().type = (windowType)Enum.Parse(typeof(windowType), windowTypeSelected);
 
        switch (window.GetComponent<windowClass>().type)
        {
            case windowType.Window:
                window.GetComponent<LineRenderer>().GetComponent<Renderer>().material.color = Color.green;
                break;
            case windowType.Door:
                window.GetComponent<LineRenderer>().GetComponent<Renderer>().material.color = Color.blue;
                break;
            case windowType.AC:
                window.GetComponent<LineRenderer>().GetComponent<Renderer>().material.color = Color.red;
                break;
        }
 
        for (int i = 0; i < arManager.windowTypePanel.GetComponentsInChildren<Button>().Length; i++)
        {
            string buttonName = arManager.windowTypePanel.GetComponentsInChildren<Button>()[i].name;
            arManager.windowTypePanel.GetComponentsInChildren<Button>()[i].GetComponent<Button>()
                .onClick.RemoveAllListeners();
        }
 
 
        arManager.windowTypePanel.SetActive(false);
        arManager.UndoButton.SetActive(true);
        arManager.FinishWindowsButton.SetActive(true);
    }
 
    private void DisplayDimensions(GameObject windowObj, GameObject wallObj)
    {
 
        tempLeftDimension.transform.position = new Vector3(1000, 1000, 1000);
        tempRightDimension.transform.position = new Vector3(1000, 1000, 1000);
        tempTopDimension.transform.position = new Vector3(1000, 1000, 1000);
        tempBottomDimension.transform.position = new Vector3(1000, 1000, 1000);
 
 
        wallClass wall = wallObj.GetComponent<wallClass>();
        windowClass window = windowObj.GetComponent<windowClass>();
 
        wall.windows.Add(window);
 
 
 
 
 
        float ld = Vector3.Distance(new Vector3(wall.points[0].x, 0, wall.points[0].z), new Vector3(window.points[0].x, 0, window.points[0].z));
        float rd = Vector3.Distance(new Vector3(wall.points[1].x, 0, wall.points[1].z), new Vector3(window.points[1].x, 0, window.points[1].z));
 
        float td = Vector3.Distance(new Vector3(0, wall.points[3].y, 0), new Vector3(0, window.points[0].y, 0));
        float bd = Vector3.Distance(new Vector3(0, wall.points[0].y, 0), new Vector3(0, window.points[3].y, 0));
 
 
        GameObject leftDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab, (window.points[0] + window.points[3]) / 2,
            Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up) * Quaternion.Euler(0, 0, 90));
        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            leftDimension.GetComponentInChildren<TMP_Text>().text = "LD: " + ld.ToString("F2") + " m H: " + Vector3.Distance(window.points[0], window.points[3]).ToString("F2") + " m";
        }
        else
        {
            leftDimension.GetComponentInChildren<TMP_Text>().text = "LD: " + (ld * 3.28084f).ToString("F2") + " ft H: " + (Vector3.Distance(window.points[0], window.points[3]) * 3.28084f).ToString("F2") + " ft";
        }
        leftDimension.transform.SetParent(windowObj.transform);
 
 
        GameObject rightDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab, (window.points[1] + window.points[2]) / 2,
        Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up) * Quaternion.Euler(0, 0, -90));
        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            rightDimension.GetComponentInChildren<TMP_Text>().text = "RD: " + rd.ToString("F2") + " m H: " + Vector3.Distance(window.points[1], window.points[2]).ToString("F2") + " m";
        }
        else
        {
            rightDimension.GetComponentInChildren<TMP_Text>().text = "RD: " + (rd * 3.28084f).ToString("F2") + " ft H: " + (Vector3.Distance(window.points[1], window.points[2]) * 3.28084f).ToString("F2") + " ft";
        }
        rightDimension.transform.SetParent(windowObj.transform);
 
 
        GameObject topDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab, (window.points[0] + window.points[1]) / 2,
        Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up));
        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            topDimension.GetComponentInChildren<TMP_Text>().text = "TD: " + td.ToString("F2") + " m W: " + Vector3.Distance(window.points[2], window.points[3]).ToString("F2") + " m";
        }
        else
        {
            topDimension.GetComponentInChildren<TMP_Text>().text = "TD: " + (td * 3.28084f).ToString("F2") + " ft W: " + (Vector3.Distance(window.points[2], window.points[3]) * 3.28084f).ToString("F2") + " ft";
        }
        topDimension.transform.SetParent(windowObj.transform);
 
 
        GameObject bottomDimension = GameObject.Instantiate(arManager.windowMeasurementTextPrefab, (window.points[2] + window.points[3]) / 2 + new Vector3(0, -0.1f, 0),
        Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up));
        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            bottomDimension.GetComponentInChildren<TMP_Text>().text = "BD: " + bd.ToString("F2") + " m W: " + Vector3.Distance(window.points[0], window.points[1]).ToString("F2") + " m";
        }
        else
        {
            bottomDimension.GetComponentInChildren<TMP_Text>().text = "BD: " + (bd * 3.28084f).ToString("F2") + " ft W: " + (Vector3.Distance(window.points[0], window.points[1]) * 3.28084f).ToString("F2") + " ft";
        }
        bottomDimension.transform.SetParent(windowObj.transform);
 
 
        window.width = Vector3.Distance(window.points[2], window.points[3]);
        window.height = Vector3.Distance(window.points[0], window.points[3]);
 
        window.margin_from_bottom = bd;
        window.margin_from_top = td;
        window.margin_from_left = ld;
        window.margin_from_right = rd;

         // Scale and rotate the cube so that one side collapses to align with the given points
        TransformCubeToMatchPoints(window.points);
 
 
    }

     void TransformCubeToMatchPoints(Vector3[] points)
    {

        // Calculate the center of the plane again to ensure accuracy
        Vector3 center = (points[0] + points[1] + points[2] + points[3]) / 4f;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = center;
        // Find the best-fitting plane using the points
        Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;


        // Set the cube's rotation to align with the plane
        Vector3 right = (points[1] - points[0]).normalized;
        Vector3 up = Vector3.Cross(normal, right).normalized;
        cube.transform.rotation = Quaternion.LookRotation(normal, up);

        // Calculate the dimensions of the plane
        float width = Vector3.Distance(points[0], points[1]);
        float height = Vector3.Distance(points[0], points[3]);

        // Scale the cube to match the dimensions of the plane
        cube.transform.localScale = new Vector3(width, height, 0.001f); // Small depth to act as a window

        // Position the cube at the correct center
        cube.transform.position = center;
        cube.GetComponent<Renderer>().material.SetOverrideTag("RenderType", "Transparent");
        cube.GetComponent<Renderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        cube.GetComponent<Renderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        cube.GetComponent<Renderer>().material.SetInt("_ZWrite", 0);
        cube.GetComponent<Renderer>().material.DisableKeyword("_ALPHATEST_ON");
        cube.GetComponent<Renderer>().material.EnableKeyword("_ALPHABLEND_ON");
        cube.GetComponent<Renderer>().material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        cube.GetComponent<Renderer>().material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        cube.GetComponent<Renderer>().material.color = new Color(cube.GetComponent<Renderer>().material.color.r, cube.GetComponent<Renderer>().material.color.g, cube.GetComponent<Renderer>().material.color.b, 1.0f);

    }
 
    private void PlaceTempDimensions(GameObject windowObj, GameObject wallObj)
    {
        wallClass wall = wallObj.GetComponent<wallClass>();
        windowClass window = windowObj.GetComponent<windowClass>();
 
 
 
        float ld = Vector3.Distance(new Vector3(wall.points[0].x, 0, wall.points[0].z), new Vector3(window.points[0].x, 0, window.points[0].z));
        float rd = Vector3.Distance(new Vector3(wall.points[1].x, 0, wall.points[1].z), new Vector3(window.points[1].x, 0, window.points[1].z));
 
        float td = Vector3.Distance(new Vector3(0, wall.points[3].y, 0), new Vector3(0, window.points[0].y, 0));
        float bd = Vector3.Distance(new Vector3(0, wall.points[0].y, 0), new Vector3(0, window.points[3].y, 0));
 
 
        tempLeftDimension.GetComponentInChildren<TMP_Text>().text = "LD: " + ld.ToString("F2") + " H: " + Vector3.Distance(window.points[0], window.points[3]).ToString("F2") + "m";
        tempRightDimension.GetComponentInChildren<TMP_Text>().text = "RD: " + rd.ToString("F2") + " H: " + Vector3.Distance(window.points[1], window.points[2]).ToString("F2") + "m";
        tempTopDimension.GetComponentInChildren<TMP_Text>().text = "TD: " + td.ToString("F2") + " W: " + Vector3.Distance(window.points[2], window.points[3]).ToString("F2") + "m";
        tempBottomDimension.GetComponentInChildren<TMP_Text>().text = "BD: " + bd.ToString("F2") + " W: " + Vector3.Distance(window.points[0], window.points[1]).ToString("F2") + "m";
 
        tempLeftDimension.transform.position = (window.points[0] + window.points[3]) / 2;
        tempRightDimension.transform.position = (window.points[1] + window.points[2]) / 2;
        tempTopDimension.transform.position = (window.points[0] + window.points[1]) / 2;
        tempBottomDimension.transform.position = (window.points[2] + window.points[3]) / 2 + new Vector3(0, -0.1f, 0);
 
 
        tempLeftDimension.transform.rotation = Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up) * Quaternion.Euler(0, 0, 90);
        tempRightDimension.transform.rotation = Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up) * Quaternion.Euler(0, 0, -90);
        tempTopDimension.transform.rotation = Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up);
        tempBottomDimension.transform.rotation = Quaternion.LookRotation(Vector3.Cross(window.points[1] - window.points[0], Vector3.up), Vector3.up);
    }
 
 
    windowClass SortWindowPoints(wallClass wall, windowClass window)
    {
        // Create a new window class to store the sorted points
        windowClass updatedWindow = new windowClass();
 
        // Create a list to track which points have already been used
        List<int> usedIndices = new List<int>();
 
        // Iterate over each point in the wall
        for (int j = 0; j < wall.points.Length; j++)
        {
            float minDistance = float.MaxValue;
            int closestPointIndex = -1;
 
            // Find the closest window point to the current wall point
            for (int i = 0; i < window.points.Length; i++)
            {
                if (usedIndices.Contains(i))
                    continue; // Skip if this window point has already been used
 
                float distance = Vector3.Distance(wall.points[j], window.points[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPointIndex = i;
                }
            }
 
            // Add the closest point to the updated window and mark it as used
            if (closestPointIndex != -1)
            {
                updatedWindow.points[j] = window.points[closestPointIndex];
                usedIndices.Add(closestPointIndex); // Mark this point as used
            }
        }
 
 
        // Reverse the order of the points in the updated window
        Array.Reverse(updatedWindow.points);
 
        // Return the updated window with sorted points
        return updatedWindow;
    }
 
 
}