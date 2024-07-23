//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using Aspose.Cells;
using Aspose.Gis;
using Aspose.Gis.SpatialReferencing;


namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var workbook = new Workbook(@"C:\Users\ronak.shirolawala\Downloads\csa_cbsa_shapes_final_Copy.csv");
            //workbook.Save(@"C:\Users\ronak.shirolawala\Downloads\csa_cbsa_shapes_final.tiff");
            CSVFileReader();
        }

        public static void CSVFileReader()
        {
            FeatureCollection featureCollection = new FeatureCollection();
            try
            {
                int linePointer = 0;
                int incTemp = 0;
                int incTempFileId = 0;
                List<double[]> corrdinates;
                List<List<double[]>> corrdinates1;
                double[] data = new double[2];
                using (var reader = new StreamReader(@"C:\Users\ronak.shirolawala\Downloads\csa_cbsa_shapes_final.csv"))
                {
                    while (!reader.EndOfStream)
                    {
                        if (linePointer == 0)//|| linePointer == 2 || linePointer == 3 || linePointer==12 || linePointer==11 || linePointer == 10)
                        {
                            reader.ReadLine();
                            linePointer++;
                            continue;
                        }
                        var line = reader.ReadLine();
                        var values = line.Split(';');
                        string[] geometryStrings;
                        Geometry geometryPolygon;
                        bool isValid = false;
                        if (!values[1].Substring(1, 40).Contains("MULTI"))
                        {
                            geometryStrings = values[1].Split(',');
                            geometryPolygon = new Geometry("Polygon");
                            corrdinates = new List<double[]>();
                            int geometryCount = 1;
                            int maxGeometryCount = geometryStrings.Count();
                            foreach (var geometry in geometryStrings)
                            {
                                if (geometryCount == 1)
                                {
                                    var geoData = geometry.Remove(0, 11).Split(' ');
                                    double d1;
                                    double d2;
                                    Double.TryParse(geoData[0], out d1);
                                    Double.TryParse(geoData[1], out d2);
                                    if (d1 != 0 && d2 != 0)
                                    {
                                        isValid = true;
                                        data = new double[2] { d1, d2 };
                                    }
                                }
                                else if (geometryCount < maxGeometryCount)
                                {
                                    var geoData = geometry.Split(' ');
                                    double d1;
                                    double d2;
                                    Double.TryParse(geoData[1], out d1);
                                    Double.TryParse(geoData[2], out d2);
                                    if (d1 != 0 && d2 != 0)
                                    {
                                        isValid = true;
                                        data = new double[2] { d1, d2 };
                                    }
                                }
                                else
                                {
                                    //var geoData = geometry.Substring(0, geometry.Length - 4).Split(' ');
                                    //double d1;
                                    //double d2;
                                    //Double.TryParse(geoData[0], out d1);
                                    //Double.TryParse(geoData[1], out d2);
                                    //if (d1 != 0 && d2 != 0)
                                    //{
                                    //    isValid = true;
                                    //    data = new double[2] { corrdinates[0] };
                                    //}
                                    corrdinates.Add(corrdinates[0]);
                                    continue;
                                }
                                //if (corrdinates.Count> 0 || corrdinates[corrdinates.Count - 1] != data)
                                corrdinates.Add(data);
                                if (corrdinates[corrdinates.Count - 1][0] != data[0] && corrdinates[corrdinates.Count - 1][1] != data[1])
                                    corrdinates.Add(data);

                                geometryCount++;
                            }
                            if (!isValid)
                            {
                                linePointer++;
                                continue;
                            }
                            geometryPolygon.coordinates = new List<List<double[]>>();
                            geometryPolygon.coordinates.Add(corrdinates);
                            featureCollection.features.Add(new Feature("Polygon")
                            {
                                geometry = geometryPolygon,
                                properties = new Payload
                                {
                                    CbsaCode = values[0]
                                }
                            });
                            corrdinates = new List<double[]>();
                        }
                        else
                        {
                            geometryStrings = values[1].Split(new string[] { "))" }, StringSplitOptions.None);
                            geometryPolygon = new Geometry("MultiPolygon");
                            corrdinates1 = new List<List<double[]>>();
                            corrdinates = new List<double[]>();
                            int outerPolygonCount = 1;
                            int subPolygonCount = 1;
                            int multiPolygonCount = geometryStrings.Count();
                            try
                            {
                                foreach (var geometry in geometryStrings)
                                {
                                    var subPolygon = geometry.Split(',');
                                    var maxPolygon = subPolygon.Length;
                                    subPolygonCount = 1;
                                    foreach (var polygon in subPolygon)
                                    {
                                        if (string.IsNullOrEmpty(polygon))
                                        {
                                            subPolygonCount++;
                                            continue;
                                        }
                                        if (subPolygonCount == 1 && multiPolygonCount != outerPolygonCount)
                                        {
                                            var geoData = polygon.Remove(0, 17).Split(' ');
                                            double d1;
                                            double d2;
                                            Double.TryParse(geoData[0], out d1);
                                            Double.TryParse(geoData[1], out d2);
                                            if (d1 != 0 && d2 != 0)
                                            {
                                                isValid = true;
                                                data = new double[2] { d1, d2 };
                                            }
                                        }
                                        else if (subPolygonCount < maxPolygon - 1)
                                        {
                                            var geoData = polygon.Split(' ');
                                            double d1;
                                            double d2;
                                            Double.TryParse(geoData[1].Replace("((", ""), out d1);
                                            Double.TryParse(geoData[2], out d2);
                                            if (d1 != 0 && d2 != 0)
                                            {
                                                isValid = true;
                                                data = new double[2] { d1, d2 };
                                            }
                                        }
                                        else if (subPolygonCount == maxPolygon && !polygon.Equals(")\""))
                                        {
                                            corrdinates.Add(corrdinates[0]);
                                            subPolygonCount++;
                                            continue;
                                        }

                                        else if (multiPolygonCount == outerPolygonCount)
                                        {
                                            continue;
                                        }
                                        corrdinates.Add(data);
                                        subPolygonCount++;
                                    }
                                    corrdinates1.Add(corrdinates);
                                    corrdinates = new List<double[]>();
                                    outerPolygonCount++;
                                }
                                geometryPolygon.coordinates = new List<List<List<double[]>>>();
                                geometryPolygon.coordinates.Add(corrdinates1);
                                featureCollection.features.Add(new Feature("MultiPolygon")
                                {
                                    geometry = geometryPolygon,
                                    properties = new Payload
                                    {
                                        CbsaCode = values[0]
                                    }
                                });
                                corrdinates1 = new List<List<double[]>>();
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }

                        }
                        linePointer++;
                        incTemp++;
                    }
                    #region single file with polygons
                    using (var writer = File.CreateText(@"C:\Users\ronak.shirolawala\Downloads\ldgeojson\Polygon_MultiFeature\Upload\Testing_MultiPoly_exclude_dup_3round_of_" + incTempFileId + ".ldgeojson"))
                    {
                        foreach (var feature in featureCollection.features)
                        {
                            var json = JsonConvert.SerializeObject(feature);
                            //json = json.Replace(",[]", "");
                            writer.WriteLine(json);
                        }
                    }
                    //using (var writer = File.CreateText(@"C:\Users\ronak.shirolawala\Downloads\ldgeojson\Polygon_MultiFeature\Upload\Testing_Poly_exclude_dup_4round_of" + incTempFileId + ".ldgeojson"))
                    //{
                    //    foreach (var feature in featureCollection.features)
                    //    {
                    //        writer.WriteLine(JsonConvert.SerializeObject(feature));
                    //    }
                    //}
                    #endregion
                    //#region Individual Polygon file
                    //int fileItem = 0;
                    //foreach (var feature1 in featureCollection.features)
                    //{
                    //    using (var writer = File.CreateText(@"C:\Users\ronak.shirolawala\Downloads\ldgeojson\Polygon_SingleFeature\csa_cbsa_shapes_20sep__" + fileItem + "_.ldgeojson"))
                    //    {
                    //        writer.WriteLine(JsonConvert.SerializeObject(feature1));
                    //    }
                    //    fileItem++;
                    //}
                    //#endregion
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }

}
public class FeatureCollection
{
    public FeatureCollection()
    {
        type = "FeatureCollection";
        features = new List<Feature>();
    }
    public string type { get; set; }
    public List<Feature> features { get; set; }
}
public class Feature
{
    public Feature(string geoType)
    {
        type = "Feature";
        properties = new Payload();
        geometry = new Geometry(geoType);
    }
    public Geometry geometry { get; set; }
    public string type { get; set; }
    public Payload properties { get; set; }
}
public class Geometry
{
    public Geometry(string geoType)
    {
        type = geoType;
    }
    public string type { get; set; }
    //[JsonProperty("coordinates")]
    //public List<List<double[]>> coordinates { get; set; }
    //[JsonProperty("coordinates")]
    //public List<List<List<double[]>>> coordinates { get; set; }
    public dynamic coordinates { get; set; }
}
 
public class Payload
{
    public string CbsaCode { get; set; }
}