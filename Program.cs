using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

class Program
{
    static void Log(string message)
    {
        Console.WriteLine(message);
        Console.Out.Flush();
    }

    static void Main(string[] args)
    {
        Log("TeklaExtractor starting...");
        Log("Ensure Tekla Structures 2026 is running with a model open.");

        Model model = new Model();

        Log("Checking Tekla connection...");

        bool connected;
        try
        {
            connected = model.GetConnectionStatus();
        }
        catch (Exception ex)
        {
            Log("Connection failed: " + ex.Message);
            if (ex.InnerException != null)
                Log("  Inner: " + ex.InnerException.Message);
            return;
        }

        if (!connected)
        {
            Log("Not connected. Start Tekla Structures 2026 and open a model, then run again.");
            return;
        }

        ModelInfo info = model.GetInfo();
        Log("Connected to Tekla Structures");
        Log("Model: " + info.ModelName);
        Log("");

        CreateSampleBeam(model);
        Log("");

        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tekla_data.json");
        ExtractBeamsAndColumns(model, info.ModelName, jsonPath);
    }

    static void CreateSampleBeam(Model model)
    {
        Beam beam = new Beam(new Point(0, 0, 0), new Point(4000, 0, 0));
        beam.Profile.ProfileString = "IPE300";
        beam.Material.MaterialString = "S275";
        beam.Name = "B-API-001";

        if (!beam.Insert())
        {
            Log("Failed to insert beam into the model.");
            return;
        }

        model.CommitChanges();
        Log("Created beam: IPE300, S275, (0,0,0) -> (4000,0,0), ID " + beam.Identifier.ID);
    }

    static void ExtractBeamsAndColumns(Model model, string modelName, string jsonPath)
    {
        ModelObjectEnumerator objects = model.GetModelObjectSelector()
            .GetAllObjectsWithType(ModelObject.ModelObjectEnum.BEAM);
        objects.SelectInstances = true;

        var elements = new List<ElementData>();

        while (objects.MoveNext())
        {
            Beam beam = objects.Current as Beam;
            if (beam == null)
                continue;

            if (beam.Type != Beam.BeamTypeEnum.BEAM && beam.Type != Beam.BeamTypeEnum.COLUMN)
                continue;

            if (!beam.Select())
                continue;

            string elementType = beam.Type == Beam.BeamTypeEnum.COLUMN ? "column" : "beam";

            elements.Add(new ElementData
            {
                Id = beam.Identifier.ID.ToString(),
                Type = elementType,
                Profile = beam.Profile != null ? beam.Profile.ProfileString : string.Empty,
                Material = beam.Material != null ? beam.Material.MaterialString : string.Empty,
                Start = ToPointData(beam.StartPoint),
                End = ToPointData(beam.EndPoint)
            });
        }

        var export = new TeklaExport
        {
            ModelName = modelName,
            Elements = elements
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(export, options);
        File.WriteAllText(jsonPath, json);

        int beamCount = 0;
        int columnCount = 0;
        foreach (ElementData element in elements)
        {
            if (element.Type == "column")
                columnCount++;
            else
                beamCount++;
        }

        Log("Type     | ID       | Profile              | Material             | Start                | End");
        Log("---------+----------+----------------------+----------------------+----------------------+----------------------");

        foreach (ElementData element in elements)
        {
            Log(string.Format(
                "{0,-8} | {1,-8} | {2,-20} | {3,-20} | ({4:F0},{5:F0},{6:F0})     | ({7:F0},{8:F0},{9:F0})",
                element.Type,
                element.Id,
                Truncate(element.Profile, 20),
                Truncate(element.Material, 20),
                element.Start.X,
                element.Start.Y,
                element.Start.Z,
                element.End.X,
                element.End.Y,
                element.End.Z));
        }

        Log("");
        Log("Summary: " + beamCount + " beam(s), " + columnCount + " column(s), " + elements.Count + " total.");
        Log("Saved to: " + jsonPath);
    }

    static PointData ToPointData(Point point)
    {
        if (point == null)
            return new PointData { X = 0, Y = 0, Z = 0 };

        return new PointData { X = point.X, Y = point.Y, Z = point.Z };
    }

    static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }
}

class TeklaExport
{
    public string ModelName { get; set; }
    public List<ElementData> Elements { get; set; }
}

class ElementData
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Profile { get; set; }
    public string Material { get; set; }
    public PointData Start { get; set; }
    public PointData End { get; set; }
}

class PointData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}
