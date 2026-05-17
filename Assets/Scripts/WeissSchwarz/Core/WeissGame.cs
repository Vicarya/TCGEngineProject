using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCG.Core;
using TCG.Weiss.UI;
using UnityEngine;

namespace TCG.Weiss
{
    public class WeissGame : GameBase
    {
        private readonly WeissBattleSetupOptions _setupOptions;

        public new WeissGameState GameState => base.GameState as WeissGameState;
        public WeissRuleEngine RuleEngine { get; protected set; }

        public WeissGame() : this(WeissBattleSetupOptions.CreateDefault())
        {
        }

        public WeissGame(WeissBattleSetupOptions setupOptions)
        {
            _setupOptions = setupOptions ?? WeissBattleSetupOptions.CreateDefault();
            RuleEngine = new WeissRuleEngine(this.GameState);
        }

        protected override GameState CreateGameState()
        {
            return new WeissGameState(this);
        }

        public async Task PerformMulliganPhase()
        {
            foreach (WeissPlayer player in GameState.Players)
            {
                if (GameView.Instance != null)
                {
                    GameView.Instance.UpdateView(player);
                }

                var handZone = player.GetZone<IHandZone<WeissCard>>();
                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
                var deckZone = player.GetZone<IDeckZone<WeissCard>>();

                var cardsToMulligan = await player.Controller.ChooseMulliganCards(player, new List<WeissCard>(handZone.Cards));
                if (cardsToMulligan != null && cardsToMulligan.Count > 0)
                {
                    foreach (var card in cardsToMulligan)
                    {
                        RuleEngine.MoveCard(card, handZone as IZone, waitingRoom as IZone, player);
                    }

                    for (int i = 0; i < cardsToMulligan.Count; i++)
                    {
                        var newCard = deckZone.DrawTop();
                        if (newCard != null)
                        {
                            RuleEngine.MoveCard(newCard as WeissCard, null, handZone as IZone, player);
                        }
                    }
                }

                if (GameView.Instance != null)
                {
                    GameView.Instance.UpdateView(player);
                }
            }
        }

        protected override void SetupGame(GameState state)
        {
            var cardDatabase = WeissCardDatabase.EnsureInitialized();
            if (cardDatabase == null)
            {
                throw new System.InvalidOperationException("CRITICAL: WeissCardDatabase Instance is missing! SetupGame cannot proceed.");
            }

            var player1 = CreatePlayer(_setupOptions.Player1);
            var player2 = CreatePlayer(_setupOptions.Player2);

            state.Players.Add(player1);
            state.Players.Add(player2);

            foreach (WeissPlayer player in new[] { player1, player2 })
            {
                var levelZone = new LevelZone(player);
                var waitingRoom = new WaitingRoomZone(player);
                var clockZone = new ClockZone(player, levelZone, waitingRoom);
                var deckZone = new DeckZone(player);
                var handZone = new HandZone(player);
                var stageZone = new StageZone(player);
                var stockZone = new StockZone(player);
                var climaxZone = new ClimaxZone(player);
                var memoryZone = new MemoryZone(player);
                var resolutionZone = new ResolutionZone(player);

                player.RegisterZone<ILevelZone<WeissCard>>(levelZone);
                player.RegisterZone<IDiscardPile<WeissCard>>(waitingRoom);
                player.RegisterZone<IClockZone<WeissCard>>(clockZone);
                player.RegisterZone<IDeckZone<WeissCard>>(deckZone);
                player.RegisterZone<IHandZone<WeissCard>>(handZone);
                player.RegisterZone<IStageZone<WeissCard>>(stageZone);
                player.RegisterZone<IStockZone<WeissCard>>(stockZone);
                player.RegisterZone<IClimaxZone<WeissCard>>(climaxZone);
                player.RegisterZone<IMemoryZone<WeissCard>>(memoryZone);
                player.RegisterZone<IResolutionZone<WeissCard>>(resolutionZone);

                var playerSetup = ResolveSetupForPlayer(player);
                var deckDefinition = WeissDeckLoader.LoadDeck(
                    playerSetup.DeckId,
                    cardDatabase,
                    _setupOptions.DeckSize,
                    _setupOptions.MaxCopiesPerCard);

                if (_setupOptions.ValidateDecks)
                {
                    var errors = WeissDeckValidator.Validate(
                        deckDefinition,
                        cardDatabase,
                        _setupOptions.DeckSize,
                        _setupOptions.MaxCopiesPerCard);

                    if (errors.Count > 0)
                    {
                        state.EventBus.Raise(new GameEvent(BaseGameEvents.PlayerLost, player));
                        throw new System.InvalidOperationException($"Invalid deck for {player.Name}: {string.Join(" / ", errors)}");
                    }
                }

                LoadDeckIntoZone(deckDefinition, player, deckZone, cardDatabase);
                deckZone.Shuffle();

                for (int i = 0; i < _setupOptions.InitialHandSize; i++)
                {
                    var card = deckZone.DrawTop();
                    if (card != null)
                    {
                        RuleEngine.MoveCard(card as WeissCard, null, handZone as IZone, player);
                    }
                }
            }

            state.CurrentPlayerIndex = Random.Range(0, state.Players.Count);
            state.EventBus.Raise(new GameEvent(BaseGameEvents.TurnStarted, state.ActivePlayer));
        }

        private WeissPlayer CreatePlayer(WeissPlayerSetupOptions options)
        {
            var playerOptions = options ?? new WeissPlayerSetupOptions();
            var controller = CreateController(playerOptions.ControlMode);
            return new WeissPlayer(playerOptions.Name, playerOptions.ControlMode, controller);
        }

        private static IWeissPlayerController CreateController(PlayerControlMode controlMode)
        {
            return controlMode == PlayerControlMode.Npc
                ? new SimpleNpcWeissPlayerController()
                : new HumanWeissPlayerController();
        }

        private WeissPlayerSetupOptions ResolveSetupForPlayer(WeissPlayer player)
        {
            if (_setupOptions.Player1 != null && _setupOptions.Player1.Name == player.Name)
            {
                return _setupOptions.Player1;
            }

            if (_setupOptions.Player2 != null && _setupOptions.Player2.Name == player.Name)
            {
                return _setupOptions.Player2;
            }

            return player.ControlMode == PlayerControlMode.Npc ? _setupOptions.Player2 : _setupOptions.Player1;
        }

        private static void LoadDeckIntoZone(
            WeissDeckDefinition deckDefinition,
            WeissPlayer player,
            DeckZone deckZone,
            WeissCardDatabase cardDatabase)
        {
            foreach (var entry in deckDefinition.Entries)
            {
                var cardData = cardDatabase.AllCards.First(card => card.CardCode == entry.CardCode);
                for (int i = 0; i < entry.Count; i++)
                {
                    deckZone.AddCard(new WeissCard(cardData, player));
                }
            }
        }
    }
}
