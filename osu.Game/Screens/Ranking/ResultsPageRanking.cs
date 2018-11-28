// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public class ResultsPageRanking : ResultsPage
    {
        public ResultsPageRanking(ScoreInfo scoreInfo, WorkingBeatmap beatmap = null) : base(scoreInfo, beatmap)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GrayE,
                    RelativeSizeAxes = Axes.Both,
                },
                new Leaderboard
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Beatmap = Beatmap.BeatmapInfo ?? ScoreInfo.BeatmapInfo,
                    Scale = new Vector2(0.7f)
                }
            };
        }
    }
}
