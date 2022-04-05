using Jotunn;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace RaidSystem
{
    public static class MapDrawer
    {
        private static Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            { "blue", Color.blue },
            { "red", Color.red },
            { "white", Color.white },
            { "green", Color.green },
            { "cyan", Color.cyan },
            { "clear", Color.clear },
            { "grey", Color.grey },
            { "magenta", Color.magenta },
            { "black", Color.black },
            { "yellow", Color.yellow },
            { "gray", Color.gray }
        };

        public static void CreateMapDrawing(string[] teamPositions)
        {
            int radius = RaidSystem.RadiusDrawMap.Value;

            if (radius == 0) return;

            var pinOverlay = MinimapManager.Instance.GetMapDrawing("RaidSystem");

            foreach (string teamPosition in teamPositions)
            {
                if (teamPosition == "") continue;
                string team = teamPosition.Split(',')[0];

                Color color;

                if (team == "Blue")
                {
                    color = Colors[RaidSystem.TeamBlueColorOverlap.Value];
                } else
                {
                    color = Colors[RaidSystem.TeamRedColorOverlap.Value];
                }

                color.a = RaidSystem.ColorAlfa.Value;

                string x = teamPosition.Split(',')[1];
                string y = teamPosition.Split(',')[2];
                string z = teamPosition.Split(',')[3];

                var pos = MinimapManager.Instance.WorldToOverlayCoords(new Vector3(float.Parse(x), float.Parse(y), float.Parse(z)), pinOverlay.TextureSize);

                Circle(pinOverlay.MainTex, (int)pos.x, (int)pos.y, radius, color);  
                Circle(pinOverlay.HeightFilter, (int)pos.x, (int)pos.y, radius, MinimapManager.MeadowHeight);
                Circle(pinOverlay.ForestFilter, (int)pos.x, (int)pos.y, radius, MinimapManager.FilterOff);
            }

            pinOverlay.MainTex.Apply();
            pinOverlay.ForestFilter.Apply();
            pinOverlay.HeightFilter.Apply();
        }

        public static void Circle(Texture2D tex, int cx, int cy, int r, Color col)
        {
            int x, y, px, nx, py, ny, d;

            for (x = 0; x <= r; x++)
            {
                d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
                for (y = 0; y <= d; y++)
                {
                    px = cx + x;
                    nx = cx - x;
                    py = cy + y;
                    ny = cy - y;

                    tex.SetPixel(px, py, col);
                    tex.SetPixel(nx, py, col);

                    tex.SetPixel(px, ny, col);
                    tex.SetPixel(nx, ny, col);
                }
            }
        }
    }
}
