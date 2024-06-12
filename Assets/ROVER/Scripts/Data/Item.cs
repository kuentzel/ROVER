using Newtonsoft.Json;

namespace ROVER
{
    /// <summary>
    /// Enum representing the type of item.
    /// </summary>
    public enum TypeOfItem
    {
        ChoiceItem,
        SliderItem,
        VoiceRecorderItem
    }

    /// <summary>
    /// Enum representing the layout variant of a choice item.
    /// </summary>
    public enum ChoiceItemLayoutVariant
    {
        ListVertical,
        ListHorizontal,
        ScaleEndpoints,
        ScaleEndpointsNoDisplay,
        PanelLabels
    }

    /// <summary>
    /// Base class representing a generic item.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Item
    {
        [JsonProperty(Order = 31)]
        protected TypeOfItem type;
        [JsonProperty(Order = 30)]
        private int index;
        [JsonProperty(Order = 29)]
        protected string id;
        [JsonProperty(Order = 28)]
        protected string title;
        private ItemSet itemSet;
        [JsonProperty(Order = 27)]
        private bool isMandatory;
        [JsonProperty(Order = 26)]
        protected string conditionItemID;
        [JsonProperty(Order = 25)]
        protected ChoiceItem conditionItem;
        [JsonProperty(Order = 24)]
        protected int[] conditionalAnswers;

        /// <summary>
        /// Constructor for the Item class.
        /// </summary>
        public Item(TypeOfItem type, int index, string id, string title, ItemSet itemSet, bool isMandatory, string conditionItemID, ChoiceItem conditionItem, int[] conditionalAnswers)
        {
            this.type = type;
            this.index = index;
            this.id = id;
            this.title = title;
            this.itemSet = itemSet;
            this.isMandatory = isMandatory;
            this.conditionItemID = conditionItemID;
            this.conditionItem = conditionItem;
            this.conditionalAnswers = conditionalAnswers;
        }

        // Properties
        public TypeOfItem Type { get => type; set => type = value; }
        public string Title { get => title; set => title = value; }
        public int[] ConditionalAnswers { get => conditionalAnswers; set => conditionalAnswers = value; }
        public string ID { get => id; set => id = value; }
        public bool IsMandatory { get => isMandatory; set => isMandatory = value; }
        public ChoiceItem ConditionItem { get => conditionItem; set => conditionItem = value; }
        public int Index { get => index; set => index = value; }
        public ItemSet ItemSet { get => itemSet; set => itemSet = value; }
        public string ConditionItemID { get => conditionItemID; set => conditionItemID = value; }
    }

    /// <summary>
    /// Class representing a choice item with additional properties for multiple choice and layout variants.
    /// </summary>
    public class ChoiceItem : Item
    {
        [JsonProperty(Order = 23)]
        private string[] paragraphs;
        [JsonProperty(Order = 22)]
        private bool isMultipleChoice;
        [JsonProperty(Order = 21)]
        private int minSelection;
        [JsonProperty(Order = 20)]
        private int maxSelection;
        [JsonProperty(Order = 19)]
        protected string[] answerScale;
        [JsonProperty(Order = 18)]
        protected string[] answerLabels;
        [JsonProperty(Order = 17)]
        protected ChoiceItemLayoutVariant layoutVariant = ChoiceItemLayoutVariant.ListVertical;

        /// <summary>
        /// Constructor for the ChoiceItem class.
        /// </summary>
        public ChoiceItem(TypeOfItem type, int index, string id, string title, ItemSet itemSet, bool mandatory, string conditionItemID, ChoiceItem conditionItem, int[] conditionalAnswers, string[] paragraphs, bool isMultipleChoice, int minSelection, int maxSelection, string[] answerScale, string[] answerLabels, ChoiceItemLayoutVariant layoutVariant) 
            : base(type, index, id, title, itemSet, mandatory, conditionItemID, conditionItem, conditionalAnswers)
        {
            this.paragraphs = paragraphs;
            this.isMultipleChoice = isMultipleChoice;
            this.minSelection = minSelection;
            this.maxSelection = maxSelection;
            this.answerScale = answerScale;
            this.answerLabels = answerLabels;
            this.layoutVariant = layoutVariant;
        }

        // Properties
        public string[] OptionScale { get => answerScale; set => answerScale = value; }
        public string[] OptionLabels { get => answerLabels; set => answerLabels = value; }
        public ChoiceItemLayoutVariant LayoutVariant { get => layoutVariant; set => layoutVariant = value; }
        public bool IsMultipleChoice { get => isMultipleChoice; set => isMultipleChoice = value; }
        public int MinSelection { get => minSelection; set => minSelection = value; }
        public int MaxSelection { get => maxSelection; set => maxSelection = value; }
        public string[] Paragraphs { get => paragraphs; set => paragraphs = value; }
    }
}
