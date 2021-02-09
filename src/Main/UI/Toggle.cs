using Modding;
using UnityEngine;

namespace spaar.ModLoader.UI
{
    public class Toggle
    {
        public GUIStyle Default { get; set; }

        internal Toggle()
        {
            Default = new GUIStyle()
            {
                normal = {
          background = ModResource.GetTexture("toggle-normal.png"),
        },
                onNormal = {
          background = ModResource.GetTexture("toggle-on-normal.png"),
        },
                hover = {
          background = ModResource.GetTexture("toggle-hover.png"),
        },
                onHover = {
          background = ModResource.GetTexture("toggle-on-hover.png"),
        },
                active = {
          background = ModResource.GetTexture("toggle-active.png"),
        },
                onActive = {
          background = ModResource.GetTexture("toggle-on-active.png"),
        },
                margin = { right = 10 }
            };
        }
    }
}
