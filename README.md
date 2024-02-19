# Discord-Bot

[![Build](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/build.yml/badge.svg)](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/build.yml)
<!--- [![Deploy](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/deploy.yml/badge.svg)](https://github.com/niklasCarstensen/Discord-Bot/actions/workflows/deploy.yml) -->

This is discord chatbot. Its commands are split into two groups, simple commands that entirely depend on side effects and commands that can be chained. Those pipes of comands can for example be used for image editing.
Some of the commands may use deprecated APIs that are now offline.

## List of Commands

Command        | Description
---------------|--------------
9ball | Decides your fate
animalCrossing | Finds an animal crossing npc
bf | Brainfuck Interpreter
c# | Run csharp code
chess | Play chess
circleIsYou | You are a circle :0
edit | This is a more advanced command which allows you to chain together functions that were made specific for this command. For more information type **°edit**.
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

## List of chainable commands

AudioCommands | Description
-------|--------------
Speak | Let's a fictional character narrate the text you input using the fifteen.ai api. Working characters include GLaDOS (Emotions: Homicidal), Twilight Sparkle (Emotions: Happy, Neutral), Fluttershy (Emotions: Neutral), Rarity (Emotions: Neutral), Applejack (Emotions: Neutral), Rainbow Dash (Emotions: Neutral), Pinkie Pie (Emotions: Neutral), Derpy Hooves (Emotions: Neutral), Solider (Emotions: Neutral), Miss Pauling (Emotions: Neutral), Rise Kujikawa (Emotions: Neutral). Default Voice is GLaDOS. Available voices last updated: 03.05.2020
PlayAudio | Plays audio in voicechat
DrawAudio | Draw the samples
Pitch | Adds a Pitch to the sound
Volume | Adds Volume to the sound

DrawCommands | Description
-------|--------------
DrawText | Draw Text into pic
DrawCircle | Draw Circle into pic
DrawRect | Draw Rect into pic
Rect | Get Rect
White | Get white
Red | Get red
Col | Get a color from rgb
ColName | Get a color from name
Lerp | Lerp two colors

FunctionalCommands | Description
-------|--------------
Vector | Creates a new Vector object
ForFunc | Traverses through a for loop and creates a new object of the resulting array in each iteration
Map | 'tis map from haskel
Error | Throws an error

GifCommands | Description
-------|--------------
Rainbow | I'll try spinning colors that's a good trick
SpinToWin | I'll try spinning that's a good trick
Pat | Pat the picture
BackAndForth | Mirror the gif in time around the axis of the end time
GetPic | Get single picture from a gif
GetNumFrames | Get the number of gif frames
ToGif | Converts a bitmap array to a gif
ToBitmapArray | Converts a gif to a bitmap array
ChangeSpeed | Change the gifs playback speed
SetDelay | Change the gifs playback speed
MultiplyFrames | Copy each frame x times
Reverse | Make the gif go backwards
ForB | ForFunc for bitmaps, requires a color to color lambda pipe
MapG | Map for gifs

InputCommands | Description
-------|--------------
LastT | Gets the last messages text
LastP | Gets the last messages picture
LastG | Gets the last messages gif
LastM | Gets the last message
ThisT | Outputs the given text
ThisP | Gets attached picture / picture from url argument
ThisG | Gets attached gif / gif from url argument
ThisA | Gets mp3 or wav audio files attached to this message
GetTweet | Gets a tweet by id
ProfilePic | Gets a profile picture
ProfilePicG | Gets a profile picture gif
MyProfilePic | Gets your profile picture
ServerPic | Gets the server picture from a server id
Emote | Gets the picture of the emote
EmoteG | Gets the pictures of the emote
StringArray | Returns an string array from the argument
IntArray | Returns an int array from the argument
Range | Returns an int array with numbers from start to start + count
Mandelbrot | Render a mandelbrot
AudioFromYT | Gets the mp3 of an youtube video
AudioFromVoice | Records audio from the voice chat you are currently in

LogicCommands | Description
-------|--------------
Gt | Greater than
Lt | Less than
Geq | Greater than or equal
Leq | Less than or equal
Eq | Equal
And | And
Or | Or

MathCommands | Description
-------|--------------
Add | Addition
Sub | Subtraction
Mult | Multiplication
Div | Dividing
Pow | Powification
Sqrt | Sqrtification
Log | Logification
Sin | Sinification
Cos | Cosification
Pi | Pi
E | E
Rdm | Get random integer number
RdmDouble | Get random number between 0 and 1

MessageCommands | Description
-------|--------------
GetContent | Gets the messages text
GetMediaLinks | Gets the messages attachment links
GetVideoLinks | Gets the messages video links

MessageDBCommands | Description
-------|--------------
GetDB | Get the DB
GetAllMessages | Get those messages
FilterMessagesFrom | Get those messages
GetMessagesFromChannel | Get those messages
CountMessages | Plot those messages over time
PlotMessages | Plot those messages over time
PlotMessagesInterval | Plot those messages over time
GetUser | Gets a user
GetDate | Gets a date

PictureCommands | Description
-------|--------------
GetColor | Get the color of a pixel
ColorChannelSwap | Swap the rgb color channels for each pixel
Reddify | Make it red af
Invert | Invert the color of each pixel
Rekt | Finds colored rectangles in pictures
Memify | Turn a picture into a meme, get a list of available templates with the argument -list
TextMemify | Put text into a meme template, input -list as Meme and get a list templates
The default Font is Arial and the fontsize refers to the number of rows of text that are supposed to fit into the textbox
Expand | Expand the pixels
Stir | Stir the pixels
Fall | Fall the pixels
Wubble | Wubble the pixels
Cya | Cya the pixels
Inpand | Inpand the pixels
Blockify | Blockify the pixels
Squiggle | Squiggle the pixels
TransformPicture | Perform a liqidify transformation on the image using a custom function
SobelEdges | Highlights horizontal edges
SobelEdgesColor | Highlights horizontal edges
LaplaceEdges | https://de.wikipedia.org/wiki/Laplace-Filter
Laplace45Edges | https://de.wikipedia.org/wiki/Laplace-Filter
Sharpen | Unblur
BoxBlur | Basic Blur
GaussianBlur | more blur owo
UnsharpMasking | Sharpening but cooler
GayPride | Gay rights
TransRights | The input image says trans rights
Merkel | Add a german flag to the background of your image
ChromaticAbberation | Shifts the color spaces
Rotate | Rotate the image
RotateColors | Rotate the color spaces of each pixel
Compress | JPEG Compress the image, lower compression level = more compression
Transground | Make the background transparent
TransgroundColor | Make the background color transparent
TransgroundHSV | Thereshold HSV values to transparency, note that the SV values are in [0, 100]
LayerHSV | Split image into two layers
HueScale | Grayscaled Hue channel of the image
SatScale | Grayscaled Saturation channel of the image
ValScale | Grayscaled Value channel of the image
Transcrop | Crop the transparency
Crop | Crop the picture
Split | Split the picture into x * y pieces
RotateWhole | Rotate the image including the bounds
Flip | Flip the image
Zoom | Zoom to a certain point, zoomlevel should be between 1 and 0
Stretch | Stretch the Image
StretchM | Stretch the Image by some multipliers
GetSize | Get the size in byte of an image
GetDimensions | Get the width and height
Resize | Resize an image by some multiplier
EmoteResize | Resize an image to 48x48px which is the highest resolution discord currently displays an emote at
SeamCarve | Carve seams
SeamCarveM | Carve seams by multiplier
ForSeamCarveM | Carve seams multiple times by multiplier
FaceDetec | Detect faces
Read | Reads text
InsertIntoRect | Inserts picture into the red rectangle of another picture
Insert | Inserts picture into the rectangle of another picture
TranslateP | Translate picture
Duplicate | Duplicate picture into gif
CircleTrans | Add circular transparency to the picture

TextCommands | Description
-------|--------------
Mock | Mock the text
Crab | Crab the text
CAPS | Convert text to CAPS
SUPERCAPS | Convert text to SUPER CAPS
CopySpoilerify | Convert text to a spoiler
Spoilerify | Convert text to a spoiler
Unspoilerify | Convert spoiler text to readable text
Aestheticify | Convert text to Aesthetic text
SearchTweet | Search for a tweet with that text
Translate | Translate the text to another language
Japanify | Convert the text into katakana symbols, doesnt actually translate
Unjapanify | Convert katakana into readable stuff

VideoCommands | Description
-------|--------------
GetVideo | Gets video from link
GetVideoFromYT | Gets video from yt link
Cut | Cut a video
ConvertToGif | Convert video to gif
BakaMitai | Picture goes Baka Mitai - using code from https://github.com/AliaksandrSiarohin/first-order-model - needs vram

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
