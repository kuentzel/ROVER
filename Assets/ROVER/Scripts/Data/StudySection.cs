using Newtonsoft.Json;

namespace ROVER
{
    /// <summary>
    /// Represents a section within a study, containing multiple study elements.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudySection
    {
        [JsonProperty(Order = 1)]
        private int index;

        [JsonProperty(Order = 2)]
        private string title;

        [JsonProperty(Order = 3)]
        private StudyElement[] elements;

        private Study study;

        /// <summary>
        /// Initializes a new instance of the StudySection class.
        /// </summary>
        /// <param name="index">The index of the section within the study.</param>
        /// <param name="title">The title of the section.</param>
        /// <param name="study">The study this section belongs to.</param>
        /// <param name="elements">The elements contained within this section.</param>
        public StudySection(int index, string title, Study study, StudyElement[] elements)
        {
            this.index = index;
            this.title = title;
            this.study = study;
            this.elements = elements;
        }

        // Properties
        public int Index 
        { 
            get => index; 
            set => index = value; 
        }

        public string Title 
        { 
            get => title; 
            set => title = value; 
        }

        public StudyElement[] Elements 
        { 
            get => elements; 
            set => elements = value; 
        }

        public Study Study 
        { 
            get => study; 
            set => study = value; 
        }
    }
}
