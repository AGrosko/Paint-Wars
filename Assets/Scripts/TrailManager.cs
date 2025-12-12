using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TrailManager : MonoBehaviour
{
    public Material teamAMaterial; // Material for Team A
    public Material teamBMaterial; // Material for Team B
    public float cellSize = 0.01f;

    [SerializeField] private TextMeshProUGUI BlueScoreText;
    [SerializeField] private TextMeshProUGUI RedScoreText;

    public int gridWidth = 100;
    public int gridHeight = 100;

    private int[,] territoryGrid; // 0 = none, 1 = team A, 2 = team B
    private Mesh territoryMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Dictionary<GameObject, TrailData> trails = new Dictionary<GameObject, TrailData>();

    void Awake()
    {
        territoryGrid = new int[gridWidth, gridHeight];

        // Create territory mesh object
        GameObject territoryObj = new GameObject("TerritoryMesh");
        territoryObj.transform.parent = transform;

        meshFilter = territoryObj.AddComponent<MeshFilter>();
        meshRenderer = territoryObj.AddComponent<MeshRenderer>();
        meshRenderer.materials = new Material[2] { teamAMaterial, teamBMaterial };

        territoryMesh = new Mesh();
        meshFilter.sharedMesh = territoryMesh;
    }

    void Update()
    {
        // Update scores display
        var (teamAScore, teamBScore) = GetScores();
        BlueScoreText.text = $"Blue Team: {teamAScore}";
        RedScoreText.text = $"Red Team: {teamBScore}";

        // Rebuild territory mesh from grid
        UpdateTerritoryMesh();

        // Update territory based on player positions
        foreach (var kvp in trails)
        {
            GameObject player = kvp.Key;
            TrailData trail = kvp.Value;

            Transform anchor = player.transform.Find("Capsule");
            if (anchor == null) continue;

            Vector3 pos = anchor.position;
            pos.y = 0.05f;

            PaintTerritory(pos, trail.team);
        }
    }

    // Register player with team assignment
    public void RegisterPlayer(GameObject player, string team)
    {
        int teamId = (team == "A") ? 1 : 2;

        if (!trails.ContainsKey(player))
        {
            trails[player] = new TrailData
            {
                team = teamId
            };
        }

        Debug.Log($"Registered player {player.name} on team {team}");
    }

    private void PaintTerritory(Vector3 pos, int teamId)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize);
        int y = Mathf.FloorToInt(pos.z / cellSize);

        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            territoryGrid[x, y] = teamId; // overwrite ownership
        }
    }

    private void UpdateTerritoryMesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> trisTeamA = new List<int>();
        List<int> trisTeamB = new List<int>();

        int vertIndex = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int teamId = territoryGrid[x, y];
                if (teamId == 0) continue; // skip empty cells

                // Cell corners in world space
                Vector3 bottomLeft = new Vector3(x * cellSize, 0.05f, y * cellSize);
                Vector3 bottomRight = bottomLeft + new Vector3(cellSize, 0, 0);
                Vector3 topLeft = bottomLeft + new Vector3(0, 0, cellSize);
                Vector3 topRight = bottomLeft + new Vector3(cellSize, 0, cellSize);

                verts.Add(bottomLeft);
                verts.Add(bottomRight);
                verts.Add(topLeft);
                verts.Add(topRight);

                if (teamId == 1)
                {
                    trisTeamA.Add(vertIndex);
                    trisTeamA.Add(vertIndex + 2);
                    trisTeamA.Add(vertIndex + 1);

                    trisTeamA.Add(vertIndex + 1);
                    trisTeamA.Add(vertIndex + 2);
                    trisTeamA.Add(vertIndex + 3);
                }
                else if (teamId == 2)
                {
                    trisTeamB.Add(vertIndex);
                    trisTeamB.Add(vertIndex + 2);
                    trisTeamB.Add(vertIndex + 1);

                    trisTeamB.Add(vertIndex + 1);
                    trisTeamB.Add(vertIndex + 2);
                    trisTeamB.Add(vertIndex + 3);
                }

                vertIndex += 4;
            }
        }

        territoryMesh.Clear();
        territoryMesh.SetVertices(verts);
        territoryMesh.subMeshCount = 2;
        territoryMesh.SetTriangles(trisTeamA, 0);
        territoryMesh.SetTriangles(trisTeamB, 1);
        territoryMesh.RecalculateNormals();
    }

    public (int teamAScore, int teamBScore) GetScores()
    {
        int teamAScore = 0;
        int teamBScore = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (territoryGrid[x, y] == 1) teamAScore++;
                else if (territoryGrid[x, y] == 2) teamBScore++;
            }
        }

        return (teamAScore, teamBScore);
    }

    private class TrailData
    {
        public int team;
    }
}