using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using TMPro;
using System.Xml.Linq;
using System.Linq;
using System.Net;

namespace ROVER
{

    public enum ImportFileType
    {
        nativeJSON,
        limesurveyXML
    }
    public class StudyJsonImportExportManager : MonoBehaviour
    {
        public StudyManager studyManager;
        private string _sessionStartTimeString;

        private int _fileCounter = 1;

        public ImportFileType fileType = ImportFileType.nativeJSON;
        
        private StreamWriter studyWriter;
        public bool debug = true;
        string importFileName;

        public TMP_InputField input;

        private string GenerateFilePath(string s)
    {
        string filePath = "";
        string directory = "";


        directory = Application.dataPath + "/Export/" + _sessionStartTimeString + "/";

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        filePath = directory +_sessionStartTimeString + ".csv";


        return filePath;
    }

        public Study ImportStudy()
        {    
            if (fileType == ImportFileType.limesurveyXML)
            {                
                ExportStudyJSON(ImportStudyFromLimesurveyXML());
                return ImportStudyFromLimesurveyXML();
            }                
            else
                return ImportStudyFromJSON();
        }

        public void Update()
        {
            if (debug && Input.GetKeyDown(KeyCode.L))
            {
                Study s = ImportStudyFromLimesurveyXML();
                Debug.Log("XML Title; ID: " + s.Title +";" + s.ID);
            }
        }

        public Study ImportStudyFromLimesurveyXML()
        {
            

            if (input.text == null || input.text.Length == 0 || input.text == "")
            {
                Debug.Log("No file name given.");
                return null;
            }
            String importPath;
            importFileName = input.text;
            if (debug)
                importFileName = "ROVERTEST_que";
            if (importFileName.Contains(".xml"))
                importFileName.Replace(".xml", "");
            if (importFileName.Contains(".XML"))
                importFileName.Replace(".XML", "");

            importPath = Application.streamingAssetsPath + "/Import/" + importFileName + ".xml";

            if (!File.Exists(importPath))
            {
                Debug.Log("File does not exist.");
                return null;
            }

            XDocument document = XDocument.Load(importPath);
            Study study = null;

            var studyXElement = document.Element("questionnaire");

            string id = "";
            id += studyXElement.Attribute("id").Value;
            string title = "";
            title += studyXElement.Element("title").Value;
            bool showProgressBar = true;
            bool allowBacksteps = false;

            study = new Study(id, title, showProgressBar, allowBacksteps, null);

            var sectionXElementS = studyXElement.Elements("section");

            StudySection[] sections = new StudySection[sectionXElementS.Count()];
            int i = 0;
            foreach (var sectionXElement in sectionXElementS ) 
            {
                
                string SStitle = "" + sectionXElement.Element("sectionInfo").Element("text").Value;

                var questionXElementS = sectionXElement.Elements("question");
                StudySection ss = new StudySection(i, SStitle, study, null);
                StudyElement[] elements = new StudyElement[questionXElementS.Count()];
                int j = 0;
                foreach (var questionXElement in questionXElementS )
                {
                    if (questionXElement.Element("response") != null && questionXElement.Element("response").Element("free") != null)
                    {
                        String text = WebUtility.HtmlDecode(questionXElement.Element("text").Value);
                        text = text.Replace("</p>", System.Environment.NewLine).Replace("<p>", "").Replace("&lt;p&gt;", "").Replace("&lt;/p&gt;", "").TrimEnd(Environment.NewLine.ToCharArray());
                        elements[j] = new Instruction(TypeOfStudyElement.Instruction, j, questionXElement.Element("response").Attribute("varName").Value, ss, null, null, null, new[] { text }, questionXElement.Element("directive") != null, questionXElement.Element("directive") != null ? questionXElement.Element("directive").Element("text").Value : "");
                        j++;
                    }
                    else
                    {
                        String hint = WebUtility.HtmlDecode(questionXElement.Element("directive") != null ? questionXElement.Element("directive").Element("text").Value : "");
                        hint = hint.Replace("</p>", System.Environment.NewLine).Replace("<p>", "").Replace("&lt;p&gt;", "").Replace("&lt;/p&gt;", "").TrimEnd( Environment.NewLine.ToCharArray());
                        elements[j] = new ItemSet(TypeOfStudyElement.ItemSet, j, questionXElement.Element("response").Attribute("varName").Value, ss, null, null, null, hint , true, (questionXElement.Element("text") != null && questionXElement.Element("text").Value.ToLower().Equals("randomize")) ? true : false, null);

                        var itemXElementS = questionXElement.Elements("subQuestion");

                        var answersXElementS = questionXElement.Element("response").Element("fixed").Elements("category");

                        string[] answerOptions = new string[answersXElementS.Count()];
                        string[] answerScale = new string[answersXElementS.Count()];
                        int m = 0;
                        foreach (  var answerOption in answersXElementS )
                        {
                            answerOptions[m] = answerOption.Element("label").Value;
                            answerScale[m] = answerOption.Element("value").Value;
                            m++;
                        }

                        Item[] items = new Item[itemXElementS.Count()];
                        int k = 0;
                        foreach ( var itemXElement in itemXElementS )
                        {
                            items[k] = new ChoiceItem(TypeOfItem.ChoiceItem, k, itemXElement.Attribute("varName").Value, questionXElement.Element("response").Attribute("varName").Value, (ItemSet)elements[j], true, null, null, null, new[] { itemXElement.Element("text").Value }, false, 0, 0, answerScale, answerOptions, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay);
                            k++;
                        }
                        ((ItemSet)elements[j]).Items = items;
                        j++;
                    }

                }

                ss.Elements = elements;
                sections[i++] = ss;
            }

            study.Sections = sections;

            return study;
        }

