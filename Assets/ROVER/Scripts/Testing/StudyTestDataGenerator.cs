
using ROVER;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ROVER.Samples
{

    public class StudyTestDataGenerator : MonoBehaviour
    {
        public string importFileName;
        [SerializeField]
        public Study activeStudy;
        private string _sessionStartTimeString;

        public string prefix;
        public string suffix;
        public bool useCustomName;
        public bool useTimestamp;
        public string customName;
        public bool useCustomDirectory;
        public string customDirectory;
        // Start is called before the first frame update
        void Start()
        {
            _sessionStartTimeString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        private string GenerateFilePath(string s)
        {
            string filePath = "";
            string directory = "";

            if (useCustomDirectory)
                directory = Application.dataPath + "/Export/" + customDirectory + "/";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (useCustomName)
                if (useTimestamp)
                    filePath = directory + _sessionStartTimeString + customName + "_" + s + ".json";
                else
                    filePath = directory + customName + "_" + s + ".json";
            else
                filePath = directory + prefix + _sessionStartTimeString + suffix + "_" + s + ".json";


            return filePath;
        }

#if UNITY_EDITOR
        // Update is called once per frame
        void Update()
        {
            if (!debug) return;
            if (Input.GetKeyDown(KeyCode.S))
            {
                ExportSample();
            }
        }
#endif
        public void ImportQuestionnaireTemplate()
        {
            if (importFileName == null || importFileName == "")
            {
                Debug.Log("No file name given.");
                return;
            }

            String importString = "Import/" + importFileName;

            if (importString.Contains(".json"))
            {
                importString.Replace(".json", "");
                Debug.Log("Filename sliced");
            }

            Debug.Log("Importing: " + importString);

            TextAsset jsonString = Resources.Load<TextAsset>(importString);

            StudyJsonConverter studyConverter = new StudyJsonConverter();
            JsonConverter[] converters = { studyConverter };

            Study importStudy = Newtonsoft.Json.JsonConvert.DeserializeObject<Study>(jsonString.text, converters);

            activeStudy = importStudy;
        }


        void ExportSample()
        {

            Study study = GenerateSample();
            String output = Newtonsoft.Json.JsonConvert.SerializeObject(study, Formatting.Indented);

            _writer = new StreamWriter(GenerateFilePath(study.ID));
            _writer.WriteLine(output);
            _writer.Flush();
            _writer.Close();

            _writer = null;
        }

        private StreamWriter _writer;
        public bool debug = true;


        public Study GenerateSample()
        {
            Study study = new Study(id: "rover_sample", title: "ROVER Sample", showProgressBar: true, allowBacksteps: true, sections: null);
            StudySection s1 = new StudySection(index: 0, title: "Section A: Instruction", study: study, elements: null);
            StudySection s2 = new StudySection(1, "Section B: Single Item", study, null);
            StudySection s3 = new StudySection(2, "Section C: Multiple Items", study, null);
            StudySection[] sections = { s1,s2,s3 };
            study.Sections = sections;

            string[] SI_OptionScale = { "1", "2", "3", "4" };
            string[] SI_OptionLabel = { "fully disagree", "disagree", "agree", "fully agree" };

            string[] MI_OptionScale = { "1", "2", "3", "4", "5", "6", "7" };
            string[] MI_OptionLabel = { "fully disagree", "disagree", "somewhat disagree", "neutral", "somewhat agree", "agree", "fully agree" };

            int z = 0;
            int j = 0;
            StudyElement[] se1 =
            {
                        //TESTS
                        new Instruction(TypeOfStudyElement.Instruction, index: j ++, title: "Instruction", section: s1, conditionItemID: "", conditionItem: null, conditionalAnswer: null, paragraphs: new[] {"This is the first instruction. The user is not presented with a button to progress. The investigator has to manually progress using the ROVER desktop UI." } , hasButton: false, buttonText: ""),
                        new Instruction(TypeOfStudyElement.Instruction, j ++, "InstructionButton", s1, "", null, null, new[]{"This is the second instruction, this time with a button, labelled NEXT. When building the instruction in code you can label the button anything you want, but make sure it fits. In the code set the first value behind the text content to true and fill in the string between the quotes."}, hasButton: true, buttonText: "NEXT"),
                        new Instruction(TypeOfStudyElement.Instruction, j ++, "InstructionParagraph", s1, "", null, null, new[]{"This is the first paragraph of the third instruction.\n\nThis is the second paragraph using backslash+n."} , true, "shy button"),
                        new Instruction(TypeOfStudyElement.Instruction, j ++, "InstructionStyling", s1, "", null, null, new[]{"Using HTML-tags in the code, you can be <b>bold</b>, <b><color=#68B3DE>colorful</color></b>, and so much more."} , true, "<b>BE BOLD</b>") 
        };
            z++;
            j = 0;
            StudyElement[] se2 =
            {
                        new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s2, "", null, null, "This is the hint. It is the same for all items of a set. You can also style it like items.\nOption scales and labels are automatically generated from the specification. Make sure they have the same length.", false, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_SI1", "Single Item (List)", null, true, "", null, null, new[]{"This is the first single item. You can only choose one answer. The layout here is a <b>vertical list</b> of all answer options."}, false, 0 , 0, SI_OptionScale, SI_OptionLabel, ChoiceItemLayoutVariant.ListVertical)
                        }),
                        new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s2, "", null, null, "This is a different hint. Option scales and labels are automatically generated from the specification. Make sure they have the same length.", false, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_SI2", "Single Item (Scale)", null, true, "", null, null, new[]{"This is the second single item. You can only choose one answer. The layout here is <b>endpoint scale</b> on the control panel"}, false, 0 , 0, SI_OptionScale, SI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay)
                        }),
                        new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s2, "", null, null, "This another hint. Use it for short form instructions or interaction help.", false, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_SI3", "Single Item (Labels)", null, true, "", null, null, new[]{"This is the third single item. You can only choose one answer. The layout here is <b>endpoint scale</b> with a legend."}, false, 0 , 0, SI_OptionScale, SI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpoints)
                        }),
						new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s2, "", null, null, "This yet another hint. There are more layout options. Refer to the code and its documentation.", false, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_SI4", "Single Item (Horizontal)", null, true, "", null, null, new[]{"This is the fourth single item. You can only choose one answer. The layout here is <b>with button labels</b> on the control panel."}, false, 0 , 0, SI_OptionScale, SI_OptionLabel, ChoiceItemLayoutVariant.PanelLabels)
                        }),
                        new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s2, "", null, null, "This the last hint. Consider which layout variant works best for you. ScaleEndpointsNoDisplay is the default. It does not show a legend on the main display.", false, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_SI5", "Single Item (Horizontal)", null, true, "", null, null, new[]{"This is the fourth single item. You can only choose one answer. The layout here is a <b>horizontal list<b> of all answer options."}, false, 0 , 0, SI_OptionScale, SI_OptionLabel, ChoiceItemLayoutVariant.ListHorizontal)
                        })
            };
            z++;
            j = 0;
            StudyElement[] se3 =
                    {
                        new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s3, "", null, null, "This is the hint for this set of multiple items. They can be in different layouts.", false, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_MI1_1", "Multi Item S1 (List)", null, true, "", null, null, new[]{"This is the first item of this multi set. You can only choose one answer. The layout here is a <b>vertical list</b> of all answer options."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ListVertical),
							new ChoiceItem(TypeOfItem.ChoiceItem, 1, "var_MI1_2", "Multi Item S1 (Scale)", null, true, "", null, null, new[]{"This is the second item of this multi set. You can only choose one answer. The layout here is <b>scale endpoints</b> on the control panel."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay)
						}),
                        new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s3, "", null, null, "This is a different hint. The items here are randomized.", true, randomizeItems: true,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_MI2_1", "Multi Item S2.1", null, true, "", null, null, new[]{"This is the first item of this multi set. You can only choose one answer. The layout here is a <b>endpoint scale</b>."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay),
							new ChoiceItem(TypeOfItem.ChoiceItem, 1, "var_MI2_2", "Multi Item S2.2", null, true, "", null, null, new[]{"This is the second item of this multi set.\nYou can also break lines here.", "Even paragraphs like this to separate scenario and question. But only on layouts without list or legend."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay),
							new ChoiceItem(TypeOfItem.ChoiceItem, 2, "var_MI2_3", "Multi Item S2.2", null, true, "", null, null, new[]{"This is the third item of this multi set. This item is multiple choice! You can specify minimum and maximum amount of possible answers. Here you can choose between 2 or 3 answers."}, isMultipleChoice: true, minSelection: 2 , maxSelection: 3, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay)
						}),
						new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s3, "", null, null, "This is another hint. You can add conditions to single items or whole sets. Only if the answer to the specified condition item includes one of the conditional answers specified, will the item referring to that condition be displayed.", allowBacksteps: true, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_MI3_1", "Multi Item S3 (Conditions)", null, true, "", null, null, new[]{"This is the first item of this multi set. The second item will only be shown if you select the 4th answer here."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay),
							new ChoiceItem(TypeOfItem.ChoiceItem, 1, "var_MI3_2", "Multi Item S3 (Conditions)", null, true, conditionItemID: "var_MI3_1", null, conditionalAnswers: new[]{3}, new[]{"This is the second item of this multi set. You selected label 4 (position 3) on the previous item. Only if you select smaller than 3 here will the next item set be displayed.\nUsage of the back button can be set globally and within each item set individually.\nTry going back one step and choosing a different outcome."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay)
						}),
						new ItemSet(TypeOfStudyElement.ItemSet, j ++, "Item Set Title (currently not shown)", s3, conditionItemID: "var_MI3_2", null, conditionalAnswer: new[]{0,1}, "This the last hint. Conditions are specified in code by specifying the variable id of the condition item and an array of numeric scale values that trigger the condition.", true, false,
                        new[]{
                            new ChoiceItem(TypeOfItem.ChoiceItem, 0, "var_MI4", "S4 (Conditions)", null, true, "", null, null, new[]{"Great, you did it! You selected smaller than 3 (position 0 or 1) on the last item and caused this item set to display."}, false, 0 , 0, MI_OptionScale, MI_OptionLabel, ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay)
                        }),
                        new Instruction(TypeOfStudyElement.Instruction, j ++, "Last Slide", s3, "", null, null, new[]{"By ending the survey, participants confirm their answers and final results are exported. The log always gets exported and you can also export manually from the UI.\nYou can't jump back over several conditionally hidden items (yet). You can always restart the survey."} , true, "<b>END</b>") 
        
					
					};

            s1.Elements = se1;
            s2.Elements = se2;
            s3.Elements = se3;

            //Make sure every linking reference is set to null to avoid circular referencing exceptions when generating the JSON
            foreach (StudySection ss in study.Sections)
            {
                foreach (StudyElement se in ss.Elements)
                {
                    if (se is ItemSet)
                        foreach (Item i in ((ItemSet)se).Items)
                            i.ItemSet = null;
                    se.Section = null;
                }
                ss.Study = null;
            }

            return study;
        }




        private void OnDestroy()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
            }
        }
    }
}
