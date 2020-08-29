# Discord-Bot

This is quite a large discord bot with a weird name. The commands are split into two groups, simple commands that entirely depend on side effects and edit commands that get data from other edit commands and output data that other edit commands can use. Those pipes of edit comands can for example be used for image editing.
Example pipe for a mandelbrot zoom with rotating colors: `$- for(i:0:20:0.2) { mandelbrot(%i, 1.01:0.28, 100) } > foreach(i:0:360) { rotateColors(%i) }` resulting in this gif:
![alt text](https://cdn.discordapp.com/attachments/630515207608729640/652122985108471828/-8586260583075901868.gif)
Another example of an edit pipe would be this: `$edit "https://cdn.discordapp.com/attachments/491277915015610389/666212145179918337/image.png" > invert > for(i:-0.9:1.9:0.02) { fall(%i:0.5, 0.5)}` resulting in ![alt text](https://cdn.discordapp.com/attachments/500759857205346304/749076750054719498/-8586029426713066715.gif)

## How to use this bad boi

If you have a dotnet core 3 runtime installed you can just type `dotnet run` in the command line and it will run. If you want to run him on a server or you like running applications as docker containers for no reason you can type `docker-compose build` and it will start as a ubuntu docker container. Alternatively if you have Visual Studio installed you can also run him there and even observe the execution in debug mode. If that is not enough for you you can also put him in a gitlab and the gitlab yml will tell the runner what to do.

But wait, if you only do this the bot will tell you that it can't log in. Thats because it needs at least a discord bot token to work. If you got one you can feed it to the bot as an enviroment variable called `BotToken`. Set it in the console, add it to the visual studio environment variables, add them to your gitlab runner or find some other way. If you want to use the bot to its full potential you also have to set the FOUR other twitter tokens named: `customer_key`, `customer_key_secret`, `access_token` and of course the `access_token_secret`. Why four? Idk.

Have fun ^^

## Acknowledgements

Thanks to everyone who found new ways to use this bot and give me nice gifs to show here :D
