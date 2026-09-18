# Trade Routes Deluxe

This is a mod I'm writing for Vintage Story. At the moment it's a poor mans Ender Chest (if you're familiar with Minecraft mods, which you are, I'm sure) but I have big plans! Big big plans!

Created this mod for two reasons:

1. I really like Vintage Story.
2. I really needed to learn C#, just because!
3. I can't count. Thought this might help.

## Currently Implemented List of Thigns

### Trading Post

The workhorse. Every post you link to a network shows the _same_ inventory.

- Put an item in one, it's in all of them. Take it out of one, it's gone from all of them. There's no transfer delay and no travel time, because nothing actually moves. They're all looking at the same box. Like an Ender Chest over on Minecraft.
- Distance doesn't matter. Neither does whether the other end is loaded. A post on the far side of the world in an unloaded chunk still contributes its contents.
- Multiple people can have the same network open at once, from different posts, without stomping on each other.
- Breaking a post just removes that node. The network and its contents carry on without it.
- Breaking the **last** post on a network drops everything at your feet, so you can't lose your stuff by dismantling your own setup.
- Contents do **not** keep. Food spoils at the same rate it would in a chest sitting out in the open, regardless of where you put the post. This is deliberate since instant transport anywhere shouldn't also be free cold storage. Perishables are the cost of doing business.

### Fur-Lined Trading Post

The insulated upgrade, for when you're shipping food and tired of losing it. It keeps things cool, despite the name being cozy.

- Made by sneak + right-clicking an existing Trading Post while holding a pelt. The block upgrades in place with the same network, same contents, and nothing to relink.
- Halves the spoilage rate, but **only when every post on the network is fur-lined**. One plain post anywhere on the route drags the whole thing back to normal. A chain is as warm (or cool) as its warmest link.
- Upgrade the last plain post and the whole network improves the moment you do it. Add a plain post to a finished network and it degrades just as fast.
- It's an improvement, not a pantry. A properly built cellar still preserves roughly twice as well, and always will. This is for keeping cheese alive in transit, not for storing it.

### Blank Parchment

A key that hasn't been cut yet. It isn't a trade route until you use it.

- Sneak + right-click a Trading Post with one. It gets consumed and you get a Trade Route back.
- If that post wasn't on a network yet, this creates one and connects it.

### Trade Route

The key to a specific network, and the only thing that knows that network exists. Don't lose all of them.

- Sneak + right-click any Trading Post with one to connect that post to the route.
- Each one is named after its route, like **Trade Route a3f9c21**, so you can tell them apart at a glance. Hover for the full ID.
- They stack and copy freely. Make as many as you like and hand them to whoever should have access!
- If the route it points at is gone (you dismantled it, and its contents were dropped), it quietly re-keys itself to a fresh one instead of failing. Old routes never become dead weight.

## Currently Not Implemented List of Things

1. ~~There's no recipes, so you can't craft anything yet.~~ There is now!
2. Literally everything else.

### Instructions for those that want to try it out!

Place down your first Trading Post. Right click with a Blank Parchment while sneaking. This will give you a Trade Route!

Place down your second, or third, or theoretically 100th trading post. Right click while sneaking and holding that Trade Route. This will "sync" this blocks inventory with the network.

You can have as many networks as you want, and you can place down as many trading posts as you want.

I have not done any profiling, server testing, network testing, or load testing so BEWARE, THERE BE DRAGONS.

Thanks for reading!

## Building

You'll need:

1. Vintage Story 1.22.7 or later.
2. The [.NET 10 SDK](https://dotnet.microsoft.com/download).

Set the `VINTAGE_STORY` environment variable to wherever you installed the game. The build reads it to find `VintagestoryAPI.dll` and friends, and it'll stop with an angry message if it can't find it.

```powershell
[Environment]::SetEnvironmentVariable("VINTAGE_STORY", "C:\path\to\Vintagestory", "User")
```

Then build and package:

```
./build.ps1
```

or `./build.sh` on Linux/macOS. The finished mod zip lands in `Releases/`.

## Shoutouts

You don't know me, but huge shoutout to Tyron for Vintage Story and to [copygirl](https://github.com/copygirl) for an excellent VS Code tutorial to get up and running with.

## Contact Me

Find me over on [Twitter](https://www.twitter.com/mrflamegoat) - I pretty much only communicate in 240 characters or less. Or feel free to file an issue.

### Note on the licensing

I've licensed my explicit code as MIT, you're free to do whatever you want with it. Use it, steal it, run around and thow it at people. It's crap, so I don't care what you do with it. That being said, a lot of it is powered by tutorials and information gathered from a variety of resources. So if you're going to use it, maybe think about contributing back by uploading your work, too.

Mod scenes thrive when code is shared for others to learn with. I wouldn't be here without those folks.
