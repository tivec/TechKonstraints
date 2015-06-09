using System;
using UnityEngine;

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
			// Look for partRequired in the AvailablePart config

		}

		private void Start()
		{
			GameEvents.OnPartPurchased.Add (PartPurchased);
		}

		void OnDestroy () 
		{
			GameEvents.OnPartPurchased.Remove (PartPurchased);
		}
	}
}

