using UnityEngine;
using UnityEngine.Playables;

namespace Bestiary
{
    public class VoiceoverReceiver : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] private VoiceoverManager voiceoverManager;

        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is VoiceoverMarker voiceoverMarker && voiceoverManager != null)
            {
                // pass the audio file name as the key to VoiceoverManager
                voiceoverManager.OnVoiceoverContentChanged(voiceoverMarker.Key);
            }
        }
    }
}