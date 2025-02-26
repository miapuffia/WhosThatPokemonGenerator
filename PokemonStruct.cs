using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemonGenerator {
	internal struct PokemonStruct(int Id, int DexNumber, string Species, string ExtraInfo, string ImageURL, int Generation, bool IsStarter, bool IsFossil, bool IsBaby, bool IsLegendary, bool IsMythical, bool IsUltraBeast, bool IsParadox, bool IsGlitch) {
		public int Id { get; } = Id;
		public int DexNumber { get; } = DexNumber;
		public string Species { get; } = Species;
		public string ExtraInfo { get; } = ExtraInfo;
		public string ImageURL { get; } = ImageURL;
		public int Generation { get; } = Generation;
		public bool IsStarter { get; } = IsStarter;
		public bool IsFossil { get; } = IsFossil;
		public bool IsBaby { get; } = IsBaby;
		public bool IsLegendary { get; } = IsLegendary;
		public bool IsMythical { get; } = IsMythical;
		public bool IsUltraBeast { get; } = IsUltraBeast;
		public bool IsParadox { get; } = IsParadox;
		public bool IsGlitch { get; } = IsGlitch;

		public override string? ToString() {
			return $"{Id}, {DexNumber}, {Species}, {ExtraInfo}, {ImageURL}, {Generation}, {IsStarter}, {IsFossil}, {IsBaby}, {IsLegendary}, {IsMythical}, {IsUltraBeast}, {IsParadox}, {IsGlitch}";
		}
	}
}
