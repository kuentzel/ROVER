using Newtonsoft.Json;

namespace ROVER
{
    /// <summary>
    /// Represents a study with various sections, and settings for progress bar and backsteps.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Study
    {
        [JsonProperty(Order = 1)]
        private string id;

        [JsonProperty(Order = 2)]
        private string title;

        [JsonProperty(Order = 3)]
        private bool showProgressBar;

        [JsonProperty(Order = 4)]
        private bool allowBacksteps;

        [JsonProperty(Order = 5)]
        private StudySection[] sections;

        /// <summary>
        /// Initializes a new instance of the Study class.
        /// </summary>
        /// <param name="id">The ID of the study.</param>
        /// <param name="title">The title of the study.</param>
        /// <param name="showProgressBar">Whether to show the progress bar.</param>
        /// <param name="allowBacksteps">Whether backsteps are allowed.</param>
        /// <param name="sections">The sections of the study.</param>
        public Study(string id, string title, bool showProgressBar, bool allowBacksteps, StudySection[] sections)
        {
            this.id = id;
            this.title = title;
            this.showProgressBar = showProgressBar;
            this.allowBacksteps = allowBacksteps;
            this.sections = sections;
        }

        // Properties
        public string ID => id;
        public string Title { get => title; set => title = value; }
        public bool ShowProgressBar { get => showProgressBar; set => showProgressBar = value; }
        public bool AllowBacksteps { get => allowBacksteps; set => allowBacksteps = value; }
        public StudySection[] Sections { get => sections; set => sections = value; }
    }
}
