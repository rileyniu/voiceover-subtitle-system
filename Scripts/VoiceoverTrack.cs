using UnityEngine.Timeline;

namespace Bestiary
{
    /// <summary>
    /// Voiceover Track is a custom track inherited from MarkerTrack for signals fired to control vocieover/subtitles.
    /// </summary>
    [TrackBindingType(typeof(VoiceoverReceiver))]
    [TrackColor(255f / 255f, 140f / 255f, 0f / 255f)]
    public class VoiceoverTrack : MarkerTrack
    {
    }
}

