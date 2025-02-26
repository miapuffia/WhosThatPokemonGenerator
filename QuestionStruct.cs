using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemonGenerator {
	internal struct QuestionStruct(int Id, string Question, string OptionA, string OptionB, string OptionC, string OptionD, bool FixedOrder, char CorrectAnswer, string Credit, string ImageURL, string AnswerExplanation = "") {
		public int Id { get; } = Id;
		public string Question { get; } = Question;
		public string OptionA { get; } = OptionA;
		public string OptionB { get; } = OptionB;
		public string OptionC { get; } = OptionC;
		public string OptionD { get; } = OptionD;
		public bool FixedOrder { get; } = FixedOrder;
		public char CorrectAnswer { get; } = CorrectAnswer;
		public string Credit { get; } = Credit;
		public string ImageURL { get; } = ImageURL;
		public string AnswerExplanation { get; } = AnswerExplanation;

		public override string? ToString() {
			return $"{Id}, {Question}, {OptionA}, {OptionB}, {OptionC}, {OptionD}, {FixedOrder}, {CorrectAnswer}, {Credit}, {ImageURL}, {AnswerExplanation}";
		}
	}
}
