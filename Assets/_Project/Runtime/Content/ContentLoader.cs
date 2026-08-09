using System;

public sealed class ContentLoader
{
    public ContentLoader(GameDefinition game)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
        Database = game.Content
            ?? throw new InvalidOperationException(
                $"{game.name} does not reference a ContentDatabase.");

        if (string.IsNullOrWhiteSpace(game.FirstStorySceneId)
            || !Database.TryGetStoryScene(game.FirstStorySceneId, out _))
        {
            throw new InvalidOperationException(
                $"{game.name} has an invalid first Story Scene "
                + $"'{game.FirstStorySceneId}'.");
        }
    }

    public GameDefinition Game { get; }
    public ContentDatabase Database { get; }
}
