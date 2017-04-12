﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HDT.Plugins.Common.Data;
using HDT.Plugins.Common.Data.Models;
using HDT.Plugins.Common.Data.Services;
using HDT.Plugins.EndGame.Models;
using HDT.Plugins.EndGame.Utilities;
using HDT.Plugins.EndGame.ViewModels;
using Moq;
using NUnit.Framework;

namespace HDT.Plugins.EndGame.Tests.ViewModels
{
	[TestFixture]
	public class NoteViewModelTest
	{
		private NoteViewModel viewModel;
		private Mock<IDataRepository> trackMock;
		private Mock<ILoggingService> logMock;

		[SetUp]
		public void TestSetup()
		{
			var deck = new Deck(PlayerClass.DRUID, true);
			deck.Cards = new List<Card>();

			logMock = new Mock<ILoggingService>();
			trackMock = new Mock<IDataRepository>();

			trackMock.Setup(x => x.GetGameNote()).Returns("Note Text");
			trackMock.Setup(x => x.GetOpponentDeck()).Returns(deck);
			trackMock.Setup(x => x.GetAllDecksWithTag(Strings.ArchetypeTag)).Returns(new List<Deck>());

			viewModel = new NoteViewModel(trackMock.Object, logMock.Object);
		}

		[Test]
		public void DefaultNoteValue_ComesFromRepository()
		{
			Assert.That(viewModel.Note, Is.EqualTo("Note Text"));
		}

		[Test]
		public void NoteChange_UpdatesRepository()
		{
			viewModel.Note = "Changed Text";

			Assert.That(() =>
				trackMock.Verify(x => x.UpdateGameNote("Changed Text")),
				Throws.Nothing);
		}

		[Test]
		public void DeckSelectedChange_WritesToNote_IfSimilarityAboveThreshold()
		{
			viewModel.SelectedDeck = new MatchResult(
				new ArchetypeDeck("A Deck", PlayerClass.DRUID, false),
				MatchResult.THRESHOLD + 0.1f);

			Assert.That(() =>
				trackMock.Verify(x => x.UpdateGameNote("[A Deck] Note Text")),
				Throws.Nothing);
		}

		[Test]
		public void EmptyDeckName_DoesNotChangeNote()
		{
			viewModel.SelectedDeck = new MatchResult(
				new ArchetypeDeck("", PlayerClass.DRUID, false), 0);

			Assert.That(viewModel.Note, Is.EqualTo("Note Text"));
		}

		[Test]
		public void DeckSelectedChange_ReplacesPreviousDeckNote_IfSimilarityAboveThreshold()
		{
			viewModel.Note = "[Previous Deck] Other notes";
			viewModel.SelectedDeck = new MatchResult(
				new ArchetypeDeck("A Deck", PlayerClass.DRUID, false),
				MatchResult.THRESHOLD + 0.1f);

			Assert.That(() =>
				trackMock.Verify(x => x.UpdateGameNote("[A Deck] Other notes")),
				Throws.Nothing);
		}

		// TODO keep for reference temporarily
		//[Test]
		//public void IfScreenshotsIsNullOnClosing_LogMessage()
		//{
		//	viewModel.Screenshots = null;
		//	viewModel.WindowClosingCommand.Execute(null);

		//	Assert.That(() =>
		//		logMock.Verify(x => x.Debug("No screenshot selected (len=)")),
		//		Throws.Nothing);
		//}

		//[Test]
		//public void OnClosing_SaveSelectedScreenshot()
		//{
		//	viewModel.Screenshots = new ObservableCollection<Screenshot>() {
		//		new Screenshot(null, null, 0) { IsSelected = true }
		//	};
		//	viewModel.WindowClosingCommand.Execute(null);

		//	Assert.That(() =>
		//		capMock.Verify(x => x.SaveImage(It.IsAny<Screenshot>()), Times.Once),
		//		Throws.Nothing);
		//}

		//[Test]
		//public void OnClosing_CatchExceptionsOnSavingImage()
		//{
		//	capMock.Setup(x => x.SaveImage(It.IsAny<Screenshot>())).Throws<Exception>();
		//	viewModel.Screenshots = new ObservableCollection<Screenshot>() {
		//		new Screenshot(null, null, 0) { IsSelected = true }
		//	};
		//	viewModel.WindowClosingCommand.Execute(null);

		//	Assert.That(() =>
		//		logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once),
		//		Throws.Nothing);
		//}
	}
}