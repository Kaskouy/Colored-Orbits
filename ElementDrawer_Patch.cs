using HarmonyLib;
using SFS.World;
using SFS.World.Maps;
using UnityEngine;
using static SFS.World.ElementDrawer;

namespace ColoredOrbits
{
    class ElementDrawer_Patch
    {
        // This part is only there to correct a tiny bug in the original code: when a textMesh had a low priority,
        // its color was redefined as white with a different alpha factor, so the color was lost.
        [HarmonyPatch(typeof(ElementDrawer), "RegisterElement")]
        class ElementDrawer_RegisterElement_Patch
        {
            [HarmonyPrefix]
            public static bool RegisterElement(int priority, Vector2 size, Vector2 position, TextMesh textMesh, bool clearBelow, ElementDrawer __instance)
            {
                Element element = new Element
                {
                    priority = priority,
                    size = size,
                    position = position,
                    textMesh = textMesh
                };

                if (!clearBelow)
                {
                    return false;
                }
                float num = Map.view.ToConstantSize(0.01f);
                foreach (Element element2 in __instance.elements)
                {
                    Vector2 a = element.position - element2.position;
                    a = a.Rotate_Radians(0f - GameCamerasManager.main.map_Camera.CameraRotationRadians);
                    float num2 = Mathf.Abs(a.x) - element.size.x - element2.size.x;
                    if (num2 > num)
                    {
                        continue;
                    }
                    float num3 = Mathf.Abs(a.y) - element.size.y - element2.size.y;
                    if (num3 > num)
                    {
                        continue;
                    }
                    float num4 = Mathf.Max(num2, num3) / num;
                    if (element.priority > element2.priority)
                    {
                        if (element2.textMesh != null)
                        {
                            if (num4 > 0f)
                            {
                                // This is the part that is modified: the original code created a white color instead
                                element2.textMesh.color = new Color(element2.textMesh.color.r, element2.textMesh.color.g, element2.textMesh.color.b, num4);
                            }
                            else if (element2.textMesh.gameObject.activeSelf)
                            {
                                element2.textMesh.gameObject.SetActive(value: false);
                            }
                        }
                    }
                    else if (element.priority < element2.priority && element.textMesh != null)
                    {
                        if (num4 > 0f)
                        {
                            // This is the part that is modified: the original code created a white color instead
                            element.textMesh.color = new Color(element.textMesh.color.r, element.textMesh.color.g, element.textMesh.color.b, num4);
                        }
                        else if (element.textMesh.gameObject.activeSelf)
                        {
                            element.textMesh.gameObject.SetActive(value: false);
                        }
                    }
                }
                __instance.elements.Add(element);

                return false;
            }
        }
    }
}