using System;
using UnityEngine;
using UnityEngine.Video;

namespace TCG.Cutscene
{
    /// <summary>
    /// One unit of content in a <see cref="CutsceneData"/> sequence.
    ///
    /// A beat's <see cref="type"/> determines which fields are active.
    /// Unused fields are ignored at runtime, so it is safe to leave them at default.
    ///
    /// <list type="table">
    ///   <listheader><term>Type</term><description>Active fields</description></listheader>
    ///   <item><term>Dialogue</term><description>character, text, waitForTap, autoAdvanceDelay</description></item>
    ///   <item><term>Image</term>   <description>backgroundSprite, spriteTint, waitForTap, autoAdvanceDelay</description></item>
    ///   <item><term>Video</term>   <description>videoClip, loopVideo, allowSkipVideo</description></item>
    ///   <item><term>Wait</term>    <description>duration</description></item>
    ///   <item><term>FadeIn</term>  <description>duration</description></item>
    ///   <item><term>FadeOut</term> <description>duration</description></item>
    /// </list>
    ///
    /// The <see cref="sfx"/> field is honoured for every beat type.
    /// </summary>
    [Serializable]
    public class CutsceneBeat
    {
        [Tooltip("What this beat does.")]
        public CutsceneBeatType type = CutsceneBeatType.Dialogue;

        // ── Dialogue ──────────────────────────────────────────────────────────────

        [Tooltip("Speaking character. Leave null for a narrator / caption line.")]
        public CharacterProfile character;

        [TextArea(2, 8)]
        [Tooltip("Text shown with the typewriter effect.")]
        public string text;

        [Tooltip("If true, wait for the player to tap before advancing. " +
                 "If false, auto-advance after autoAdvanceDelay seconds (Dialogue & Image only).")]
        public bool waitForTap = true;

        [Tooltip("Seconds before auto-advance when waitForTap is false.")]
        public float autoAdvanceDelay = 2f;

        // ── Image ─────────────────────────────────────────────────────────────────

        [Tooltip("Sprite to display as the full-screen background. " +
                 "If null the previous background stays visible.")]
        public Sprite backgroundSprite;

        [Tooltip("Tint applied to the background image.")]
        public Color  spriteTint = Color.white;

        // ── Video ─────────────────────────────────────────────────────────────────

        [Tooltip("VideoClip to play. Requires a VideoPlayer + RawImage in the scene.")]
        public VideoClip videoClip;

        [Tooltip("Loop the video instead of advancing when it ends.")]
        public bool loopVideo = false;

        [Tooltip("Whether the player can skip the video with a button.")]
        public bool allowSkipVideo = true;

        // ── Wait / Fade ───────────────────────────────────────────────────────────

        [Tooltip("Duration in seconds used by Wait, FadeIn and FadeOut beats.")]
        public float duration = 1f;

        // ── Shared ────────────────────────────────────────────────────────────────

        [Tooltip("One-shot sound effect played when this beat begins. Leave null for silence.")]
        public AudioClip sfx;
    }
}
