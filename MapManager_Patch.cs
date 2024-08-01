using System;
using System.Collections.Generic;

using UnityEngine;
using SFS;
using SFS.World;
using SFS.WorldBase;
using SFS.World.Maps;
using HarmonyLib;


namespace ColoredOrbits
{
	[HarmonyPatch(typeof(LineDrawer), "Start")]
	class LineDrawer_Patch
	{
		[HarmonyPostfix]
		public static void Start(LineDrawer __instance)
		{
			if (__instance == MapManager_DrawTrajectories_Patch._lineDrawer_Bold)
			{
				//UnityEngine.Debug.Log("Start postfix called for custom LineDrawer");
				MapManager_DrawTrajectories_Patch._instantiated = true;
			}
		}
	}


	[HarmonyPatch(typeof(MapManager), "DrawReset")]
	class MapManager_DrawReset_Patch
	{
		[HarmonyPostfix]
		public static void DrawReset_Postfix()
		{
			MapManager_DrawTrajectories_Patch.ResetLineDrawers();
		}
	}


	[HarmonyPatch(typeof(MapManager), "DrawTrajectories")]
	class MapManager_DrawTrajectories_Patch
	{
		public static bool _instantiated = false;
		public static LineDrawer _lineDrawer_Bold = null;


		// Called when a world session is opened
		public static void InstantiateObjects()
		{
			//UnityEngine.Debug.Log("Instantiation of custom LineDrawers");
			GameObject gameObject = new GameObject("OrbitColored_lineDrawerBold", typeof(LineDrawer));
			_lineDrawer_Bold = gameObject.GetComponent<LineDrawer>();
			_lineDrawer_Bold.linePrefab = Map.solidLine.linePrefab;
			_lineDrawer_Bold.lineTextureMode = Map.solidLine.lineTextureMode;
			_lineDrawer_Bold.lineWidth = Map.solidLine.lineWidth * 1.5f;
		}

		// Called when a world session is closed
		public static void DesallocateResources()
		{
			_instantiated = false;
		}

		public static void ResetLineDrawers()
		{
			if (_instantiated)
			{
				_lineDrawer_Bold.pool.Reset();
			}
		}


		public static void DrawOrbit_Custom(Orbit orbit, Color c, bool drawStats, bool drawStartText, bool drawEndText, LineDrawer lineDrawer)
		{
			//Traverse.Create<TrajectoryDrawer>().Method("DrawOrbit", orbit, c, drawStats, drawStartText, drawEndText, lineDrawer)
			//Traverse.CreateWithType("TrajectoryDrawer").Method("DrawOrbit", orbit, c, drawStats, drawStartText, drawEndText, lineDrawer).GetValue(orbit, c, drawStats, drawStartText, drawEndText, lineDrawer);
			Traverse.Create(typeof(TrajectoryDrawer)).Method("DrawOrbit", orbit, c, drawStats, drawStartText, drawEndText, lineDrawer).GetValue();
		}


		// This function draws all orbits when it's called.
		// It mimics the original method, but uses a custom line renderer for planet orbits
		[HarmonyPrefix]
		public static bool DrawTrajectories()
		{
			if (!_instantiated)
			{
				return false;
			}

			Dictionary<SelectableObject, Action> dictionary = new Dictionary<SelectableObject, Action>();
			foreach (Planet planet in Base.planetLoader.planets.Values)
			{
				if (planet.HasParent)
				{
					dictionary[planet.mapPlanet] = delegate
					{
						foreach (I_Path path in planet.trajectory.paths)
						{
							if (path is Orbit orbit)
							{
								float alpha = (planet.data.basics.significant ? 1f : Mathf.Lerp(0.3f, 1f, MapDrawer.GetFadeOut(Map.view.view.distance, planet.Radius * 30000.0, planet.Radius * 30000.0 * 1.5)));
								Color lineColor = ColorRefiner.getPlanetLineColor(planet.data.basics.mapColor);
								lineColor.a = alpha;
								DrawOrbit_Custom(orbit, lineColor, false, false, false, _lineDrawer_Bold);
							}
						}
					};
				}
			}
			if (Map.view.view.target.Value is MapPlayer) // ship focused on (through Focus command or "timewarp to" event)
			{
				dictionary[Map.view.view.target.Value] = delegate
				{
					Map.view.view.target.Value.Trajectory.DrawSolid(drawStats: true, drawStartAndEndText: true);
				};
			}
			if ((Map.navigation.target != null) && !(Map.navigation.target is MapPlanet)) // object targetted for navigation
			{
				dictionary[Map.navigation.target] = delegate
				{
					Map.navigation.target.Trajectory.DrawSolid(drawStats: false, drawStartAndEndText: false);
				};
			}
			if (GameSelector.main.selected_Map.Value is MapPlayer) // object under selection
			{
				dictionary[GameSelector.main.selected_Map.Value] = delegate
				{
					GameSelector.main.selected_Map.Value.Trajectory.DrawSolid(drawStats: true, drawStartAndEndText: true);
				};
			}
			if (PlayerController.main.player.Value != null && PlayerController.main.player.Value.mapPlayer != null) // Object under player control
			{
				dictionary[PlayerController.main.player.Value.mapPlayer] = delegate
				{
					PlayerController.main.player.Value.mapPlayer.Trajectory.DrawSolid(drawStats: true, drawStartAndEndText: true);
				};
			}
			foreach (Action value in dictionary.Values)
			{
				value();
			}

			// Don't execute the original method
			return false;
		}
	}


	[HarmonyPatch(typeof(MapManager), "DrawPlanetName")]
	class MapManager_DrawPlanetName_Patch
	{
		// This patch allows to draw the planet name in the same color as the planet color.
		// --------------------------------------------------------------------------------
		[HarmonyPrefix]
		public static bool DrawPlanetName(Vector2 normal, Planet planet, bool selected, bool target)
		{
			Color nameColor = ColorRefiner.getPlanetLineColor(planet.data.basics.mapColor);

			if (selected)
			{
				Map.elementDrawer.DrawTextElement("    ", normal, 40, nameColor, planet.mapHolder.position, 0, clearBelow: true, 2);
				return false;
			}
			float num = MapDrawer.GetFadeIn(Map.view.view.distance, planet.Radius * 200.0, planet.Radius * 200.0 * 1.3);
			int priority = ((!target) ? (planet.orbitalDepth * -100 - 100 + planet.satelliteIndex) : 0);
			if (!planet.data.basics.significant && !target)
			{
				num = Mathf.Min(MapDrawer.GetFadeOut(Map.view.view.distance, planet.Radius * 10000.0, planet.Radius * 10000.0 * 1.3), num);
			}
			nameColor.a = num;
			Map.elementDrawer.DrawTextElement(planet.DisplayName, normal, 40, nameColor, planet.mapHolder.position, priority, clearBelow: true, 2);

			return false;
		}
	}
}

