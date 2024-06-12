using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ROVER
{
    public enum TypeOfFeedbackTrigger
    {
        HeartRateTrigger,
        TimeTrigger,
        ActivityTrigger,
        ManualTrigger
    }

    public class FeedbackTrigger
    {
        int index;
        string label;
        string title;
        string description;
        TypeOfFeedbackTrigger type;
    }

    public class HeartRateTrigger : FeedbackTrigger
    {
        int heartRate;
        int buffer;
    }

    public class TimeTrigger : FeedbackTrigger
    {
        int time;
        bool repeating;
        int period;
    }

    public class ActivityTrigger : FeedbackTrigger
    {
        string triggeringActivity;
    }

    public class ManualTrigger : FeedbackTrigger
    {

    }
}