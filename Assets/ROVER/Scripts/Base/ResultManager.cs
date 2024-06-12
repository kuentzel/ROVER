using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ROVER
{
    // Enum to define the export mode
    public enum ExportMode
    {
        Default,
        LexSortedColumns
    }

    public class ResultManager : MonoBehaviour
    {
        // Public fields
        public StudyManager studyManager;
        public ExportMode exportMode;
        public bool debug;

        // Private fields
        private string vpn = "";
        private Dictionary<Item, List<int>> results;
        private StreamWriter resultsWriter;
        private bool isLogging;
        private bool exported;
        private string highestAnswerIndex;
        private int duplicateCounter = 2;

        // Properties
        public bool Exported { get => exported; set => exported = value; }
        public Dictionary<Item, List<int>> Results { get => results; set => results = value; }
        public string Vpn { get => vpn; set => vpn = value; }

        // Unity Start method
        void Start()
        {
            results = new Dictionary<Item, List<int>>();
        }

        /// <summary>
        /// Writes the answer for a given item to the results dictionary and to the results file.
        /// </summary>
        /// <param name="item">The item being answered.</param>
        /// <param name="selection">The list of selected answers.</param>
        public void WriteItemAnswer(Item item, List<int> selection)
        {
            // Cast the item to a ChoiceItem. This cast is safe because we know that the item being passed in is a ChoiceItem.
            ChoiceItem choiceItem = (ChoiceItem)item;

            // Add the item and its selection to the results dictionary if it does not already exist.
            // If it does exist, update the selection for the existing item.
            if (!results.ContainsKey(item))
                results.Add(item, selection);
            else
                results[item] = selection;

            // Generate a string that uniquely identifies this answer by combining the indexes of the section, item set, and item.
            string answerIndex = $"{item.ItemSet.Section.Index}{item.ItemSet.Index}{item.Index}";
            // If this answer's index is higher than the current highest answer index, update the highest answer index.
            if (String.Compare(answerIndex, highestAnswerIndex) == 1)
                highestAnswerIndex = answerIndex;

            // Build a string containing all the paragraphs of the item, separated by " - ".
            // This is used for logging purposes.
            string paragraphs = "";
            if (item is ChoiceItem)
            {
                foreach (string paragraph in choiceItem.Paragraphs)
                    paragraphs += $" - {paragraph}";
                paragraphs = paragraphs.Replace("\n", ""); // Remove newline characters from the paragraphs string.
            }

            // If the resultsWriter is initialized (i.e., we are writing results to a file), construct and write the answer line.
            if (resultsWriter != null)
            {
                // Construct a CSV-formatted string containing all relevant information about the answer.
                string line = $"{DateTime.UtcNow:HH:mm:ss:fff};{item.ItemSet.Section.Index};{item.ItemSet.Section.Title};{item.ItemSet.Index};{item.ItemSet.Title};{item.Index};{item.Title};{paragraphs};{item.ID}";

                // If the item allows multiple choices, append all selected options and their labels to the line.
                if (choiceItem.IsMultipleChoice)
                {
                    // Join all selected options' scales into a single string separated by ", ".
                    string options = string.Join(", ", results[item].Select(i => choiceItem.OptionScale[i]));
                    // Join all selected options' labels into a single string separated by ", ".
                    string optionLabels = string.Join(", ", results[item].Select(i => choiceItem.OptionLabels[i]));
                    // Add the options and option labels to the line.
                    line += $";{options};{optionLabels}";
                }
                else
                {
                    // If the item allows only a single choice, add the selected option's scale and label to the line.
                    line += $";{choiceItem.OptionScale[selection[0]]};{choiceItem.OptionLabels[selection[0]]}";
                }

                // Write the constructed line to the results file.
                resultsWriter.WriteLine(line);
                // Ensure all data is written to the file by flushing the writer's buffer.
                resultsWriter.Flush();
            }
        }


        /// <summary>
        /// Starts collecting results and initializes the result export file.
        /// </summary>
        /// <param name="vpn">The VPN identifier.</param>
        public void StartCollectingResults(string vpn)
        {
            if (isLogging || !gameObject.activeInHierarchy)
                return;

            this.vpn = vpn;
#if UNITY_EDITOR
            if (studyManager.debug)
#endif
                OpenResultExportFile(1);

#if !UNITY_EDITOR
            if (Results != null)
                Results.Clear();
#endif
            highestAnswerIndex = "";
            isLogging = true;
        }

        /// <summary>
        /// Stops collecting results and closes the result export file.
        /// </summary>
        public void StopCollectingResults()
        {
            if (!isLogging)
                return;

            if (resultsWriter != null)
            {
                resultsWriter.Flush();
                resultsWriter.Close();
                resultsWriter = null;
            }
#if UNITY_EDITOR
            if (debug)
#endif
                if (exportMode == ExportMode.LexSortedColumns)
                {
                    ExportAnswers(false);
                }
            isLogging = false;
        }

        /// <summary>
        /// Exports the questionnaire results to a CSV file.
        /// </summary>
        /// <param name="quest">The study questionnaire.</param>
        /// <returns>True if the export was successful.</returns>
        public bool ExportQuestionnaireResults(Study quest)
        {
            resultsWriter = new StreamWriter(GenerateFilePath() + ".csv");
            string columns = "ApplicationTime;";

            char[] toTrim = { ';' };
            columns = columns.TrimEnd(toTrim);

            if (debug)
                Debug.Log($"CSV generated at {GenerateFilePath()} with columns: {columns}");

            resultsWriter.WriteLine(columns);
            resultsWriter.Flush();
            return true;
        }

        /// <summary>
        /// Generates the file path for the export file.
        /// </summary>
        /// <returns>The generated file path.</returns>
        private string GenerateFilePath()
        {
            string directory = $"{Application.dataPath}/Export/{vpn}_{studyManager.SessionStartTimeString}";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return $"{directory}/{vpn}_{studyManager.SessionStartTimeString}";
        }

        /// <summary>
        /// Opens the result export file based on the specified log type.
        /// </summary>
        /// <param name="log">The log type (1 for answer log, 2 for final answers, others for export answers).</param>
        public void OpenResultExportFile(int log)
        {
            try
            {
                string filePath;
                if (log == 1)
                    filePath = GenerateFilePath() + "_AnswersLog.csv";
                else if (log == 2)
                    filePath = GenerateFilePath() + "_FinalAnswers.csv";
                else
                    filePath = GenerateFilePath() + "_ExportAnswers_" + duplicateCounter++ + ".csv";

                resultsWriter = new StreamWriter(filePath, false, System.Text.Encoding.UTF8, 512);
            }
            catch (IOException e)
            {
                resultsWriter?.Close();
                string filePath = GenerateFilePath() + "_AnswersLog_" + duplicateCounter++ + ".csv";
                resultsWriter = new StreamWriter(filePath, false, System.Text.Encoding.UTF8, 512);
            }

            if (log == 1)
            {
                resultsWriter.WriteLine($"VPN:;{vpn};Session:;{studyManager.SessionStartTimeString};StudyFile:;{studyManager.ActiveStudy.Title}");
                resultsWriter.WriteLine();
                resultsWriter.WriteLine("Time;Section;Item Set;Item;Item Order;Item Label;Item Text;Scale;Response");
                resultsWriter.Flush();
            }
        }

        /// <summary>
        /// Exports the answers to the result file, optionally marking it as the final result.
        /// </summary>
        /// <param name="finalResult">Indicates whether this is the final result.</param>
        public void ExportAnswers(bool finalResult)
        {
            // Ensure any existing writer is properly flushed and closed before opening a new one.
            if (resultsWriter != null)
            {
                resultsWriter.Flush();   // Flush any remaining data to the file.
                resultsWriter.Close();   // Close the file writer to release the file handle.
                resultsWriter = null;    // Set the writer to null to indicate no active writer.
            }

            // Check if the export mode is LexSortedColumns.
            if (exportMode == ExportMode.LexSortedColumns)
            {
                // Open a new result export file depending on whether this is a final result or an intermediate result.
                OpenResultExportFile(finalResult ? 2 : 3);

                // Create a dictionary to hold the final results with item IDs as keys.
                Dictionary<string, Item> finalResults = results.Keys.ToDictionary(i => i.ID, i => i);

                // Create a list of sorted item IDs.
                List<string> sorted = finalResults.Keys.ToList();
                sorted.Sort(); // Sort the item IDs lexicographically.

                // Construct the first line of the CSV file with sorted item IDs.
                string firstLine = string.Join(";", sorted);
                resultsWriter?.WriteLine(firstLine); // Write the first line to the CSV file.

                // Construct the second line of the CSV file with corresponding option scales for each item.
                string secondLine = "";
                foreach (string s in sorted)
                {
                    // If the item allows multiple choices, append all selected options and their labels to the line.
                    if (results[finalResults[s]].Count > 1)
                    {
                        // Join all selected options' scales into a single string separated by ", ".
                        for (int i = 0; i < results[finalResults[s]].Count; i++)
                            secondLine += ((ChoiceItem)finalResults[s]).OptionScale[results[finalResults[s]][i]] + ",";
                        // Trim the trailing comma and add a semicolon to the line.
                        secondLine = secondLine.Trim(',');
                        secondLine += ";";

                    }
                    else
                    {
                        // If the item allows only a single choice, add the selected option's scale and label to the line.
                        secondLine += ((ChoiceItem)finalResults[s]).OptionScale[results[finalResults[s]][0]] + ";";
                    }
                }

                //string.Join(";", sorted.Select(s => ((ChoiceItem)finalResults[s]).OptionScale[results[finalResults[s]][0]]));

                resultsWriter?.WriteLine(secondLine); // Write the second line to the CSV file.

                // Ensure the data is flushed to the file and close the writer.
                resultsWriter?.Flush();   // Flush any remaining data to the file.
                resultsWriter?.Close();   // Close the file writer to release the file handle.
                resultsWriter = null;     // Set the writer to null to indicate no active writer.
            }
        }


        /// <summary>
        /// Cleans up the result manager by ensuring the result file is closed properly.
        /// </summary>
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (debug)
#endif
                if (exportMode == ExportMode.LexSortedColumns)
                {
                    ExportAnswers(false);
                }

            if (resultsWriter != null)
            {
                resultsWriter.Flush();
                resultsWriter.Close();
                resultsWriter = null;
            }
        }
    }
}