        public Study ImportStudyFromJSON()
        {
            if (input.text == null || input.text.Length == 0 || input.text == "")
            {
                Debug.Log("No file name given.");
                return null;
            }
            String importPath;
            importFileName = input.text;

            if (importFileName.Contains(".json"))
                importFileName.Replace(".json", "");

            importPath = Application.streamingAssetsPath + "/Import/" + importFileName + ".json";

            if (!File.Exists(importPath))
            {
                Debug.Log("File does not exist.");
                return null;
            }
            StreamReader reader = new StreamReader(importPath);
            String jsonString = reader.ReadToEnd().Trim();
            reader.Close();
            StudyJsonConverter studyConverter = new StudyJsonConverter();
            JsonConverter[] converters = { studyConverter };
            Study importStudy = Newtonsoft.Json.JsonConvert.DeserializeObject<Study>(jsonString, converters);

            return importStudy;

        }

        void ExportStudyJSON(Study study)
        {
            foreach (StudySection ss in study.Sections)
            {
                foreach (StudyElement se in ss.Elements)
                {
                    if (se is ItemSet)
                        foreach (Item it in ((ItemSet)se).Items)
                            it.ItemSet = null;
                    se.Section = null;
                }
                ss.Study = null;
            }
            String output = Newtonsoft.Json.JsonConvert.SerializeObject(study, Formatting.Indented);
            string directory = Application.dataPath + "/Export/Conversions/";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            studyWriter = new StreamWriter(Application.dataPath + "/Export/Conversions/" + study.ID + "_" + studyManager.SessionStartTimeString + ".json");
            studyWriter.WriteLine(output);
            studyWriter.Flush();
            studyWriter.Close();

            studyWriter = null;
        }


    

    private void OnDestroy()
    {
            if (studyManager.ActiveStudy != null && fileType == ImportFileType.limesurveyXML)
                ExportStudyJSON(studyManager.ActiveStudy);
            if (studyWriter != null)
        {
            studyWriter.Flush();
            studyWriter.Close();
        }

     }

}

public class StudyJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var temp = objectType == typeof(Study);
            return temp;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            string id = Convert.ToString(((JValue)obj["id"]).Value);
            string title = Convert.ToString(((JValue)obj["title"]).Value);
            bool showProgressBar = false;
            if ((JValue)obj["showProgressBar"] != null)
                showProgressBar = Convert.ToBoolean(((JValue)obj["showProgressBar"]).Value);
            bool allowBacksteps = true;
            if ((JValue)obj["allowBacksteps"] != null)
                allowBacksteps = Convert.ToBoolean(((JValue)obj["allowBacksteps"]).Value);
            
            
            
