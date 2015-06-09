using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Astrotech;

namespace TechKonstraints
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class TechKonstraints : MonoBehaviour
	{

		public TechKonstraints ()
		{
		}

		private string GetPartNameFromID(string id)
		{
			foreach (RDNode node in RDController.Instance.nodes) {
				if (node.tech != null)
				{
					foreach (AvailablePart ap in node.tech.partsAssigned) {
						if (ap.name == id) {
							return ap.title;
						}
					}
				}
			}
			return "unknown part";
		}

		private void PartPurchased(AvailablePart part)
		{
			if (HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch) 
			{
				return; // just break, there's no need to do anything here if you don't purchase parts.
			}

			Utilities.Log ("TechKonstraints", GetInstanceID (), "Part purchased: " + part.name + " (" + part.configFileFullName + ")");
			// Look for partRequired in the AvailablePart config
			ConfigNode partConfig = part.partConfig;

			if (partConfig.HasValue("requiredParts")) {
				Utilities.Log ("TechKonstraints", GetInstanceID (), "Part " + part.name + " has the following requiredParts: ");

				string requiredParts = partConfig.GetValue ("requiredParts");

				Utilities.Log ("TechKonstraints", GetInstanceID (), requiredParts);

				// OK! We've got some required parts. Now, is this a list?
				List<string> partsRequired = requiredParts.Split (',').ToList ();
				bool canResearch = true;

				Utilities.Log ("TechKonstraints", GetInstanceID (), "Size of partsRequired is " + partsRequired.Count);

				foreach (string rPart in partsRequired)
				{
					Utilities.Log ("TechKonstraints", GetInstanceID (), "Part '" + rPart.Trim() + "' is required. Searching...");

					bool partFound = false;

					foreach (RDNode node in RDController.Instance.nodes) {
						if (node.tech != null)
						{
							ProtoTechNode protoNode = ResearchAndDevelopment.Instance.GetTechState (node.tech.techID);
							if (protoNode == null) 
								continue;
							foreach (AvailablePart ap in protoNode.partsPurchased) {
								if (ap.name == rPart.Trim()) {
									Utilities.Log ("TechKonstraints", GetInstanceID (), "Part " + ap.name + " is purchased.");
									partFound = true;
									break;
								}
							}

						}

						if (partFound) {
							break; // we found it, no reason to continue searching.
						}
					}

					if (!partFound)
					{
						Utilities.Log ("TechKonstraints", GetInstanceID (), "We can't research this!");
						canResearch = false;
						ScreenMessages.PostScreenMessage ("You must purchase " + GetPartNameFromID (rPart.Trim ()) + " first!", 5, ScreenMessageStyle.UPPER_CENTER);
						break;
					}
				}

				if (!canResearch) {
					Utilities.Log ("TechKonstraints", GetInstanceID (), "Time for cleanup, removing researched part.");
					Utilities.Log ("TechKonstraints", GetInstanceID (), "Finding " + part.TechRequired);

					RDNode node = RDController.Instance.nodes.Find (n => n.tech.techID == part.TechRequired);
					if (node)
					{
						Utilities.Log ("TechKonstraints", GetInstanceID (), "In tech node " + node.name);
						ProtoTechNode protoNode = ResearchAndDevelopment.Instance.GetTechState (node.tech.techID);
						if (protoNode.partsPurchased.Contains (part))
						{
							Utilities.Log ("TechKonstraints", GetInstanceID (), "Removing part from list of purchased parts.");
							protoNode.partsPurchased.Remove (part);
							Funding.Instance.AddFunds (part.entryCost, TransactionReasons.RnDPartPurchase); // give back money!
						}
					}
				}


			}
		}

		private void Start()
		{
			Utilities.Log ("TechKonstraints", GetInstanceID (), "Start()");
			GameEvents.OnPartPurchased.Add (PartPurchased);
		}

		void OnDestroy () 
		{
			Utilities.Log ("TechKonstraints", GetInstanceID (), "OnDestroy()");
			GameEvents.OnPartPurchased.Remove (PartPurchased);
		}
	}
}

