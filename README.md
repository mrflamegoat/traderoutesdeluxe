# Trade Routes Deluxe

This is a mod I'm writing for Vintage Story. At the moment it's a poor mans Ender Chest (if you're familiar with Minecraft mods, which you are, I'm sure) but I have big plans! Big big plans!

Created this mod for two reasons:

1. I really like Vintage Story.
2. I really needed to learn C#, just because!
3. I can't count. Thought this might help.

## Currently Implemented List of Thigns

1. Not a whole lot.
2. A Trading Post.
3. Unwritten Parchment.
4. Written Parchment.

## Currently Not Implemented List of Things

1. ~~There's no recipes, so you can't craft anything yet.~~ There is now!
2. Literally everything else.

### Instructions for those that want to try it out!

Place down your first Trading Post. Right click with the unwritten parchment while sneaking. This will give you a Written Parchment - which is basically a network ID attached to an Item.

Place down your second, or third, or theoretically 100th trading post. Right click while sneaking and holding the written parchment. This will "sync" this blocks inventory with the network.

You can have as many networks as you want, and you can place down as many trading posts as you want.

I have not done any profiling, server testing, network testing, or load testing so BEWARE, THERE BE DRAGONS.

Thanks for reading!

## Style Notice

No new line brackets! If you're using Omnisharp, here's my current omnisharp.json:

```json
{
	"FormattingOptions": {
		"NewLinesForBracesInLambdaExpressionBody": false,
		"NewLinesForBracesInAnonymousMethods": false,
		"NewLinesForBracesInAnonymousTypes": false,
		"NewLinesForBracesInControlBlocks": false,
		"NewLinesForBracesInTypes": false,
		"NewLinesForBracesInMethods": false,
		"NewLinesForBracesInProperties": false,
		"NewLinesForBracesInAccessors": false,
		"NewLineForElse": false,
		"NewLineForCatch": false,
		"NewLineForFinally": false
	}
}
```

## Shoutouts

You don't know me, but huge shoutout to Tyron for Vintage Story and to [copygirl](https://github.com/copygirl) for an excellent VS Code tutorial to get up and running with.

## Contact Me

Find me over on [Twitter](https://www.twitter.com/mrflamegoat) - I pretty much only communicate in 240 characters or less. Or feel free to file an issue.

### Note on the licensing

I've licensed my explicit code as MIT, you're free to do whatever you want with it. Use it, steal it, run around and thow it at people. It's crap, so I don't care what you do with it. That being said, a lot of it is powered by tutorials and information gathered from a variety of resources. So if you're going to use it, maybe think about contributing back by uploading your work, too.

Mod scenes thrive when code is shared for others to learn with. I wouldn't be here without those folks.