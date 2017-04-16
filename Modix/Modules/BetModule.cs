using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Group("bet"), Name("Bet"), Summary("A fun betting game involving random numbers!")]
    public class BetModule : ModuleBase
    {
        private enum BettingState
        {
            Idle,
            Stage,
            Rolling,
            Results
        }

        private const int StageDurationInSeconds = 30;
        private const int StageStepLengthInSeconds = 5;
        private const int RollingDurationInSeconds = 30;
        private const int RollingStepLengthInSeconds = 5;

        private static int _stageCounter;
        private static int _rollingCounter;
        private static IUser _sessionOwner;
        private static BettingState _state;
        private static Timer _stageTimer;
        private static Timer _rollingTimer;
        private static List<IUser> _joinedUsers = new List<IUser>();
        private static Dictionary<IUser, int> _rolls = new Dictionary<IUser, int>();
        private static Random _randomGenerator = new Random();

        [Command("Stage"), Summary("Sets the stage for a new betting game. Players can join during this time with !bet join")]
        public async Task StageAsync()
        {
            if (_state != BettingState.Idle)
            {
                if (_state == BettingState.Stage && Context.User.Id != _sessionOwner.Id)
                {
                    await Context.Channel.SendMessageAsync($"MODiX is currently setting the stage for a bet. There is only `{StageDurationInSeconds - _stageCounter}` seconds left to join. `!bet join` to participate in this session");
                }
                else if (_state == BettingState.Stage && Context.User.Id == _sessionOwner.Id)
                {
                    await Context.Channel.SendMessageAsync($"You're the owner of the current session, {Context.User.Mention}. Type `!bet start` to begin the game!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("MODiX is currently in a bet. Please wait for the current game to end");
                }

                return;
            }

            _state = BettingState.Stage;
            _sessionOwner = Context.User;
            _joinedUsers.Add(_sessionOwner);
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} has set the stage for a new betting game. `!bet join` to participate in this session. `!bet start` to begin!");

            _stageTimer = new Timer(async (state) =>
            {
                if (_state == BettingState.Rolling)
                {
                    // It's possible there could be a race condition. Account for it here.
                    ResetStageTimer();
                }
                else if (_stageCounter >= StageDurationInSeconds && _joinedUsers.Count < 2)
                {
                    await Context.Channel.SendMessageAsync($"Not enough players joined {Context.User.Mention}'s session. Start another game with `!bet stage`");
                    ResetStageTimer();
                }

                _stageCounter += StageStepLengthInSeconds;

            }, _stageCounter, 0, 1000 * StageStepLengthInSeconds);
        }

        [Command("Start"), Summary("Starts a new session of the game")]
        public async Task StartAsync()
        {
            if (_state == BettingState.Stage && Context.User.Id == _sessionOwner.Id)
            {
                ResetStageTimer();
                _state = BettingState.Rolling;

                await Context.Channel.SendMessageAsync("Ok everyone, The stage is set! `!bet roll` to place your bets!");

                _rollingTimer = new Timer(async (state) =>
                {
                    if (_rollingCounter >= RollingDurationInSeconds && _rolls.Count == 0)
                    {
                        await Context.Channel.SendMessageAsync($"Well this is awkward... no one rolled. Really {_sessionOwner.Mention}, you didn't even roll in your own game?");
                        _state = BettingState.Idle;
                        ResetRollTimer();
                        return;
                    }
                    else if (_rollingCounter >= RollingDurationInSeconds || _state == BettingState.Results)
                    {
                        // Show the results!
                        var sb = new StringBuilder();
                        int ctr = 1;
                        IUser winner = null;
                        foreach (var roll in _rolls.OrderByDescending(r => r.Value))
                        {
                            if (ctr == 1)
                            {
                                winner = roll.Key;
                            }
                            sb.AppendLine($"{ctr++}.  {roll.Key.Username}: {roll.Value}");
                        }

                        var builder = new EmbedBuilder()
                            .WithColor(new Color(95, 186, 125))
                            .WithTitle($"Congratulations {winner.Username}, you are the winner!")
                            .WithDescription(sb.ToString())
                            .WithFooter(new EmbedFooterBuilder().WithText($"Thank you {_sessionOwner.Username} for hosting this game."));

                        builder.Build();
                        await Context.Channel.SendMessageAsync("", embed: builder);
                        _state = BettingState.Idle;
                        ResetRollTimer();
                        return;
                    }
                    else if (RollingDurationInSeconds - _rollingCounter <= 5)
                    {
                        await Context.Channel.SendMessageAsync("5 seconds... `!bet roll` to play!");
                    }
                    else if (RollingDurationInSeconds - _rollingCounter <= 10)
                    {
                        await Context.Channel.SendMessageAsync("There's only 10 seconds left to roll. `!bet join` to play!");
                    }

                    _rollingCounter += RollingStepLengthInSeconds;

                }, _rollingCounter, 0, 1000 * RollingStepLengthInSeconds);
            }
            else if (_state == BettingState.Stage && Context.User.Id != _sessionOwner.Id)
            {
                await Context.Channel.SendMessageAsync($"MODiX is currently setting the stage for a bet. There is only `{StageDurationInSeconds - _stageCounter}` seconds left to join. `!bet join` to participate in this session");
            }
            else if (_state == BettingState.Rolling)
            {
                await Context.Channel.SendMessageAsync("MODiX is currently in a bet. Please wait for the current game to end");
            }
            else
            {
                await Context.Channel.SendMessageAsync("There is no bet currently in progress. `!bet stage` to start a new session");
            }
        }

        [Command("Join"), Summary("Joins a session")]
        public async Task JoinAsync()
        {
            if (_state == BettingState.Stage && Context.User.Id != _sessionOwner.Id && !_joinedUsers.Any(u => u.Id == Context.User.Id))
            {
                _joinedUsers.Add(Context.User);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention}, has joined the bet.");
            }
            else if (_state == BettingState.Stage && Context.User.Id == _sessionOwner.Id)
            {
                await Context.Channel.SendMessageAsync($"You're the owner of the current session, {Context.User.Mention}. Type `!bet start` to begin the game!");
            }
            else if (_state == BettingState.Rolling)
            {
                await Context.Channel.SendMessageAsync("MODiX is currently in a bet. Please wait for the current game to end");
            }
            else
            {
                await Context.Channel.SendMessageAsync("There is no bet currently in progress. `!bet stage` to start a new session");
            }
        }

        [Command("Roll"), Summary("Roll the dice")]
        public async Task RollAsync()
        {
            if (_state == BettingState.Rolling && _joinedUsers.Any(u => u.Id == Context.User.Id) && !_rolls.Any(r => r.Key.Id == Context.User.Id))
            {
                _rolls.Add(Context.User, _randomGenerator.Next(50001));
                await Context.Channel.SendMessageAsync($"{Context.User.Mention}, your roll has been recorded.");

                if (_rolls.Count == _joinedUsers.Count)
                {
                    await Context.Channel.SendMessageAsync("Everyone has rolled... tallying up results");
                    _state = BettingState.Results;
                }
            }
            else if (_state == BettingState.Stage && Context.User.Id != _sessionOwner.Id)
            {
                await Context.Channel.SendMessageAsync($"MODiX is currently setting the stage for a bet. There is only `{StageDurationInSeconds - _stageCounter}` seconds left to join. `!bet join` to participate in this session");
            }
            else
            {
                await Context.Channel.SendMessageAsync("There is no bet currently in progress. `!bet stage` to start a new session");
            }
        }

        private static void ResetStageTimer()
        {
            _stageTimer.Dispose();
            _stageCounter = 0;

            if (_state == BettingState.Idle)
            {
                _rolls.Clear();
                _joinedUsers.Clear();
                _sessionOwner = null;
            }

            _state = BettingState.Idle;
        }

        private static void ResetRollTimer()
        {
            _rollingTimer.Dispose();
            _rollingCounter = 0;

            _rolls.Clear();
            _joinedUsers.Clear();
            _sessionOwner = null;
        }
    }
}
