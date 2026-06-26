using KSP.UI.Screens;
using System;
using System.Collections.Generic;
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
			GameEvents.onPartDeCoupleNewVesselComplete.Add(OnDeCoupleNewVesselComplete);
			GameEvents.onPartUndockComplete.Add(OnUndockComplete);
		}

		public void OnDestroy()
		{
			GameEvents.OnVesselRollout.Remove(RenameVessel);
			GameEvents.onPartDeCoupleNewVesselComplete.Remove(OnDeCoupleNewVesselComplete);
			GameEvents.onPartUndockComplete.Remove(OnUndockComplete);
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

		private void OnDeCoupleNewVesselComplete(Vessel oldVessel, Vessel newVessel)
		{
			if (newVessel != null)
				ApplyTemplate(newVessel);
		}

		private void OnUndockComplete(Part p)
		{
			if (p != null && p.vessel != null)
				ApplyTemplate(p.vessel);
		}

		private void ApplyTemplate(Vessel v)
		{
			foreach (var part in v.parts)
			{
				var template = part.GetComponent<LaunchNumberTemplate>();
				if (template == null || template.used || string.IsNullOrEmpty(template.templateName))
					continue;

				var parentData = template.GetResolvedData();

				string newName = ProcessTags(template.templateName, parentData, out var resolvedData) ?? template.templateName;

				v.vesselName = newName;
				template.used = true;

				if (resolvedData != null && resolvedData.Count > 0)
					template.SetResolvedData(resolvedData);

				ScreenMessages.PostScreenMessage("New vessel: " + newName, MessageDisplayLength, ScreenMessageStyle.UPPER_CENTER);
				break;
			}
		}

		public void RenameVessel(ShipConstruct sc)
		{
			Vessel v = FlightGlobals.ActiveVessel;

			string vname = v.vesselName;
			if (vname.Contains("#"))
				vname = Regex.Replace(vname, "#.*$", "");

			string newName = ProcessTags(vname, null, out var resolvedData);
			if (newName == null)
				return;

			v.vesselName = newName;

			if (resolvedData != null && resolvedData.Count > 0)
			{
				foreach (var part in v.parts)
				{
					var t = part.GetComponent<LaunchNumberTemplate>();
					if (t != null)
						t.SetResolvedData(resolvedData);
				}
			}

			ScreenMessages.PostScreenMessage("Launch: " + newName, MessageDisplayLength, ScreenMessageStyle.UPPER_CENTER);
		}

		private string ProcessTags(string input, Dictionary<string, int> parentData, out Dictionary<string, int> resolvedData)
		{
			string pattern = @"\[(\w*)\]";
			MatchCollection matches = Regex.Matches(input, pattern);
			if (matches.Count == 0)
			{
				resolvedData = null;
				return null;
			}

			var tagToNumber = new Dictionary<string, int>();

			foreach (Match m in matches)
			{
				string tag = m.Groups[1].Value;
				if (tag == "")
					tag = input;

				if (tagToNumber.ContainsKey(tag))
					continue;

				int number;
				if (parentData == null || !parentData.TryGetValue(tag, out number))
				{
					if (_numbering.ContainsKey(tag))
					{
						number = _numbering[tag] + 1;
						_numbering[tag] = number;
					}
					else
					{
						_numbering.Add(tag, 1);
						number = 1;
					}
				}
				tagToNumber[tag] = number;
			}

			string result = input;
			foreach (var kvp in tagToNumber)
				result = Regex.Replace(result, @"\[" + kvp.Key + @"\]", kvp.Value.ToString());

			resolvedData = tagToNumber;
			return result;
		}
	}
}
