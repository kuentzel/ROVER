using Newtonsoft.Json;

namespace ROVER
{
    /// <summary>
    /// Enum representing the type of study element.
    /// </summary>
    public enum TypeOfStudyElement
    {
        ItemSet,
        Instruction
    }

    /// <summary>
    /// Base class representing a generic study element.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudyElement
    {
        [JsonProperty(Order = 1)]
        protected TypeOfStudyElement type;

        [JsonProperty(Order = 2)]
        private int index;

        [JsonProperty(Order = 3)]
        protected string title;

        [JsonProperty(Order = 4)]
        private StudySection section;

        [JsonProperty(Order = 5)]
        protected string conditionItemID;

        protected ChoiceItem conditionItem;

        [JsonProperty(Order = 6)]
        protected int[] conditionalAnswers;

        /// <summary>
        /// Initializes a new instance of the StudyElement class.
        /// </summary>
        /// <param name="type">The type of study element.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="title">The title of the element.</param>
        /// <param name="section">The section the element belongs to.</param>
        /// <param name="conditionItemID">The ID of the condition item.</param>
        /// <param name="conditionItem">The condition item.</param>
        /// <param name="conditionalAnswers">The conditional answers.</param>
        public StudyElement(TypeOfStudyElement type, int index, string title, StudySection section, string conditionItemID, ChoiceItem conditionItem, int[] conditionalAnswers)
        {
            this.type = type;
            this.index = index;
            this.title = title;
            this.section = section;
            this.conditionItemID = conditionItemID;
            this.conditionItem = conditionItem;
            this.conditionalAnswers = conditionalAnswers;
        }

        // Properties
        public TypeOfStudyElement Type { get => type; set => type = value; }
        public int Index { get => index; set => index = value; }
        public string Title { get => title; set => title = value; }
        public StudySection Section { get => section; set => section = value; }
        public string ConditionItemID { get => conditionItemID; set => conditionItemID = value; }
        public ChoiceItem ConditionItem { get => conditionItem; set => conditionItem = value; }
        public int[] ConditionalAnswers { get => conditionalAnswers; set => conditionalAnswers = value; }
    }

    /// <summary>
    /// Class representing an instruction within a study, including paragraphs and an optional button.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Instruction : StudyElement
    {
        [JsonProperty(Order = 7)]
        private string[] paragraphs;

        [JsonProperty(Order = 8)]
        private bool hasButton;

        [JsonProperty(Order = 9)]
        private string buttonText;

        /// <summary>
        /// Initializes a new instance of the Instruction class.
        /// </summary>
        /// <param name="type">The type of study element.</param>
        /// <param name="index">The index of the instruction.</param>
        /// <param name="title">The title of the instruction.</param>
        /// <param name="section">The section the instruction belongs to.</param>
        /// <param name="conditionItemID">The ID of the condition item.</param>
        /// <param name="conditionItem">The condition item.</param>
        /// <param name="conditionalAnswer">The conditional answers.</param>
        /// <param name="paragraphs">The paragraphs of the instruction.</param>
        /// <param name="hasButton">Whether the instruction has a button.</param>
        /// <param name="buttonText">The text of the button.</param>
        public Instruction(TypeOfStudyElement type, int index, string title, StudySection section, string conditionItemID, ChoiceItem conditionItem, int[] conditionalAnswer, string[] paragraphs, bool hasButton, string buttonText)
            : base(type, index, title, section, conditionItemID, conditionItem, conditionalAnswer)
        {
            this.paragraphs = paragraphs;
            this.hasButton = hasButton;
            this.buttonText = buttonText;
        }

        // Properties
        public string[] Paragraphs { get => paragraphs; set => paragraphs = value; }
        public bool HasButton { get => hasButton; set => hasButton = value; }
        public string ButtonText { get => buttonText; set => buttonText = value; }
    }
}
