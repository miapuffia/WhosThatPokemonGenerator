using nietras.SeparatedValues;

namespace WhosThatPokemonGenerator {
	internal class Program {
		static List<PokemonStruct> pokemonData = [];
		static Random random = new Random();

		static void Main(string[] args) {
			Console.OutputEncoding = System.Text.Encoding.Unicode;

			Console.WriteLine("Using PokemonData.csv as input");
			Console.WriteLine("Outputting to WhosThatPokenon.csv and WhosThatGlitchPokemon.csv");
			Console.WriteLine();

			using var rawPokemonData = Sep.Reader().FromFile("PokemonData.csv");

			foreach(var row in rawPokemonData) {
				if(string.IsNullOrEmpty(row["Species"].ToString()))
					continue;

				int dexNumber = row["DexNumber"].Parse<int>();

				//Replace parentheses with unicode versions to pass Trivia Tricks validation
				string species = row["Species"].ToString().Replace('(', '❨').Replace(')', '❩');

				string imageURL = row["Image"].ToString();

				if(string.IsNullOrEmpty(imageURL))
					imageURL = "https://www.serebii.net/pokemon/art/" + (dexNumber + "").PadLeft(3, '0') + ".png";

				pokemonData.Add(
					new PokemonStruct(
						row["Id"].Parse<int>(),
						dexNumber,
						species,
						row["ExtraInfo"].ToString(),
						imageURL,
						row["Generation"].Parse<int>(),
						row["Starter"].ToString() == "TRUE",
						row["Fossil"].ToString() == "TRUE",
						row["Baby"].ToString() == "TRUE",
						row["Legendary"].ToString() == "TRUE",
						row["Mythical"].ToString() == "TRUE",
						row["UltraBeast"].ToString() == "TRUE",
						row["Paradox"].ToString() == "TRUE",
						row["Glitch"].ToString() == "TRUE"
					)
				);
			}

			List<QuestionStruct> glitchQuestions = [];
			List<QuestionStruct> normalQuestions = [];

			int i = 1;

			foreach(var pokemon in pokemonData) {
				ClearCurrentConsoleLine();
				Console.Write($"Calculating questions... {i}/{pokemonData.Count}");

				string question = (pokemon.IsGlitch ? "Who's That Glitch Pokemon?" : "Who's That Pokemon?");

				if(!string.IsNullOrEmpty(pokemon.ExtraInfo))
					question += " (" + pokemon.ExtraInfo + ")";

				string[] answers = GetAnswers(pokemon);

				if(pokemon.IsGlitch)
					glitchQuestions.Add(new QuestionStruct(pokemon.Id, question, answers[0], answers[1], answers[2], answers[3], false, 'A', "miapuffia", pokemon.ImageURL));
				else
					normalQuestions.Add(new QuestionStruct(pokemon.Id, question, answers[0], answers[1], answers[2], answers[3], false, 'A', "miapuffia", pokemon.ImageURL));

				i++;
			}

			Console.WriteLine();
			Console.WriteLine();

			using var glitchQuestionWriter = rawPokemonData.Spec.Writer().ToFile("WhosThatGlitchPokemon.csv");

			glitchQuestionWriter.Header.Add("uniqueId", "question", "optionA", "optionB", "optionC", "optionD", "fixedOrder", "correctAnswer", "credit", "imageURL", "answerExplanation");

			foreach(var question in glitchQuestions) {
				var row = glitchQuestionWriter.NewRow();
				row["uniqueId"].Set(question.Id + "");
				row["question"].Set(question.Question);
				row["optionA"].Set(question.OptionA);
				row["optionB"].Set(question.OptionB);
				row["optionC"].Set(question.OptionC);
				row["optionD"].Set(question.OptionD);
				row["fixedOrder"].Set(question.FixedOrder ? "TRUE" : "FALSE");
				row["correctAnswer"].Set(question.CorrectAnswer + "");
				row["credit"].Set(question.Credit);
				row["imageURL"].Set(question.ImageURL);
				row["answerExplanation"].Set(question.AnswerExplanation);
				row.Dispose();
			}

			glitchQuestionWriter.Flush();

			using var normalQuestionWriter = rawPokemonData.Spec.Writer().ToFile("WhosThatPokemon.csv");

			normalQuestionWriter.Header.Add("uniqueId", "question", "optionA", "optionB", "optionC", "optionD", "fixedOrder", "correctAnswer", "credit", "imageURL", "answerExplanation");

			foreach(var question in normalQuestions) {
				var row = normalQuestionWriter.NewRow();
				row["uniqueId"].Set(question.Id + "");
				row["question"].Set(question.Question);
				row["optionA"].Set(question.OptionA);
				row["optionB"].Set(question.OptionB);
				row["optionC"].Set(question.OptionC);
				row["optionD"].Set(question.OptionD);
				row["fixedOrder"].Set(question.FixedOrder ? "TRUE" : "FALSE");
				row["correctAnswer"].Set(question.CorrectAnswer + "");
				row["credit"].Set(question.Credit);
				row["imageURL"].Set(question.ImageURL);
				row["answerExplanation"].Set(question.AnswerExplanation);
				row.Dispose();
			}

			normalQuestionWriter.Flush();

			Console.WriteLine("Done");
		}

