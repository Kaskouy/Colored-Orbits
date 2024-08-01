using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ColoredOrbits
{
    class ColorRefiner
    {
        private const float C_MIN_COLOR_COMPONENT = 0.75f;

        public static Color getPlanetLineColor(Color originalColor)
        {
            Color newColor = new Color();

            // Make the color as bright as possible
            // ------------------------------------
            float maxColorComponent = originalColor.maxColorComponent;

            if (maxColorComponent < 0.01f)
            {
                // Original color is black -> make it white
                newColor.r = 1.0f;
                newColor.g = 1.0f;
                newColor.b = 1.0f;
                newColor.a = 1.0f;
            }
            else
            {
                // Scale color components so that highest is 1.0
                newColor.r = originalColor.r / maxColorComponent;
                newColor.g = originalColor.g / maxColorComponent;
                newColor.b = originalColor.b / maxColorComponent;
                newColor.a = 1.0f;
            }


            // If it's "too white", increase the contrast
            float minColorComponent = Math.Min(Math.Min(newColor.r, newColor.g), newColor.b);

            if (minColorComponent > C_MIN_COLOR_COMPONENT)
            {
                if (minColorComponent > 0.99f)
                {
                    // color is pure white, make it a little blue-greyish
                    newColor.r = C_MIN_COLOR_COMPONENT;
                    newColor.g = C_MIN_COLOR_COMPONENT;
                    newColor.b = 1.0f;
                }
                else
                {
                    // increase the contrast
                    newColor.r = 1.0f - (1.0f - C_MIN_COLOR_COMPONENT) * (1.0f - newColor.r) / (1.0f - minColorComponent);
                    newColor.g = 1.0f - (1.0f - C_MIN_COLOR_COMPONENT) * (1.0f - newColor.g) / (1.0f - minColorComponent);
                    newColor.b = 1.0f - (1.0f - C_MIN_COLOR_COMPONENT) * (1.0f - newColor.b) / (1.0f - minColorComponent);
                }
            }

            return newColor;
        }
    }
}