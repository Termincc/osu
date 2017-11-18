﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using System.Linq;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private readonly ScrollContainer scrollContainer;
        private FillFlowContainer<LeaderboardScore> scrollFlow;

        public Action<Score> ScoreSelected;

        private readonly LoadingAnimation loading;

        private IEnumerable<Score> scores;

        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;
                getScoresRequest?.Cancel();

                int i = 150;
                if (scores == null)
                {
                    if (scrollFlow != null)
                    {
                        foreach (var c in scrollFlow.Children)
                            c.FadeOut(i += 10);

                        foreach (var c in scrollFlow.Children)
                            c.LifetimeEnd = Time.Current + i;
                    }

                    return;
                }

                // schedule because we may not be loaded yet (LoadComponentAsync complains).
                Schedule(() =>
                {
                    LoadComponentAsync(new FillFlowContainer<LeaderboardScore>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 5f),
                        Padding = new MarginPadding { Top = 10, Bottom = 5 },
                        ChildrenEnumerable = scores.Select((s, index) => new LeaderboardScore(s, index + 1) { Action = () => ScoreSelected?.Invoke(s) })
                    }, f =>
                    {
                        scrollFlow?.Expire();
                        scrollContainer.Add(scrollFlow = f);

                        i = 0;
                        foreach (var s in f.Children)
                        {
                            using (s.BeginDelayedSequence(i++ * 50, true))
                                s.Show();
                        }

                        scrollContainer.ScrollTo(0f, false);
                    });
                });
            }
        }

        public Leaderboard()
        {
            Children = new Drawable[]
            {
                scrollContainer = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                },
                loading = new LoadingAnimation()
            };
        }

        private APIAccess api;

        private BeatmapInfo beatmap;

        private ScheduledDelegate pendingBeatmapSwitch;

        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (beatmap == value) return;

                beatmap = value;
                Scores = null;

                pendingBeatmapSwitch?.Cancel();
                pendingBeatmapSwitch = Schedule(updateScores);
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private GetScoresRequest getScoresRequest;

        private void updateScores()
        {
            if (!IsLoaded) return;

            Scores = null;
            getScoresRequest?.Cancel();

            if (api == null || Beatmap?.OnlineBeatmapID == null) return;

            loading.Show();

            getScoresRequest = new GetScoresRequest(Beatmap);
            getScoresRequest.Success += r =>
            {
                Scores = r.Scores;
                loading.Hide();
            };
            api.Queue(getScoresRequest);
        }

        protected override void Update()
        {
            base.Update();

            var fadeStart = scrollContainer.Current + scrollContainer.DrawHeight;

            if (!scrollContainer.IsScrolledToEnd())
                fadeStart -= LeaderboardScore.HEIGHT;

            if (scrollFlow == null) return;

            foreach (var c in scrollFlow.Children)
            {
                var topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, scrollFlow).Y;
                var bottomY = topY + LeaderboardScore.HEIGHT;

                if (bottomY < fadeStart)
                    c.Colour = Color4.White;
                else if (topY > fadeStart + LeaderboardScore.HEIGHT)
                    c.Colour = Color4.Transparent;
                else
                {
                    c.Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(Math.Min(1 - (topY - fadeStart) / LeaderboardScore.HEIGHT, 1)),
                        Color4.White.Opacity(Math.Min(1 - (bottomY - fadeStart) / LeaderboardScore.HEIGHT, 1)));
                }
            }
        }
    }
}
