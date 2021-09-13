namespace RCC.Network
{
	public class CCharacterSummary
    {
		public CCharacterSummary()
		{
			Mainland = 0;
			Name = string.Empty;
			Location = 0;
			VisualPropA = 0;
			VisualPropB = 0;
			VisualPropC = 0;
			People = 142;
			Title = 238;
			CharacterSlot = 255;
			InRingSession = false;
			HasEditSession = false;
			InNewbieland = false;
		}

		/// mainland
		uint Mainland;

		/// name
		public string Name;

		/// Localisation
		uint Location;

		/// visual property for appearance
		long VisualPropA;
		long VisualPropB;
		long VisualPropC;

		int People;

		int SheetId;

		int Title;

		byte CharacterSlot;

		bool InRingSession;
		bool HasEditSession;
		bool InNewbieland;

		/// serialisation coming from a stream (net message)
		public void serial(CBitMemStream f)
		{
			f.serialVersion(0);
			f.serial(ref Mainland);
			f.serial(ref Name);
			f.serial(ref People);
			f.serial(ref Location);
			f.serial(ref VisualPropA);
			f.serial(ref VisualPropB);
			f.serial(ref VisualPropC);
			f.serial(ref SheetId);
			f.serial(ref Title);
			f.serial(ref CharacterSlot);
			f.serial(ref InRingSession);
			f.serial(ref HasEditSession);

			bool serialNB = true; // TMP TMP, for test
			if (serialNB)
			{
				f.serial(ref InNewbieland);
			}
		}
	}
}