﻿using System;
using System.Collections.Generic;
using System.Linq;
using DosGame.Deck;
using DosGame.Deck.Generation;
using DosGame.Extensions;
using DosGame.Model;
using DosGame.State;
using DosGame.Util;

namespace DosGame
{
    public class Game : IGame
    {
        public List<Card> centerRow = new List<Card>(8);
        public List<List<Card>> centerRowAdditional = new List<List<Card>>(8);
        public bool CurrentPlayerDidNotCallDos = false;
        public int CurrentPlayerPenalty = 0;

        public Stack<Card> deck;
        public Stack<Card> discardPile;

        public List<Card>[] playerHands;

        public Game() : this(new ShuffledDeckGenerator(Decks.Classic), 2, 7)
        {
        }

        public Game(IDeckGenerator deckGenerator, int playersCount, int initialHandSize)
        {
            deck = new Stack<Card>(deckGenerator.Generate());
            discardPile = new Stack<Card>();

            playerHands = new List<Card>[playersCount];
            for (var i = 0; i < playersCount; i++)
            {
                playerHands[i] = new List<Card>(10);
                DealCards(i, initialHandSize);
            }

            EnsureCenterRowIsValid();
            CurrentPlayer = new Random().Next(playersCount);
            CurrentState = new MatchingCenterRowState(this);
        }

        public GameState CurrentState { get; set; }

        public int CalloutPenalty { get; set; }
        public int FalseCalloutPenalty { get; set; }

        public int CurrentPlayer { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string>();
        public int PlayersCount => playerHands.Length;

        public List<Card> CurrentPlayerHand => playerHands[CurrentPlayer];

        public List<(string name, int score)> ScoreTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), playerHands[i].Sum(c => c.Points)))
                      .ToList();

        public List<(string name, int cardsCount)> HandsTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), playerHands[i].Count))
                      .ToList();

        public string GetPlayerName(int id) => PlayerNames.TryGetValue(id, out var name) ? name : "Player " + id;

        public void DealCards(int player, int amount)
        {
            for (var i = 0; i < amount; i++) DealCard(player);
        }

        public void DealCard(int player)
        {
            playerHands[player].Add(DrawCard());

            if (player == CurrentPlayer)
            {
                CheckCurrentPlayerForDos();
            }
        }

        public void CheckCurrentPlayerForDos()
        {
            if (playerHands[CurrentPlayer].Count == 2) CurrentPlayerDidNotCallDos = true;
        }

        public Card DrawCard()
        {
            EnsureDeckHasCards();
            return deck.Pop();
        }

        public void EnsureDeckHasCards()
        {
            if (deck.Any()) return;
            var newDeck = discardPile.ToList();
            newDeck.Shuffle();
            deck = new Stack<Card>(newDeck);
            discardPile.Clear();
        }

        public void EnsureCenterRowIsValid()
        {
            while (centerRow.Count < 2)
            {
                centerRow.Add(DrawCard());
            }

            while (centerRowAdditional.Count < centerRow.Count)
            {
                centerRowAdditional.Add(new List<Card>());
            }
        }

        public void MoveTurnToNextPlayer()
        {
            DealCards(CurrentPlayer, CurrentPlayerPenalty);
            CurrentPlayerPenalty = 0;
            CurrentPlayerDidNotCallDos = false;
            CurrentPlayer = (CurrentPlayer + 1) % PlayersCount;
        }

        public Result<string> MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            CurrentState.MatchCenterRowCard(player, target, cardsToPlay);

        public Result<string> FinishMatching(int player) => CurrentState.FinishMatching(player);
        public Result<string> Draw(int player) => CurrentState.Draw(player);

        public Result<string> AddCardToCenterRow(int player, Card card) =>
            CurrentState.AddCardToCenterRow(player, card);

        public Result<string> Callout(int caller) => CurrentState.Callout(caller);
        public Result<string> CallDos(int caller) => CurrentState.CallDos(caller);
    }
}