            Study study = new Study(id, title, showProgressBar, allowBacksteps, null);
            study.Sections = jsonToSections(obj, study).ToArray();

            foreach (StudySection section in study.Sections)
                foreach (StudyElement element in section.Elements)
                {
                    if (element.ConditionItemID != null && element.ConditionItemID != "")
                        {
                            for (int j = 0; j <= section.Index; j++)
                                for (int k = 0; k < element.Index; k++)
                                    if (section.Study.Sections[j].Elements[k].Type == TypeOfStudyElement.ItemSet)
                                        foreach (Item item in ((ItemSet)section.Study.Sections[j].Elements[k]).Items)
                                            if (item is ChoiceItem && item.ID == element.ConditionItemID)
                                                element.ConditionItem = (ChoiceItem)item;
                        }
                        if (element is ItemSet)
                            {
                                foreach (Item item in ((ItemSet)element).Items)
                                    if (item.ConditionItemID != null && item.ConditionItemID != "")
                                    {
                                        Debug.Log("Trying to find item level condition item");
                                        for (int j = 0; j <= section.Index; j++)
                                            for (int k = 0; k <= element.Index; k++)
                                                if (section.Study.Sections[j].Elements[k].Type == TypeOfStudyElement.ItemSet)
                                                    foreach (Item conItem in ((ItemSet)section.Study.Sections[j].Elements[k]).Items)
                                                        if (conItem.ItemSet == item.ItemSet && conItem.Index < item.Index)
                                                        {
                                                            Debug.Log("Checking item level condition item " + conItem.ID + " against " + item.ConditionItemID);
                                                        
                                                        if (conItem is ChoiceItem && conItem.ID == item.ConditionItemID)
                                                        {
                                                            Debug.Log("Set item level condition item");
                                                            item.ConditionItem = (ChoiceItem)conItem;
                                                        }
                                                        }
                                                            
                                    }
                            }
                }

