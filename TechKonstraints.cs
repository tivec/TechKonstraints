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

		private void PartPurchased(AvailablePart part)
		{
			Utilities.Log ("TechKonstraints", GetInstanceID (), "Part purchased: " + part.name + " (" + part.configFileFullName + ")");
			// Look for partRequired in the AvailablePart config
			ConfigNode partConfig = part.partConfig;

			if (partConfig.HasValue("requiredPart")) {
				Utilities.Log ("TechKonstraints", GetInstanceID (), "Part " + part.name + " has the following requiredPart: ");

				string requiredParts = partConfig.GetValue ("requiredPart");

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

