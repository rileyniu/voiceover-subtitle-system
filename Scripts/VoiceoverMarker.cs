using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Bestiary
{
    public class VoiceoverMarker : Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] private string key = "";
        // More subtitle attributes could be declared here to pass over to VoiceoverManager, i.e. text color, text alighment etc.
        // so that we have control over individual voiceover/subtitle lines
        [Space(20)]
        [SerializeField] private bool retroactive = false;
        [SerializeField] private bool emitOnce = false;
        public PropertyName id
        {
            get
            {
                return new PropertyName();
            }
        }

        public string Key {
            get
            {
                return key;
            }
        }

        public NotificationFlags flags
        {
            get
            {
                return (retroactive ? NotificationFlags.Retroactive : default) |
                    (emitOnce ? NotificationFlags.TriggerOnce : default);
            }
        }

    }
}