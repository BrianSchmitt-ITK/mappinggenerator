using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

class Program
{
    static string Main()
    {
        var allowList = new Dictionary<string, string[]>()
                {
                    {"Court", new string[] { "CourtType", "Division", "Part", "IndexNumber", "DistrictNumber", "DistrictCourtCaseNo", "DocketNumber", "CaseNumber" } },
                    {"Property", new string[] { "ApartmentOrShop", "Block", "BuildingName", "City", "County", "EqualizedValuation", "Lot", "ModelBlock", "ModelLot", "MunicipalityType", "NewConstruction", "StreetNumber", "PercentageInterest", "PostalTownTownship", "Qualifier", "StreetName", "Section", "State", "Street", "TypeOfUse", "Zip" }},
                    {"Bankruptcy", new string[] { "IrsFamilyNumber" }},
                    {"Address", new string[] {}}
                };

        var dataTypeMap = new Dictionary<string, string> { { "ShortText", "string" } };

        // get files doesn't support folder globbing so work around it
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var rootDirectory = appData + @"\LEAP Desktop\User\";
        var files = Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories).Where(
            (directoryPath) => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(directoryPath), @"Tables") == 0).SelectMany(
            (directoryPath) => Directory.GetFiles(directoryPath, "*.xml"));

        foreach (var file in files)
        {
            var xmlString = File.ReadAllText(file);
            var fileName = Path.GetFileNameWithoutExtension(file);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var tableNode = xmlDoc.SelectSingleNode("//Table");
            var className = tableNode.Attributes["ClassName"]?.InnerText;
            if (!allowList.ContainsKey(className)) { continue; } // check to see if config has setting

            var fieldNodes = xmlDoc.SelectNodes("//Field").Cast<XmlNode>()
                    .OrderBy(r => r.SelectSingleNode("Name").InnerText);

            // clean name removes the special chars - a regex would suffice
            // but in case you need to over-ride a particular value leaving as is...
            Func<string, string> cleanName = str => Regex.Replace(new CultureInfo("en-US", false).TextInfo.ToTitleCase(str), "[^a-zA-Z0-9]", String.Empty);
            //Func<string, string> cleanName = str => new CultureInfo("en-US", false).TextInfo.ToTitleCase(str)
            //                    .Replace(" ", string.Empty)
            //                    .Replace(" ", string.Empty)
            //                    .Replace("[", string.Empty)
            //                    .Replace("]", string.Empty)
            //                    .Replace("-", string.Empty)
            //                    .Replace("–", string.Empty)
            //                    .Replace("(", string.Empty)
            //                    .Replace(")", string.Empty)
            //                    .Replace(",", string.Empty)
            //                    .Replace("/", string.Empty)
            //                    .Replace("'", string.Empty)
            //                    .Replace(".", string.Empty)
            //                    .Replace("$", string.Empty)
            //                    .Replace(":", string.Empty)
            //                    .Replace("&", string.Empty)
            //                    .Replace("’", string.Empty)
            //                    .Replace("?", string.Empty)
            //                    .Replace("|", string.Empty)
            //                    .Replace("{", string.Empty)
            //                    .Replace("}", string.Empty)
            //                    .Replace("§", string.Empty)
            //                    .Replace("‐", string.Empty)
            //                    .Replace("\"", string.Empty);

            // map the FieldId and Name values
            // then organize by our allowlist
            var allNodes = fieldNodes.Select(f => new { fieldId = f.SelectSingleNode("FieldId")?.InnerText, name = cleanName(f.SelectSingleNode("Name")?.InnerText), datatype = f.SelectSingleNode("DataType")?.InnerText });
            var allowedFieldNodes = allNodes.Where(n => allowList.ContainsKey(className) && allowList[className].Contains(n.name, StringComparer.CurrentCultureIgnoreCase));
            var disallowedFieldNodes = allNodes.Where(n => !allowList.ContainsKey(className) || !allowList[className].Contains(n.name, StringComparer.CurrentCultureIgnoreCase));

            if (allowedFieldNodes.Count() != allowList[className].Count())
            {
            }

            foreach (var ele in allowedFieldNodes)
            {
                var dataType = dataTypeMap.ContainsKey(ele.datatype) ? dataTypeMap[ele.datatype] : ele.datatype;
            } // end of foreach allowedFieldNodes loop

            foreach (var ele in disallowedFieldNodes)
            {
            } // end of foreach disallowedFieldNodes loop
        }

        Console.ReadLine();
    }
}
