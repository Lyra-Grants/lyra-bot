# Lyrabot Pre-Avalon _/

Pre Avalon Lyra Bot, integrated with twitter and telegram.

## Overview

Bot is .Net Core using the Nethereum + Tweetinvi libraries.

### Setup

Replace the following settings in the app config file for Twitter.

**Important** to tweet using the Twitter API you must have [elevated access](https://developer.twitter.com/en/docs/twitter-api/getting-started/about-twitter-api) which you should apply for.

```bash
 "TwitterConfig": {
    "Consumerkey": "<TWITTER_CONSUMER_KEY>",
    "ConsumerSecret": "<TWITTER_CONSUMER_SECRET>",
    "AccessToken": "<ACCESS_TOKEN>",
    "AccessTokenSecret": "ACCESS_TOKEN_SECRET>"
  },
```

Telegram requires you to set up a bot + a channel.

```bash
 "TelegramConfig": {
    "AccessToken": "<TELEGRAM_ACCESS_TOKEN>",
    "ChannelName": "<TELEGRAM_CHANNEL_NAME>"
```

### Build + Run

Publish as a self-contained net5.0 app, with single file option checked.
I had issues with running this on Linux OS, so I have this currently deployed on AWS, smallest available EC2 instance.

The following command will run the bot for ETH, params are:

```bash
    lyrabot.exe <ASSET> <TWITTER_ENABLED> <TELEGRAM_ENABLED>
```

Example:

```bash
    lyrabot.exe eth true false
```

Use the lyrabot.bat file in the scripts folder to spin up 4 versions of the bot, 1 for each asset.

## Contributing

Lyra grantsDao welcomes contributors. Regardless of the time you have available, everyone can provide meaningful contributions to the project by submitting bug reports, feature requests or even the smallest of fixes! To submit your contribution, please fork, fix, commit and create a pull-request describing your work in detail. For more details, please have a look at the [Contribution guidelines](https://github.com/Lyra-Grants/docs/blob/main/CONTRIBUTING.md).

## Contact

Join the community on the [Lyra Discord server](https://https://discord.gg/lyra)!
