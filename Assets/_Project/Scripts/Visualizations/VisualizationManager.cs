using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FlutterUnityIntegration;
using UnityEngine.SceneManagement;
using TMPro;

public class VisualizationManager : MonoBehaviour
{
    public List<wallClass> walls;
    public List<windowClass> windows;
    public List<PlaceObject> measurements;

    public GameObject Camera2D;
    public GameObject Camera3D;
    public GameObject camera2DButton;
    public GameObject camera3DButton;

    public TMP_InputField roomName;
    public GameObject switchDimensionPanel;
    public GameObject meterButton;
    public GameObject feetButton;


    public GameObject measurementTextPrefab;

    public GameObject createPolygonWithHolesPrefab;
    public GameObject windowLinePrefab;
    public Room room;

    public TextMeshProUGUI windowtext;
    public TextMeshProUGUI Doortext;
    public TextMeshProUGUI ACtext;

    private wallClass wall;

    private List<GameObject> measurementDisplays = new List<GameObject>();
    private string exportPath;

    private void Start()
    {
        foreach (var wall in GameObject.FindObjectsOfType<wallClass>())
        {
            walls.Add(wall);
        }

        foreach (var window in GameObject.FindObjectsOfType<windowClass>())
        {
            windows.Add(window);
        }

        foreach (var measurement in GameObject.FindObjectsOfType<PlaceObject>())
        {
            measurements.Add(measurement);
        }

        // exportPath = Application.dataPath + "/Resources/export.json";
        exportPath = Application.persistentDataPath + "/export.json";

        List<Vector3> floorPoints = new List<Vector3>();
        for (int i = 0; i < walls.Count; i++)
        {
            floorPoints.Add(walls[i].points[0]);
        }

        GameObject floor = GameObject.Instantiate(createPolygonWithHolesPrefab);
        floor.GetComponent<CreatePolygonWithHoles>().outerPoints = floorPoints;
        floor.GetComponent<CreatePolygonWithHoles>().CreateCustomPolygonWithHoles(false);


        //Display Wall's dimensions
        for (int i = 0; i < measurements.Count - 1; i++)
        {
            DisplayMeasurement(measurements[i].GetComponent<PlaceObject>().position, measurements[i + 1].GetComponent<PlaceObject>().position, -0.1f);

        }
        DisplayMeasurement(measurements[measurements.Count - 1].GetComponent<PlaceObject>().position, measurements[0].GetComponent<PlaceObject>().position, -0.1f);



        for (int j = 0; j < walls.Count; j++)
        {
            List<Vector3> wallPoints = new List<Vector3>();
            for (int i = 0; i < walls[j].GetComponent<wallClass>().points.Length; i++)
            {
                wallPoints.Add(walls[j].GetComponent<wallClass>().points[i]);
            }
            GameObject wall = GameObject.Instantiate(createPolygonWithHolesPrefab);
            wall.GetComponent<CreatePolygonWithHoles>().outerPoints = wallPoints;

            for (int i = 0; i < walls[j].GetComponent<wallClass>().windows.Count; i++)
            {
                windowClass window = walls[j].GetComponent<wallClass>().windows[i].GetComponent<windowClass>();
                if (window != null)
                {
                    List<Vector3> windowPoints = new List<Vector3>();
                    windowPoints = window.points.ToList();

                    Hole hole = new Hole();
                    hole.points = windowPoints;
                    wall.GetComponent<CreatePolygonWithHoles>().holes.holes.Add(hole);

                    Vector3 point2 = window.points[2];
                    Vector3 point1 = window.points[3];
                    float margin = 0f; // Default margin
                    switch (window.type)
                    {
                        case windowType.Door:
                            margin = 0f;
                            break;
                        case windowType.Window:
                            margin = 0.05f;
                            break;
                        case windowType.AC:
                            margin = 0.1f;
                            break;
                    }
                    DisplayMeasurementOpposite(point1, point2, margin);// for door window and AC far length

                    Vector3[] windowPoints3D = windowPoints.ToArray();
                    DisplayWindows(windowPoints3D, walls[j].GetComponent<wallClass>(), window.type);
                }
            }

            wall.GetComponent<CreatePolygonWithHoles>().CreateCustomPolygonWithHoles(true);

        }

        if (PlayerPrefs.GetInt("measurement") == 1)
        {
            SwitchToFt();
        }

    }


    public static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        Vector3 fromPointToP1 = point - p1;
        float distance = Vector3.Dot(fromPointToP1, normal);
        Vector3 projectedPoint = point - distance * normal;

