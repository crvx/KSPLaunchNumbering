using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LaunchNumbering
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT })]
	public class LaunchNumberer : ScenarioModule
	{
		public static LaunchNumberer Instance;

		internal Dictionary<string, int> Numbering { get { return _numbering; } }

		public override void OnAwake()
		{
			Instance = this;
			_numbering = new Dictionary<string, int>();
			GameEvents.OnVesselRollout.Add(RenameVessel);
		}

		public void OnDestroy()
		{
			GameEvents.OnVesselRollout.Remove(RenameVessel);
		}

		private const string TopLevelNodeLabel = "LAUNCHNUMBERS";
		private const string SeriesNodeLabel = "VESSELS";
		private const string VesselNameLabel = "vessel-name";
		private const string VesselCountLabel = "vessel-number";
		private const float MessageDisplayLength = 5.0f;

		public override void OnLoad(ConfigNode node)
		{
			_numbering = new Dictionary<string, int>();
			foreach (var serNode in node.GetNodes(SeriesNodeLabel)) {
				_numbering.Add(serNode.GetValue(VesselNameLabel), int.Parse(serNode.GetValue(VesselCountLabel)));
			}
		}

		public override void OnSave(ConfigNode node)
		{
			node.ClearNodes();
			foreach (var series in _numbering) {
				var serNode = new ConfigNode(SeriesNodeLabel);
				serNode.AddValue(VesselNameLabel, series.Key);
				serNode.AddValue(VesselCountLabel, series.Value);
				node.AddNode(serNode);
			}
		}

		private Dictionary<string, int> _numbering;

		internal void SetVesselNumber(string key, int number)
		{
			if (_numbering.ContainsKey(key))
				_numbering[key] = number;
		}

		internal void DeleteVessel(string key)
		{
			if (_numbering.ContainsKey(key))
				_numbering.Remove(key);
		}

		public void RenameVessel(ShipConstruct sc)
		{
			Vessel v = FlightGlobals.ActiveVessel;
			var vesselNumber = 1;

			string vname = v.vesselName;
			if (vname.Contains("#"))
				vname = Regex.Replace(vname, "#.*$", "");

			string pattern = "\\[(\\w*)\\]";
			Match m = Regex.Match(vname, pattern);
			if (!m.Success)
				return;

			string key = m.Groups[1].ToString();
			if (key == "")
				key = vname;

			if (_numbering.ContainsKey(key)) {
				vesselNumber = _numbering[key] + 1;
				_numbering[key] = vesselNumber;
			} else {
				_numbering.Add(key, 1);
			}

			string newName = Regex.Replace(vname, pattern, vesselNumber.ToString());
			v.vesselName = newName;
			ScreenMessages.PostScreenMessage("Launch: " + newName, MessageDisplayLength, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}