		public static string[] GetAnswers(PokemonStruct answerPokemon) {
			Dictionary<PokemonStruct, double> weightedPokemonData = GetWeightedPokemonData();

			double lengthMaxWeight = 0.3;
			double lengthDifferenceWeight = 0.1;
			double firstLetterWeight = 0.2;
			double categoryWeight = 0.3;
			double glitchCategoryWeight = 10;
			double generationWeight = 0.1;
			double specialCharactersWeight = 0.2;
			double randomMaxWeight = 0.1;

			foreach(var weightedPokemon in weightedPokemonData) {
				if(weightedPokemon.Key.Species == answerPokemon.Species)
					continue;

				weightedPokemonData[weightedPokemon.Key] += Math.Max(0, lengthMaxWeight - (Math.Abs(answerPokemon.Species.Length - weightedPokemon.Key.Species.Length) * lengthDifferenceWeight));

				if(answerPokemon.Species[0] == weightedPokemon.Key.Species[0])
					weightedPokemonData[weightedPokemon.Key] += firstLetterWeight;

				if(GetNormalCategoryWeight(answerPokemon, weightedPokemon.Key))
					weightedPokemonData[weightedPokemon.Key] += categoryWeight;

				if(GetGlitchCategoryWeight(answerPokemon, weightedPokemon.Key))
					weightedPokemonData[weightedPokemon.Key] += glitchCategoryWeight;

				if(answerPokemon.Generation == weightedPokemon.Key.Generation)
					weightedPokemonData[weightedPokemon.Key] += generationWeight;

				weightedPokemonData[weightedPokemon.Key] += specialCharactersWeight * GetSpecialCharactersWeight(answerPokemon, weightedPokemon.Key);

				weightedPokemonData[weightedPokemon.Key] += random.NextDouble() * randomMaxWeight;
			}

			weightedPokemonData = weightedPokemonData.OrderByDescending(e => e.Value).ToDictionary();

			//Prevent first 3 answers from starting with the same letter
			if(
				weightedPokemonData.ElementAt(0).Key.Species[0] == answerPokemon.Species[0]
				&& weightedPokemonData.ElementAt(1).Key.Species[0] == answerPokemon.Species[0]
			) {
				int i = 2;

				while(weightedPokemonData.ElementAt(i).Key.Species[0] == answerPokemon.Species[0]) {
					weightedPokemonData[weightedPokemonData.ElementAt(i).Key] -= 4;
					i++;
				}
			}

			//Require first 3 answers to have one that starts with the same letter
			if(
				weightedPokemonData.ElementAt(0).Key.Species[0] != answerPokemon.Species[0]
				&& weightedPokemonData.ElementAt(1).Key.Species[0] != answerPokemon.Species[0]
			) {
				int i = 2;

				while(weightedPokemonData.ElementAt(i).Key.Species[0] != answerPokemon.Species[0]) {
					i++;
				}

				if(i < weightedPokemonData.Count) {
					weightedPokemonData[weightedPokemonData.ElementAt(i).Key] += 4;
				}
			}

			weightedPokemonData = weightedPokemonData.OrderByDescending(e => e.Value).ToDictionary();

			return [answerPokemon.Species, weightedPokemonData.ElementAt(0).Key.Species, weightedPokemonData.ElementAt(1).Key.Species, weightedPokemonData.ElementAt(2).Key.Species];
		}

		public static Dictionary<PokemonStruct, double> GetWeightedPokemonData() {
			Dictionary<PokemonStruct, double> weightedPokemonData = [];

			foreach(var pokemon in pokemonData) {
				weightedPokemonData.Add(pokemon, 0);
			}

			return weightedPokemonData;
		}

		public static bool GetNormalCategoryWeight(PokemonStruct pokemonA, PokemonStruct pokemonB) {
			if(pokemonA.IsStarter && pokemonB.IsStarter)
				return true;

			if(pokemonA.IsFossil && pokemonB.IsFossil)
				return true;

			if(pokemonA.IsBaby && pokemonB.IsBaby)
				return true;

			if((pokemonA.IsLegendary || pokemonA.IsMythical) && (pokemonB.IsLegendary || pokemonB.IsMythical))
				return true;

			if(pokemonA.IsUltraBeast && pokemonB.IsUltraBeast)
				return true;

			if(pokemonA.IsParadox && pokemonB.IsParadox)
				return true;

			return false;
		}

		public static bool GetGlitchCategoryWeight(PokemonStruct pokemonA, PokemonStruct pokemonB) {
			return pokemonA.IsGlitch == pokemonB.IsGlitch;
		}

		public static int GetSpecialCharactersWeight(PokemonStruct pokemonA, PokemonStruct pokemonB) {
			int numberMatchingSC = 0;
			string checkedChars = "";

			foreach(char c1 in pokemonA.Species) {
				if(char.IsLetterOrDigit(c1))
					continue;

				if(checkedChars.Contains(c1))
					continue;

				foreach(char c2 in pokemonB.Species) {
					if(c2 == c1) {
						numberMatchingSC++;
						break;
					}
				}

				checkedChars += c1;
			}

			return numberMatchingSC;
		}

		public static void ClearCurrentConsoleLine() {
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, currentLineCursor);
		}
	}
}
