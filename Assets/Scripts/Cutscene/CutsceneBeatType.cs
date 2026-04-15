namespace TCG.Cutscene
{
    /// <summary>
    /// Determines what a <see cref="CutsceneBeat"/> displays or does.
    /// </summary>
    public enum CutsceneBeatType
    {
        /// <summary>Show a character portrait + name + text with typewriter animation. Waits for player tap.</summary>
        Dialogue  = 0,

        /// <summary>Replace or reveal a full-screen background image. Auto-advances or waits for tap.</summary>
        Image     = 1,

        /// <summary>Play a VideoClip via Unity VideoPlayer. Auto-advances when the clip ends.</summary>
        Video     = 2,

        /// <summary>Invisible pause for a fixed number of seconds, then auto-advance.</summary>
        Wait      = 3,

        /// <summary>Animate the fade overlay from opaque black to transparent (reveal scene).</summary>
        FadeIn    = 4,

        /// <summary>Animate the fade overlay from transparent to opaque black (conceal scene).</summary>
        FadeOut   = 5,
    }
}
