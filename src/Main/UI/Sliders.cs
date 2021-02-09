using Modding;
using UnityEngine;

namespace spaar.ModLoader.UI
{
    public class Sliders
    {
        public GUIStyle Horizontal { get; set; }
        public GUIStyle Vertical { get; set; }

        public GUIStyle ThumbHorizontal { get; set; }
        public GUIStyle ThumbVertical { get; set; }

        internal Sliders()
        {
            Horizontal = new GUIStyle(GUI.skin.horizontalSlider)
            {
                normal = { background = ModResource.GetTexture("blue-normal.png") },
            };
            Vertical = new GUIStyle(GUI.skin.verticalSlider)
            {
                normal = { background = ModResource.GetTexture("blue-normal.png") },
            };
            ThumbHorizontal = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                normal = { background = ModResource.GetTexture("slider-thumb.png") },
                hover = { background = ModResource.GetTexture("slider-thumb-hover.png") },
                active = { background = ModResource.GetTexture("slider-thumb-active.png") }
            };
            ThumbVertical = new GUIStyle(GUI.skin.verticalSliderThumb)
            {
                normal = { background = ModResource.GetTexture("slider-thumb.png") },
                hover = { background = ModResource.GetTexture("slider-thumb-hover.png") },
                active = { background = ModResource.GetTexture("slider-thumb-active.png") }
            };
        }
    }
}
