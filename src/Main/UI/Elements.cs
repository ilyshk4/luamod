using LuaScripting;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace spaar.ModLoader.UI
{
    /// <summary>
    /// Container for GUIStyle elements.
    /// </summary>
    public static class Elements
    {
        private static Settings _settings;
        public static Settings Settings { get { return _settings ?? (_settings = new Settings()); } }

        public static bool IsInitialized { get; private set; }

        public static Colors Colors { get; private set; }
        public static Windows Windows { get; private set; }
        public static Labels Labels { get; private set; }
        public static Buttons Buttons { get; private set; }
        public static Tools Tools { get; private set; }
        public static InputFields InputFields { get; private set; }
        public static Scrollview Scrollview { get; private set; }
        public static Sliders Sliders { get; private set; }
        public static Toggle Toggle { get; private set; }

        /// <summary>
        /// Rebuilds all elements to make them match the settings in Elements.Settings.
        /// <para>This will also call VGUI.Instance.RebuildSkin().</para>
        /// </summary>
        public static void RebuildElements()
        {
            IsInitialized = true;

            Colors = new Colors();
            Labels = new Labels();
            Windows = new Windows();
            Buttons = new Buttons();
            Tools = new Tools();
            InputFields = new InputFields();
            Scrollview = new Scrollview();
            Sliders = new Sliders();
            Toggle = new Toggle();

            Mod.Skin = ScriptableObject.CreateInstance<GUISkin>();

            Mod.Skin.window = Elements.Windows.Default;
            Mod.Skin.label = Elements.Labels.Default;
            Mod.Skin.button = Elements.Buttons.Default;
            Mod.Skin.textField = Mod.Skin.textArea = Elements.InputFields.Default;
            Mod.Skin.horizontalScrollbar = Elements.Scrollview.Horizontal;
            Mod.Skin.verticalScrollbar = Elements.Scrollview.Vertical;
            Mod.Skin.verticalScrollbarThumb = Elements.Scrollview.ThumbVertical;
            Mod.Skin.horizontalScrollbar = Elements.Scrollview.Horizontal;
            Mod.Skin.horizontalScrollbarThumb = Elements.Scrollview.ThumbHorizontal;
            Mod.Skin.scrollView = Elements.Windows.ClearDark;
            Mod.Skin.horizontalSlider = Elements.Sliders.Horizontal;
            Mod.Skin.horizontalSliderThumb = Elements.Sliders.ThumbHorizontal;
            Mod.Skin.verticalSlider = Elements.Sliders.Vertical;
            Mod.Skin.verticalSliderThumb = Elements.Sliders.ThumbVertical;
            Mod.Skin.toggle = Elements.Toggle.Default;
        }
    }
}