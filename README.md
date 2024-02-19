# Discord-Bot

[![Build](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/build.yml/badge.svg)](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/build.yml)
<!--- [![Deploy](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/deploy.yml/badge.svg)](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/deploy.yml) -->

This is discord chatbot. Its commands are split into two groups, simple commands that entirely depend on side effects and commands that can be chained. Those pipes of comands can for example be used for image editing.

## List of Commands

Command        | Description
---------------|--------------
edit | This is a little more advanced command which allows you to chain together functions that were made specific for this command. For more information type **°edit**.
9ball | Decides your fate
animalCrossing | Finds an animal crossing npc
bf | Brainfuck Interpreter
c# | Run csharp code
chess | Play chess
circleIsYou | You are a circle :0
embed | Posts an embed
emojiUsage | Which emojis are actually used on this server?
environment | Prints bot environment info
Explain | -
GetVideoLink | -
loadConfig | loads the attached config
meme | Automatically steals a meme from reddit
messageDB | Builds all messages of a server into a database (large json file) and implements operations on it such as generating an activity plot
messageInfo | Posts message information, takes message ID as argument
nice | Check if a user is nice
overwatch | Prints todays overwatch arcade game modes
Padoru | -
Ping | -
place | Basically just r/place
play | Plays youtube videos in voice chats
poll | Creates a poll
profile | Prints your bot profile (GDPR?-Style)
Reacter | -
reaction | Post reaction images
sendRoleEmojiMessage | Let users of your server get roles from emote reactions
serverInfo | Posts server information
tictactoe | Play TicTacToe against other people!
timer | Posts a updating message for some event
toggleMessageLinkPreviews | Preview message links on this server
togglePatchNotes | Get annoying messages
uno | Play uno with other humans
userInfo | Posts user information, takes message ID as argument
warframe | Get notifications for warframe rewards or check the world state

## Examples

An example pipe for a mandelbrot zoom with rotating colors: `$- for(i:0:20:0.2) { mandelbrot(%i, 1.01:0.28, 100) } > foreach(i:0:360) { rotateColors(%i) }` resulting in this gif:
![alt text](https://cdn.discordapp.com/attachments/630515207608729640/652122985108471828/-8586260583075901868.gif)

Another example of an edit pipe would be this: `$edit "https://cdn.discordapp.com/attachments/491277915015610389/666212145179918337/image.png" > invert > for(i:-0.9:1.9:0.02) { fall(%i:0.5, 0.5)}` resulting in:![alt text](https://cdn.discordapp.com/attachments/500759857205346304/749076750054719498/-8586029426713066715.gif)

## Deployment

If you have a dotnet core 3 runtime installed you can just type `dotnet run` in the command line and it will run. If you want to run him on a server or you like running applications as docker containers for no reason you can type `docker-compose build` and it will start as a ubuntu docker container. Alternatively if you have Visual Studio installed you can also run him there and even observe the execution in debug mode. If that is not enough for you you can also put him in a gitlab and the gitlab yml will tell the runner what to do.

However, if you only do this the bot will tell you that it can't log in. That's because it needs a discord bot token to work. If you got one you can feed it to the bot as an environment variable called `BotToken`. <!--- Set it in the console, add it to the visual studio environment variables, add them to your gitlab runner or find some other way. If you want to use the bot to its full potential you also have to set the FOUR other twitter tokens named: `customer_key`, `customer_key_secret`, `access_token` and of course the `access_token_secret`. Why four? Idk. -->

Have fun ^^

## Acknowledgements

Thanks to everyone who found new ways to use this bot and give me gifs to show here :D
