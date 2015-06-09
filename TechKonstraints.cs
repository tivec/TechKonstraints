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

			// Look for partRequired in the AvailablePart config
			ConfigNode partConfig = part.partConfig;

			if (partConfig.HasValue("requiredParts")) {
				string requiredParts = partConfig.GetValue ("requiredParts");

				// OK! We've got some required parts. Now, is this a list?
				List<string> partsRequired = requiredParts.Split (',').ToList ();
				bool canResearch = true;

				foreach (string rPart in partsRequired)
				{
					bool partFound = false;

					foreach (RDNode node in RDController.Instance.nodes) {
						if (node.tech != null)
						{
							ProtoTechNode protoNode = ResearchAndDevelopment.Instance.GetTechState (node.tech.techID);
							if (protoNode == null) 
								continue;
							foreach (AvailablePart ap in protoNode.partsPurchased) {
								if (ap.name == rPart.Trim()) {
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
						canResearch = false;
						ScreenMessages.PostScreenMessage ("You must purchase " + GetPartNameFromID (rPart.Trim ()) + " first!", 5, ScreenMessageStyle.UPPER_CENTER);
						break;
					}
				}

				if (!canResearch) {
					RDNode node = RDController.Instance.nodes.Find (n => n.tech.techID == part.TechRequired);
					if (node)
					{
						ProtoTechNode protoNode = ResearchAndDevelopment.Instance.GetTechState (node.tech.techID);
						if (protoNode.partsPurchased.Contains (part))
						{
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

