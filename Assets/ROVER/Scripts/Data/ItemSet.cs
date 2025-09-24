using Newtonsoft.Json;

namespace ROVER
{
    /// <summary>
    /// Represents a set of items within a study, with options to hint, allow backsteps, and randomize items.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemSet : StudyElement
    {
        [JsonProperty(Order = 7)]
        private string hint;

        [JsonProperty(Order = 8)]
        private bool allowBacksteps;

        [JsonProperty(Order = 9)]
        private bool randomizeItems;

        [JsonProperty(Order = 10)]
        private Item[] items;

        /// <summary>
        /// Initializes a new instance of the ItemSet class.
        /// </summary>
        /// <param name="type">The type of study element.</param>
        /// <param name="index">The index of the item set.</param>
        /// <param name="title">The title of the item set.</param>
        /// <param name="section">The study section the item set belongs to.</param>
        /// <param name="conditionItemID">The ID of the condition item.</param>
        /// <param name="conditionItem">The condition item.</param>
        /// <param name="conditionalAnswer">The conditional answers.</param>
        /// <param name="hint">The hint for the item set.</param>
        /// <param name="allowBacksteps">Whether backsteps are allowed.</param>
        /// <param name="randomizeItems">Whether items should be randomized.</param>
        /// <param name="items">The array of items in the item set.</param>
        public ItemSet(TypeOfStudyElement type, int index, string title, StudySection section, string conditionItemID, ChoiceItem conditionItem, int[] conditionalAnswer, string hint, bool allowBacksteps, bool randomizeItems, Item[] items)
            : base(type, index, title, section, conditionItemID, conditionItem, conditionalAnswer)
        {
            this.hint = hint;
            this.allowBacksteps = allowBacksteps;
            this.randomizeItems = randomizeItems;
            this.items = items;
        }

        // Properties
        public string Hint
        {
            get => hint;
            set => hint = value;
        }

        public bool AllowBacksteps
        {
            get => allowBacksteps;
            set => allowBacksteps = value;
        }

        public bool RandomizeItems
        {
            get => randomizeItems;
            set => randomizeItems = value;
        }

        public Item[] Items
        {
            get => items;
            set => items = value;
        }
    }
}