            return study;
        }

        private List<StudySection> jsonToSections(JObject jObj, Study study)
        {
            List<StudySection> sections = new List<StudySection>();

            JArray sections_JSON = jObj["sections"].Value<JArray>();

            if (sections_JSON != null && sections_JSON.Count > 0)
            {
                for (int i = 0; i < sections_JSON.Count; i++)
                {
                    JObject section_JSON = sections_JSON[i].Value<JObject>();

                    int index = Convert.ToInt32(((JValue)section_JSON["index"]).Value);
                    string title = Convert.ToString(((JValue)section_JSON["title"]).Value);                    

                    StudySection section = new StudySection(index, title, study, null);

                    section.Elements = jsonToElements(section_JSON, section).ToArray();
                    sections.Add(section);
                }
            }
            return sections;
        }

        private List<StudyElement> jsonToElements(JObject jObj, StudySection section)
        {
            List<StudyElement> elements = new List<StudyElement>();

            JArray elements_JSON = jObj["elements"].Value<JArray>();

            if (elements_JSON != null && elements_JSON.Count > 0)
            {
                for (int i = 0; i < elements_JSON.Count; i++)
                {

                    JObject element_JSON = elements_JSON[i].Value<JObject>();

                    int index = Convert.ToInt32(((JValue)element_JSON["index"]).Value);
                    string title = Convert.ToString(((JValue)element_JSON["title"]).Value);
                    string conditionItemID = Convert.ToString(((JValue)element_JSON["conditionItemID"]).Value);
                    int[] conditionalAnswers = null;
                    if (element_JSON["conditionalAnswers"] != null && element_JSON["conditionalAnswers"].Value<JArray>() != null)
                        conditionalAnswers = element_JSON["conditionalAnswers"].Value<JArray>().ToObject<int[]>();

                    ChoiceItem conditionItem = null;

                    if ((((JValue)element_JSON["type"]).Value).ToString() == "0")
                    {
                        string hint = Convert.ToString(((JValue)element_JSON["hint"]).Value);
                        bool randomizeItems = Convert.ToBoolean(((JValue)element_JSON["randomizeItems"]).Value);
                        bool allowBacksteps = Convert.ToBoolean(((JValue)element_JSON["allowBacksteps"]).Value);

                        ItemSet itemSet = new ItemSet(TypeOfStudyElement.ItemSet, index, title, section, conditionItemID, conditionItem, conditionalAnswers, hint, allowBacksteps, randomizeItems, null);
                       
                        itemSet.Items = jsonToItems(element_JSON, itemSet).ToArray();

                        elements.Add(itemSet);
                    }
                    else if ((((JValue)element_JSON["type"]).Value).ToString() == "1")
                    {

                        string[] paragraphs = JArray.Parse(element_JSON["paragraphs"].ToString()).ToObject<string[]>(); 
                        bool hasButton = Convert.ToBoolean(((JValue)element_JSON["hasButton"]).Value);
                        string buttonText = Convert.ToString(((JValue)element_JSON["buttonText"]).Value);
                        elements.Add(new Instruction(TypeOfStudyElement.Instruction, index, title, section, conditionItemID, conditionItem, conditionalAnswers, paragraphs, hasButton, buttonText));
                    }
                }
            }

            return elements;
        }

        private List<Item> jsonToItems(JObject jObj, ItemSet itemSet)
        {
            List<Item> items = new List<Item>();

            JArray items_JSON = jObj["items"].Value<JArray>();

            if (items_JSON != null && items_JSON.Count > 0)
            {
                for (int i = 0; i < items_JSON.Count; i++)
                {

                    JObject item_JSON = items_JSON[i].Value<JObject>();
                    int index = Convert.ToInt32(((JValue)item_JSON["index"]).Value);
                    string id = Convert.ToString(((JValue)item_JSON["id"]).Value);
                    string title = Convert.ToString(((JValue)item_JSON["title"]).Value);
                    bool isMandatory = Convert.ToBoolean(((JValue)item_JSON["isMandatory"]).Value);
                    string conditionItemID = Convert.ToString(((JValue)item_JSON["conditionItemID"]).Value);
                    int[] conditionalAnswers = null;
                    if (item_JSON["conditionalAnswers"] != null && item_JSON["conditionalAnswers"].Value<JArray>() != null)
                        conditionalAnswers = item_JSON["conditionalAnswers"].Value<JArray>().ToObject<int[]>();

                    ChoiceItem conditionItem = null;

                    if ((((JValue)item_JSON["type"]).Value).ToString() == "0")
                    {
                        string[] answerScale = JArray.Parse(item_JSON["answerScale"].ToString()).ToObject<string[]>();
                        string[] answerLabels = JArray.Parse(item_JSON["answerLabels"].ToString()).ToObject<string[]>();

                        bool isMultipleChoice = Convert.ToBoolean(((JValue)item_JSON["isMultipleChoice"]).Value);
                        int minSelection = Convert.ToInt32(((JValue)item_JSON["minSelection"]).Value);
                        int maxSelection = Convert.ToInt32(((JValue)item_JSON["maxSelection"]).Value);


                        string[] paragraphs = JArray.Parse(item_JSON["paragraphs"].ToString()).ToObject<string[]>();

                        int layoutVariant = 0;
                        if ((JValue)item_JSON["layoutVariant"] != null)
                            layoutVariant = Convert.ToInt32(((JValue)item_JSON["layoutVariant"]).Value);

                        ChoiceItem item = new ChoiceItem(TypeOfItem.ChoiceItem, index, id, title, itemSet, isMandatory, conditionItemID, conditionItem, conditionalAnswers, paragraphs, isMultipleChoice, minSelection, maxSelection, answerScale, answerLabels, (ChoiceItemLayoutVariant)layoutVariant);
                        
                        items.Add(item);

                    }
                }
            }

            return items;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