        return projectedPoint;
    }

    // public void SwitchCamera()
    // {

    //     Camera2D.SetActive(!Camera2D.activeSelf);
    //     Camera3D.SetActive(!Camera3D.activeSelf);
    //     camera2DButton.GetComponent<Button>().interactable = !Camera2D.activeSelf;
    //     camera3DButton.GetComponent<Button>().interactable = !Camera3D.activeSelf;
    // }

    public void Activate3DCamera()
    {
        Camera2D.SetActive(false);
        Camera3D.SetActive(true);
        windowtext.gameObject.SetActive(false);
        Doortext.gameObject.SetActive(false);
        ACtext.gameObject.SetActive(false);

        camera2DButton.GetComponent<Button>().interactable = true;
        camera3DButton.GetComponent<Button>().interactable = false;

        foreach (GameObject g in measurementDisplays)
        {
            g.SetActive(false);
        }

        switchDimensionPanel.SetActive(false);
        roomName.gameObject.SetActive(false);
    }

    public void Activate2DCamera()
    {
        Camera2D.SetActive(true);
        Camera3D.SetActive(false);
        windowtext.gameObject.SetActive(true);
        Doortext.gameObject.SetActive(true);
        ACtext.gameObject.SetActive(true);
        camera2DButton.GetComponent<Button>().interactable = false;
        camera3DButton.GetComponent<Button>().interactable = true;
        foreach (GameObject g in measurementDisplays)
        {
            g.SetActive(true);
        }

        switchDimensionPanel.SetActive(true);
        roomName.gameObject.SetActive(true);
    }


    public void DisplayMeasurement(Vector3 point1, Vector3 point2, float farValue)
    {
        float distance = Vector3.Distance(point2, point1);
        GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2,
            Quaternion.LookRotation(point2 - point1, Vector3.up) * Quaternion.Euler(90, 90, 0));
        g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + " m";
        g.GetComponentInChildren<TMP_Text>().transform.localPosition += new Vector3(0, farValue, 0);
        measurementDisplays.Add(g);
    }

    public void DisplayMeasurementOpposite(Vector3 point1, Vector3 point2, float farValue)
    {
        float distance = Vector3.Distance(point2, point1);
        GameObject g = Instantiate(measurementTextPrefab, (point1 + point2) / 2,
            Quaternion.LookRotation(point2 - point1, Vector3.up) * Quaternion.Euler(90, -90, 0));
        g.GetComponentInChildren<TMP_Text>().text = distance.ToString("F2") + " m";
        g.GetComponentInChildren<TMP_Text>().transform.localPosition += new Vector3(0, farValue, 0);
        measurementDisplays.Add(g);
    }



    public void DisplayWindows(Vector3[] windowPoints, wallClass wall, windowType type)
    {
        TransformCubeToMatchPoints(windowPoints, type);

        GameObject windowLine = GameObject.Instantiate(windowLinePrefab);
        windowLine.GetComponent<LineRenderer>().positionCount = 2;
        windowLine.GetComponent<LineRenderer>().SetPosition(0, windowPoints[0]);
        windowLine.GetComponent<LineRenderer>().SetPosition(1, windowPoints[1]);

        windowPoints[0] = new Vector3(windowPoints[0].x, 0, windowPoints[0].z);
        windowPoints[1] = new Vector3(windowPoints[1].x, 0, windowPoints[1].z);

        Vector3 point3 = new Vector3(wall.points[0].x, 0, wall.points[0].z);
        Vector3 point4 = new Vector3(wall.points[1].x, 0, wall.points[1].z);

        float margin = 0f; // Default margin

        switch (type)
                    {
                        case windowType.Door:
                            margin = 0.05f;
                            break;
                        case windowType.Window:
                            margin = 0.1f;
                            break;
                        case windowType.AC:
                            margin = 0f;
                            break;
                    }


        if (Vector3.Distance(windowPoints[0], point3) < Vector3.Distance(windowPoints[1], point4))
        {
            DisplayMeasurement(windowPoints[0], point3, margin);//left margin
        }
        else
        {
            DisplayMeasurementOpposite(windowPoints[1], point4, margin);// right margin
        }

        // Calculate the dimensions of the window (width and height)
        float width = Vector3.Distance(windowPoints[0], windowPoints[1]);
        float height = Vector3.Distance(windowPoints[0], windowPoints[3]);

        // Update the corresponding text field based on the window type
        // switch (type)
        // {
        //     case windowType.Door:
        //         Doortext.text = $"Door: W = {width:F2} m, H = {height:F2} m";
        //         break;
        //     case windowType.Window:
        //         windowtext.text = $"Window: W = {width:F2} m, H = {height:F2} m";
        //         break;
        //     case windowType.AC:
        //         ACtext.text = $"AC: W = {width:F2} m, H = {height:F2} m";
        //         break;
        // }
    }


    void TransformCubeToMatchPoints(Vector3[] points, windowType type)
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
        cube.transform.localScale = new Vector3(width, height, 0.009f); // Small depth to act as a window

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

        Color windowColor = Color.white;

        switch (type)
        {
            case windowType.AC:
                windowColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                break;
            case windowType.Door:
                windowColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                break;
            case windowType.Window:
                windowColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
                break;

        }

        cube.GetComponent<Renderer>().material.color = windowColor;

    }

    public void ExportRoom()
    {
        // if (room.name == "")
        // {
        //     room.name = "Living Room";
        // }
        room.walls = new List<Wall>();


        if (PlayerPrefs.GetInt("measurement") == 0)
        {
            room.unit = "m";
            foreach (wallClass wall in walls)
            {
                Wall m_wall = new Wall();
                m_wall.name = wall.name;
                m_wall.length = Vector3.Distance(wall.points[0], wall.points[1]);
                Vector3 direction = wall.points[1] - wall.points[0];
                // m_wall.angle_from_segment_point = Vector3.SignedAngle(direction, Vector3.left, Vector3.up);
                m_wall.angle_from_segment_point = (Vector3.SignedAngle(direction, Vector3.left, Vector3.up) + 360) % 360;

                m_wall.items = new List<Item>();

                for (int i = 0; i < wall.windows.Count; i++)
                {
                    windowClass window = wall.windows[i].GetComponent<windowClass>();
                    if (window != null)
                    {
                        Item item = new Item();
                        item.type = window.type.ToString();
                        item.width = window.width;
                        item.height = window.height;
                        item.margin_from_left = window.margin_from_left;
                        item.margin_from_right = window.margin_from_right;
                        item.margin_from_bottom = window.margin_from_bottom;
                        item.margin_from_top = window.margin_from_top;
                        m_wall.items.Add(item);

                    }
                }
                room.walls.Add(m_wall);
            }
            room.height = Vector3.Distance(walls[0].points[0], walls[0].points[3]);
        }
        else
        {
            room.unit = "ft";
            foreach (wallClass wall in walls)
            {
                Wall m_wall = new Wall();
                m_wall.name = wall.name;
                m_wall.length = Vector3.Distance(wall.points[0], wall.points[1]) * 3.28084f; ;
                Vector3 direction = wall.points[1] - wall.points[0];
                // m_wall.angle_from_segment_point = Vector3.SignedAngle(direction, Vector3.left, Vector3.up);
                m_wall.angle_from_segment_point = (Vector3.SignedAngle(direction, Vector3.left, Vector3.up) + 360) % 360;

                m_wall.items = new List<Item>();

                for (int i = 0; i < wall.windows.Count; i++)
                {
                    windowClass window = wall.windows[i].GetComponent<windowClass>();
                    if (window != null)
                    {
                        Item item = new Item();
                        item.type = window.type.ToString();
                        item.width = window.width * 3.28084f; ;
                        item.height = window.height * 3.28084f; ;
                        item.margin_from_left = window.margin_from_left * 3.28084f; ;
                        item.margin_from_right = window.margin_from_right * 3.28084f; ;
                        item.margin_from_bottom = window.margin_from_bottom * 3.28084f; ;
                        item.margin_from_top = window.margin_from_top * 3.28084f; ;
                        m_wall.items.Add(item);

                    }
                }
                room.walls.Add(m_wall);
            }
            room.height = Vector3.Distance(walls[0].points[0], walls[0].points[3]) * 3.28084f; ;
        }

        ExportRoomToJsonFile();

        SendMessageToFlutter();

    }

    public void SendMessageToFlutter()
    {
        UnityMessageManager.Instance.SendMessageToFlutter("SwitchToFlutter");
    }

    public void ExportRoomToJsonFile()
    {
        try
        {
            RoomWrapper wrapper = new RoomWrapper { room = room };
            string json = JsonUtility.ToJson(wrapper, true); // Pretty print
            File.WriteAllText(exportPath, json);
            UnityMessageManager.Instance.SendMessageToFlutter(json);
            Debug.Log("Room exported successfully to " + exportPath);
        }
        catch (IOException e)
        {
            Debug.LogError("Error writing file: " + e.Message);
        }
    }

    // public void SetRoomName()
    // {
    //     room.name = roomName.text;
    // }

    public void SwitchToFt()
    {
        PlayerPrefs.SetInt("measurement", 1);
        meterButton.GetComponent<Button>().interactable = true;
        feetButton.GetComponent<Button>().interactable = false;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("windowMeasurementDisplay"))
        {
            string s = g.GetComponentInChildren<TMP_Text>().text;
            string[] split = s.Split(' ');
            s = split[0];
            float f = float.Parse(s);
            f = f * 3.28084f;
            g.GetComponentInChildren<TMP_Text>().text = f.ToString("F2") + " ft";
        }
    }

    public void SwitchToMeter()
    {
        PlayerPrefs.SetInt("measurement", 0);
        meterButton.GetComponent<Button>().interactable = false;
        feetButton.GetComponent<Button>().interactable = true;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("windowMeasurementDisplay"))
        {
            string s = g.GetComponentInChildren<TMP_Text>().text;
            string[] split = s.Split(' ');
            s = split[0];
            float m = float.Parse(s);
            m = m / 3.28084f;
            g.GetComponentInChildren<TMP_Text>().text = m.ToString("F2") + " m";
        }
    }

    public void UpdateRoomDimensions(string jsonData)
    {
        SingletonExample.Instance.JsonData = jsonData;
        // Load the "Hello" scene if data is received
        SceneManager.LoadScene("Loading 1");
    }

}



